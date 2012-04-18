using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Digillect.Xde
{
	public interface IXdeAdapter
	{
		string GetConnectionString(string server, string database, string login, string password);
		IDbConnection GetConnection(string connectionString);
		IDbCommand GetCommand(IDbConnection connection);
		IDbCommand GetCommand(IDbConnection connection, XdeCommand command);
		void AddParameter(IDbCommand command, int parameterIndex, object parameterValue);
		//DbType GetDbType(object val);
		string PrepareCommand(string command);
		Guid GetGuid(IDataReader dataReader, int index);
		int GetInt32(IDataReader dataReader, int index);
		object GetValue(IDataReader dataReader, int index);
		object GetValue(IDataReader dataReader, string columnName);
	}
}
