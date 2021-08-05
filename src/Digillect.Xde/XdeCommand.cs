using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Digillect.Xde
{
	/// <summary>
	/// �������.
	/// </summary>
	/// <remarks>
	/// <para>
	/// ������ ����� �������� ����������� SQL-��������� � ���������� � ���.
	/// �������� <see cref="CommandText">����� �������</see> � ��������� ������������ ����������, ���������� �������������� <see cref="IList"/>.
	/// </para>
	/// <para>
	/// ������� �� ��������� � ������������ <see cref="XdeSession">������</see> � ����� ���� ��������� 
	/// ���� ��������������� �������� <b>XdeSession.Execute</b>, ���� � ������� <see cref="XdeBatch">����������</see>.
	/// </para>
	/// </remarks>
	public sealed class XdeCommand : XdeObject
	{
		private readonly string m_commandText;
		private readonly IList m_parameters = new ArrayList();
		private int m_commandTimeout = -1;

		#region ctor
		/// <summary>
		/// ������� ����� ��������� ��������.
		/// </summary>
		/// <param name="commandText">����� �������.</param>
		public XdeCommand(string commandText)
			: base("Command")
		{
			if ( String.IsNullOrWhiteSpace(commandText) )
			{
				throw new ArgumentException("Empty command.", "commandText");
			}

			m_commandText = commandText;
		}

		/// <summary>
		/// ������� ����� ��������� ��������.
		/// </summary>
		/// <param name="commandText">����� �������.</param>
		/// <param name="parameters">��������� ����������.</param>
		public XdeCommand(string commandText, params object[] parameters)
			: this(commandText)
		{
			if ( parameters != null )
			{
				Array.ForEach(parameters, x => m_parameters.Add(x));
			}
		}
		#endregion

		#region properties
		/// <summary>
		/// ���������� ��� ������������� ����� �������� �������.
		/// </summary>
		public string CommandText
		{
			get { return m_commandText; }
		}

		/// <summary>
		/// ���������� ��������� ���������� �������� �������.
		/// </summary>
		public IList Parameters
		{
			get { return m_parameters; }
		}

		public int CommandTimeout
		{
			get { return m_commandTimeout; }
			set { m_commandTimeout = value; }
		}
		#endregion

		public override string ToString()
		{
			StringBuilder buffer = new StringBuilder(m_commandText);

			foreach ( object parameter in m_parameters )
			{
				buffer.Append(Environment.NewLine).Append('\t');
				XdeDump.ProcessDumpValue(buffer, parameter);
			}

			return buffer.ToString();
		}

		#region XdeObject Overrides
		public override bool EnableDump
		{
			get { return XdeDump.Sql; }
		}

		protected override void ProcessDump(StringBuilder buffer, string prefix)
		{
			if ( buffer == null )
			{
				throw new ArgumentNullException("buffer");
			}

			if ( this.EnableDump )
			{
				buffer.Append(Environment.NewLine).Append(m_commandText);

				foreach ( object parameter in m_parameters )
				{
					buffer.Append(Environment.NewLine).Append('\t');
					XdeDump.ProcessDumpValue(buffer, parameter);
				}
			}
		}

		public override IEnumerable<XdeCommand> GetCommand()
		{
			yield return this;
		}
		#endregion
	}
}
