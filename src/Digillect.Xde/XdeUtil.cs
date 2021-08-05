using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Digillect.Xde
{
	public static class XdeUtil
	{
		public const string ScriptsResourcePath = "Digillect.Xde.Scripts.";

		/// <summary>
		/// �������� ������� '?' �� ��������������� ������������ ����������.
		/// </summary>
		/// <param name="src">�������� ������.</param>
		/// <param name="prefix">������� ���������.</param>
		/// <returns>��������������� ������.</returns>
		/// <remarks>
		/// ������ ���� "part1 ? part2 ? part3 ?" � ��������� '@' ����� ������������� � ���� "part1 @P0 part2 @P1 part3 @P2".
		/// </remarks>
		public static string ReplaceSequental(string src, char prefix)
		{
			if ( String.IsNullOrEmpty(src) )
			{
				return String.Empty;
			}

			if ( src.IndexOf('?') == -1 )
			{
				return src;
			}

			StringBuilder buff = new StringBuilder(src, src.Length + 100);

			int paramIndex = 0;
			int index = 0;

			while ( index < buff.Length )
			{
				if ( buff[index] == '?' )
				{
					buff[index] = prefix;

					if ( index == buff.Length - 1 )
					{
						buff.Append('P').Append(paramIndex.ToString(NumberFormatInfo.InvariantInfo));

						break;
					}

					string paramName = "P" + paramIndex.ToString(NumberFormatInfo.InvariantInfo);

					buff.Insert(index + 1, paramName);

					paramIndex++;
					index += paramName.Length;
				}

				index++;
			}

			return buff.ToString();
		}

		/// <summary>
		/// ����������� ��������� � ��������� � ������ ���� ��� �������� ����������� ��� ��������.
		/// </summary>
		/// <param name="commandText">����� �������.</param>
		/// <param name="sourceParameters">�������� ��������� ����������.</param>
		/// <returns>��������������� ��������� ����������.</returns>
		internal static IList ExpandCommandTextAndParameters(StringBuilder commandText, IEnumerable sourceParameters)
		{
			IList destinationParameters = new ArrayList();

			int position = 0;

			foreach ( object parameter in sourceParameters )
			{
				while ( position < commandText.Length && commandText[position] != '?' )
				{
					position++;
				}

				if ( parameter is ICollection && !(parameter is byte[]) )
				{
					ICollection values = (ICollection) parameter;

					commandText.Remove(position, 1);

					if ( values.Count == 0 )
					{
						commandText.Insert(position, "NULL");

						position += 4;
					}
					else
					{
						StringBuilder paramBuffer = new StringBuilder('(', values.Count * 3);

						foreach ( object value in values )
						{
							if ( paramBuffer.Length > 1 )
							{
								paramBuffer.Append(", ");
							}

							paramBuffer.Append('?');

							destinationParameters.Add(value);
						}

						paramBuffer.Append(')');

						commandText.Insert(position, paramBuffer.ToString());

						position += paramBuffer.Length;
					}

					continue;
				}

				destinationParameters.Add(parameter);
				position++;
			}

			return destinationParameters;
		}

		#region Extensions
		/// <summary>
		/// ���������� ��������� �� �������� ��������� Xde-������.
		/// </summary>
		/// <param name="obj">������, ��� �������� ������ Xde-������.</param>
		/// <returns>��������� �� �������� ��������� XdeSession ��� <b>null</b> ���� ������� �� ������.</returns>
		public static XdeSession GetSession(this IXdeHierarchyObject obj)
		{
			if ( obj == null )
				return null;
			else if ( obj is XdeSession )
				return (XdeSession) obj;
			else
				return GetSession(obj.Owner);
		}

		/// <summary>
		/// ���������� ��������� �� �������� ������ ��������� ���� ��� <b>null</b> ���� �������� ���.
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="ownerType">��� �������� �������.</param>
		/// <returns>��������� �� �������� ������ ��������� ���� ��� <b>null</b> ���� �������� ���.</returns>
		public static IXdeHierarchyObject GetOwnerOf(this IXdeHierarchyObject obj, Type ownerType)
		{
			if ( obj == null )
			{
				return null;
			}

			if ( ownerType == null )
			{
				throw new ArgumentNullException("ownerType");
			}

			if ( ownerType.IsAssignableFrom(obj.GetType()) )
			{
				return obj;
			}

			return GetOwnerOf(obj.Owner, ownerType);
		}

		/// <summary>
		/// ���������� ��������� �� �������� ������ ��������� ���� ��� <b>null</b> ���� �������� ���.
		/// </summary>
		/// <typeparam name="T">��� �������� �������.</typeparam>
		/// <param name="obj"></param>
		/// <returns>��������� �� �������� ������ ��������� ���� ��� <b>null</b> ���� �������� ���.</returns>
		/// <seealso cref="GetOwnerOf(IXdeHierarchyObject,Type)"/>
		public static T GetOwnerOf<T>(this IXdeHierarchyObject obj) where T : IXdeHierarchyObject
		{
			return (T) GetOwnerOf(obj, typeof(T));
		}
		#endregion
	}
}
