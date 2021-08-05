using System;

namespace Digillect.Xde
{
	/// <summary>
	/// ���������� ������ �������� ��������.
	/// </summary>
	public interface IXdeHierarchyObject
	{
		/// <summary>
		/// ���������� ������������ ������ ��� ������� �������.
		/// </summary>
		IXdeHierarchyObject Owner
		{
			get;
		}
	}
}
