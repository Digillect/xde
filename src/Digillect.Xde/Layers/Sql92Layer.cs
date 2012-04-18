using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Digillect.Xde.Layers
{
	public abstract class Sql92Layer : BaseLayer
	{
		#region core
		protected abstract string TableSchema
		{
			get;
		}

		protected void AppendPrimaryKeyWhereClause(StringBuilder buffer, XdeUnit unit, bool appendWhereKeyword)
		{
			if ( buffer == null )
			{
				throw new ArgumentNullException("buffer");
			}

			if ( appendWhereKeyword )
			{
				buffer.Append(Environment.NewLine).Append("WHERE").Append(NewLinePlusTab).Append('\t');
			}

			if ( unit.PrimaryKeyMode == XdeUnitPrimaryKeyMode.Identifier || unit.Entity == null || unit.Entity.PrimaryKey == null )
			{
				buffer.Append(GetIdColumnName()).Append(" = ?");
			}
			else
			{
				bool first = true;

				foreach ( var column in unit.Entity.PrimaryKey.Columns )
				{
					if ( first )
					{
						first = false;
					}
					else
					{
						buffer.Append(" AND ");
					}

					buffer.Append(GetColumnName(column.Name)).Append(" = ?");
				}
			}
		}

		protected void AddPrimaryKeyWhereParameters(IList parameters, XdeUnit unit)
		{
			if ( unit.PrimaryKeyMode == XdeUnitPrimaryKeyMode.Identifier || unit.Entity == null || unit.Entity.PrimaryKey == null )
			{
				parameters.Add(unit.Id);
			}
			else
			{
				foreach ( var column in unit.Entity.PrimaryKey.Columns )
				{
					parameters.Add(unit.Properties[column.Name].Value);
				}
			}
		}
		#endregion

		#region ddl
		#region old
		public override string GetColumnUniqueConstraint()
		{
			return "UNIQUE";
		}

		public override string GetColumnForeignKeyConstraint(string refEntity, string refColumn, string name, XdeForeignKeyConstraintRules options)
		{
			StringBuilder rc = new StringBuilder();

			if ( !String.IsNullOrWhiteSpace(name) )
				rc.Append("CONSTRAINT FK_").Append(name.ToUpper().Trim()).Append(' ');

			rc.Append("FOREIGN KEY REFERENCES ").Append(GetQualifiedTableName(null, refEntity)).Append(" (").Append(GetColumnName(refColumn)).Append(')');

			if ( options.HasFlag(XdeForeignKeyConstraintRules.CascadeDelete) )
				rc.Append(" ON DELETE CASCADE");

			if ( options.HasFlag(XdeForeignKeyConstraintRules.CascadeUpdate) )
				rc.Append(" ON UPDATE CASCADE");

			return rc.ToString();
		}

		public override string GetColumnDefinition(bool nullable, string defval, IEnumerable<string> constraints)
		{
			StringBuilder buffer = new StringBuilder();

			if ( !nullable )
			{
				buffer.Append(" NOT");
			}

			buffer.Append(" NULL");

			if ( !String.IsNullOrWhiteSpace(defval) )
			{
				buffer.Append(" DEFAULT ").Append(defval);
			}

			if ( constraints != null )
			{
				foreach ( string constraint in constraints )
				{
					buffer.Append(' ').Append(constraint);
				}
			}

			return buffer.ToString();
		}
		#endregion

		public override XdeEntityMetadata GetEntityMetaData(IXdeAdapter adapter, IDataReader rs)
		{
			XdeEntityMetadata rc = new XdeEntityMetadata();

			rc.TableName = rs.GetString(0); // TABLE_NAME
			rc.TableType = rs.GetString(1); // TABLE_TYPE
			rc.Name = rc.TableName.Substring(3).ToUpper();
			rc.Type = rc.TableType.Equals("VIEW", StringComparison.OrdinalIgnoreCase) ? XdeEntityType.View : XdeEntityType.Table;

			return rc;
		}

		public override XdeEntityColumnMetadata GetColumnMetaData(IXdeAdapter adapter, IDataReader rs)
		{
			XdeEntityColumnMetadata rc = new XdeEntityColumnMetadata();

			rc.ColumnName = rs.GetString(0); // COLUMN_NAME
			rc.DataTypeName = rs.GetString(1); // DATA_TYPE
			//rc.Length = Convert.ToInt32(adapter.GetValue(rs, "CHARACTER_OCTET_LENGTH"));
			//rc.Precision = Convert.ToInt32(adapter.GetValue(rs, "NUMERIC_PRECISION"));
			//rc.Scale = Convert.ToInt32(adapter.GetValue(rs, "NUMERIC_SCALE"));
			rc.Nullable = "YES".Equals((string) adapter.GetValue(rs, "IS_NULLABLE"), StringComparison.OrdinalIgnoreCase);
			rc.DefaultValue = Convert.ToString(adapter.GetValue(rs, "COLUMN_DEFAULT"));
			rc.Name = rc.ColumnName.Substring(3).ToUpper();

			return rc;
		}

		public override XdeCommand GetEntitiesSelectCommand()
		{
			const string select =
				"SELECT TABLE_NAME,"
					+ " TABLE_TYPE"
				+ " FROM INFORMATION_SCHEMA.TABLES"
				+ " WHERE TABLE_SCHEMA = ?"
					+ " AND TABLE_NAME LIKE 'XT_%'"
					+ " AND TABLE_TYPE IN ('BASE TABLE', 'VIEW')";

			return new XdeCommand(select, this.TableSchema);
		}

		public override XdeCommand GetColumnsSelectCommand(string entityName)
		{
			const string select =
				"SELECT COLUMN_NAME,"
					+ " DATA_TYPE,"
					+ " CHARACTER_MAXIMUM_LENGTH,"
					+ " CHARACTER_OCTET_LENGTH,"
					+ " NUMERIC_PRECISION,"
					//+ " NUMERIC_PRECISION_RADIX,"
					+ " NUMERIC_SCALE,"
					+ " DATETIME_PRECISION,"
					+ " IS_NULLABLE,"
					+ " COLUMN_DEFAULT"
				+ " FROM INFORMATION_SCHEMA.COLUMNS"
				+ " WHERE TABLE_SCHEMA = ?"
					+ " AND TABLE_NAME = ?"
					+ " AND COLUMN_NAME LIKE 'XM_%'"
				+ " ORDER BY ORDINAL_POSITION";

			return new XdeCommand(select, this.TableSchema, GetTableName(entityName));
		}

		public override XdeCommand GetEntityReferencesCommand(string entityName)
		{
			const string select =
				"SELECT tc.CONSTRAINT_NAME,"
					+ " ccu.TABLE_NAME,"
					+ " ccu.COLUMN_NAME,"
					+ " rccu.TABLE_NAME AS RTABLE_NAME,"
					+ " rccu.COLUMN_NAME AS RCOLUMN_NAME"
				+ " FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc"
					+ " INNER JOIN INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE ccu ON tc.CONSTRAINT_CATALOG = ccu.CONSTRAINT_CATALOG AND tc.CONSTRAINT_SCHEMA = ccu.CONSTRAINT_SCHEMA AND tc.CONSTRAINT_NAME = ccu.CONSTRAINT_NAME AND tc.TABLE_CATALOG = ccu.TABLE_CATALOG AND tc.TABLE_SCHEMA = ccu.TABLE_SCHEMA AND tc.TABLE_NAME = ccu.TABLE_NAME"
					+ " INNER JOIN INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS rc ON rc.CONSTRAINT_CATALOG = ccu.CONSTRAINT_CATALOG AND rc.CONSTRAINT_SCHEMA = ccu.CONSTRAINT_SCHEMA AND rc.CONSTRAINT_NAME = ccu.CONSTRAINT_NAME"
					+ " INNER JOIN INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE rccu ON rc.UNIQUE_CONSTRAINT_CATALOG = rccu.CONSTRAINT_CATALOG AND rc.UNIQUE_CONSTRAINT_SCHEMA = rccu.CONSTRAINT_SCHEMA AND rc.UNIQUE_CONSTRAINT_NAME = rccu.CONSTRAINT_NAME"
				+ " WHERE tc.TABLE_SCHEMA = ? AND rccu.TABLE_NAME = ? AND tc.CONSTRAINT_TYPE = 'FOREIGN KEY'"
				+ " ORDER BY tc.CONSTRAINT_NAME";

			return new XdeCommand(select, this.TableSchema, GetTableName(entityName));
		}

		public override string GetColumnDefinition(XdeEntityColumn column, bool addDefaults)
		{
			string value = GetColumnCondensedTypeName(column) + (column.Nullable ? " NULL" : " NOT NULL");

			if ( addDefaults )
			{
				value += " CONSTRAINT DF_" + column.Entity.Name + "_" + column.Name + " DEFAULT " + GetColumnDefault(column);
			}

			return value;
		}

		/// <summary>
		/// Возвращает строковое представление типа колонки в таблице или представлении базы данных.
		/// </summary>
		protected abstract string GetColumnCondensedTypeName(XdeEntityColumn column);

		protected virtual string GetEntityExistsSelectString(string entityName)
		{
			return String.Format(CultureInfo.InvariantCulture, "EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = '{0}' AND TABLE_NAME = '{1}')", this.TableSchema, GetTableName(entityName));
		}

		protected virtual string GetColumnExistsSelectString(string entityName, string columnName)
		{
			return String.Format(CultureInfo.InvariantCulture, "EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = '{0}' AND TABLE_NAME = '{1}' AND COLUMN_NAME = '{2}')", this.TableSchema, GetTableName(entityName), GetColumnName(columnName));
		}

		protected virtual string GetPrimaryKeyExistsSelectString(string primaryKeyName)
		{
			return String.Format(CultureInfo.InvariantCulture, "EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE TABLE_SCHEMA = '{0}' AND CONSTRAINT_NAME = '{1}' and CONSTRAINT_TYPE = 'PRIMARY KEY')", this.TableSchema, primaryKeyName);
		}

		protected virtual string GetForeignKeyExistsSelectString(string foreignKeyName)
		{
			return String.Format(CultureInfo.InvariantCulture, "EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE TABLE_SCHEMA = '{0}' AND CONSTRAINT_NAME = '{1}' and CONSTRAINT_TYPE = 'FOREIGN KEY')", this.TableSchema, foreignKeyName);
		}
		#endregion

		#region query
		protected virtual bool ProcessBuildQueryEnumerateStaticFields
		{
			get { return false; }
		}

		public override void ProcessBuildQuerySelectList(XdeQuery query, XdeQueryBuildData buildData)
		{
			bool enumerateStaticFields = this.ProcessBuildQueryEnumerateStaticFields;
			ICollection<XdeQuery.SelectItem> realSelectItems = query.GetFinalSelectItems();
			string fieldAlias = buildData.GetFieldAlias(enumerateStaticFields);
			if ( buildData.Subquery )
			{
				if ( buildData.SelectBuffer.Length > 0 )
				{
					buildData.SelectBuffer.Remove(0, buildData.SelectBuffer.Length);
				}

				buildData.SelectBuffer.Append(NewLinePlusTab).Append("TT.M_ID");

				return;
			}
			if ( query.RootQuery.CountOnly )
			{
				if ( query.Mode != XdeQueryMode.Join )
				{
					buildData.SelectBuffer.Append(NewLinePlusTab)
						.Append(XdeQueryTags.Header.Object).Append(", ").Append(XdeQueryTags.Object.Tag).Append(", COUNT(*), ")
						.Append(XdeQueryTags.Header.Properties).Append(", 0, ")
						.Append(XdeQueryTags.Header.Joins).Append(", 0");
				}

				return;
			}
			//string prefixT = query.GetPrefixT();
			string prefixTT = query.GetPrefixTT();
			if ( query.Mode == XdeQueryMode.Join )
				buildData.SelectBuffer.Append(',');
			buildData.SelectBuffer.Append(NewLinePlusTab).Append(XdeQueryTags.Header.Object).Append(buildData.GetFieldAlias(enumerateStaticFields)).Append(',');
			if ( query.EliminateId )
			{
				buildData.SelectBuffer.Append(XdeQueryTags.Object.Alias | XdeQueryTags.Object.Id | XdeQueryTags.Object.Entity | XdeQueryTags.Object.Database)
					.Append(buildData.GetFieldAlias(enumerateStaticFields)).Append(",\r\n\t\t'").Append(query.Name).Append('\'').Append(buildData.GetFieldAlias(enumerateStaticFields)).Append(", cast(null as uniqueidentifier)")
					.Append(buildData.GetFieldAlias(enumerateStaticFields)).Append(", '").Append(query.EntityName).Append('\'').Append(buildData.GetFieldAlias(enumerateStaticFields))
					.Append(", '").Append(query.Database).Append('\'').Append(buildData.GetFieldAlias(enumerateStaticFields));
			}
			else
			{
				buildData.SelectBuffer.Append(XdeQueryTags.Object.Alias | XdeQueryTags.Object.Id | XdeQueryTags.Object.Entity | XdeQueryTags.Object.Database)
					.Append(fieldAlias).Append(",\r\n\t\t'").Append(query.Name).Append('\'').Append(fieldAlias).Append(',').Append(prefixTT).Append('.').Append(GetIdColumnName())
					.Append(fieldAlias).Append(", '").Append(query.EntityName).Append('\'').Append(fieldAlias)
					.Append(", '").Append(query.Database).Append('\'').Append(buildData.GetFieldAlias(enumerateStaticFields));
			}
			buildData.SelectBuffer.Append(',').Append(NewLinePlusTab).Append(XdeQueryTags.Header.Properties).Append(fieldAlias).Append(',').Append(realSelectItems.Count).Append(fieldAlias);
			foreach ( XdeQuery.SelectItem item in realSelectItems )
			{
				buildData.SelectBuffer.Append(",\r\n\t\t").Append('\'').Append(item.Alias).Append('\'').Append(fieldAlias).Append(',');
				if ( item.Expression == null )
					buildData.SelectBuffer.Append(prefixTT).Append('.').Append(GetColumnName(item.Alias)).Append(fieldAlias);
				else
					buildData.SelectBuffer.Append(item.Expression);
				buildData.SelectBuffer.Append(fieldAlias);
			}
		}

		public override void ProcessBuildQueryFromClause(XdeQuery query, XdeQueryBuildData buildData)
		{
			if ( query.Mode == XdeQueryMode.Query || query.Mode == XdeQueryMode.Union )
			{
				buildData.FromBuffer.Append(Environment.NewLine).Append("FROM").Append(NewLinePlusTab)
					.Append(GetQualifiedTableName(query.Database, query.EntityName)).Append(" TT");
			}
			else if ( query.Mode == XdeQueryMode.Join )
			{
				XdeJoin join = (XdeJoin) query;

				switch ( join.JoinType )
				{
					case XdeJoinType.Inner:
						buildData.FromBuffer.Append(NewLinePlusTab).Append("INNER JOIN ");
						break;
					case XdeJoinType.Left:
						buildData.FromBuffer.Append(NewLinePlusTab).Append("LEFT OUTER JOIN ");
						break;
					case XdeJoinType.Right:
						buildData.FromBuffer.Append(NewLinePlusTab).Append("RIGHT OUTER JOIN ");
						break;
					case XdeJoinType.Full:
						buildData.FromBuffer.Append(NewLinePlusTab).Append("FULL OUTER JOIN ");
						break;
					default:
						throw new NotSupportedException(join.JoinType.ToString());
				}

				//string prefixT = query.GetPrefixT();
				string prefixTT = query.GetPrefixTT();
				//string parentPrefixT = (query.Owner as XdeQuery).GetPrefixT();
				string parentPrefixTT = (query.Owner as XdeQuery).GetPrefixTT();

				buildData.FromBuffer.Append(GetQualifiedTableName(query.Database, query.EntityName)).Append(' ').Append(prefixTT)
					.Append(" ON ").Append(parentPrefixTT).Append('.').Append(GetMasterKeyNative(join)).Append(" = ").Append(prefixTT).Append('.').Append(GetDetailKeyNative(join));
			}
		}

		public override void ProcessBuildQueryClauses(XdeQuery query, XdeQueryBuildData buildData)
		{
			if ( query.Mode == XdeQueryMode.Query || query.Mode == XdeQueryMode.Union )
			{
				if ( String.IsNullOrWhiteSpace(query.Where) )
					buildData.WhereBuffer.Append("(1 = 1)");
				else
					buildData.WhereBuffer.Append('(').Append(query.Where).Append(')');
			}

			if ( query.Mode == XdeQueryMode.Query )
			{
				if ( !String.IsNullOrWhiteSpace(query.GroupBy) )
					buildData.GroupByBuffer.Append(query.GroupBy);

				if ( !String.IsNullOrWhiteSpace(query.Having) )
					buildData.HavingBuffer.Append(query.Having);

				if ( !buildData.Subquery /*&& !buildData.Union*/ )
				{
					if ( !query.CountOnly && !String.IsNullOrWhiteSpace(query.OrderBy) )
						buildData.OrderByBuffer.Append(query.OrderBy);

					if ( buildData.OrderByBuffer.Length != 0 )
						buildData.OrderByBuffer.Append(", ");

					buildData.OrderByBuffer.Append("1 ASC");
				}
			}
		}
		#endregion

		#region data
		public override IEnumerable<XdeCommand> GetUnitDeleteCommand(XdeUnit unit)
		{
			StringBuilder sb = new StringBuilder();
			IList parameters = new ArrayList();

			sb.Append("DELETE FROM ").Append(GetQualifiedTableName(unit.Database, unit.EntityName)); // + " WHERE " + GetIdColumnName() + " = ?", unit.Id);

			AppendPrimaryKeyWhereClause(sb, unit, true);
			AddPrimaryKeyWhereParameters(parameters, unit);

			yield return new XdeCommand(sb.ToString(), parameters);
		}
		#endregion
	}
}
