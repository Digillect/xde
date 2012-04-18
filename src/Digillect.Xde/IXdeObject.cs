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
		/// Возвращает наименование данного объекта.
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
		/// Возвращает набор SQL выражений для выполнения операций данного объекта.
		/// </summary>
		IEnumerable<XdeCommand> GetCommand();
	}
}
