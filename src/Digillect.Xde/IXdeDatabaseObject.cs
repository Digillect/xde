using System;
using System.Collections.Generic;
using System.Text;

namespace Digillect.Xde
{
	/// <summary>
	/// The database oriented object interface.
	/// </summary>
	public interface IXdeDatabaseObject : IXdeObject
	{
		/// <summary>
		/// Возвращает признак существования данного объекта.
		/// </summary>
		/// <value>
		/// <list type="table">
		/// <item><term><b>null</b></term><description>Существование объекта точно не определено.</description></item>
		/// <item><term><b>false</b></term><description>Объект гарантированно не существует.</description></item>
		/// <item><term><b>true</b></term><description>Объект гарантированно существует.</description></item>
		/// </list>
		/// </value>
		/// <remarks>
		/// Не все реализации данного интерфейса гарантированно поддерживают значение <b>null</b>.
		/// </remarks>
		bool? IsExists
		{
			get;
		}

		/// <summary>
		/// Возвращает или устанавливает признак модифицированностии данного объекта.
		/// </summary>
		bool Modified
		{
			get;
			set;
		}

		/// <summary>
		/// Возвращает или устанавливает операцию для данного объекта.
		/// </summary>
		XdeExecutionOperation Operation
		{
			get;
			set;
		}
	}
}
