using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace Digillect.Xde.Adapters
{
	public class SqlClientAdapter : BaseAdapter
	{
		public override string GetConnectionString(string server, string database, string login, string password)
		{
			return "Application Name=XDE;Data Source=" + server + ";Initial Catalog=" + database + (String.IsNullOrEmpty(login) ? ";Integrated Security=sspi" : (";User ID=" + login + ";Password=" + password));
		}

		public override IDbConnection GetConnection(string connectionString)
		{
			return new SqlConnection(connectionString);
		}

		public override IDbCommand GetCommand(IDbConnection connection)
		{
			IDbCommand rc = new SqlCommand();

			rc.Connection = connection;

			return rc;
		}

		public override void AddParameter(IDbCommand command, int parameterIndex, object parameterValue)
		{
			if ( parameterValue == null )
			{
				parameterValue = DBNull.Value;
			}
			else if ( parameterValue is TimeSpan )
			{
				TimeSpan timeSpanValue = (TimeSpan) parameterValue;
				parameterValue = (timeSpanValue == TimeSpan.MinValue ? (object) DBNull.Value : (object) timeSpanValue.Ticks);
			}

			SqlParameter sqlParameter = new SqlParameter("@P" + parameterIndex, parameterValue);

			if ( parameterValue is string )
			{
				sqlParameter.Size = ((string) parameterValue).Length;

				if ( sqlParameter.Size > 4000 )
					sqlParameter.SqlDbType = SqlDbType.NText;
			}
			else if ( parameterValue is byte[] )
			{
				sqlParameter.Size = ((byte[]) parameterValue).Length;

				if ( sqlParameter.Size > 8000 )
					sqlParameter.SqlDbType = SqlDbType.Image;
			}

			command.Parameters.Add(sqlParameter);
		}

		public override string PrepareCommand(string command)
		{
			return XdeUtil.ReplaceSequental(command, '@');
		}
	}
}
