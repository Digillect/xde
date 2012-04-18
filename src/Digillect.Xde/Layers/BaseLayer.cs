using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Digillect.Xde.Layers
{
	public abstract class BaseLayer : IXdeLayer
	{
		protected static readonly string NewLinePlusTab = String.Intern(Environment.NewLine + '\t');

		#region core
		public abstract void CreateDatabase(IXdeAdapter adapter, string serverName, string databaseName, string userName, string password);

		public virtual string GetTableName(string entityName)
		{
			return String.Intern("XT_" + entityName);
		}

		public abstract string GetQualifiedTableName(string catalogName, string entityName);

		public virtual string GetColumnName(string propertyName)
		{
			return String.IsNullOrWhiteSpace(propertyName) ? GetIdColumnName() : String.Intern("XM_" + propertyName);
		}

		public virtual string GetIdColumnName()
		{
			return "M_ID";
		}

		public virtual string GetIsTableExistsExpression(string entityName)
		{
			return String.Empty;
		}

		//protected abstract StringBuilder AppendUnitIfNotExistsExpression(StringBuilder buffer, XdeUnit unit);
		//protected abstract void AddUnitIfNotExistsParameters(IList parameters, XdeUnit unit);
		//protected abstract StringBuilder AppendElseExpression(StringBuilder buffer, XdeUnit unit);
		//protected abstract StringBuilder AppendPrimaryKeyWhereClause(StringBuilder buffer, XdeUnit unit, bool appendWhereKeyword);
		//protected virtual void AddPrimaryKeyWhereParameters(IList parameters, XdeUnit unit)
		//{
		//    AddUnitIfNotExistsParameters(parameters, unit);
		//}
		#endregion

		#region ddl
		#region old
		public abstract string GetColumnUniqueConstraint();

		public string GetColumnForeignKeyConstraint(string ref_entity, string ref_col, string name)
		{
			return GetColumnForeignKeyConstraint(ref_entity, ref_col, name, XdeForeignKeyConstraintRules.None);
		}

		public abstract string GetColumnForeignKeyConstraint(string ref_entity, string ref_col, string name, XdeForeignKeyConstraintRules options);

		public abstract string GetColumnDefinition(bool nullable, string defval, IEnumerable<string> constraints);
		public abstract string GetNumberColumnDefinition(bool nullable, int precision, int scale, string defval, IEnumerable<string> constraints);
		public abstract string GetStringColumnDefinition(bool nullable, int length, string defval, IEnumerable<string> constraints);
		public abstract string GetBlobColumnDefinition(bool nullable, int length, string defval, IEnumerable<string> constraints);
		public abstract string GetCurrencyColumnDefinition(bool nullable, string defval, IEnumerable<string> constraints);
		public abstract string GetIntegerColumnDefinition(bool nullable, string defval, IEnumerable<string> constraints);
		public abstract string GetLongColumnDefinition(bool nullable, string defval, IEnumerable<string> constraints);
		public abstract string GetDoubleColumnDefinition(bool nullable, string defval, IEnumerable<string> constraints);
		public abstract string GetBoolColumnDefinition(bool nullable, string defval, IEnumerable<string> constraints);
		public abstract string GetDateTimeColumnDefinition(bool nullable, string defval, IEnumerable<string> constraints);
		public abstract string GetGuidColumnDefinition(bool nullable, string defval, IEnumerable<string> constraints);

		public string GetGuidColumnDefinition(bool nullable, string defval, string reference, string fk_name)
		{
			return GetGuidColumnDefinition(nullable, defval, reference, fk_name, XdeForeignKeyConstraintRules.None);
		}

		public string GetGuidColumnDefinition(bool nullable, string defval, string reference, string fk_name, XdeForeignKeyConstraintRules options)
		{
			return GetGuidColumnDefinition(nullable, defval, String.IsNullOrEmpty(reference) ? null : new string[] { GetColumnForeignKeyConstraint(reference, null, fk_name, options) });
		}
		#endregion

		public abstract XdeEntityMetadata GetEntityMetaData(IXdeAdapter adapter, IDataReader rs);
		public abstract XdeEntityColumnMetadata GetColumnMetaData(IXdeAdapter adapter, IDataReader rs);

		public abstract XdeCommand GetEntitiesSelectCommand();
		public abstract XdeCommand GetColumnsSelectCommand(string entityName);

		public abstract XdeCommand GetEntityIndexesCommand(string entityName);
		public abstract XdeCommand GetEntityReferencesCommand(string entityName);

		public virtual IEnumerable<XdeCommand> GetCreateEntityCommand(string entityName)
		{
			return GetCreateEntityCommand(entityName, null);
		}

		public abstract IEnumerable<XdeCommand> GetCreateEntityCommand(string entityName, IEnumerable<KeyValuePair<string, string>> fields);

		void IXdeLayer.GetCreateEntityCommand(ICollection<XdeCommand> command, string entityName, IDictionary fields)
		{
			foreach ( var cmd in GetCreateEntityCommand(entityName, fields.Cast<DictionaryEntry>().Select(x => new KeyValuePair<string, string>((string) x.Key, (string) x.Value))) )
			{
				command.Add(cmd);
			}
		}

		public abstract IEnumerable<XdeCommand> GetDropEntityCommand(string entityName);
		public abstract IEnumerable<XdeCommand> GetRenameEntityCommand(string entityName, string newEntityName);

		public abstract IEnumerable<XdeCommand> GetCreatePrimaryKeyCommand(string entityName, IEnumerable<string> columnNames);

		public virtual IEnumerable<XdeCommand> GetFieldsAddCommand(string entityName, IEnumerable<KeyValuePair<string, string>> fields)
		{
			foreach ( var cmd in fields.SelectMany(x => GetAddColumnCommand(entityName, x.Key, x.Value)) )
			{
				yield return cmd;
			}
		}

		public virtual IEnumerable<XdeCommand> GetFieldsDropCommand(string entityName, IEnumerable<string> fields)
		{
			foreach ( var cmd in fields.SelectMany(x =>  GetDropColumnCommand(entityName, x)) )
			{
				yield return cmd;
			}
		}

		public abstract string GetColumnDefault(XdeEntityColumn column);
		public abstract string GetColumnDefinition(XdeEntityColumn column, bool addDefaults);

		public virtual IEnumerable<XdeCommand> GetAddColumnCommand(XdeEntityColumn column)
		{
			if ( column == null )
			{
				throw new ArgumentNullException("column");
			}

			return GetAddColumnCommand(column.Entity.Name, column.Name, GetColumnDefinition(column, true));
		}

		public abstract IEnumerable<XdeCommand> GetAddColumnCommand(string entityName, string fieldName, string columnDefinition);

		public virtual IEnumerable<XdeCommand> GetChangeColumnCommand(XdeEntityColumn column)
		{
			if ( column == null )
			{
				throw new ArgumentNullException("column");
			}

			return GetChangeColumnCommand(column.Entity.Name, column.Name, GetColumnDefinition(column, false));
		}

		public abstract IEnumerable<XdeCommand> GetChangeColumnCommand(string entityName, string field_name, string columnDefinition);

		public abstract IEnumerable<XdeCommand> GetDropColumnCommand(string entityName, string field_name);

		public abstract IEnumerable<XdeCommand> GetRenameColumnCommand(string entityName, string columnName, string newColumnName);

		public virtual IEnumerable<XdeCommand> GetSaveColumnCommand(XdeEntityColumn column)
		{
			if ( column == null )
			{
				throw new ArgumentNullException("column");
			}

			if ( column.IsExists ?? true )
			{
				foreach ( var cmd in GetChangeColumnCommand(column) )
				{
					yield return cmd;
				}
			}

			if ( !column.IsExists ?? true )
			{
				foreach ( var cmd in GetAddColumnCommand(column) )
				{
					yield return cmd;
				}
			}
		}

		public virtual IEnumerable<XdeCommand> GetSaveColumnCommand(string entityName, string columnName, string columnDefinition)
		{
			return GetChangeColumnCommand(entityName, columnName, columnDefinition).Concat(
				GetAddColumnCommand(entityName, columnName, columnDefinition));
		}

		public abstract Type GetColumnUnderlyingType(XdeEntityColumn column);
		public abstract void SetColumnUndelyingType(XdeEntityColumn column, Type dataType, int scale, int precision);

		//public abstract string GetEntityExistsSelectString(string entityName);
		//public abstract string GetColumnExistsSelectString(string entityName, string columnName);
		//public abstract string GetPrimaryKeyExistsSelectString(string primaryKeyName);
		//public abstract string GetForeignKeyExistsSelectString(string foreignKeyName);
		#endregion

		#region query
		//public abstract bool ProcessBuildQueryEnumerateStaticFields
		//{
		//    get;
		//}

		public abstract void ProcessBuildQueryFromClause(XdeQuery Query, XdeQueryBuildData buildData);
		public abstract void ProcessBuildQuerySelectList(XdeQuery query, XdeQueryBuildData buildData);
		public abstract void ProcessBuildQueryClauses(XdeQuery query, XdeQueryBuildData buildData);

		protected string GetDetailKeyNative(XdeJoin join)
		{
			if ( String.IsNullOrEmpty(join.DetailKey) )
				return GetIdColumnName();

			if ( join.DetailKey[0] == '$' )
				return GetColumnName(join.DetailKey.Substring(1));

			if ( join.DetailKey[0] == '\\' )
				return "M_" + join.DetailKey.Substring(1);

			return GetColumnName(join.DetailKey);
		}

		protected string GetMasterKeyNative(XdeJoin join)
		{
			if ( String.IsNullOrEmpty(join.MasterKey) )
				return GetColumnName(join.EntityName + "_ID");

			if ( join.MasterKey[0] == '$' )
				return GetColumnName(join.MasterKey.Substring(1));

			if ( join.MasterKey[0] == '\\' )
				return "M_" + join.MasterKey.Substring(1);

			return GetColumnName(join.MasterKey);
		}
		#endregion

		#region data
		public abstract IEnumerable<XdeCommand> GetUnitDeleteCommand(XdeUnit unit);
		public abstract IEnumerable<XdeCommand> GetUnitSaveCommand(XdeUnit unit);
		public abstract IEnumerable<XdeCommand> GetPropertySaveCommand(XdeProperty property);
		#endregion
	}
}
