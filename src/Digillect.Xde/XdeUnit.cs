using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;

namespace Digillect.Xde
{
	/// <summary>
	/// ������ ������.
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
		/// ���������� ��������� ������� ������� ������.
		/// </summary>
		public XdeObjectCollection<XdeProperty> Properties
		{
			get { return m_properties; }
		}

		/// <summary>
		/// ���������� ��������� �������������� �������� ������� ������.
		/// </summary>
		public XdeObjectCollection<XdeUnit> Joins
		{
			get { return m_joins; }
		}

		/// <summary>
		/// ���������� ������������ �������� ������� ������.
		/// </summary>
		public string EntityName
		{
			get;
			private set;
		}

		/// <summary>
		/// ���������� �������� ������� ������.
		/// </summary>
		public XdeEntity Entity
		{
			get { return this.GetSession().Entities[this.EntityName]; }
		}

		/// <summary>
		/// ���������� ��� ������������� ������������� ������� ������.
		/// </summary>
		public Guid Id
		{
			get;
			// TODO: Make private.
			set;
		}

#if false
		/// <summary>
		/// ���������� ������� ���������� ������� ������ � ��.
		/// </summary>
		/// <value>
		/// ��� �������� ������, ����������� �������� <see cref="XdeSession.NewUnit(string)"/> � <see cref="XdeSession.NewUnit(string,Guid,bool?)"/> � ����� ��������� <b>existence</b> ������ <b>false</b>,
		/// ������ �������� ����� ��������, ������ <see langword="true"/>;
		/// ��� �������� ������, ������������ �� ��,
		/// ������ �������� ����� �������� ������ <see langword="false"/>.
		/// </value>
		/// <remarks>
		/// ��� ������� ������ �� ��������� ������� ��������, ������ <see langword="true"/>, ��� ���������� ��� � �� ����������� �������� <b>INSERT</b>;
		/// ��� �������� <see langword="false"/> - <b>INSERT</b> ��� <b>UPDATE</b> (� ����������� �� �������� �������� <see cref="Existence"/>).
		/// </remarks>
		/// <seealso cref="IsExists"/>
		public bool IsNew
		{
			get { return !this.Existence ?? false; }
		}
#endif

		/// <summary>
		/// ���������� ��� ������������� ��� ��� ������� ������.
		/// </summary>
		public int Tag
		{
			get;
			set;
		}

		/// <summary>
		/// ������������� �������� �������� ������� ������.
		/// </summary>
		/// <param name="key">������������ ��������.</param>
		[EditorBrowsable(EditorBrowsableState.Never)]
		//[Obsolete("Use direct access to properties collection.")]
		public object this[string key]
		{
			set { m_properties[key].Value = value; }
		}

		/// <summary>
		/// ���������� ��� ������������� ������������ ���� ������ ������� ������.
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
		/// ���������� <see langword="true"/>, ���� ������ ������ �������� ����������� ��������� ��������.
		/// </summary>
		/// <param name="entityName">������������ ��������.</param>
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

			// ��������� ��������� �������
			int objectDescriptor = adapter.GetInt32(rs, fieldIndex++);

			// ��������� �����
			if ( (objectDescriptor & XdeQueryTags.Object.Alias) != 0 )
			{
				if ( rs.IsDBNull(fieldIndex) )
					this.Name = null;
				else
					this.Name = rs.GetString(fieldIndex);

				fieldIndex++;
			}

			// ��������� �������������
			if ( (objectDescriptor & XdeQueryTags.Object.Id) != 0 )
			{
				this.Id = adapter.GetGuid(rs, fieldIndex);

				m_properties.Add(new XdeProperty.IdentifierProperty(this, string.Empty));

				fieldIndex++;
			}

			// ��������� ������������ ��������
			if ( (objectDescriptor & XdeQueryTags.Object.Entity) != 0 )
			{
				if ( rs.IsDBNull(fieldIndex) )
					this.EntityName = null;
				else
					this.EntityName = rs.GetString(fieldIndex);

				fieldIndex++;
			}

			// ��������� ������������ ��
			if ( (objectDescriptor & XdeQueryTags.Object.Database) != 0 )
			{
				if ( rs.IsDBNull(fieldIndex) )
					this.Database = null;
				else
					this.Database = rs.GetString(fieldIndex);

				fieldIndex++;
			}

			// ��������� ���
			if ( (objectDescriptor & XdeQueryTags.Object.Tag) != 0 )
			{
				if ( rs.IsDBNull(fieldIndex) )
					this.Tag = 0;
				else
					this.Tag = rs.GetInt32(fieldIndex);

				fieldIndex++;
			}

			// ������ ���������� � �� - ������ ��������������� ����
			this.IsExists = true;

			// ��������� �������� �������
			if ( fieldIndex < fieldCount )
			{
				int unitsHeader;

				try
				{
					unitsHeader = adapter.GetInt32(rs, fieldIndex);
				}
				catch
				{
					// ������ ������ ��������� ��������� ������� - �������, ��� ��������� ����� - �� �����
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

						if ( data is int ) // ���� ������ ������ - ������ ���� � ������ �������� ���� int
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
						else // ����� ����� ������ - ������-������������/��������
						{
							propertyName = data as string;
							propertyValue = adapter.GetValue(rs, fieldIndex++);
						}

						m_properties.Add(new XdeProperty(this, propertyName, propertyValue));
					}
				}
			}

			// ��������� �������������� �������
			if ( fieldIndex < fieldCount )
			{
				int joinHeader;

				try
				{
					// ��������� ��������� ��������� �������������� ��������
					joinHeader = rs.GetInt32(fieldIndex);
				}
				catch
				{
					// ������ ������ ��������� ��������� �������������� �������� - �������, ��� ��������� ����� - �� �����
					return;
				}

				if ( joinHeader == XdeQueryTags.Header.Joins )
				{
					fieldIndex++;

					// ��������� ���������� �������������� ��������
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
		/// ���������� ������� (����� SQL ���������) ��� ������� ������� � ��� �������� ���������.
		/// </summary>
		/// <returns>������� (����� SQL ���������).</returns>
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
		/// �� �������������, ������ ����� ������ ���� ������������ <see cref="GetCommand"/> � ����������� ����������� ��� ����������� ������ ������� ������ � ���������� � �� ����������.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("Not recommended. Use GetCommand() and execute it or add this object into transaction immediately.")]
		public void Prepare()
		{
			this.GetSession().Execute(GetCommand());
		}

		/// <summary>
		/// ��������� ������ ������ � ��.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("Not recommended. Set the operation explicitly then use GetCommand() and execute it or add this object into transaction immediately.")]
		public void Save()
		{
			XdeSession session = this.GetSession();

			session.Execute(session.Layer.GetUnitSaveCommand(this));
		}

		/// <summary>
		/// ������� ������ ������ �� ��.
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
		/// ���������� ������� ������������� ������� ������ � ��.
		/// </summary>
		/// <value>
		/// ��� �������� ������, ����������� ������� <see cref="XdeSession.NewUnit(string,Guid,bool?)"/> � ����� ��������� <b>existence</b> ������ <b>true</b>,
		/// � ����� ��� �������� ������, ������������ �� ��,
		/// ������ �������� ����� ��������, ������ <see langword="true"/>;
		/// � ��������� ������� - <see langword="false"/>.
		/// </value>
		/// <remarks>
		/// ��� ������� ������ �� ��������� ������� ��������, ������ <see langword="true"/>, ��� ���������� ��� � �� ����������� �������� <b>UPDATE</b>;
		/// ��� �������� <see langword="false"/> - <b>INSERT</b>; ���� ������������� ����� �� ��������, �� �������������� �������� ���������� ���������� <b>INSERT/UPDATE</b>.
		/// </remarks>
		public bool? IsExists
		{
			get;
			set;
		}

		/// <summary>
		/// ���������� ��� ������������� ������� ������������������ ������� ������.
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
		/// ���������� ��� ������������� �������� ��� �������� ������.
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
