using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Digillect.Xde
{
	/// <summary>
	/// Коллекция объектов данных.
	/// </summary>
	public sealed class XdeUnitCollection : Collection<XdeUnit>, IXdeObject
	{
		private readonly XdeRegistration m_registration;

		#region ctor
		internal XdeUnitCollection(XdeSession session)
		{
			m_registration = session.Registration;
		}
		#endregion

		public void Add(XdeUnit item, XdeExecutionOperation operation)
		{
			Add(item);

			item.Operation = operation;
		}

		public void AddRange(IEnumerable<XdeUnit> collection)
		{
			if ( collection != null )
			{
				foreach ( var item in collection )
				{
					Add(item);
				}
			}
		}

		public void AddRange(IEnumerable<XdeUnit> collection, XdeExecutionOperation operation)
		{
			if ( collection != null )
			{
				foreach ( var item in collection )
				{
					Add(item, operation);
				}
			}
		}

		public void ForEach(Action<XdeUnit> action)
		{
			foreach ( var item in this.Items )
			{
				action(item);
			}
		}

		#region Obsolete Methods
		/// <summary>
		/// Не рекомендуется, вместо этого вызова надо использовать <see cref="IXdeObject.GetCommand"/> с последующим выполнением или добавлением самого объекта данных в транзакцию и ее выполнение.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("Not recommended. Use GetCommand() and execute it or add this object into transaction immediately.")]
		public void Prepare()
		{
			m_registration.NewSession().Execute(GetCommand());
		}

		/// <summary>
		/// Выполняет операцию сохранения в БД для всех элементов данной коллекции.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("Not recommended. Set the operation explicitly then use GetCommand() and execute it or add this object into transaction immediately.")]
		public void Save()
		{
			m_registration.NewSession().Execute(this.Items.SelectMany(unit => m_registration.Layer.GetUnitSaveCommand(unit)));
		}

		/// <summary>
		/// Выполняет операцию удаления из БД для всех элементов данной коллекции.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("Not recommended. Set the operation explicitly then use GetCommand() and execute it or add this object into transaction immediately.")]
		public void Delete()
		{
			m_registration.NewSession().Execute(this.Items.SelectMany(unit => m_registration.Layer.GetUnitDeleteCommand(unit)));
		}
		#endregion

		#region IXdeObject Members
		string IXdeObject.Name
		{
			get { return "Unit collection"; }
		}

		bool IXdeObject.EnableDump
		{
			get { return XdeDump.Object; }
		}

		public void Dump()
		{
			if ( XdeDump.Object )
			{
				try
				{
					StringBuilder buffer = new StringBuilder();
					
					XdeDump.ProcessDumpItems(this.Items, buffer, String.Empty, "Unit collection");

					if ( buffer.Length != 0 )
					{
						XdeDump.Writer.Write(buffer.ToString());
					}
				}
				catch ( Exception ex )
				{
					XdeDump.Writer.Write("Xde dump error: ");
					XdeDump.Writer.WriteLine(ex.Message);
				}
				finally
				{
					XdeDump.Writer.Flush();
				}
			}
		}

		void IXdeObject.Dump(StringBuilder buffer, string prefix)
		{
			if ( XdeDump.Object )
			{
				XdeDump.ProcessDumpItems(this.Items, buffer, prefix, "Unit collection");
			}
		}

		public IEnumerable<XdeCommand> GetCommand()
		{
			return this.Items.SelectMany(x => x.GetCommand());
		}
		#endregion

		#region Collection`1 Overrides
		protected override void InsertItem(int index, XdeUnit item)
		{
			if ( item == null )
			{
				throw new ArgumentNullException("item");
			}

			if ( item.Owner != m_registration )
			{
				throw new ArgumentException("Invalid object hierarchy.", "item");
			}

			base.InsertItem(index, item);
		}

		protected override void SetItem(int index, XdeUnit item)
		{
			if ( item == null )
			{
				throw new ArgumentNullException("item");
			}

			if ( item.Owner != m_registration )
			{
				throw new ArgumentException("Invalid object hierarchy.", "item");
			}

			base.SetItem(index, item);
		}
		#endregion
	}
}
