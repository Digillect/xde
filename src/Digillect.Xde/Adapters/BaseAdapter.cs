using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Digillect.Xde.Adapters
{
	public abstract class BaseAdapter : IXdeAdapter
	{
		public abstract string GetConnectionString(string server, string database, string login, string password);
		public abstract IDbConnection GetConnection(string connectionString);

		public virtual IDbCommand GetCommand(IDbConnection connection)
		{
			return connection.CreateCommand();
		}

		public virtual IDbCommand GetCommand(IDbConnection connection, XdeCommand command)
		{
			IDbCommand rc = GetCommand(connection);

			rc.CommandType = CommandType.Text;
			rc.CommandText = PrepareCommand(command.CommandText);

			for ( int idx = 0; idx < command.Parameters.Count; idx++ )
			{
				AddParameter(rc, idx, command.Parameters[idx]);
			}

			return rc;
		}

		public virtual void AddParameter(IDbCommand command, int parameterIndex, object parameterValue)
		{
			if ( parameterValue == null )
			{
				parameterValue = DBNull.Value;
			}

			IDbDataParameter dbParameter = command.CreateParameter();

			dbParameter.ParameterName = "P" + parameterIndex;
			dbParameter.Value = parameterValue;

			command.Parameters.Add(dbParameter);
		}

		/*
		public virtual DbType GetDbType(object value)
		{
			if ( value is string )
				return DbType.String;
			else if ( value is short )
				return DbType.Int16;
			else if ( value is ushort )
				return DbType.UInt16;
			else if ( value is int )
				return DbType.Int32;
			else if ( value is uint )
				return DbType.UInt32;
			else if ( value is long )
				return DbType.Int64;
			else if ( value is ulong )
				return DbType.UInt64;
			else if ( value is bool )
				return DbType.Boolean;
			else if ( value is byte )
				return DbType.Byte;
			else if ( value is Guid )
				return DbType.Guid;
			else if ( value is double )
				return DbType.Double;
			else if ( value is decimal )
				return DbType.Decimal;
			else if ( value is DateTime )
				return DbType.DateTime;
			else if ( value is TimeSpan )
				return DbType.Int64;
			else if ( value is byte[] )
				return DbType.Binary;
			else
				return DbType.Object;
		}
		*/

		public virtual string PrepareCommand(string command)
		{
			return command;
		}

		public virtual Guid GetGuid(IDataReader dataReader, int index)
		{
			return dataReader.IsDBNull(index) ? Guid.Empty : dataReader.GetGuid(index);
		}

		public virtual int GetInt32(IDataReader dataReader, int index)
		{
			return dataReader.GetInt32(index);
		}

		public virtual object GetValue(IDataReader dataReader, int index)
		{
			if ( dataReader.IsDBNull(index) )
			{
				return null;
			}

			return dataReader.GetValue(index);
		}

		public object GetValue(IDataReader dataReader, string columnName)
		{
			return GetValue(dataReader, dataReader.GetOrdinal(columnName));
		}
	}
}
