using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Digillect.Xde
{
	/// <summary>
	/// Сессия.
	/// </summary>
	public sealed class XdeSession : IXdeHierarchyObject
	{
		private readonly XdeRegistration m_registration;
		private int m_commandTimeout = -1;

		#region ctor
		internal XdeSession(XdeRegistration owner)
		{
			m_registration = owner;
		}
		#endregion

		#region properties
		/// <summary>
		/// Возвращает регистрацию, от которой порождена данная сессия.
		/// </summary>
		public XdeRegistration Registration
		{
			get { return m_registration; }
		}

		/// <summary>
		/// Возвращает адаптер для данной сессии.
		/// </summary>
		public IXdeAdapter Adapter
		{
			get { return m_registration.Adapter; }
		}

		/// <summary>
		/// Возвращает лейер для данной сессии.
		/// </summary>
		public IXdeLayer Layer
		{
			get { return m_registration.Layer; }
		}

		public XdeEntityCollection Entities
		{
			get { return GetEntities(false); }
		}

		public int CommandTimeout
		{
			get { return m_commandTimeout; }
			set { m_commandTimeout = value; }
		}
		#endregion

		#region Events
		public event EventHandler<XdeSessionExecuteEventArgs> ExecuteStarted;

		public event EventHandler<XdeSessionExecuteEventArgs> ExecuteProgress;

		public event EventHandler ExecuteFinished;
		#endregion

		#region methods
		public XdeUnitCollection NewUnits()
		{
			return new XdeUnitCollection(this);
		}

		public XdeBatch NewBatch()
		{
			return new XdeBatch(this);
		}

		public XdeQuery NewQuery()
		{
			return NewQuery(String.Empty);
		}

		public XdeQuery NewQuery(string entityName)
		{
			return NewQuery(entityName, null);
		}

		public XdeQuery NewQuery(string entityName, string whereClause, params object[] values)
		{
			return new XdeQuery(this, String.Empty, entityName, whereClause, values);
		}

		public XdeUnit NewUnit(string entityName)
		{
			return NewUnit(entityName, Guid.NewGuid(), false);
		}

		public XdeUnit NewUnit(string entityName, Guid id)
		{
			return NewUnit(entityName, id, null);
		}

		public XdeUnit NewUnit(string entityName, Guid id, bool? exists)
		{
			XdeEntityCollection entites = GetEntities(false);

			if ( !entites.Contains(entityName) )
			{
				throw new Exception(String.Format("Entity {0} not found.", entityName));
			}

			XdeUnit unit = entites[entityName].NewUnit(id);

			unit.IsExists = exists;

			return unit;
		}

		public XdeEntityCollection GetEntities(bool refresh)
		{
			XdeRegistration registration = this.Registration;
			XdeEntityCollection entities = registration.Entities;

			if ( refresh || entities == null )
			{
				lock ( this.Registration )
				{
					entities = new XdeEntityCollection(this);

					IXdeAdapter adapter = this.Adapter;
					IXdeLayer layer = this.Layer;

					using ( IDbConnection dbConnection = adapter.GetConnection(registration.ConnectionString) )
					{
						dbConnection.Open();

						XdeCommand command = layer.GetEntitiesSelectCommand();

						command.Dump();

						using ( IDbCommand dbCommand = adapter.GetCommand(dbConnection, command) )
						{
							using ( IDataReader dataReader = dbCommand.ExecuteReader() )
							{
								while ( dataReader.Read() )
								{
									XdeEntityMetadata entityInfo = layer.GetEntityMetaData(adapter, dataReader);
									XdeEntity entity = new XdeEntity(this, entityInfo.Name, entityInfo.Type, false);
									entity.IsExists = true;

									entities.Add(entity);
								}
							}
						}
					}

					registration.Entities = entities;
				}
			}

			return entities;
		}
		#endregion

		#region Execute
		public void Execute(IEnumerable<XdeCommand> commands)
		{
			Execute(commands, -1, IsolationLevel.Unspecified);
		}

		public void Execute(IEnumerable<XdeCommand> commands, IDbTransaction outerTransaction)
		{
			Execute(commands, -1, outerTransaction);
		}

		public void Execute(IEnumerable<XdeCommand> commands, int defaultCommandTimeout, IsolationLevel isolationLevel)
		{
			ICollection<XdeCommand> commandCollection = commands is ICollection<XdeCommand> ? (ICollection<XdeCommand>) commands : new List<XdeCommand>(commands);
			DateTime startDate = DateTime.Now;

			try
			{
				if ( ExecuteStarted != null )
				{
					ExecuteStarted(this, new XdeSessionExecuteEventArgs(commandCollection.Count));
				}

				using ( IDbConnection connection = this.Adapter.GetConnection(this.Registration.ConnectionString) )
				{
					connection.Open();

					using ( IDbTransaction transaction = connection.BeginTransaction(isolationLevel) )
					{
						ProcessExecute(commandCollection, transaction, defaultCommandTimeout);

						transaction.Commit();
					}
				}

				TimeSpan processTime = DateTime.Now - startDate;

				if ( XdeDump.Action )
					XdeDump.Write("I: transaction <{0}> started at {1} and successfully processed in {2}.", null, startDate, processTime);
			}
			catch
			{
				if ( XdeDump.Action )
					XdeDump.Write("E: transaction <{0}> started at {1} and failed.", null, startDate);

				throw;
			}
			finally
			{
				if ( ExecuteFinished != null )
				{
					ExecuteFinished(this, EventArgs.Empty);
				}
			}
		}

		public void Execute(IEnumerable<XdeCommand> commands, int defaultCommandTimeout, IDbTransaction outerTransaction)
		{
			ICollection<XdeCommand> commandCollection = commands is ICollection<XdeCommand> ? (ICollection<XdeCommand>) commands : new List<XdeCommand>(commands);
			DateTime startDate = DateTime.Now;

			try
			{
				if ( ExecuteStarted != null )
				{
					ExecuteStarted(this, new XdeSessionExecuteEventArgs(commandCollection.Count));
				}

				ProcessExecute(commandCollection, outerTransaction, defaultCommandTimeout);

				TimeSpan processTime = DateTime.Now - startDate;

				if ( XdeDump.Action )
					XdeDump.Write("I: transaction <{0}> started at {1} and successfully processed in {2}.", null, startDate, processTime);
			}
			catch
			{
				if ( XdeDump.Action )
					XdeDump.Write("E: transaction <{0}> started at {1} and failed.", null, startDate);

				throw;
			}
			finally
			{
				if ( ExecuteFinished != null )
				{
					ExecuteFinished(this, EventArgs.Empty);
				}
			}
		}

		public void Execute(XdeCommand command)
		{
			DateTime startDate = DateTime.Now;

			try
			{
				if ( ExecuteStarted != null )
				{
					ExecuteStarted(this, new XdeSessionExecuteEventArgs(1));
				}

				using ( IDbConnection connection = this.Adapter.GetConnection(this.Registration.ConnectionString) )
				{
					connection.Open();

					using ( IDbTransaction transaction = connection.BeginTransaction() )
					{
						ProcessExecute(command, transaction);

						transaction.Commit();
					}
				}

				TimeSpan processTime = DateTime.Now - startDate;

				if ( XdeDump.Action )
					XdeDump.Write("I: transaction <{0}> started at {1} and successfully processed in {2}.", null, startDate, processTime);
			}
			catch
			{
				if ( XdeDump.Action )
					XdeDump.Write("E: transaction <{0}> started at {1} and failed.", null, startDate);

				throw;
			}
			finally
			{
				if ( ExecuteFinished != null )
				{
					ExecuteFinished(this, EventArgs.Empty);
				}
			}
		}

		public void Execute(XdeCommand command, IDbTransaction outerTransaction)
		{
			DateTime startDate = DateTime.Now;

			try
			{
				if ( ExecuteStarted != null )
				{
					ExecuteStarted(this, new XdeSessionExecuteEventArgs(1));
				}

				ProcessExecute(command, outerTransaction);

				TimeSpan processTime = DateTime.Now - startDate;

				if ( XdeDump.Action )
					XdeDump.Write("I: transaction <{0}> started at {1} and successfully processed in {2}.", null, startDate, processTime);
			}
			catch
			{
				if ( XdeDump.Action )
					XdeDump.Write("E: transaction <{0}> started at {1} and failed.", null, startDate);

				throw;
			}
			finally
			{
				if ( ExecuteFinished != null )
				{
					ExecuteFinished(this, EventArgs.Empty);
				}
			}
		}

		private void ProcessExecute(ICollection<XdeCommand> commands, IDbTransaction transaction, int defaultCommandTimeout = -1)
		{
			int index = 0;

			foreach ( XdeCommand command in commands )
			{
				ProcessExecute(command, transaction, defaultCommandTimeout, index);

				if ( ExecuteProgress != null )
				{
					try
					{
						ExecuteProgress(this, new XdeSessionExecuteEventArgs(index + 1, commands.Count));
					}
					catch
					{
					}
				}

				index++;
			}
		}

		private void ProcessExecute(XdeCommand command, IDbTransaction transaction, int defaultCommandTimeout = -1, int commandIndex = 0)
		{
			try
			{
				command.Dump();

				using ( IDbCommand dbCommand = this.Adapter.GetCommand(transaction.Connection) )
				{
					dbCommand.Transaction = transaction;

					if ( command.CommandTimeout >= 0 )
						dbCommand.CommandTimeout = command.CommandTimeout;
					else if ( defaultCommandTimeout >= 0 )
						dbCommand.CommandTimeout = defaultCommandTimeout;
					else if ( m_commandTimeout >= 0 )
						dbCommand.CommandTimeout = m_commandTimeout;

					StringBuilder commandString = new StringBuilder(command.CommandText);
					IList parameters = XdeUtil.ExpandCommandTextAndParameters(commandString, command.Parameters);

					dbCommand.CommandText = this.Adapter.PrepareCommand(commandString.ToString());

					int parameterIndex = 0;

					foreach ( object parameter in parameters )
					{
						this.Adapter.AddParameter(dbCommand, parameterIndex++, parameter);
					}

					dbCommand.ExecuteNonQuery();
				}
			}
			catch
			{
				if ( XdeDump.Error )
				{
					XdeDump.Writer.WriteLine(commandIndex);
					XdeDump.Writer.WriteLine(command.ToString());
					XdeDump.Writer.Flush();
				}

				throw;
			}
		}
		#endregion

		#region IXdeHierarchyObject Members
		IXdeHierarchyObject IXdeHierarchyObject.Owner
		{
			get { return m_registration; }
		}
		#endregion
	}

	#region class XdeSessionExecuteEventArgs
	public sealed class XdeSessionExecuteEventArgs : EventArgs
	{
		private readonly int m_current;
		private readonly int m_total;

		public XdeSessionExecuteEventArgs(int total)
			: this(0, total)
		{
		}

		public XdeSessionExecuteEventArgs(int current, int total)
		{
			if ( current < 0 )
			{
				throw new ArgumentOutOfRangeException("current", "Must be non-negative.");
			}

			if ( total < 0 )
			{
				throw new ArgumentOutOfRangeException("total", "Must be non-negative.");
			}

			m_current = current;
			m_total = total;
		}

		public int Current
		{
			get { return m_current; }
		}

		public int Total
		{
			get { return m_total; }
		}
	}
	#endregion
}
