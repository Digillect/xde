using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Digillect.Xde
{
	/// <summary>
	/// Коллекция объектов данных.
	/// </summary>
	public sealed class XdeUnitCollection : XdeObjectCollection<XdeUnit>, IXdeObject
	{
		#region ctor
		internal XdeUnitCollection(XdeSession owner)
			: base(owner)
		{
		}
		#endregion

		public void Add(XdeUnit item, XdeExecutionOperation operation)
		{
			item.Operation = operation;

			Add(item);
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

		#region Obsolete Methods
		/// <summary>
		/// Не рекомендуется, вместо этого вызова надо использовать <see cref="IXdeObject.GetCommand"/> с последующим выполнением или добавлением самого объекта данных в транзакцию и ее выполнение.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("Not recommended. Use GetCommand() and execute it or add this object into transaction immediately.")]
		public void Prepare()
		{
			((XdeSession) m_owner).Execute(this.Items.SelectMany(x => x.GetCommand()));
		}

		/// <summary>
		/// Выполняет операцию сохранения в БД для всех элементов данной коллекции.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("Not recommended. Set the operation explicitly then use GetCommand() and execute it or add this object into transaction immediately.")]
		public void Save()
		{
			XdeSession session = ((XdeSession) m_owner);

			session.Execute(this.Items.SelectMany(unit => session.Layer.GetUnitSaveCommand(unit)));
		}

		/// <summary>
		/// Выполняет операцию удаления из БД для всех элементов данной коллекции.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("Not recommended. Set the operation explicitly then use GetCommand() and execute it or add this object into transaction immediately.")]
		public void Delete()
		{
			XdeSession session = ((XdeSession) m_owner);

			session.Execute(this.Items.SelectMany(unit => session.Layer.GetUnitDeleteCommand(unit)));
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
	}
}
