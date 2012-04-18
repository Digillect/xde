using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Digillect.Xde
{
	/// <summary>
	/// Перечисление типов операций.
	/// </summary>
	public enum XdeExecutionOperation
	{
		/// <summary>
		/// Нет операции.
		/// </summary>
		None,

		/// <summary>
		/// Безусловная операция сохранения.
		/// </summary>
		Save,

		/// <summary>
		/// Безусловная операция удаления.
		/// </summary>
		Delete,

		/// <summary>
		/// Операция запроса данных.
		/// </summary>
		Select,

		/// <summary>
		/// Операция не определена.
		/// </summary>
		Undefined = -1,
	}
}
