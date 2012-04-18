using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Digillect.Xde.Layers
{
	// Oracle в Xde дальше поддерживаться не будет. Код пока оставлен как пример работы с оракловыми объектами
	public abstract class OracleLayer : BaseLayer
	{
		#region core
		public override string GetQualifiedTableName(string catalogName, string entityName)
		{
			return GetTableName(entityName);
		}
		#endregion

		#region ddl
		#region old
		public override string GetColumnUniqueConstraint()
		{
			return "unique";
		}

		public override string GetColumnForeignKeyConstraint(string ref_entity, string ref_col, string name, XdeForeignKeyConstraintRules options)
		{
			string rc = string.Format("references {0} ({1})", GetTableName(ref_entity), GetColumnName(ref_col));
			rc = "constraint FK_" + name.ToUpper().Trim() + " foreign key " + rc;
			return rc;
		}

		public override string GetColumnDefinition(bool nullable, string defval, IEnumerable<String> constraints)
		{
			string rc = string.Empty;
			if ( !nullable )
				rc += " not null";
			else
				rc += " ";
			if ( defval != null )
				rc += " default " + defval;
			if ( constraints != null )
				foreach ( string constraint in constraints )
					rc += (" " + constraint);
			return rc;
		}

		public override string GetNumberColumnDefinition(bool nullable, int precision, int scale, string defval, IEnumerable<String> constraints)
		{
			string rc = string.Format("number({0}, {1})", precision, scale);
			return rc + GetColumnDefinition(nullable, defval, constraints);
		}

		public override string GetStringColumnDefinition(bool nullable, int length, string defval, IEnumerable<String> constraints)
		{
			string rc = string.Empty;
			if ( length > 0 )
				rc += string.Format("varchar2({0})", length);
			else
				rc += "clob";
			return rc + GetColumnDefinition(nullable, defval, constraints);
		}

		public override string GetBlobColumnDefinition(bool nullable, int length, string defval, IEnumerable<String> constraints)
		{
			string rc = string.Empty;
			if ( length > 0 )
				rc += string.Format("raw({0})", length);
			else
				rc += "blob";
			return rc + GetColumnDefinition(nullable, defval, constraints);
		}

		public override string GetCurrencyColumnDefinition(bool nullable, string defval, IEnumerable<String> constraints)
		{
			return "number(38, 2)" + GetColumnDefinition(nullable, defval, constraints);
		}

		public override string GetIntegerColumnDefinition(bool nullable, string defval, IEnumerable<String> constraints)
		{
			return "number(22)" + GetColumnDefinition(nullable, defval, constraints);
		}

		public override string GetLongColumnDefinition(bool nullable, string defval, IEnumerable<String> constraints)
		{
			return "number(38)" + GetColumnDefinition(nullable, defval, constraints);
		}

		public override string GetDoubleColumnDefinition(bool nullable, string defval, IEnumerable<String> constraints)
		{
			return "number(38, 16)" + GetColumnDefinition(nullable, defval, constraints);
		}

		public override string GetBoolColumnDefinition(bool nullable, string defval, IEnumerable<String> constraints)
		{
			return "number(1)" + GetColumnDefinition(nullable, defval, constraints);
		}

		public override string GetDateTimeColumnDefinition(bool nullable, string defval, IEnumerable<String> constraints)
		{
			return "date" + GetColumnDefinition(nullable, defval, constraints);
		}

		public override string GetGuidColumnDefinition(bool nullable, string defval, IEnumerable<String> constraints)
		{
			return "raw(16)" + GetColumnDefinition(nullable, defval, constraints);
		}
		#endregion

		public override XdeEntityMetadata GetEntityMetaData(IXdeAdapter adapter, IDataReader rs)
		{
			XdeEntityMetadata rc = new XdeEntityMetadata();

			rc.TableName = rs.GetString(0);
			rc.TableType = rs.GetString(1);
			rc.Name = rc.TableName.Substring(3).ToUpper();
			rc.Type = rc.TableType.Equals("VIEW", StringComparison.OrdinalIgnoreCase) ? XdeEntityType.View : XdeEntityType.Table;

			return rc;
		}

		public override XdeEntityColumnMetadata GetColumnMetaData(IXdeAdapter adapter, IDataReader rs)
		{
			XdeEntityColumnMetadata rc = new XdeEntityColumnMetadata();

			rc.ColumnName = (string) adapter.GetValue(rs, 0);
			rc.DataTypeName = (string) adapter.GetValue(rs, 1);
			rc.Length = (int) ((decimal) adapter.GetValue(rs, 2));
			rc.Scale = (int) adapter.GetValue(rs, 3);
			rc.Precision = (int) adapter.GetValue(rs, 4);
			rc.Nullable = "NULL".Equals(((string) adapter.GetValue(rs, 5)).Trim(), StringComparison.OrdinalIgnoreCase);
			rc.DefaultValue = (string) adapter.GetValue(rs, 6);

			return rc;
		}

		public override XdeCommand GetEntitiesSelectCommand()
		{
			const string cmd_text =
				"select t.table_name as TABLE_NAME, 'TABLE' as TABLE_TYPE from sys.user_tables t where t.table_name like 'XT_%' and t.tablespace_name = 'USERS'"
					+ " union"
					+ " select t.view_name as TABLE_NAME, 'VIEW' as TABLE_TYPE from sys.user_views t where t.view_name like 'XT_%'"
					+ " order by TABLE_TYPE";

			return new XdeCommand(cmd_text);
		}

		public override XdeCommand GetColumnsSelectCommand(string entityName)
		{
			const string cmd_text =
				"select"
					+ " t.cname,"
					+ " t.coltype,"
					+ " t.width,"
					+ " t.scale,"
					+ " t.precision,"
					+ " t.nulls,"
					+ " t.defaultval"
				+ " from sys.col t"
				+ " where"
					+ " t.tname = ? and"
					+ " t.cname like 'XM_%'"
				+ " order by t.cname";

			return new XdeCommand(cmd_text, GetTableName(entityName));
		}

		public override XdeCommand GetEntityIndexesCommand(string entityName)
		{
			throw new NotImplementedException();
		}

		public override XdeCommand GetEntityReferencesCommand(string entityName)
		{
			throw new NotImplementedException();
		}

		#endregion

		#region entity
		public override IEnumerable<XdeCommand> GetCreateEntityCommand(string entityName, IEnumerable<KeyValuePair<String, String>> fields)
		{
			if ( entityName == null )
			{
				throw new ArgumentNullException("entityName");
			}

			entityName = entityName.ToUpper().Trim();
			// build header of CREATE TABLE command 
			string create_table_cmd = "create table " + GetTableName(entityName) + "\n(\n\tm_id raw(16) not null constraint PK_" + entityName.ToUpper() + " primary key";
			// add fields 
			if ( fields != null )
				foreach ( var field in fields )
				{
					create_table_cmd += ",";
					create_table_cmd += "\n\t" + this.GetColumnName(field.Key) + " " + field.Value;
				}
			// finish CREATE TABLE command	
			create_table_cmd += "\n)";

			yield return new XdeCommand(create_table_cmd);
		}
		#endregion

		#region column
		#endregion

		#region query
		public override void ProcessBuildQueryFromClause(XdeQuery query, XdeQueryBuildData buildData)
		{
			if ( query.Mode == XdeQueryMode.Query || query.Mode == XdeQueryMode.Union )
			{
				buildData.FromBuffer.Append("\nFROM\n\t").Append(GetTableName(query.EntityName)).Append(" TT");
			}
			else if ( query.Mode == XdeQueryMode.Join )
			{
				XdeJoin join = (XdeJoin) query;

				//string f_t = query.GetPrefixT();
				string f_tt = query.GetPrefixTT();
				//string p_t = (query.Owner as XdeQuery).GetPrefixT();
				string p_tt = (query.Owner as XdeQuery).GetPrefixTT();
				string f_k = GetDetailKeyNative(join);
				string p_k = GetMasterKeyNative(join);

				switch ( join.JoinType )
				{
					case XdeJoinType.Inner:
						buildData.FromBuffer.Append(",\n\t").Append(GetTableName(query.EntityName)).Append(" ").Append(f_tt);
						buildData.WhereBuffer.Append(p_tt).Append(".").Append(p_k).Append(" = ").Append(f_tt).Append(".").Append(f_k).Append(" and (").Append(buildData.WhereBuffer).Append(")");
						break;
					case XdeJoinType.Left:
						buildData.FromBuffer.Append(",\n\t").Append(GetTableName(query.EntityName)).Append(" ").Append(f_tt);
						buildData.WhereBuffer.Append(f_tt).Append(".").Append(f_k).Append(" (+)= ").Append(p_tt).Append(".").Append(p_k).Append(" and (").Append(buildData.WhereBuffer).Append(")");
						break;
					case XdeJoinType.Right:
						buildData.FromBuffer.Append(",\n\t").Append(GetTableName(query.EntityName)).Append(" ").Append(f_tt);
						buildData.WhereBuffer.Append(p_tt).Append(".").Append(p_k).Append(" (+)= ").Append(f_tt).Append(".").Append(f_k).Append(" and (").Append(buildData.WhereBuffer).Append(")");
						break;
					default:
						throw new NotSupportedException(join.JoinType.ToString());
				}
			}
		}
		#endregion

		#region data
		#endregion
	}

	#region oracle 8i
	public abstract class Oracle8Layer : OracleLayer
	{
	}
	#endregion
}
