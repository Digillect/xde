using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Digillect.Xde
{
	/// <summary>
	/// Данный класс служит для управления дампом объектов, команд и ошибок.
	/// </summary>
	public static class XdeDump
	{
		private const int StringLengthDumpLimit = 500;
		private const int ArrayLengthDumpLimit = 200;

		private static TextWriter m_writer = Console.Error;

		static XdeDump()
		{
			XdeDump.Error = true;
		}

		/// <summary>
		/// Возвращает или устанавливает <see cref="TextWriter">text writer</see>, куда будет осуществляться дамп.
		/// </summary>
		/// <value>
		/// <b>null</b> -> <see cref="Console.Error"/>.
		/// </value>
		public static TextWriter Writer
		{
			get { return m_writer; }
			set { m_writer = value == null ? Console.Error : value; }
		}

		public static bool All
		{
			get { return Object && Sql && Action; }
			set { Object = Sql = Action = value; }
		}

		public static bool Object
		{
			get;
			set;
		}

		public static bool Sql
		{
			get;
			set;
		}

		public static bool Action
		{
			get;
			set;
		}

		public static bool Error
		{
			get;
			set;
		}

		public static void Write(string format, params object[] parameters)
		{
			m_writer.WriteLine(format, parameters);
			m_writer.Flush();
		}

		internal static void ProcessDumpValue(StringBuilder buffer, object value)
		{
			if ( value == null || Convert.IsDBNull(value) )
			{
				buffer.Append("<NULL>");
			}
			else if ( value is string )
			{
				string stringValue = (string) value;

				if ( stringValue.Length > StringLengthDumpLimit )
				{
					buffer.Append(stringValue, 0, StringLengthDumpLimit).Append(" ...");
				}
				else
				{
					buffer.Append(stringValue);
				}
			}
			else if ( value is byte[] )
			{
				byte[] byteArray = (byte[]) value;

				buffer.Append('[').Append(byteArray.Length.ToString(NumberFormatInfo.InvariantInfo)).Append("] 0x");

				foreach ( var b in byteArray.TakeWhile((x, i) => i < ArrayLengthDumpLimit) )
				{
					buffer.Append(b.ToString("X2", NumberFormatInfo.InvariantInfo));
				}

				if ( byteArray.Length > ArrayLengthDumpLimit )
				{
					buffer.Append(" ...");
				}
			}
			else
			{
				buffer.Append(Convert.ToString(value, CultureInfo.InvariantCulture));
			}
		}

		internal static void ProcessDumpItems<T>(IList<T> items, StringBuilder buffer, string prefix, string dumpName) where T : IXdeObject
		{
			if ( buffer == null )
			{
				throw new ArgumentNullException("buffer");
			}

			buffer.Append(Environment.NewLine).Append(prefix).Append(dumpName).Append(" (").Append(items.Count).Append("): ");

			for ( int i = 0; i < items.Count; i++ )
			{
				buffer.Append(Environment.NewLine).Append(prefix).Append("\tItem ").Append((i + 1).ToString(NumberFormatInfo.InvariantInfo));

				if ( items[i] == null )
				{
					buffer.Append(Environment.NewLine).Append("<NULL>");
				}
				else if ( items[i].EnableDump )
				{
					items[i].Dump(buffer, prefix + "\t");
				}
			}
		}
	}
}
