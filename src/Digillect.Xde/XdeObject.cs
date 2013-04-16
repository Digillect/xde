using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Digillect.Xde
{
	[System.Diagnostics.DebuggerDisplay("Name = {Name}")]
	public abstract class XdeObject : IXdeObject
	{
		private string m_name;

		#region Constructor
		protected XdeObject(string name)
		{
			m_name = name;
		}
		#endregion

		#region IXdeObject Members
		/// <summary>
		/// Возвращает или устанавливает идентификатор-наименование объекта.
		/// </summary>
		public virtual string Name
		{
			get { return m_name; }
			protected set { m_name = value; }
		}

		public virtual bool EnableDump
		{
			get { return XdeDump.Object; }
		}

		/// <summary>
		/// Выводит данные об объекте.
		/// </summary>
		public void Dump()
		{
			if ( this.EnableDump )
			{
				try
				{
					StringBuilder buffer = new StringBuilder();
					
					ProcessDump(buffer, String.Empty);

					if ( buffer.Length != 0 )
					{
						XdeDump.Writer.Write(buffer.ToString());
					}
				}
				catch ( Exception err )
				{
					XdeDump.Writer.Write("Xde dump error: ");
					XdeDump.Writer.WriteLine(err.Message);
				}
				finally
				{
					XdeDump.Writer.Flush();
				}
			}
		}

		void IXdeObject.Dump(StringBuilder buffer, string prefix)
		{
			ProcessDump(buffer, prefix);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void ProcessDump(StringBuilder buffer, string prefix)
		{
			if ( buffer == null )
			{
				throw new ArgumentNullException("buffer");
			}

			if ( this.EnableDump )
			{
				buffer.Append(Environment.NewLine).Append(prefix).Append(this.Name);
			}
		}

		/// <summary>
		/// Возвращает необходимые выражения и параметры в виде списка команд.
		/// </summary>
		public abstract IEnumerable<XdeCommand> GetCommand();
		#endregion
	}
}
