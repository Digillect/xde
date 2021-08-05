using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Digillect.Xde
{
	public class XdeIndex : XdeHierarchyObject
	{
		private bool m_unique;
		private readonly IList<XdeIndexColumn> m_columns = new List<XdeIndexColumn>();

		#region ctor
		/// <summary>
		/// ������� ����� ���������� �������.
		/// </summary>
		/// <param name="owner">��������.</param>
		/// <param name="name">������������ �������.</param>
		public XdeIndex(XdeEntity owner, string name)
			: base(owner, name)
		{
		}
		#endregion

		#region properties
		/// <summary>
		/// ���������� ��� ������������� ��� ������� �������.
		/// </summary>
		public XdeIndexType IndexType
		{
			get;
			set;
		}

		/// <summary>
		/// ���������� ��� ������������� ������� ������������ ��� ������� �������.
		/// </summary>
		public bool Unique
		{
			get { return this.IndexType == XdeIndexType.Index ? m_unique : true; }
			set { m_unique = value; }
		}

		/// <summary>
		/// ���������� ��� ������������� ������� ������������� ��� ������� �������.
		/// </summary>
		public bool Clustered
		{
			get;
			set;
		}

		/// <summary>
		/// ���������� ��������� ������� ������� �������.
		/// </summary>
		public IList<XdeIndexColumn> Columns
		{
			get { return m_columns; }
		}
		#endregion

		public XdeIndexColumn AddColumn(string columnName)
		{
			var column = new XdeIndexColumn(columnName);

			m_columns.Add(column);

			return column;
		}

		public override IEnumerable<XdeCommand> GetCommand()
		{
			return Enumerable.Empty<XdeCommand>();
		}
	}

	#region enum XdeIndexType
	/// <summary>
	/// ��� �������.
	/// </summary>
	public enum XdeIndexType
	{
		/// <summary>
		/// ������.
		/// </summary>
		Index,

		/// <summary>
		/// ��������� ����.
		/// </summary>
		PrimaryKey,

		/// <summary>
		/// ���������� ��������.
		/// </summary>
		UniqueKey
	}
	#endregion

	/// <summary>
	/// ���������� ������� �������.
	/// </summary>
	public sealed class XdeIndexColumn
	{
		private readonly string m_name;

		#region ctor
		public XdeIndexColumn(string name)
		{
			m_name = name;
		}
		#endregion

		#region properties
		public string Name
		{
			get { return m_name; }
		}

		public XdeIndexColumnSortOrder SortOrder
		{
			get;
			set;
		}
		#endregion
	}

	public enum XdeIndexColumnSortOrder
	{
		Ascending,
		Descending
	}
}
