using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;

namespace Digillect.Xde
{
	/// <summary>
	/// Объект данных.
	/// </summary>
	[System.Diagnostics.DebuggerDisplay("EnityName = {EntityName} Id = {Id}")]
	public sealed class XdeUnit : XdeHierarchyObject, IXdeDatabaseObject
	{
		private readonly XdeObjectCollection<XdeProperty> m_properties;
		private readonly XdeObjectCollection<XdeUnit> m_joins;
		private XdeExecutionOperation m_operation = XdeExecutionOperation.Save;

		#region ctor
		internal XdeUnit(XdeSession owner, string entityName, Guid id)
			: this(owner)
		{
			this.EntityName = entityName;
			this.Id = id;
		}

		internal XdeUnit(XdeSession owner, IDataReader rs)
			: this(owner)
		{
			int ix = 0;

			Init(rs, ref ix);
		}

		private XdeUnit(IXdeHierarchyObject owner)
			: base(owner, String.Empty)
		{
			m_properties = new XdeObjectCollection<XdeProperty>(this);
			m_joins = new XdeObjectCollection<XdeUnit>(this);
		}
		#endregion

		#region properties
		/// <summary>
		/// Возвращает коллекцию свойств объекта данных.
		/// </summary>
		public XdeObjectCollection<XdeProperty> Properties
		{
			get { return m_properties; }
		}

		/// <summary>
		/// Возвращает коллекцию присоединенных объектов объекта данных.
		/// </summary>
		public XdeObjectCollection<XdeUnit> Joins
		{
			get { return m_joins; }
		}

		/// <summary>
		/// Возвращает наименование сущности объекта данных.
		/// </summary>
		public string EntityName
		{
			get;
			private set;
		}

		/// <summary>
		/// Возвращает сущность объекта данных.
		/// </summary>
		public XdeEntity Entity
		{
			get { return this.GetSession().Entities[this.EntityName]; }
		}

		/// <summary>
		/// Возвращает или устанавливает идентификатор объекта данных.
		/// </summary>
		public Guid Id
		{
			get;
			// TODO: Make private.
			set;
		}

#if false
		/// <summary>
		/// Возвращает признак отсутствия объекта данных в БД.
		/// </summary>
		/// <value>
		/// Для объектов данных, создаваемых методами <see cref="XdeSession.NewUnit(string)"/> и <see cref="XdeSession.NewUnit(string,Guid,bool?)"/> с явным указанием <b>existence</b> равным <b>false</b>,
		/// данное свойство имеет значение, равное <see langword="true"/>;
		/// для объектов данных, возвращаемых из БД,
		/// данное свойство имеет значение равное <see langword="false"/>.
		/// </value>
		/// <remarks>
		/// Для объекта данных со значением данного свойства, равным <see langword="true"/>, при сохранении его в БД выполняется операция <b>INSERT</b>;
		/// для значения <see langword="false"/> - <b>INSERT</b> или <b>UPDATE</b> (в зависимости от значения свойства <see cref="Existence"/>).
		/// </remarks>
		/// <seealso cref="IsExists"/>
		public bool IsNew
		{
			get { return !this.Existence ?? false; }
		}
#endif

		/// <summary>
		/// Возвращает или устанавливает тег для объекта данных.
		/// </summary>
		public int Tag
		{
			get;
			set;
		}

		/// <summary>
		/// Устанавливает значение свойства объекта данных.
		/// </summary>
		/// <param name="key">Наименование свойства.</param>
		[EditorBrowsable(EditorBrowsableState.Never)]
		//[Obsolete("Use direct access to properties collection.")]
		public object this[string key]
		{
			set { m_properties[key].Value = value; }
		}

		/// <summary>
		/// Возвращает или устанавливает наименование базы данных объекта данных.
		/// </summary>
		public string Database
		{
			get;
			set;
		}

		public XdeUnitPrimaryKeyMode PrimaryKeyMode
		{
			get;
			set;
		}
		#endregion

		/// <summary>
		/// Возвращает <see langword="true"/>, если объект данных является экземпляром указанной сущности.
		/// </summary>
		/// <param name="entityName">Наименование сущности.</param>
		/// <returns></returns>
		public bool Is(string entityName)
		{
			return StringComparer.OrdinalIgnoreCase.Equals(this.EntityName, entityName);
		}

		#region data loading
		private void Init(IDataReader rs, ref int fieldIndex)
		{
			int fieldCount = rs.FieldCount;

			IXdeAdapter adapter = this.GetSession().Adapter;

			int objectHeader = adapter.GetInt32(rs, fieldIndex++);

			if ( objectHeader != XdeQueryTags.Header.Object )
			{
				throw new DataException("XdeUnit.Load: invalid object header mark.");
			}

			// загружаем заголовок объекта
			int objectDescriptor = adapter.GetInt32(rs, fieldIndex++);

			// загружаем алиас
			if ( (objectDescriptor & XdeQueryTags.Object.Alias) != 0 )
			{
				if ( rs.IsDBNull(fieldIndex) )
					this.Name = null;
				else
					this.Name = rs.GetString(fieldIndex);

				fieldIndex++;
			}

			// загружаем идентификатор
			if ( (objectDescriptor & XdeQueryTags.Object.Id) != 0 )
			{
				this.Id = adapter.GetGuid(rs, fieldIndex);

				m_properties.Add(new XdeProperty.IdentifierProperty(this, string.Empty));

				fieldIndex++;
			}

			// загружаем наименование сущности
			if ( (objectDescriptor & XdeQueryTags.Object.Entity) != 0 )
			{
				if ( rs.IsDBNull(fieldIndex) )
					this.EntityName = null;
				else
					this.EntityName = rs.GetString(fieldIndex);

				fieldIndex++;
			}

			// загружаем наименование БД
			if ( (objectDescriptor & XdeQueryTags.Object.Database) != 0 )
			{
				if ( rs.IsDBNull(fieldIndex) )
					this.Database = null;
				else
					this.Database = rs.GetString(fieldIndex);

				fieldIndex++;
			}

			// загружаем тег
			if ( (objectDescriptor & XdeQueryTags.Object.Tag) != 0 )
			{
				if ( rs.IsDBNull(fieldIndex) )
					this.Tag = 0;
				else
					this.Tag = rs.GetInt32(fieldIndex);

				fieldIndex++;
			}

			// объект существует в БД - ставим соответствующий флаг
			this.IsExists = true;

			// загружаем свойства объекта
			if ( fieldIndex < fieldCount )
			{
				int unitsHeader;

				try
				{
					unitsHeader = adapter.GetInt32(rs, fieldIndex);
				}
				catch
				{
					// ошибка чтения заголовка коллекции свойств - считаем, что коллекция пуста - на выход
					return;
				}

				if ( unitsHeader == XdeQueryTags.Header.Properties )
				{
					fieldIndex++;

					int propertiesCount = adapter.GetInt32(rs, fieldIndex++);

					for ( int propertyIndex = 0; propertyIndex < propertiesCount; propertyIndex++ )
					{
						object data = rs.GetValue(fieldIndex++);
						string propertyName = null;
						object propertyValue = null;

						if ( data is int ) // если старый формат - первое поле в записи свойства есть int
						{
							int unit_descr = (int) data;

							if ( (unit_descr & XdeQueryTags.Property.Name) != 0 )
							{
								if ( !rs.IsDBNull(fieldIndex) )
								{
									propertyName = rs.GetString(fieldIndex);
								}

								fieldIndex++;
							}

							if ( (unit_descr & XdeQueryTags.Property.Value) != 0 )
							{
								propertyValue = adapter.GetValue(rs, fieldIndex++);
							}
						}
						else // иначе новый формат - строка-наименование/значение
						{
							propertyName = data as string;
							propertyValue = adapter.GetValue(rs, fieldIndex++);
						}

						m_properties.Add(new XdeProperty(this, propertyName, propertyValue));
					}
				}
			}

			// загружаем присоединенные объекты
			if ( fieldIndex < fieldCount )
			{
				int joinHeader;

				try
				{
					// загружаем заголовок коллекции присоединенных объектов
					joinHeader = rs.GetInt32(fieldIndex);
				}
				catch
				{
					// ошибка чтения заголовка коллекции присоединенных объектов - считаем, что коллекция пуста - на выход
					return;
				}

				if ( joinHeader == XdeQueryTags.Header.Joins )
				{
					fieldIndex++;

					// загружаем количество присоединенных объектов
					int joinsCount = adapter.GetInt32(rs, fieldIndex++);

					for ( int i = 0; i < joinsCount; i++ )
					{
						XdeUnit joined = new XdeUnit(this);

						joined.Init(rs, ref fieldIndex);

						m_joins.Add(joined);
					}
				}
			}
		}
		#endregion

		#region command
		/// <summary>
		/// Возвращает команду (набор SQL выражений) для данного объекта и его текущего состояния.
		/// </summary>
		/// <returns>Команда (набор SQL выражений).</returns>
		public override IEnumerable<XdeCommand> GetCommand()
		{
			IXdeLayer layer = this.GetSession().Layer;

			switch ( this.Operation )
			{
				case XdeExecutionOperation.Save:
					return layer.GetUnitSaveCommand(this);
				case XdeExecutionOperation.Delete:
					return layer.GetUnitDeleteCommand(this);
			}

			throw new NotSupportedException("Only Save or Delete operations are supported.");
		}
		#endregion

		#region Obsolete Methods
		/// <summary>
		/// Не рекомендуется, вместо этого вызова надо использовать <see cref="GetCommand"/> с последующим выполнением или добавлением самого объекта данных в транзакцию и ее выполнение.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("Not recommended. Use GetCommand() and execute it or add this object into transaction immediately.")]
		public void Prepare()
		{
			this.GetSession().Execute(GetCommand());
		}

		/// <summary>
		/// Сохраняет данный объект в БД.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("Not recommended. Set the operation explicitly then use GetCommand() and execute it or add this object into transaction immediately.")]
		public void Save()
		{
			XdeSession session = this.GetSession();

			session.Execute(session.Layer.GetUnitSaveCommand(this));
		}

		/// <summary>
		/// Удаляет объект данных из БД.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("Not recommended. Set the operation explicitly then use GetCommand() and execute it or add this object into transaction immediately.")]
		public void Delete()
		{
			if ( !this.IsExists ?? false )
				throw new InvalidOperationException("Unit.Delete: this operation is not applicable to 'new' units.");

			XdeSession session = this.GetSession();

			session.Execute(session.Layer.GetUnitDeleteCommand(this));
		}
		#endregion

		#region dump
		protected override void ProcessDump(StringBuilder buffer, string prefix)
		{
			if ( buffer == null )
			{
				throw new ArgumentNullException("buffer");
			}

			if ( this.EnableDump )
			{
				buffer.Append(Environment.NewLine).Append(prefix).Append("unit").Append(this.Modified ? '*' : ' ').Append(": ").Append(this.EntityName).Append("; operation: ").Append(this.Operation).Append("; exists: ").Append(this.IsExists).Append("; tag: ").Append(this.Tag);
				buffer.Append(Environment.NewLine).Append(prefix).Append("\tid    : ").Append(this.Id != Guid.Empty ? this.Id.ToString("B") : "null");

				if ( m_properties.Count > 0 )
				{
					XdeDump.ProcessDumpItems(m_properties, buffer, prefix + "\t", "Properties");
				}

				if ( m_joins.Count > 0 )
				{
					XdeDump.ProcessDumpItems(m_joins, buffer, prefix + "\t", "Joins");
				}
			}
		}
		#endregion

		#region IXdeDatabaseObject Members
		/// <summary>
		/// Возвращает признак существования объекта данных в БД.
		/// </summary>
		/// <value>
		/// Для объектов данных, создаваемых методом <see cref="XdeSession.NewUnit(string,Guid,bool?)"/> с явным указанием <b>existence</b> равным <b>true</b>,
		/// а также для объектов данных, возвращаемых из БД,
		/// данное свойство имеет значение, равное <see langword="true"/>;
		/// в остальных случаях - <see langword="false"/>.
		/// </value>
		/// <remarks>
		/// Для объекта данных со значением данного свойства, равным <see langword="true"/>, при сохранении его в БД выполняется операция <b>UPDATE</b>;
		/// для значения <see langword="false"/> - <b>INSERT</b>; если существование точно не известно, то осуществляется условное выполнение комбинации <b>INSERT/UPDATE</b>.
		/// </remarks>
		public bool? IsExists
		{
			get;
			set;
		}

		/// <summary>
		/// Возвращает или устанавливает признак модифицированности объекта данных.
		/// </summary>
		public bool Modified
		{
			get { return m_properties.Any(x => x != null && x.Modified); ; }
			set
			{
				foreach ( var item in m_properties.Where(x => x != null) )
				{
					item.Modified = value;
				}
			}
		}

		/// <summary>
		/// Возвращает или устанавливает операцию над объектом данных.
		/// </summary>
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
	}

	public enum XdeUnitPrimaryKeyMode
	{
		/// <summary>
		/// Use primary key from database.
		/// </summary>
		Native,

		/// <summary>
		/// Use <see cref="XdeUnit">unit</see> identifier.
		/// </summary>
		/// <seealso cref="XdeUnit.Id"/>
		Identifier
	}
}
