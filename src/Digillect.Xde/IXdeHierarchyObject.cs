using System;
using System.Collections.Generic;
using System.Text;

namespace Digillect.Xde
{
	/// <summary>
	/// Определяет методы иерархии объектов.
	/// </summary>
	public interface IXdeHierarchyObject
	{
		/// <summary>
		/// Возвращает или устанавливает родительский объект для данного объекта.
		/// </summary>
		IXdeHierarchyObject Owner
		{
			get;
			//set;
		}
	}
}
