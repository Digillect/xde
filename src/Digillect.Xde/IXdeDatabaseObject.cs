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
		/// ���������� ������� ������������� ������� �������.
		/// </summary>
		/// <value>
		/// <list type="table">
		/// <item><term><b>null</b></term><description>������������� ������� ����� �� ����������.</description></item>
		/// <item><term><b>false</b></term><description>������ �������������� �� ����������.</description></item>
		/// <item><term><b>true</b></term><description>������ �������������� ����������.</description></item>
		/// </list>
		/// </value>
		/// <remarks>
		/// �� ��� ���������� ������� ���������� �������������� ������������ �������� <b>null</b>.
		/// </remarks>
		bool? IsExists
		{
			get;
		}

		/// <summary>
		/// ���������� ��� ������������� ������� ������������������� ������� �������.
		/// </summary>
		bool Modified
		{
			get;
			set;
		}

		/// <summary>
		/// ���������� ��� ������������� �������� ��� ������� �������.
		/// </summary>
		XdeExecutionOperation Operation
		{
			get;
			set;
		}
	}
}
