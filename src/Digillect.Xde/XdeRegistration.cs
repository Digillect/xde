using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Digillect.Xde
{
	/// <summary>
	/// Дескриптор сессий.
	/// </summary>
	/// <remarks>
	/// Данный клас служит для хранения параметров и создания экземпляров <see cref="XdeSession">сессии Xde</see>.
	/// </remarks>
	public sealed class XdeRegistration : IXdeHierarchyObject
	{
		private static readonly Collection m_registrations = new Collection();

		#region ctor
		/// <summary> Создает новый экземпляр дескриптора сессий.
		/// </summary>
		/// <param name="name">Наименование дескриптора сессий.</param>
		/// <param name="adapter">Наименование <see cref="IXdeAdapter">класса адаптера</see>.</param>
		/// <param name="layer">Наименование <see cref="IXdeLayer">класса лейера</see>.</param>
		/// <param name="connectionString">Строка коннекта к БД.</param>
		public XdeRegistration(string name, string adapter, string layer, string connectionString)
			: this(name, (IXdeAdapter) Activator.CreateInstance(Type.GetType(adapter, true)), (IXdeLayer) Activator.CreateInstance(Type.GetType(layer, true)), connectionString)
		{
		}

		/// <summary> Создает новый экземпляр дескриптора сессий.
		/// </summary>
		/// <param name="name">Наименование дескриптора сессий.</param>
		/// <param name="adapter"><see cref="IXdeAdapter">Адаптер</see>.</param>
		/// <param name="layer"><see cref="IXdeLayer">Лейер</see>.</param>
		/// <param name="connectionString">Строка коннекта к БД.</param>
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
		/// Возвращает строку коннекта к БД.
		/// </summary>
		public string ConnectionString
		{
			get;
			private set;
		}

		/// <summary>
		/// Возвращает <see cref="IXdeAdapter">адаптер.</see>.
		/// </summary>
		public IXdeAdapter Adapter
		{
			get;
			private set;
		}

		/// <summary>
		/// Возвращает <see cref="IXdeLayer">лейер.</see>.
		/// </summary>
		public IXdeLayer Layer
		{
			get;
			private set;
		}

		/// <summary>
		/// Возвращает коллекцию дескрипторов сессий.
		/// </summary>
		public static Collection Registrations
		{
			get { return m_registrations; }
		}

		/// <summary>
		/// Возвращает коллекцию сущностей для данной регистрации.
		/// </summary>
		internal XdeEntityCollection Entities
		{
			get;
			set;
		}
		#endregion

		#region NewSession
		/// <summary>
		/// Создает и возвращает новую сессию для данной регистрации.
		/// </summary>
		/// <returns>Новая сессия.</returns>
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
