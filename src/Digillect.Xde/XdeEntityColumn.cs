using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Digillect.Xde
{
	public class XdeEntityColumn : XdeHierarchyObject, IXdeDatabaseObject
	{
		private string m_dataTypeName;
		private int m_length;
		private int m_precision;
		private int m_scale;
		private bool m_nullable;
		private string m_defaultValue;
		private bool m_exists;
		private XdeExecutionOperation m_operation = XdeExecutionOperation.Save;

		#region ctor
		public XdeEntityColumn(XdeEntity owner, string name, Type undelyingType, int precision, int scale, bool nullable)
			: base(owner, name.Trim().ToUpper())
		{
			this.GetSession().Layer.SetColumnUndelyingType(this, undelyingType, precision, scale);
			this.Nullable = nullable;
		}

		internal XdeEntityColumn(XdeEntity owner, XdeEntityColumnMetadata data)
			: base(owner, data.Name.ToUpper())
		{
			this.DataTypeName = data.DataTypeName;
			this.DataLength = data.Length;
			this.Precision = data.Precision;
			this.Scale = data.Scale;
			this.Nullable = data.Nullable;
			this.DefaultValue = data.DefaultValue;
		}
		#endregion

		#region properties
		/*
		private string m_originId;
		/// <summary>
		/// Возвращает изначальное идентификатор-наименование колонки.
		/// </summary>
		/// <remarks>
		/// Используется при переименовании колонки.
		/// </remarks>
		internal string OriginId
		{
			get;
			private set;
		}

		/// <summary>
		/// Возвращает или устанавливает идентификатор-наименование колонки.
		/// </summary>
		public override string Id
		{
			get { return base.Id; }
			set
			{
				bool idModified = string.Compare(Id, value, true) != 0;
				m_modified = m_modified | idModified;
				if (String.IsNullOrEmpty(OriginId) && idModified)
					OriginId = Id;
				if (ColumnCollection != null)
					ColumnCollection.ChangeId(this, value);
				base.Id = value;
			}
		}
		*/

		/// <summary>
		/// Возвращает сущность к которой принадлежит данная колонка.
		/// </summary>
		public XdeEntity Entity
		{
			get { return this.GetOwnerOf<XdeEntity>(); }
		}

		public string DataTypeName
		{
			get { return m_dataTypeName; }
			set
			{
				if ( m_dataTypeName != value )
				{
					this.Modified = true;
				}

				m_dataTypeName = value;
			}
		}

		public int DataLength
		{
			get { return m_length; }
			set
			{
				if ( m_length != value )
				{
					this.Modified = true;
				}

				m_length = value;
			}
		}

		public int Precision
		{
			get { return m_precision; }
			set
			{
				if ( m_precision != value )
				{
					this.Modified = true;
				}

				m_precision = value;
			}
		}

		public int Scale
		{
			get { return m_scale; }
			set
			{
				if ( m_scale != value )
				{
					this.Modified = true;
				}

				m_scale = value;
			}
		}

		public bool Nullable
		{
			get { return m_nullable; }
			set
			{
				if ( m_nullable != value )
				{
					this.Modified = true;
				}

				m_nullable = value;
			}
		}

		/// <summary>
		/// Возвращает или устанавливает строковое представление значения по умолчанию колонки в таблице или представлении базы данных.
		/// </summary>
		public string DefaultValue
		{
			get { return m_defaultValue; }
			set
			{
				if ( m_defaultValue != value )
				{
					this.Modified = true;
				}

				m_defaultValue = value;
			}
		}

		/*
		/// <summary>
		/// Возвращает или устанавливает тип данных, который асоциируется с данной колонкой.
		/// </summary>
		public Type UnderlyingType
		{
			get { return this.GetSession().Layer.GetColumnUnderlyingType(this); }
			set
			{
				IXdeLayer layer = this.GetSession().Layer;

				if ( value == typeof(String) || value == typeof(Byte[]) )
					layer.SetColumnUndelyingType(this, value, Math.Max(m_precision, 50), 0);
				else if ( value == typeof(Decimal) )
					layer.SetColumnUndelyingType(this, value, Math.Max(m_precision, 18), Math.Max(m_scale, 0));
				else
					layer.SetColumnUndelyingType(this, value, m_precision, m_scale);
			}
		}
		*/
		#endregion

		#region XdeObject Overrides
		protected override void ProcessDump(StringBuilder buffer, string prefix)
		{
			base.ProcessDump(buffer, prefix);

			buffer.Append(" ").Append(this.GetSession().Layer.GetColumnDefinition(this, false));
		}

		public override IEnumerable<XdeCommand> GetCommand()
		{
			IXdeLayer layer = this.GetSession().Layer;

			if ( m_operation == XdeExecutionOperation.Delete )
			{
				return layer.GetDropColumnCommand(this.Entity.Name, this.Name);
			}
			else if ( m_operation == XdeExecutionOperation.Save )
			{
				// need renaming
				/* Commented out by Vertigo since OriginId is never set to a non-null value
				if ( !String.IsNullOrEmpty(this.OriginId) )
				{
					commands.AddRange(layer.GetRenameColumnCommand(this.Entity.Name, this.OriginId, this.Name));
				}
				*/

				if ( this.Modified )
				{
					return layer.GetSaveColumnCommand(this);
				}
			}

			throw new NotSupportedException("Only Save or Delete operations are supported.");
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
			get;
			set;
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
	}

	#region class XdeEntityColumnMetadata
	/// <summary>
	/// Контейнер метаданных колонки сущности.
	/// </summary>
	public class XdeEntityColumnMetadata
	{
		public string Name;
		public string ColumnName;
		public string DataTypeName;
		public int Length;
		public int Precision;
		public int Scale;
		public bool Nullable;
		public string DefaultValue;

		/// <summary>
		/// Возвращает контейнер метаданных для колонки типа uniqueidentifier.
		/// </summary>
		public static XdeEntityColumnMetadata GetUniqueidentifierColumnInfo()
		{
			return new XdeEntityColumnMetadata() {
				Name = String.Empty,
				DataTypeName = "uniqueidentifier",
				Length = 16,
				Precision = 36,
				Nullable = false
			};
		}
	}
	#endregion
}
