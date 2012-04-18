using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Digillect.Xde
{
	public class XdeObjectCollection<T> : KeyedCollection<string, T>
		where T : IXdeObject, IXdeHierarchyObject
	{
		protected readonly IXdeHierarchyObject m_owner;

		#region ctor
		public XdeObjectCollection(IXdeHierarchyObject owner)
			: base(StringComparer.OrdinalIgnoreCase)
		{
			if ( owner == null )
			{
				throw new ArgumentNullException("owner");
			}

			m_owner = owner;
		}
		#endregion

#if false
		protected virtual bool ErrorIfItemNotFound
		{
			get { return true; }
		}

		/// <summary>
		/// Возвращает объект по заданному строковому ключу.
		/// </summary>
		/// <param name="key">Строковый ключ, по которому ищется объект.</param>
		/// <remarks>
		/// Если объект не найден, и свойство <see cref="ErrorIfItemNotFound"/> возвращает <see langword="true"/>, то возбуждается исключение; иначе, возвращается <see langword="null"/>.
		/// </remarks>
		public new T this[string key]
		{
			get
			{
				if ( !Contains(key) )
				{
					if ( this.ErrorIfItemNotFound )
					{
						throw new ArgumentException("Item [" + key + "] not found.", "key");
					}
					else
					{
						return default(T);
					}
				}

				return base[key];
			}
		}
#endif

		protected override string GetKeyForItem(T item)
		{
			return item.Name;
		}

		protected override void InsertItem(int index, T item)
		{
			if ( item == null )
			{
				throw new ArgumentNullException("item");
			}

			if ( item.Owner != m_owner )
			{
				throw new ArgumentException("Invalid object hierarchy.", "item");
			}

			base.InsertItem(index, item);
		}

		protected override void SetItem(int index, T item)
		{
			if ( item == null )
			{
				throw new ArgumentNullException("item");
			}

			if ( item.Owner != m_owner )
			{
				throw new ArgumentException("Invalid object hierarchy.", "item");
			}

			base.SetItem(index, item);
		}
	}
}
