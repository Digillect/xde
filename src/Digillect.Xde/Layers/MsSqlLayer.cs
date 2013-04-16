using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Digillect.Xde.Layers
{
	public class MsSqlLayer : Sql92Layer
	{
		#region core
		protected override string TableSchema
		{
			get { return "dbo"; }
		}

		public override void CreateDatabase(IXdeAdapter adapter, string serverName, string databaseName, string userName, string password)
		{
			string connectionString = adapter.GetConnectionString(serverName, "master", userName, password);

			using ( IDbConnection cnt = adapter.GetConnection(connectionString) )
			{
				cnt.Open();

				using ( IDbCommand cmd = adapter.GetCommand(cnt) )
				{
					cmd.CommandType = CommandType.Text;
					cmd.CommandText = "CREATE DATABASE [" + databaseName + "]";

					cmd.ExecuteNonQuery();
				}
			}
		}

		public override string GetQualifiedTableName(string catalogName, string entityName)
		{
			string value = String.Intern("[" + this.TableSchema + "].[" + GetTableName(entityName) + "]");

			if ( String.IsNullOrWhiteSpace(catalogName) )
			{
				return value;
			}

			return String.Intern("[" + catalogName + "]." + value);
		}

		public override string GetIsTableExistsExpression(string entityName)
		{
			return "IF " + GetEntityExistsSelectString(entityName) + Environment.NewLine;
		}

		protected void AppendUnitIfNotExistsExpression(StringBuilder buffer, XdeUnit unit)
		{
			if ( buffer == null )
			{
				throw new ArgumentNullException("buffer");
			}

			buffer.Append(Environment.NewLine).Append("IF NOT EXISTS(SELECT * FROM ").Append(GetQualifiedTableName(unit.Database, unit.EntityName)).Append(" WHERE ");

			AppendPrimaryKeyWhereClause(buffer, unit, false);

			buffer.Append(')');
		}

		protected void AddUnitIfNotExistsParameters(IList parameters, XdeUnit unit)
		{
			AddPrimaryKeyWhereParameters(parameters, unit);
		}

		protected void AppendElseExpression(StringBuilder buffer, XdeUnit unit)
		{
			if ( buffer == null )
			{
				throw new ArgumentNullException("buffer");
			}

			buffer.Append(Environment.NewLine).Append("ELSE");
		}
		#endregion

		#region ddl
		#region old
		public override string GetNumberColumnDefinition(bool nullable, int precision, int scale, string defval, IEnumerable<string> constraints)
		{
			return String.Format(CultureInfo.InvariantCulture, "numeric({0}, {1})", precision, scale) + GetColumnDefinition(nullable, defval, constraints);
		}

		public override string GetStringColumnDefinition(bool nullable, int length, string defval, IEnumerable<string> constraints)
		{
			return (0 < length && length <= 4000 ? String.Format(CultureInfo.InvariantCulture, "nvarchar({0})", length) : "ntext") + GetColumnDefinition(nullable, defval, constraints);
		}

		public override string GetBlobColumnDefinition(bool nullable, int length, string defval, IEnumerable<string> constraints)
		{
			return (0 < length && length <= 8000 ? String.Format(CultureInfo.InvariantCulture, "varbinary({0})", length) : "image") + GetColumnDefinition(nullable, defval, constraints);
		}

		public override string GetCurrencyColumnDefinition(bool nullable, string defval, IEnumerable<string> constraints)
		{
			return "money" + GetColumnDefinition(nullable, defval, constraints);
		}

		public override string GetIntegerColumnDefinition(bool nullable, string defval, IEnumerable<string> constraints)
		{
			return "integer" + GetColumnDefinition(nullable, defval, constraints);
		}

		public override string GetLongColumnDefinition(bool nullable, string defval, IEnumerable<string> constraints)
		{
			return "bigint" + GetColumnDefinition(nullable, defval, constraints);
		}

		public override string GetDoubleColumnDefinition(bool nullable, string defval, IEnumerable<string> constraints)
		{
			return "float" + GetColumnDefinition(nullable, defval, constraints);
		}

		public override string GetBoolColumnDefinition(bool nullable, string defval, IEnumerable<string> constraints)
		{
			return "bit" + GetColumnDefinition(nullable, defval, constraints);
		}

		public override string GetDateTimeColumnDefinition(bool nullable, string defval, IEnumerable<string> constraints)
		{
			return "datetime" + GetColumnDefinition(nullable, defval, constraints);
		}

		public override string GetGuidColumnDefinition(bool nullable, string defval, IEnumerable<string> constraints)
		{
			return "uniqueidentifier" + GetColumnDefinition(nullable, defval, constraints);
		}
		#endregion

#if false
		public override XdeEntityMetadata GetEntityMetaData(IXdeAdapter adapter, IDataReader rs)
		{
			XdeEntityMetadata rc = new XdeEntityMetadata();

			rc.TableName = Convert.ToString(adapter[rs, "TABLE_NAME"]);
			rc.TableType = Convert.ToString(adapter[rs, "TABLE_TYPE"]);
			rc.Name = rc.TableName.Substring(3).ToUpper();
			rc.Type = rc.TableType.Equals("VIEW", StringComparison.OrdinalIgnoreCase) ? XdeEntityType.View : XdeEntityType.Table;

			return rc;
		}
#endif
		public override XdeEntityColumnMetadata GetColumnMetaData(IXdeAdapter adapter, IDataReader rs)
		{
			XdeEntityColumnMetadata rc = new XdeEntityColumnMetadata();

			rc.ColumnName = Convert.ToString(adapter.GetValue(rs, "COLUMN_NAME"));
			rc.DataTypeName = Convert.ToString(adapter.GetValue(rs, "TYPE_NAME"));
			rc.Length = Convert.ToInt32(adapter.GetValue(rs, "LENGTH"));
			rc.Precision = Convert.ToInt32(adapter.GetValue(rs, "PRECISION"));
			rc.Scale = Convert.ToInt32(adapter.GetValue(rs, "SCALE"));
			rc.Nullable = Convert.ToBoolean(adapter.GetValue(rs, "NULLABLE"));
			rc.DefaultValue = Convert.ToString(adapter.GetValue(rs, "COLUMN_DEF"));
			rc.Name = rc.ColumnName.Substring(3).ToUpper();

			return rc;
		}

#if false
		public override XdeCommand GetEntitiesSelectCommand()
		{
			return new XdeCommand("EXEC SP_TABLES @table_name = 'XT_%'");
		}
#endif

		public override XdeCommand GetColumnsSelectCommand(string entityName)
		{
			return new XdeCommand("EXEC SP_COLUMNS @table_name = ?, @column_name = 'XM_%'", GetTableName(entityName));
		}

		public override XdeCommand GetEntityIndexesCommand(string entityName)
		{
			return new XdeCommand("EXEC SP_HELPINDEX ?", GetQualifiedTableName(null, entityName));
		}

		public override XdeCommand GetEntityReferencesCommand(string entityName)
		{
			const string select =
				"SELECT"
					+ " TS.NAME AS CONSTRAINT_NAME,"
					+ " TTF.NAME AS TABLE_NAME,"
					+ " TCF.NAME AS COLUMN_NAME,"
					+ " TTR.NAME AS RTABLE_NAME,"
					+ " TCR.NAME AS RCOLUMN_NAME"
				+ " FROM SYSOBJECTS TS"
					+ " INNER JOIN SYSFOREIGNKEYS TF ON TF.CONSTID = TS.ID"
					+ " INNER JOIN SYSCOLUMNS TCF ON TF.FKEYID = TCF.ID AND TF.FKEY = TCF.COLID"
					+ " INNER JOIN SYSOBJECTS TTF ON TTF.ID = TCF.ID"
					+ " INNER JOIN SYSCOLUMNS TCR ON TF.RKEYID = TCR.ID AND TF.RKEY = TCR.COLID"
					+ " INNER JOIN SYSOBJECTS TTR ON TTR.ID = TCR.ID"
				+ " WHERE TS.TYPE = 'F' AND TTR.NAME = ?"
				+ " ORDER BY TS.NAME";

			return new XdeCommand(select, GetColumnName(entityName));
		}

		#region entity
#if false
		public override IEnumerable<XdeCommand> GetCreateEntityCommand(string entityName)
		{
			if ( entityName == null )
			{
				throw new ArgumentNullException("entityName");
			}

			entityName = String.Intern(entityName.Trim().ToUpper());

			yield return new XdeCommand(String.Format(CultureInfo.InvariantCulture,
				"\r\nIF NOT {0}\r\n\tCREATE TABLE {1} ([M_ID] [UNIQUEIDENTIFIER] NOT NULL CONSTRAINT PK_{2} PRIMARY KEY CLUSTERED)",
				GetEntityExistsSelectString(entityName),
				GetQualifiedTableName(null, entityName),
				entityName));
		}
#endif

		public override IEnumerable<XdeCommand> GetCreateEntityCommand(string entityName, IEnumerable<KeyValuePair<string, string>> fields)
		{
			if ( entityName == null )
			{
				throw new ArgumentNullException("entityName");
			}

			entityName = String.Intern(entityName.Trim().ToUpper());

			// build header of CREATE TABLE command 
			StringBuilder buffer = new StringBuilder(Environment.NewLine, 1000);
			buffer.Append("IF NOT ").Append(GetEntityExistsSelectString(entityName)).Append(NewLinePlusTab);
			buffer.Append("CREATE TABLE ").Append(GetQualifiedTableName(null, entityName)).Append(NewLinePlusTab);
			buffer.Append("(\r\n\t\t[M_ID] [UNIQUEIDENTIFIER] NOT NULL CONSTRAINT PK_").Append(entityName).Append(" PRIMARY KEY CLUSTERED");

			// add fields 
			if ( fields != null )
			{
				foreach ( var field in fields )
				{
					buffer.Append(',').Append(NewLinePlusTab).Append('\t').Append(GetColumnName(field.Key)).Append(' ').Append(field.Value);
				}
			}

			// finalize CREATE TABLE command	
			buffer.Append(NewLinePlusTab).Append(')');

			yield return new XdeCommand(buffer.ToString());
		}

		public override IEnumerable<XdeCommand> GetDropEntityCommand(string entityName)
		{
			yield return new XdeCommand(Environment.NewLine + "IF " + GetEntityExistsSelectString(entityName) + NewLinePlusTab + "DROP TABLE " + GetQualifiedTableName(null, entityName));
		}

		public override IEnumerable<XdeCommand> GetRenameEntityCommand(string entityName, string newEntityName)
		{
			if ( newEntityName == null )
			{
				throw new ArgumentNullException("newEntityName");
			}

			yield return new XdeCommand(String.Format(CultureInfo.InvariantCulture,
				"\r\nIF {0}\r\n\tEXEC SP_RENAME '{1}', '{2}', 'OBJECT'",
				GetEntityExistsSelectString(entityName),
				GetQualifiedTableName(null, entityName),
				GetQualifiedTableName(null, newEntityName.ToUpper())));
		}

		public override IEnumerable<XdeCommand> GetCreatePrimaryKeyCommand(string entityName, IEnumerable<string> columnNames)
		{
			if ( entityName == null )
			{
				throw new ArgumentNullException("entityName");
			}

			if ( columnNames == null )
			{
				throw new ArgumentNullException("columnNames");
			}

			entityName = String.Intern(entityName.ToUpper().Trim());
			string primaryKeyName = String.Intern("PK_" + entityName);

			yield return new XdeCommand(String.Format(CultureInfo.InvariantCulture,
				"\r\nIF {0} AND {1}\r\n\tALTER TABLE {2} DROP CONSTRAINT {3}",
				GetEntityExistsSelectString(entityName),
				GetPrimaryKeyExistsSelectString(primaryKeyName),
				GetQualifiedTableName(null, entityName),
				primaryKeyName));

			if ( columnNames.Count() > 0 )
			{
				StringBuilder columns = new StringBuilder();

				foreach ( string columnName in columnNames )
				{
					if ( columns.Length > 0 )
					{
						columns.Append(',');
					}

					columns.Append(GetColumnName(columnName));
				}

				yield return new XdeCommand(String.Format(CultureInfo.InvariantCulture,
					"\r\nIF {0}\r\n\tALTER TABLE {1} ADD CONSTRAINT {2} PRIMARY KEY CLUSTERED ({3})",
					GetEntityExistsSelectString(entityName),
					GetQualifiedTableName(null, entityName),
					primaryKeyName,
					columns.ToString()));
			}
		}
		#endregion

		#region column
		public override string GetColumnDefault(XdeEntityColumn column)
		{
			Type columnUnderlyingType = GetColumnUnderlyingType(column);

			if ( columnUnderlyingType == typeof(Guid) )
			{
				return "0x00000000000000000000000000000000";
			}
			else if ( columnUnderlyingType == typeof(Int16)
				|| columnUnderlyingType == typeof(Int32)
				|| columnUnderlyingType == typeof(Int64)
				|| columnUnderlyingType == typeof(UInt16)
				|| columnUnderlyingType == typeof(UInt32)
				|| columnUnderlyingType == typeof(UInt64)
				|| columnUnderlyingType == typeof(Boolean)
				|| columnUnderlyingType == typeof(Byte)
				|| columnUnderlyingType == typeof(Decimal)
				|| columnUnderlyingType == typeof(Double)
				|| columnUnderlyingType == typeof(Single) )
			{
				return "0";
			}
			else if ( columnUnderlyingType == typeof(String) )
			{
				return "''";
			}
			else if ( columnUnderlyingType == typeof(DateTime) )
			{
				return "GETDATE()";
			}
			else
			{
				throw new NotSupportedException(columnUnderlyingType.ToString());
			}
		}

		public override IEnumerable<XdeCommand> GetAddColumnCommand(string entityName, string columnName, string columnDefinition)
		{
			if ( columnDefinition == null )
			{
				throw new ArgumentNullException("columnDefinition");
			}

			yield return new XdeCommand(String.Format(CultureInfo.InvariantCulture,
				"\r\nIF NOT {0}\r\n\tALTER TABLE {1} ADD {2} {3}",
				GetColumnExistsSelectString(entityName, columnName),
				GetQualifiedTableName(null, entityName),
				GetColumnName(columnName),
				columnDefinition));
		}

		public override IEnumerable<XdeCommand> GetChangeColumnCommand(XdeEntityColumn column)
		{
			if ( new[] { "text", "ntext", "image" }.Contains(column.DataTypeName, StringComparer.OrdinalIgnoreCase) )
			{
				return Enumerable.Empty<XdeCommand>();
			}

			return base.GetChangeColumnCommand(column);
		}

		public override IEnumerable<XdeCommand> GetChangeColumnCommand(string entityName, string columnName, string columnDefinition)
		{
			if ( columnDefinition == null )
			{
				throw new ArgumentNullException("columnDefinition");
			}

			yield return new XdeCommand(String.Format(CultureInfo.InvariantCulture,
				"\r\nIF {0}\r\n\tALTER TABLE {1} ALTER COLUMN {2} {3}",
				GetColumnExistsSelectString(entityName, columnName),
				GetQualifiedTableName(null, entityName),
				GetColumnName(columnName),
				columnDefinition));
		}

		public override IEnumerable<XdeCommand> GetDropColumnCommand(string entityName, string columnName)
		{
			yield return new XdeCommand(String.Format(CultureInfo.InvariantCulture,
				"\r\nIF {0}\r\n\tALTER TABLE {1} DROP COLUMN {2}",
				GetColumnExistsSelectString(entityName, columnName),
				GetQualifiedTableName(null, entityName),
				GetColumnName(columnName)));
		}

		public override IEnumerable<XdeCommand> GetRenameColumnCommand(string entityName, string columnName, string newColumnName)
		{
			if ( newColumnName == null )
			{
				throw new ArgumentNullException("newColumnName");
			}

			yield return new XdeCommand(String.Format(CultureInfo.InvariantCulture,
				"\r\nIF {0}\r\n\tEXEC SP_RENAME '{1}.{2}', '{3}', 'COLUMN'",
				GetColumnExistsSelectString(entityName, columnName),
				GetQualifiedTableName(null, entityName),
				GetColumnName(columnName),
				GetColumnName(newColumnName.ToUpper())));
		}

		public override Type GetColumnUnderlyingType(XdeEntityColumn column)
		{
			string dataTypeName = column.DataTypeName;

			if ( new[] { "char", "varchar", "nchar", "nvarchar", "text", "ntext" }.Contains(dataTypeName, StringComparer.OrdinalIgnoreCase) )
				return typeof(String);
			else if ( new[] { "binary", "varbinary", "image" }.Contains(dataTypeName, StringComparer.OrdinalIgnoreCase) )
				return typeof(Byte[]);
			else if ( "uniqueidentifier".Equals(dataTypeName, StringComparison.OrdinalIgnoreCase) )
				return typeof(Guid);
			else if ( "bit".Equals(dataTypeName, StringComparison.OrdinalIgnoreCase) )
				return typeof(Boolean);
			else if ( new[] { "decimal", "numeric", "money", "smallmoney" }.Contains(dataTypeName, StringComparer.OrdinalIgnoreCase) )
				return typeof(Decimal);
			else if ( "float".Equals(dataTypeName, StringComparison.OrdinalIgnoreCase) )
				return typeof(Double);
			else if ( "real".Equals(dataTypeName, StringComparison.OrdinalIgnoreCase) )
				return typeof(Single);
			else if ( "bigint".Equals(dataTypeName, StringComparison.OrdinalIgnoreCase) )
				return typeof(Int64);
			else if ( "int".Equals(dataTypeName, StringComparison.OrdinalIgnoreCase) )
				return typeof(Int32);
			else if ( "smallint".Equals(dataTypeName, StringComparison.OrdinalIgnoreCase) )
				return typeof(Int16);
			else if ( "tinyint".Equals(dataTypeName, StringComparison.OrdinalIgnoreCase) )
				return typeof(Byte);
			else if ( new[] { "datetime", "smalldatetime" }.Contains(dataTypeName, StringComparer.OrdinalIgnoreCase) )
				return typeof(DateTime);

			throw new NotSupportedException(dataTypeName);
		}

		public override void SetColumnUndelyingType(XdeEntityColumn column, Type dataType, int precision, int scale)
		{
			if ( dataType == typeof(String) )
			{
				if ( 0 < precision && precision <= 4000 )
				{
					column.DataTypeName = "nvarchar";
					column.Precision = precision;
				}
				else
				{
					column.DataTypeName = "ntext";
					column.Precision = 0x3FFFFFFF;
				}

				column.DataLength = column.Precision * 2;
				column.Scale = 0;
			}
			else if ( dataType == typeof(Byte[]) )
			{
				if ( 0 < precision && precision <= 8000 )
				{
					column.DataTypeName = "varbinary";
					column.Precision = precision;
				}
				else
				{
					column.DataTypeName = "image";
					column.Precision = 0x7FFFFFFF;
				}

				column.DataLength = column.Precision;
				column.Scale = 0;
			}
			else if ( dataType == typeof(Guid) )
			{
				column.DataTypeName = "uniqueidentifier";
				column.Precision = 36;
				column.DataLength = 16;
				column.Scale = 0;
			}
			else if ( dataType == typeof(DateTime) )
			{
				column.DataTypeName = "datetime";
				column.Precision = 23;
				column.DataLength = 16;
				column.Scale = 3;
			}
			else if ( dataType == typeof(Int64) )
			{
				column.DataTypeName = "bigint";
				column.Precision = 19;
				column.DataLength = 8;
				column.Scale = 0;
			}
			else if ( dataType == typeof(Int32) )
			{
				column.DataTypeName = "int";
				column.Precision = 10;
				column.DataLength = 4;
				column.Scale = 0;
			}
			else if ( dataType == typeof(Int16) )
			{
				column.DataTypeName = "smallint";
				column.Precision = 5;
				column.DataLength = 2;
				column.Scale = 0;
			}
			else if ( dataType == typeof(Byte) )
			{
				column.DataTypeName = "tinyint";
				column.Precision = 3;
				column.DataLength = 1;
				column.Scale = 0;
			}
			else if ( dataType == typeof(Decimal) )
			{
				column.DataTypeName = "numeric";
				column.Precision = precision;

				if ( 0 < column.Precision && column.Precision <= 9 )
					column.DataLength = 5;
				else if ( 9 < column.Precision && column.Precision <= 19 )
					column.DataLength = 9;
				else if ( 19 < column.Precision && column.Precision <= 28 )
					column.DataLength = 13;
				else
					column.DataLength = 17;

				column.Scale = scale;
			}
			else if ( dataType == typeof(Double) )
			{
				column.DataTypeName = "float";
				column.Precision = 15;
				column.DataLength = 8;
				column.Scale = 0;
			}
			else if ( dataType == typeof(Single) )
			{
				column.DataTypeName = "real";
				column.Precision = 7;
				column.DataLength = 4;
				column.Scale = 0;
			}
			else if ( dataType == typeof(Boolean) )
			{
				column.DataTypeName = "bit";
				column.Precision = 1;
				column.DataLength = 0;
				column.Scale = 0;
			}
			else
			{
				throw new NotSupportedException();
			}
		}

		protected override string GetColumnCondensedTypeName(XdeEntityColumn column)
		{
			Type columnUnderlyingType = GetColumnUnderlyingType(column);

			if ( columnUnderlyingType == typeof(String) )
				return 0 < column.Precision && column.Precision <= 4000 ? String.Format("{0}({1})", column.DataTypeName, column.Precision) : column.DataTypeName;
			else if ( columnUnderlyingType == typeof(Byte[]) )
				return 0 < column.Precision && column.Precision <= 8000 ? String.Format("{0}({1})", column.DataTypeName, column.Precision) : column.DataTypeName;
			else if ( columnUnderlyingType == typeof(Decimal) )
				return String.Format("{0}({1}, {2})", column.DataTypeName, column.Precision, column.Scale);

			return column.DataTypeName;
		}
		#endregion

#if false
		protected override string GetEntityExistsSelectString(string entityName)
		{
			return String.Format("exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[XT_{0}]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)", entityName);
		}

		protected override string GetColumnExistsSelectString(string entityName, string columnName)
		{
			return String.Format("exists (select * from dbo.sysobjects ts inner join syscolumns tc on tc.id = ts.id and tc.name='XM_{1}' where ts.id = object_id(N'[dbo].[XT_{0}]') and OBJECTPROPERTY(ts.id, N'IsUserTable') = 1)", entityName, columnName);
		}

		protected override string GetPrimaryKeyExistsSelectString(string primaryKeyName)
		{
			return String.Format("exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[{0}]') and OBJECTPROPERTY(id, N'IsPrimaryKey') = 1)", primaryKeyName);
		}

		protected override string GetForeignKeyExistsSelectString(string foreignKeyName)
		{
			return string.Format("exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[{0}]') and OBJECTPROPERTY(id, N'IsForeignKey') = 1)", foreignKeyName);
		}
#endif
		#endregion

		#region data
		public override IEnumerable<XdeCommand> GetUnitSaveCommand(XdeUnit unit)
		{
			StringBuilder buffer = new StringBuilder();
			IList parameters = new ArrayList();

			if ( !unit.IsExists.HasValue )
			{
				AppendUnitIfNotExistsExpression(buffer, unit);
				AddUnitIfNotExistsParameters(parameters, unit);
			}

			if ( !unit.IsExists ?? true )
			{
				buffer.Append(Environment.NewLine).Append("\tINSERT INTO ");
				buffer.Append(GetQualifiedTableName(unit.Database, unit.EntityName)).Append(" (\r\n\t\tM_ID");
				StringBuilder valuesBuffer = new StringBuilder("?");
				parameters.Add(unit.Id);
				foreach ( XdeProperty property in unit.Properties )
				{
					if ( !String.IsNullOrEmpty(property.Name) )
					{

						if ( property.Column.Nullable && property.IsNull )
							continue;

						buffer.Append(", \r\n\t\t").Append(GetColumnName(property.Name));
						valuesBuffer.Append(", ?");
						if ( property.IsNull )
							parameters.Add(DBNull.Value);
						else
							parameters.Add(property.Value);
					}
				}
				buffer.Append(") \r\n\tVALUES (").Append(valuesBuffer.ToString()).Append(')');
			}

			if ( !unit.IsExists.HasValue )
			{
				AppendElseExpression(buffer, unit);
			}

			if ( unit.IsExists ?? true )
			{
				StringBuilder updateBuffer = new StringBuilder();

				foreach ( XdeProperty property in unit.Properties )
				{
					if ( property.Modified )
					{
						if ( updateBuffer.Length > 0 )
							updateBuffer.Append(", ");

						updateBuffer.Append(Environment.NewLine).Append("\t\t").Append(GetColumnName(property.Name));

						if ( property.IsNull )
						{
							updateBuffer.Append(" = NULL");
						}
						else
						{
							updateBuffer.Append(" = ?");
							parameters.Add(property.Value);
						}
					}
				}

				if ( updateBuffer.Length > 0 )
				{
					buffer.Append(Environment.NewLine).Append("\tUPDATE ").Append(GetQualifiedTableName(unit.Database, unit.EntityName)).Append(" SET ");
					buffer.Append(updateBuffer.ToString());

					AppendPrimaryKeyWhereClause(buffer, unit, true);
					AddPrimaryKeyWhereParameters(parameters, unit);
				}
				else
				{
					// ≈сли нет изменЄнных свойств, то добавление команды, котора€ ничего не делает (NOP)
					buffer.Append(Environment.NewLine).Append("\twhile (1 = 0) break;");
				}
			}

			yield return new XdeCommand(buffer.ToString(), parameters);
		}

		public override IEnumerable<XdeCommand> GetPropertySaveCommand(XdeProperty property)
		{
			XdeUnit unit = property.Unit;
			StringBuilder buffer = new StringBuilder();
			IList parameters = new ArrayList();

			AppendUnitIfNotExistsExpression(buffer, unit);
			AddUnitIfNotExistsParameters(parameters, unit);
			buffer.Append(Environment.NewLine).Append("\tRAISERROR(").Append("'Save Xde property error due : unit ").Append(unit.EntityName).Append(" with id ").Append(unit.Id).Append(" does not exists.'").Append(", 16, -1)");

			AppendElseExpression(buffer, unit);

			buffer.Append(Environment.NewLine).Append("\tUPDATE ").Append(GetQualifiedTableName(unit.Database, unit.EntityName)).Append(" SET ").Append(GetColumnName(property.Name));

			if ( property.IsNull )
			{
				buffer.Append(" = NULL");
			}
			else
			{
				buffer.Append(" = ?");
				parameters.Add(property.Value);
			}

			AppendPrimaryKeyWhereClause(buffer, unit, true);
			AddPrimaryKeyWhereParameters(parameters, unit);

			yield return new XdeCommand(buffer.ToString(), parameters);
		}
		#endregion
	}
}
