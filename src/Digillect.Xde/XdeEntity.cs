using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Digillect.Xde
{
	/// <summary>
	/// Сущность объекта данных.
	/// </summary>
	[System.Diagnostics.DebuggerDisplay("Name = {Name} EntityType = {EntityType}")]
	public class XdeEntity : XdeHierarchyObject, IXdeDatabaseObject
	{
		private bool m_exists;
		private bool m_modified = true;
		private ColumnCollection m_columns;
		private ICollection<XdeConstraint> m_constraints;
		private ICollection<XdeIndex> m_indexes;
		private ICollection<XdeEntityColumn> m_deletedColumns;
		private readonly XdeEntityType m_entityType;
		private XdeIndex m_primaryKey;
		private XdeExecutionOperation m_operation = XdeExecutionOperation.Save;

		#region ctor
		public XdeEntity(XdeSession session, string name, XdeEntityType type, bool initializeColumns)
			: base(session.Registration, name.ToUpper())
		{
			m_entityType = type;

			if ( initializeColumns )
			{
				m_columns = new ColumnCollection(this);
				m_constraints = new XdeObjectCollection<XdeConstraint>(this);
				m_indexes = new XdeObjectCollection<XdeIndex>(this);

				XdeEntityColumn idColumn = new XdeEntityColumn(this, XdeEntityColumnMetadata.GetUniqueidentifierColumnInfo());
				idColumn.Modified = false;

				m_columns.Add(idColumn);
			}

			RepairPrimaryKey();
		}
		#endregion

		#region properties
		/* Commented out by Vertigo since never used
		internal string OriginId
		{
			get;
			private set;
		}

		public override string Id
		{
			get { return base.Id; }
			set
			{
				bool idModified = string.Compare(Id, value, true) != 0;
				m_modified = m_modified | idModified;
				if (String.IsNullOrEmpty(OriginId) && idModified)
					OriginId = Id;
				base.Id = value;
			}
		}
		*/

		public XdeObjectCollection<XdeEntityColumn> Columns
		{
			get
			{
				if ( m_columns == null )
				{
					lock ( this )
					{
						Init();
					}
				}

				return m_columns;
			}
		}

		public ICollection<XdeConstraint> Constraints
		{
			get
			{
				if ( m_constraints == null )
				{
					lock ( this )
					{
						Init();
					}
				}

				return m_constraints;
			}
		}

		public ICollection<XdeIndex> Indexes
		{
			get
			{
				if ( m_indexes == null )
				{
					lock ( this )
					{
						Init();
					}
				}

				return m_indexes;
			}
		}

		public XdeEntityType EntityType
		{
			get { return m_entityType; }
		}

		public bool IsTable
		{
			get { return m_entityType == XdeEntityType.Table; }
		}

		public bool IsView
		{
			get { return m_entityType == XdeEntityType.View; }
		}

		/* Not used internally but easy to implement externally
		public IEnumerable<XdeForeignKeyConstraint> References
		{
			get
			{
				return this.GetSession().Entities
					.SelectMany(entity => entity.Constraints
						.OfType<XdeForeignKeyConstraint>()
						.Where(fk => fk.ReferencingEntityName.Equals(this.Name, StringComparison.OrdinalIgnoreCase)));
			}
		}
		*/

		public XdeIndex PrimaryKey
		{
			get { return m_primaryKey; }
		}
		#endregion

		#region methods
		public void ResetPrimaryKey()
		{
			m_primaryKey = new XdeIndex(this, "PK_" + this.Name);
		}

		public void RepairPrimaryKey()
		{
			if ( m_primaryKey == null )
				ResetPrimaryKey();

			if ( m_primaryKey.Columns.Count == 0 )
				m_primaryKey.AddColumn(String.Empty);
		}

		public void Refresh()
		{
			lock ( this )
			{
				m_columns = null;
				m_constraints = null;
				m_indexes = null;
				m_deletedColumns = null;

				Init();
			}
		}

		private void Init()
		{
			m_columns = new ColumnCollection(this);
			m_constraints = new XdeObjectCollection<XdeConstraint>(this);
			m_indexes = new XdeObjectCollection<XdeIndex>(this);

			XdeEntityColumn idColumn = new XdeEntityColumn(this, XdeEntityColumnMetadata.GetUniqueidentifierColumnInfo());
			idColumn.Modified = false;

			m_columns.Add(idColumn);

			var registration = this.GetOwnerOf<XdeRegistration>();
			var entities = registration.Entities ?? registration.NewSession().Entities;

			if ( !entities.Contains(this.Name) )
			{
				return;
			}

			IXdeAdapter adapter = registration.Adapter;
			IXdeLayer layer = registration.Layer;

			using ( IDbConnection dbConnection = registration.GetConnection() )
			{
				dbConnection.Open();

				XdeCommand command = layer.GetColumnsSelectCommand(this.Name);

				command.Dump();

				using ( IDbCommand dbCommand = adapter.GetCommand(dbConnection, command) )
				{
					using ( IDataReader rs = dbCommand.ExecuteReader() )
					{
						while ( rs.Read() )
						{
							XdeEntityColumnMetadata columnInfo = layer.GetColumnMetaData(adapter, rs);
							XdeEntityColumn column = new XdeEntityColumn(this, columnInfo);
							column.Modified = false;
							column.IsExists = true;

							m_columns.Add(column);
						}
					}
				}

				command = layer.GetEntityReferencesCommand(this.Name);

				command.Dump();

				using ( IDbCommand dbCommand = adapter.GetCommand(dbConnection, command) )
				{
					using ( IDataReader dataReader = dbCommand.ExecuteReader() )
					{
						while ( dataReader.Read() )
						{
							try
							{
								string name = dataReader.GetString(0); // CONSTRAINT_NAME
								string foreignEntityName = dataReader.GetString(1).Substring(3); // TABLE_NAME
								string foreignColumnName = dataReader.GetString(2); // COLUMN_NAME
								//string entityName = dataReader.GetString(3).Substring(3); // RTABLE_NAME
								string columnName = dataReader.GetString(4); // RCOLUMN_NAME

								if ( foreignColumnName.Equals(layer.GetIdColumnName(), StringComparison.OrdinalIgnoreCase) )
									foreignColumnName = String.Empty;
								else
									foreignColumnName = foreignColumnName.Substring(3);

								if ( columnName.Equals(layer.GetIdColumnName(), StringComparison.OrdinalIgnoreCase) )
									columnName = String.Empty;
								else
									columnName = columnName.Substring(3);

								XdeForeignKeyConstraint foreignKey = new XdeForeignKeyConstraint(this, name, columnName, foreignEntityName, foreignColumnName);

								m_constraints.Add(foreignKey);
							}
							catch
							{
							}
						}
					}
				}

				command = layer.GetEntityIndexesCommand(this.Name);

				command.Dump();

				using ( IDbCommand dbCommand = adapter.GetCommand(dbConnection, command) )
				{
					using ( IDataReader dataReader = dbCommand.ExecuteReader() )
					{
						while ( dataReader.Read() )
						{
							try
							{
								string indexName = dataReader.GetString(0);
								XdeIndex index = new XdeIndex(this, indexName);

								string indexDescription = dataReader.GetString(1);

								if ( indexDescription.IndexOf("primary key", StringComparison.OrdinalIgnoreCase) >= 0 )
								{
									index.IndexType = XdeIndexType.PrimaryKey;
									m_primaryKey = index;
								}
								else if ( indexDescription.IndexOf("unique key", StringComparison.OrdinalIgnoreCase) >= 0 )
								{
									index.IndexType = XdeIndexType.UniqueKey;
								}
								else
								{
									index.IndexType = XdeIndexType.Index;
									index.Unique = indexDescription.IndexOf("unique", StringComparison.OrdinalIgnoreCase) != -1;
								}

								index.Clustered = indexDescription.IndexOf("nonclustered", StringComparison.OrdinalIgnoreCase) == -1;

								string indexKeys = dataReader.GetString(2);

								foreach ( string indexKey in indexKeys.Split(',') )
								{
									string[] ss = indexKey.Split('(');
									string key = ss[0].Trim();
									string indexColumnName;

									if ( key.Equals(layer.GetIdColumnName(), StringComparison.OrdinalIgnoreCase) )
									{
										indexColumnName = String.Empty;
									}
									else
									{
										if ( key.StartsWith("XM_", StringComparison.OrdinalIgnoreCase) )
											indexColumnName = key.Substring(3).ToUpper();
										else
											throw new NotSupportedException();
									}

									XdeIndexColumn indexColumn = index.AddColumn(indexColumnName);

									indexColumn.SortOrder = ss.Length > 1 ? XdeIndexColumnSortOrder.Descending : XdeIndexColumnSortOrder.Ascending;
								}

								m_indexes.Add(index);
							}
							catch
							{
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// Возвращает новый экземпляр объекта данных данной сущности.
		/// </summary>
		/// <param name="id">Идентификатор объекта данных.</param>
		/// <returns>Новый объект данных.</returns>
		public XdeUnit NewUnit(Guid id)
		{
			XdeUnit unit = new XdeUnit(this, id);

			foreach ( XdeEntityColumn column in this.Columns )
			{
				if ( !unit.Properties.Contains(column.Name) )
				{
					unit.Properties.Add(String.IsNullOrEmpty(column.Name) ? new XdeProperty.IdentifierProperty(unit) : new XdeProperty(unit, column.Name, null));
				}
			}

			return unit;
		}

		/// <summary>
		/// Возвращает новый экземпляр объекта данных данной сущности.
		/// </summary>
		/// <returns>Новый объект данных.</returns>
		/// <remarks>
		/// Для созданного объекта данных устанавливается уникальный идентификатор, признак <see cref="XdeUnit.IsExists"/> устанавливается в <c>false</c>.
		/// </remarks>
		public XdeUnit NewUnit()
		{
			XdeUnit unit = NewUnit(Guid.NewGuid());

			unit.IsExists = false;

			return unit;
		}
		#endregion

		#region XdeObject Overrides
		protected override void ProcessDump(StringBuilder buffer, string prefix)
		{
			if ( buffer == null )
			{
				throw new ArgumentNullException("buffer");
			}

			if ( this.EnableDump )
			{
				buffer.Append(Environment.NewLine).Append(prefix).Append("entity : ").Append(this.Name).Append(", ").Append(m_entityType);
				XdeDump.ProcessDumpItems(m_columns, buffer, prefix + "\t", "Columns");
			}
		}

		public override IEnumerable<XdeCommand> GetCommand()
		{
			if ( !this.IsTable )
			{
				throw new NotSupportedException(String.Format("Can't save or drop non-table entity {0}.", this.Name));
			}

			IXdeLayer layer = this.GetXdeLayer();

			if ( m_operation == XdeExecutionOperation.Delete )
			{
				// TODO: Drop Constraints first

				foreach ( var cmd in layer.GetDropEntityCommand(this.Name) )
				{
					yield return cmd;
				}
			}
			else if ( m_operation == XdeExecutionOperation.Save )
			{
				/* Commented out by Vertigo since OriginId is never set to a non-null value
				if ( !String.IsNullOrEmpty(this.OriginId) )
					commands.AddRange(layer.GetRenameEntityCommand(OriginId, Name));
				*/

				foreach ( var cmd in layer.GetCreateEntityCommand(this.Name) )
				{
					yield return cmd;
				}

				if ( m_deletedColumns != null )
				{
					foreach ( var cmd in m_deletedColumns.SelectMany(x => x.GetCommand()) )
					{
						yield return cmd;
					}
				}

				if ( m_columns != null )
				{
					foreach ( var cmd in m_columns.SelectMany(x => x.GetCommand()) )
					{
						yield return cmd;
					}
				}

				if ( this.PrimaryKey.Columns.Count != 0 )
				{
					foreach ( var cmd in layer.GetCreatePrimaryKeyCommand(this.Name, this.PrimaryKey.Columns.Select(x => x.Name)) )
					{
						yield return cmd;
					}
				}

				// TODO: Indexes and Constraints
			}
		}
		#endregion

		#region IXdeDatabaseObject Members
		public bool? IsExists
		{
			get { return m_exists; }
			internal set
			{
				if ( !value.HasValue )
				{
					throw new ArgumentException("Null value isn't supported.");
				}

				m_exists = value.Value;
			}
		}

		public bool Modified
		{
			get { return m_modified; }
			set { m_modified = value; }
		}

		public XdeExecutionOperation Operation
		{
			get { return m_operation; }
			set
			{
				if ( value != XdeExecutionOperation.Save && value != XdeExecutionOperation.Delete )
				{
					throw new ArgumentException("Only Save or Delete operations are supported.", "value");
				}

				m_operation = value;
			}
		}
		#endregion

		#region class ColumnCollection
		private class ColumnCollection : XdeObjectCollection<XdeEntityColumn>
		{
			#region ctor
			internal ColumnCollection(XdeEntity owner)
				: base(owner)
			{
			}
			#endregion

			#region RemoveItem
			protected override void RemoveItem(int index)
			{
				var item = this.Items[index];

				base.RemoveItem(index);

				item.Operation = XdeExecutionOperation.Delete;
				XdeEntity entity = item.Entity;

				if ( entity.m_deletedColumns == null || !entity.m_deletedColumns.Contains(item) )
				{
					if ( entity.m_deletedColumns == null )
					{
						entity.m_deletedColumns = new XdeObjectCollection<XdeEntityColumn>(entity);
					}

					entity.m_deletedColumns.Add(item);
				}
			}
			#endregion
		}
		#endregion
	}

	/// <summary>
	/// Перечисление возможных типов сущности.
	/// </summary>
	/// <seealso cref="XdeEntity.EntityType"/>
	public enum XdeEntityType
	{
		/// <summary>
		/// Тип сущности неизвестен.
		/// </summary>
		Unknown,

		/// <summary>
		/// Сущность является таблицей БД.
		/// </summary>
		Table,

		/// <summary>
		/// Сущность является представлением (view) БД.
		/// </summary>
		View
	}

	#region class XdeEntityMetadata
	/// <summary>
	/// Контейнер метаданных сущности.
	/// </summary>
	public class XdeEntityMetadata
	{
		public string TableName;
		public string TableType;
		public string Name;
		public XdeEntityType Type;
	}
	#endregion

	#region class XdeEntityCollection
	public sealed class XdeEntityCollection : XdeObjectCollection<XdeEntity>
	{
		#region ctor
		internal XdeEntityCollection(XdeRegistration owner)
			: base(owner)
		{
		}
		#endregion
	}
	#endregion
}
