using System;
using System.Collections.Generic;
using System.Data;

namespace Digillect.Xde
{
	/// <summary>
	/// Результат выполнения запроса методом <see cref="XdeQuery.Open()"/>.
	/// </summary>
	public sealed class XdeQueryResultSet : IEnumerable<XdeUnit>, IDisposable
	{
		private readonly XdeSession m_session;
		private readonly int m_pageSize;
		private XdeUnit m_unit;
		private int m_recordIndex;

		#region ctor
		internal XdeQueryResultSet(XdeSession session, int pageSize)
		{
			m_session = session;
			m_pageSize = pageSize;
		}

		void IDisposable.Dispose()
		{
			Close();
		}
		#endregion

		#region properties
		public XdeUnit Unit
		{
			get { return m_unit; }
		}

		internal IDbConnection DbConnection
		{
			get;
			set;
		}

		internal IDbCommand DbCommand
		{
			get;
			set;
		}

		internal IDataReader DataReader
		{
			get;
			set;
		}

		internal TimeSpan OpenTime
		{
			get;
			set;
		}

		internal DateTime StartDate
		{
			get;
			set;
		}
		#endregion

		public bool Read()
		{
			if ( (m_pageSize <= XdeQuery.UnlimitedPageSize || m_recordIndex < m_pageSize) && this.DataReader.Read() )
			{
				m_unit = new XdeUnit(m_session, this.DataReader);

				m_recordIndex++;

				return true;
			}
			else
			{
				m_unit = null;

				if ( this.DbCommand != null )
					this.DbCommand.Cancel();

				return false;
			}
		}

		public void Close()
		{
			TimeSpan processTime = DateTime.Now - this.StartDate;

			if ( XdeDump.Action )
				XdeDump.Write("I: query <{0}> closed and processed in {1} ({2} + {3}) ms.", null, Math.Round(processTime.TotalMilliseconds), Math.Round(this.OpenTime.TotalMilliseconds), Math.Round((processTime - this.OpenTime).TotalMilliseconds));

			if ( m_unit != null )
			{
				m_unit = null;
			}

			if ( this.DataReader != null )
			{
				this.DataReader.Dispose();
				this.DataReader = null;
			}

			if ( this.DbCommand != null )
			{
				this.DbCommand.Dispose();
				this.DbCommand = null;
			}

			if ( this.DbConnection != null )
			{
				this.DbConnection.Dispose();
				this.DbConnection = null;
			}

			GC.SuppressFinalize(this);
		}

		#region IEnumerable<XdeUnit> Members
		IEnumerator<XdeUnit> IEnumerable<XdeUnit>.GetEnumerator()
		{
			if ( Read() )
			{
				yield return m_unit;
			}
		}
		#endregion

		#region IEnumerable Members
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<XdeUnit>) this).GetEnumerator();
		}
		#endregion
	}
}
