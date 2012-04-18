using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

using MySql.Data.MySqlClient;

namespace Digillect.Xde.Adapters
{
	public class MySqlAdapter : BaseAdapter
	{
		public override string GetConnectionString(string server, string database, string login, string password)
		{
			throw new NotImplementedException();
		}

		public override IDbConnection GetConnection(string connectionString)
		{
			return new MySqlConnection(connectionString);
		}

		public override void AddParameter(IDbCommand command, int parameterIndex, object parameterValue)
		{
			base.AddParameter(command, parameterIndex, parameterValue);
		}

		public override Guid GetGuid(IDataReader dataReader, int index)
		{
			return base.GetGuid(dataReader, index);
		}
	}
}
