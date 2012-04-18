using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Digillect.Xde
{
	/// <summary>
	/// Контейнер данных для построителя запросов.
	/// </summary>
	public sealed class XdeQueryBuildData
	{
		internal XdeQueryBuildData()
		{
		}

		//public bool Union;
		public bool Subquery;
		public readonly StringBuilder SelectBuffer = new StringBuilder();
		public readonly StringBuilder FromBuffer = new StringBuilder();
		public readonly StringBuilder WhereBuffer = new StringBuilder();
		public readonly StringBuilder GroupByBuffer = new StringBuilder();
		public readonly StringBuilder HavingBuffer = new StringBuilder();
		public readonly StringBuilder OrderByBuffer = new StringBuilder();
		//public readonly StringBuilder UnionBuffer = new StringBuilder();
		private int m_counter;

		/// <summary>
		/// Возвращает нумерованный алиас поля в select list.
		/// </summary>
		/// <param name="enumerateStaticFields">Если <b>true</b> то возвращает нумерованный алиас поля, иначе <see cref="String.Empty"/>.</param>
		/// <returns></returns>
		public string GetFieldAlias(bool enumerateStaticFields)
		{
			if ( !enumerateStaticFields )
			{
				return String.Empty;
			}

			m_counter++;

			return " AS P" + m_counter.ToString(NumberFormatInfo.InvariantInfo);
		}
	}
}
