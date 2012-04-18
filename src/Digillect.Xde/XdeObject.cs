using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Digillect.Xde
{
	public abstract class XdeObject : IXdeObject
	{
		#region Constructor
		protected XdeObject(string name)
		{
			this.Name = name;
		}
		#endregion

		#region IXdeObject Members
		/// <summary>
		/// Возвращает или устанавливает идентификатор-наименование объекта.
		/// </summary>
		public virtual string Name
		{
			get;
			protected set;
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

	public abstract class XdeHierarchyObject : XdeObject, IXdeHierarchyObject
	{
		private readonly IXdeHierarchyObject m_owner;

		#region Constructor
		protected XdeHierarchyObject(IXdeHierarchyObject owner, string name)
			: base(name)
		{
			if ( owner == null )
			{
				throw new ArgumentNullException("owner");
			}

			m_owner = owner;
		}
		#endregion

		#region IXdeHierarchyObject Members
		public IXdeHierarchyObject Owner
		{
			get { return m_owner; }
		}
		#endregion
	}
}
