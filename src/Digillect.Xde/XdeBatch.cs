using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;

namespace Digillect.Xde
{
	public sealed class XdeBatch : Collection<IXdeObject>, ICollection<XdeCommand>
	{
		private readonly XdeSession m_session;
		private int m_commandTimeout = -1;
		private IsolationLevel m_isolationLevel = IsolationLevel.Unspecified;

		#region ctor
		public XdeBatch(XdeSession session)
		{
			if ( session == null )
			{
				throw new ArgumentNullException("session");
			}

			m_session = session;
		}
		#endregion

		#region Properties
		public XdeSession Session
		{
			get { return m_session; }
		}
		#endregion

		#region methods
		public void Add(string commandText, params object[] parameters)
		{
			Add(new XdeCommand(commandText, parameters));
		}

		public void AddRange(IEnumerable<IXdeObject> collection)
		{
			if ( collection != null )
			{
				foreach ( var item in collection )
				{
					Add(item);
				}
			}
		}

		protected override void InsertItem(int index, IXdeObject item)
		{
			if ( item == null )
			{
				throw new ArgumentNullException("item");
			}

			base.InsertItem(index, item);
		}

		protected override void SetItem(int index, IXdeObject item)
		{
			if ( item == null )
			{
				throw new ArgumentNullException("item");
			}

			base.SetItem(index, item);
		}
		#endregion

		#region Execute
		public int CommandTimeout
		{
			get { return m_commandTimeout; }
			set { m_commandTimeout = value; }
		}

		public IsolationLevel IsolationLevel
		{
			get { return m_isolationLevel; }
			set { m_isolationLevel = value; }
		}

		public void Execute()
		{
			m_session.Execute(this.Items.SelectMany(x => x.GetCommand()), m_commandTimeout, m_isolationLevel);
		}

		public void Execute(IDbTransaction outerTransaction)
		{
			m_session.Execute(this.Items.SelectMany(x => x.GetCommand()), m_commandTimeout, outerTransaction);
		}
		#endregion

		#region Dump
		public void Dump()
		{
			try
			{
				StringBuilder buffer = new StringBuilder();
				
				XdeDump.ProcessDumpItems(this.Items, buffer, String.Empty, "Batch");

				if ( buffer.Length != 0 )
				{
					XdeDump.Writer.Write(buffer.ToString());
				}
			}
			catch ( Exception ex )
			{
				XdeDump.Writer.Write("Xde dump error: ");
				XdeDump.Writer.WriteLine(ex.Message);
			}
			finally
			{
				XdeDump.Writer.Flush();
			}
		}
		#endregion

		#region ICollection<XdeCommand> Members
		int ICollection<XdeCommand>.Count
		{
			get { return this.Items.OfType<XdeCommand>().Count(); }
		}

		void ICollection<XdeCommand>.Add(XdeCommand item)
		{
			Add(item);
		}

		bool ICollection<XdeCommand>.Contains(XdeCommand item)
		{
			return Contains(item);
		}

		void ICollection<XdeCommand>.CopyTo(XdeCommand[] array, int arrayIndex)
		{
			foreach ( var item in this.Items.OfType<XdeCommand>() )
			{
				array[arrayIndex++] = item;
			}
		}

		bool ICollection<XdeCommand>.IsReadOnly
		{
			get { return this.Items.IsReadOnly; }
		}

		bool ICollection<XdeCommand>.Remove(XdeCommand item)
		{
			return Remove(item);
		}
		#endregion

		#region IEnumerable<XdeCommand> Members
		IEnumerator<XdeCommand> IEnumerable<XdeCommand>.GetEnumerator()
		{
			return this.Items.OfType<XdeCommand>().GetEnumerator();
		}
		#endregion
	}
}
