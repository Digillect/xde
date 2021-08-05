using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Digillect.Xde
{
	/// <summary>
	/// ���������� ������.
	/// </summary>
	/// <remarks>
	/// ������ ���� ������ ��� �������� ���������� � �������� ����������� <see cref="XdeSession">������ Xde</see>.
	/// </remarks>
	public sealed class XdeRegistration : IXdeHierarchyObject
	{
		private static readonly Collection m_registrations = new Collection();

		#region ctor
		/// <summary> ������� ����� ��������� ����������� ������.
		/// </summary>
		/// <param name="name">������������ ����������� ������.</param>
		/// <param name="adapter">������������ <see cref="IXdeAdapter">������ ��������</see>.</param>
		/// <param name="layer">������������ <see cref="IXdeLayer">������ ������</see>.</param>
		/// <param name="connectionString">������ �������� � ��.</param>
		public XdeRegistration(string name, string adapter, string layer, string connectionString)
			: this(name, (IXdeAdapter) Activator.CreateInstance(Type.GetType(adapter, true)), (IXdeLayer) Activator.CreateInstance(Type.GetType(layer, true)), connectionString)
		{
		}

		/// <summary> ������� ����� ��������� ����������� ������.
		/// </summary>
		/// <param name="name">������������ ����������� ������.</param>
		/// <param name="adapter"><see cref="IXdeAdapter">�������</see>.</param>
		/// <param name="layer"><see cref="IXdeLayer">�����</see>.</param>
		/// <param name="connectionString">������ �������� � ��.</param>
		public XdeRegistration(string name, IXdeAdapter adapter, IXdeLayer layer, string connectionString)
		{
			this.Name = name;
			this.ConnectionString = connectionString;
			this.Adapter = adapter;
			this.Layer = layer;
		}
		#endregion

		#region properties
		public string Name
		{
			get;
			private set;
		}

		/// <summary>
		/// ���������� ������ �������� � ��.
		/// </summary>
		public string ConnectionString
		{
			get;
			private set;
		}

		/// <summary>
		/// ���������� <see cref="IXdeAdapter">�������.</see>.
		/// </summary>
		public IXdeAdapter Adapter
		{
			get;
			private set;
		}

		/// <summary>
		/// ���������� <see cref="IXdeLayer">�����.</see>.
		/// </summary>
		public IXdeLayer Layer
		{
			get;
			private set;
		}

		/// <summary>
		/// ���������� ��������� ������������ ������.
		/// </summary>
		public static Collection Registrations
		{
			get { return m_registrations; }
		}

		/// <summary>
		/// ���������� ��������� ��������� ��� ������ �����������.
		/// </summary>
		internal XdeEntityCollection Entities
		{
			get;
			set;
		}
		#endregion

		#region NewSession
		/// <summary>
		/// ������� � ���������� ����� ������ ��� ������ �����������.
		/// </summary>
		/// <returns>����� ������.</returns>
		public XdeSession NewSession()
		{
			return new XdeSession(this);
		}
		#endregion

		#region IXdeHierarchyObject Members
		IXdeHierarchyObject IXdeHierarchyObject.Owner
		{
			get { return null; }
		}
		#endregion

		#region class Collection
		public sealed class Collection : KeyedCollection<string, XdeRegistration>
		{
			internal Collection()
			{
			}

			protected override string GetKeyForItem(XdeRegistration item)
			{
				return item.Name;
			}

			protected override void InsertItem(int index, XdeRegistration item)
			{
				if ( item == null )
				{
					throw new ArgumentNullException("item");
				}

				base.InsertItem(index, item);
			}

			protected override void SetItem(int index, XdeRegistration item)
			{
				if ( item == null )
				{
					throw new ArgumentNullException("item");
				}

				base.SetItem(index, item);
			}
		}
		#endregion
	}
}
