using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Text;

namespace Digillect.Xde.Adapters
{
	public class OleDbAdapter : BaseAdapter
	{
		public override string GetConnectionString(string server, string database, string login, string password)
		{
			return "Provider=SQLOLEDB;Application Name=XDE;Data Source=" + server + ";Initial Catalog=" + database + (String.IsNullOrEmpty(login) ? ";Integrated Security=SSPI" : (";User ID=" + login + ";Password=" + password));
		}

		public override IDbConnection GetConnection(string connectionString)
		{
			return new OleDbConnection(connectionString);
		}

		public override IDbCommand GetCommand(IDbConnection connection)
		{
			IDbCommand rc = new OleDbCommand();

			rc.Connection = connection;

			return rc;
		}

		public override void AddParameter(IDbCommand cmd, int par_ix, object par)
		{
			if ( par is TimeSpan )
			{
				TimeSpan v = ((TimeSpan) par);
				par = (v == TimeSpan.MinValue ? (object) DBNull.Value : (object) v.Ticks);
			}
			OleDbParameter p = new OleDbParameter();
			p.ParameterName = par_ix.ToString();
			p.Value = par;
			//p.DbType= GetDbType(par);
			cmd.Parameters.Add(p);
		}
	}
}
