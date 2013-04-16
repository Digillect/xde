using System;

namespace Digillect.Xde
{
	/// <summary>
	/// Определяет методы иерархии объектов.
	/// </summary>
	public interface IXdeHierarchyObject
	{
		/// <summary>
		/// Возвращает родительский объект для данного объекта.
		/// </summary>
		IXdeHierarchyObject Owner
		{
			get;
		}
	}
}
