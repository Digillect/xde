using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Digillect.Xde
{
	/// <summary>
	/// Свойство объекта данных.
	/// </summary>
	public class XdeProperty : XdeHierarchyObject, IXdeDatabaseObject, IConvertible
	{
		private object m_value;

		#region ctor
		internal XdeProperty(XdeUnit owner, string name, object value)
			: base(owner, name)
		{
			m_value = value;
		}
		#endregion

		#region properties
		/// <summary>
		/// Возвращает или устанавливает значение свойства объекта данных.
		/// </summary>
		/// <value></value>
		public virtual object Value
		{
			get { return m_value; }
			set
			{
				if ( value != null )
				{
					if ( value is Guid && (Guid) value == Guid.Empty )
						value = null;
					else if ( value is DateTime && (DateTime) value == DateTime.MinValue )
						value = null;
					else if ( value is string && ((string) value).Length == 0 )
						value = null;
					else if ( value is Enum )
						value = Convert.ToInt32(((Enum) value));
					else if ( value is TimeSpan )
						value = ((TimeSpan) value).Ticks;
				}

				if ( !this.Modified )
				{
					if ( m_value != null && value != null )
					{
						if ( m_value is byte[] && value is byte[] )
						{
							byte[] dst = (byte[]) m_value;
							byte[] src = (byte[]) value;

							if ( dst.Length != src.Length )
							{
								this.Modified = true;
							}
							else
							{
								for ( int i = 0; i < dst.Length; i++ )
								{
									if ( dst[i] != src[i] )
									{
										this.Modified = true;
										break;
									}
								}
							}
						}
						else if ( !m_value.Equals(value) )
						{
							this.Modified = true;
						}
					}
					else if ( m_value != null || value != null )
					{
						this.Modified = true;
					}
				}

				m_value = value;
			}
		}

		/// <summary>
		/// Возвращает <b>true</b> если <see cref="Value"/> is <b>null</b>, иначе <b>false</b>.
		/// </summary>
		public virtual bool IsNull
		{
			get { return this.Value == null || Convert.IsDBNull(this.Value); }
		}

		public XdeUnit Unit
		{
			get { return this.GetOwnerOf<XdeUnit>(); }
		}

		public XdeEntityColumn Column
		{
			get { return this.Unit.Entity.Columns[this.Name]; }
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
				buffer.Append(Environment.NewLine).Append(prefix).Append("property").Append(this.Modified ? '*' : ' ').Append(": ").Append(this.Name).Append(' ');

				XdeDump.ProcessDumpValue(buffer, this.Value);
			}
		}

		public override IEnumerable<XdeCommand> GetCommand()
		{
			return this.GetSession().Layer.GetPropertySaveCommand(this);
		}
		#endregion

		#region IXdeDatabaseObject Members
		public bool? IsExists
		{
			get { return true; }
		}

		/// <summary>
		/// Возвращает или устанавливает признак модификации свойства объекта данных.
		/// </summary>
		public virtual bool Modified
		{
			get;
			set;
		}

		public XdeExecutionOperation Operation
		{
			get { return XdeExecutionOperation.Save; }
			set { }
		}
		#endregion

		#region implicits
		/// <summary>
		/// Преобразует значение свойства к типу <see cref="System.Decimal"/>.
		/// <seealso cref="System.Convert.ToDecimal(System.Object)"/>
		/// </summary>
		/// <param name="property">Свойство объекта данных.</param>
		/// <returns><see cref="System.Decimal"/> эквивалентное значению свойства объекта данных, или ноль если значение свойства есть null.</returns>
		public static implicit operator decimal(XdeProperty property)
		{
			return Convert.ToDecimal(property.Value, CultureInfo.InvariantCulture);
		}
		public static implicit operator short(XdeProperty property)
		{
			return Convert.ToInt16(property.Value, CultureInfo.InvariantCulture);
		}
		public static implicit operator int(XdeProperty property)
		{
			return Convert.ToInt32(property.Value, CultureInfo.InvariantCulture);
		}
		public static implicit operator int?(XdeProperty property)
		{
			return property.IsNull ? null : (int?) Convert.ToInt32(property.Value, CultureInfo.InvariantCulture);
		}
		public static implicit operator long(XdeProperty property)
		{
			return Convert.ToInt64(property.Value);
		}
		[CLSCompliant(false)]
		public static implicit operator ushort(XdeProperty property)
		{
			return Convert.ToUInt16(property.Value, CultureInfo.InvariantCulture);
		}
		[CLSCompliant(false)]
		public static implicit operator uint(XdeProperty p)
		{
			return Convert.ToUInt32(p.Value, CultureInfo.InvariantCulture);
		}
		[CLSCompliant(false)]
		public static implicit operator ulong(XdeProperty property)
		{
			return Convert.ToUInt64(property.Value, CultureInfo.InvariantCulture);
		}
		[CLSCompliant(false)]
		public static implicit operator sbyte(XdeProperty property)
		{
			return Convert.ToSByte(property.Value, CultureInfo.InvariantCulture);
		}
		public static implicit operator byte(XdeProperty property)
		{
			return Convert.ToByte(property.Value, CultureInfo.InvariantCulture);
		}
		public static implicit operator bool(XdeProperty p)
		{
			return Convert.ToBoolean(p.Value, CultureInfo.InvariantCulture);
		}
		public static implicit operator char(XdeProperty property)
		{
			return Convert.ToChar(property.Value, CultureInfo.InvariantCulture);
		}
		public static implicit operator string(XdeProperty property)
		{
			// Convert.ToString returns String.Empty if the value is null
			if ( property.IsNull )
			{
				return null;
			}

			return Convert.ToString(property.Value);
		}
		public static implicit operator float(XdeProperty property)
		{
			return Convert.ToSingle(property.Value, CultureInfo.InvariantCulture);
		}
		public static implicit operator double(XdeProperty property)
		{
			return Convert.ToDouble(property.Value, CultureInfo.InvariantCulture);
		}
		/// <summary> Преобразует значение свойства к <see cref="Array">Byte[]</see>.
		/// </summary>
		/// <param name="property">Свойство объекта данных.</param>
		/// <returns><see cref="Byte">Byte[]</see> эквивалентное значению свойства объекта данных, или null если значение свойства есть null.</returns>
		public static implicit operator byte[](XdeProperty property)
		{
			object value = property.Value;

			if ( value == null )
				return null;

			if ( value is byte[] )
				return (byte[]) value;

			if ( value is string )
				return Encoding.UTF8.GetBytes((string) value);

			throw new InvalidCastException("can not implicitly convert property value of type " + value.GetType().Name + " to byte[].");
		}
		/// <summary> Преобразует значение свойства к типу <see cref="DateTime"/>.
		/// </summary>
		/// <param name="property">Свойство объекта данных.</param>
		/// <returns><see cref="System.DateTime"/> эквивалентное значению свойства объекта данных, или <see cref="DateTime.MinValue"/> если значение свойства есть null.</returns>
		public static implicit operator DateTime(XdeProperty property)
		{
			return property.IsNull ? DateTime.MinValue : Convert.ToDateTime(property.Value, CultureInfo.InvariantCulture);
		}
		public static implicit operator DateTime?(XdeProperty property)
		{
			return property.IsNull ? null : (DateTime?) Convert.ToDateTime(property.Value);
		}
		/// <summary> Преобразует значение свойства к типу <see cref="System.TimeSpan"/>.
		/// </summary>
		/// <param name="property">Свойство объекта данных.</param>
		/// <returns><see cref="System.TimeSpan"/> эквивалентное значению свойства объекта данных, или <see cref="TimeSpan.MinValue"/> если значение свойства есть null.</returns>
		public static implicit operator TimeSpan(XdeProperty property)
		{
			return TimeSpan.FromTicks(Convert.ToInt64(property.Value));
		}
		/// <summary> Преобразует значение свойства к типу <see cref="System.Guid"/>.
		/// </summary>
		/// <param name="property">Свойство объекта данных.</param>
		/// <returns><see cref="System.Guid"/> эквивалентное значению свойства объекта данных, или <see cref="Guid.Empty">Guid.Empty</see> если значение свойства есть null.</returns>
		public static implicit operator Guid(XdeProperty property)
		{
			if ( property.IsNull )
				return Guid.Empty;
			else if ( property.Value is Guid )
				return (Guid) property.Value;
			else if ( property.Value is byte[] )
				return new Guid((byte[]) property.Value);
			else
				return new Guid(property.Value.ToString());
		}
		public static implicit operator Guid?(XdeProperty property)
		{
			if ( property.IsNull )
				return null;
			else if ( property.Value is Guid )
				return (Guid) property.Value;
			else if ( property.Value is byte[] )
				return new Guid((byte[]) property.Value);
			else
				return new Guid(property.Value.ToString());
		}
		#endregion

		#region IConvertible Members
		TypeCode IConvertible.GetTypeCode()
		{
			return Convert.GetTypeCode(this.Value);
		}

		bool IConvertible.ToBoolean(IFormatProvider provider)
		{
			return Convert.ToBoolean(this.Value, provider);
		}

		byte IConvertible.ToByte(IFormatProvider provider)
		{
			return Convert.ToByte(this.Value, provider);
		}

		char IConvertible.ToChar(IFormatProvider provider)
		{
			return Convert.ToChar(this.Value, provider);
		}

		DateTime IConvertible.ToDateTime(IFormatProvider provider)
		{
			return Convert.ToDateTime(this.Value, provider);
		}

		decimal IConvertible.ToDecimal(IFormatProvider provider)
		{
			return Convert.ToDecimal(this.Value, provider);
		}

		double IConvertible.ToDouble(IFormatProvider provider)
		{
			return Convert.ToDouble(this.Value, provider);
		}

		short IConvertible.ToInt16(IFormatProvider provider)
		{
			return Convert.ToInt16(this.Value, provider);
		}

		int IConvertible.ToInt32(IFormatProvider provider)
		{
			return Convert.ToInt32(this.Value, provider);
		}

		long IConvertible.ToInt64(IFormatProvider provider)
		{
			return Convert.ToInt64(this.Value, provider);
		}

		sbyte IConvertible.ToSByte(IFormatProvider provider)
		{
			return Convert.ToSByte(this.Value, provider);
		}

		float IConvertible.ToSingle(IFormatProvider provider)
		{
			return Convert.ToSingle(this.Value, provider);
		}

		string IConvertible.ToString(IFormatProvider provider)
		{
			// Convert.ToString returns String.Empty if the value is null
			if ( this.IsNull )
			{
				return null;
			}

			return Convert.ToString(this.Value, provider);
		}

		object IConvertible.ToType(Type conversionType, IFormatProvider provider)
		{
			object value = this.Value;

			if ( value != null && value.GetType() == conversionType )
			{
				return value;
			}

			if ( conversionType == typeof(Guid) || conversionType == typeof(Guid?) )
			{
				if ( value == null )
				{
					return conversionType == typeof(Guid?) ? null : (object) Guid.Empty;
				}

				if ( value is byte[] )
				{
					return new Guid((byte[]) value);
				}

				if ( value is string )
				{
					return new Guid((string) value);
				}
			}
			else if ( conversionType == typeof(byte[]) )
			{
				if ( value == null )
				{
					return null;
				}

				if ( value is Guid )
				{
					return ((Guid) value).ToByteArray();
				}

				if ( value is string )
				{
					return Encoding.UTF8.GetBytes((string) value);
				}
			}

			return Convert.ChangeType(value, conversionType, provider);
		}

		ushort IConvertible.ToUInt16(IFormatProvider provider)
		{
			return Convert.ToUInt16(this.Value, provider);
		}

		uint IConvertible.ToUInt32(IFormatProvider provider)
		{
			return Convert.ToUInt32(this.Value, provider);
		}

		ulong IConvertible.ToUInt64(IFormatProvider provider)
		{
			return Convert.ToUInt64(this.Value, provider);
		}
		#endregion

		#region class IdentifierProperty
		internal sealed class IdentifierProperty : XdeProperty
		{
			internal IdentifierProperty(XdeUnit owner, string name)
				: base(owner, name, Guid.Empty)
			{
			}

			public override object Value
			{
				get
				{
					XdeUnit unit = this.Unit;

					if ( unit == null )
					{
						throw new InvalidOperationException("XdeProperty.IdentifierProperty.getValue : no unit specified.");
					}

					return unit.Id;
				}
				set
				{
					throw new NotSupportedException("Changing the identifier property's value is not allowed.");
				}
			}

			public override bool IsNull
			{
				get { return false; }
			}

			public override bool Modified
			{
				get { return false; }
				set { }
			}
		}
		#endregion
	}
}
