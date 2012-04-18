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
		/// Создает новый дескриптор индекса.
		/// </summary>
		/// <param name="owner">Контекст.</param>
		/// <param name="name">Наименование индекса.</param>
		public XdeIndex(XdeEntity owner, string name)
			: base(owner, name)
		{
		}
		#endregion

		#region properties
		/// <summary>
		/// Возвращает или устанавливает тип данного индекса.
		/// </summary>
		public XdeIndexType IndexType
		{
			get;
			set;
		}

		/// <summary>
		/// Возвращает или устанавливает признак уникальности для данного индекса.
		/// </summary>
		public bool Unique
		{
			get { return this.IndexType == XdeIndexType.Index ? m_unique : true; }
			set { m_unique = value; }
		}

		/// <summary>
		/// Возвращает или устанавливает признак кластеризации для данного индекса.
		/// </summary>
		public bool Clustered
		{
			get;
			set;
		}

		/// <summary>
		/// Возвращает коллекцию колонок данного индекса.
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
	/// Тип индекса.
	/// </summary>
	public enum XdeIndexType
	{
		/// <summary>
		/// Индекс.
		/// </summary>
		Index,

		/// <summary>
		/// Первичный ключ.
		/// </summary>
		PrimaryKey,

		/// <summary>
		/// Уникальное значение.
		/// </summary>
		UniqueKey
	}
	#endregion

	/// <summary>
	/// Дескриптор колонки индекса.
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
