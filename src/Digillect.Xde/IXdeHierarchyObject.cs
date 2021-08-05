using System;
using System.Collections.Generic;
using System.Text;

namespace Digillect.Xde
{
	/// <summary>
	/// ���������� ������ �������� ��������.
	/// </summary>
	public interface IXdeHierarchyObject
	{
		/// <summary>
		/// ���������� ��� ������������� ������������ ������ ��� ������� �������.
		/// </summary>
		IXdeHierarchyObject Owner
		{
			get;
			//set;
		}
	}
}
