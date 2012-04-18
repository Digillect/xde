using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OracleClient;
using System.Text;

namespace Digillect.Xde.Adapters
{
	// Oracle в Xde дальше поддерживаться не будет. Код пока оставлен как пример работы с оракловыми объектами
	public class OracleAdapter : BaseAdapter
	{
		public override string GetConnectionString(string server, string dbname, string user, string pwd)
		{
			throw new NotImplementedException();
		}

		public override IDbConnection GetConnection(string connection_string)
		{
			return new OracleConnection(connection_string);
		}

		public override IDbCommand GetCommand(IDbConnection connection)
		{
			IDbCommand rc = new OracleCommand();

			rc.Connection = connection;

			return rc;
		}

		public override void AddParameter(IDbCommand command, int parameterIndex, object parameterValue)
		{
			if ( parameterValue == null )
			{
				parameterValue = DBNull.Value;
			}

			if ( parameterValue is TimeSpan )
			{
				TimeSpan v = ((TimeSpan) parameterValue);
				parameterValue = (v == TimeSpan.MinValue ? (object) DBNull.Value : (object) v.Ticks);
			}
			else if ( parameterValue is Guid )
			{
				parameterValue = ((Guid) parameterValue).ToByteArray();
			}

			OracleParameter p = new OracleParameter("P" + parameterIndex, parameterValue);

			command.Parameters.Add(p);
		}

		public override string PrepareCommand(string command)
		{
			return XdeUtil.ReplaceSequental(command, ':');
		}

		public override Guid GetGuid(IDataReader dataReader, int index)
		{
			object rc = GetValue(dataReader, index);

			if ( rc == null )
				return Guid.Empty;

			if ( rc is Guid )
				return (Guid) rc;

			throw new InvalidCastException();
			//return rs.IsDBNull(ix) ? Guid.Empty : rs.GetGuid(ix);
		}

		public override object GetValue(IDataReader dataReader, int index)
		{
			object rc = base.GetValue(dataReader, index);

			if ( rc is byte[] && ((byte[]) rc).Length == 16 )
				return new Guid((byte[]) rc);

			return rc;
		}
	}
}
