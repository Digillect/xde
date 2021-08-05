using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;

namespace Digillect.Xde
{
	/// <summary>
	/// The Xde object interface.
	/// </summary>
	public interface IXdeObject
	{
		/// <summary>
		/// ���������� ������������ ������� �������.
		/// </summary>
		string Name
		{
			get;
		}

		bool EnableDump
		{
			get;
		}

		void Dump();

		[EditorBrowsable(EditorBrowsableState.Never)]
		void Dump(StringBuilder buffer, string prefix);

		/// <summary>
		/// ���������� ����� SQL ��������� ��� ���������� �������� ������� �������.
		/// </summary>
		IEnumerable<XdeCommand> GetCommand();
	}
}
