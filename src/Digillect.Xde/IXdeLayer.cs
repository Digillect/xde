using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Text;

namespace Digillect.Xde
{
	public interface IXdeLayer
	{
		#region core
		void CreateDatabase(IXdeAdapter adapter, string serverName, string databaseName, string userName, string password);
		string GetTableName(string entityName);
		string GetQualifiedTableName(string catalogName, string entityName);
		string GetColumnName(string fieldName);
		string GetIdColumnName();
		string GetIsTableExistsExpression(string entityName);
		//StringBuilder AppendUnitIfNotExistsExpression(StringBuilder buffer, Unit unit);
		//void AddUnitIfNotExistsParameters(IList parameters, Unit unit);
		//StringBuilder AppendElseExpression(StringBuilder buffer, Unit unit);
		//StringBuilder AppendPrimaryKeyWhereClause(StringBuilder buffer, Unit unit, bool appendWhereKeyword);
		//void AddPrimaryKeyWhereParameters(IList parameters, Unit unit);
		#endregion

		#region ddl
		#region old
		string GetColumnUniqueConstraint();
		string GetColumnForeignKeyConstraint(string ref_entity, string ref_col, string name);
		string GetColumnForeignKeyConstraint(string ref_entity, string ref_col, string name, XdeForeignKeyConstraintRules options);

		string GetColumnDefinition(bool nullable, string defval, IEnumerable<string> constraints);
		string GetNumberColumnDefinition(bool nullable, int precision, int scale, string defval, IEnumerable<string> constraints);
		string GetStringColumnDefinition(bool nullable, int length, string defval, IEnumerable<string> constraints);
		string GetBlobColumnDefinition(bool nullable, int length, string defval, IEnumerable<string> constraints);
		string GetCurrencyColumnDefinition(bool nullable, string defval, IEnumerable<string> constraints);
		string GetIntegerColumnDefinition(bool nullable, string defval, IEnumerable<string> constraints);
		string GetLongColumnDefinition(bool nullable, string defval, IEnumerable<string> constraints);
		string GetDoubleColumnDefinition(bool nullable, string defval, IEnumerable<string> constraints);
		string GetBoolColumnDefinition(bool nullable, string defval, IEnumerable<string> constraints);
		string GetDateTimeColumnDefinition(bool nullable, string defval, IEnumerable<string> constraints);
		string GetGuidColumnDefinition(bool nullable, string defval, IEnumerable<string> constraints);
		string GetGuidColumnDefinition(bool nullable, string defval, string reference, string fk_name);
		string GetGuidColumnDefinition(bool nullable, string defval, string reference, string fk_name, XdeForeignKeyConstraintRules options);
		#endregion

		XdeEntityMetadata GetEntityMetaData(IXdeAdapter adapter, IDataReader rs);
		XdeEntityColumnMetadata GetColumnMetaData(IXdeAdapter adapter, IDataReader rs);

		XdeCommand GetEntitiesSelectCommand();
		XdeCommand GetColumnsSelectCommand(string entityName);

		XdeCommand GetEntityIndexesCommand(string entityName);
		XdeCommand GetEntityReferencesCommand(string entityName);

		IEnumerable<XdeCommand> GetCreateEntityCommand(string entityName);
		IEnumerable<XdeCommand> GetCreateEntityCommand(string entityName, IEnumerable<KeyValuePair<string, string>> fields);
		[EditorBrowsable(EditorBrowsableState.Never)]
		//[Obsolete("Use the generic alternative.")]
		void GetCreateEntityCommand(ICollection<XdeCommand> command, string entityName, IDictionary fields);
		IEnumerable<XdeCommand> GetDropEntityCommand(string entityName);
		IEnumerable<XdeCommand> GetRenameEntityCommand(string entityName, string newEntityName);

		IEnumerable<XdeCommand> GetCreatePrimaryKeyCommand(string entityName, IEnumerable<string> columnNames);

		IEnumerable<XdeCommand> GetFieldsAddCommand(string entityName, IEnumerable<KeyValuePair<string, string>> fields);
		IEnumerable<XdeCommand> GetFieldsDropCommand(string entityName, IEnumerable<string> fields);

		//string GetColumnDefault(XdeEntityColumn column);
		string GetColumnDefinition(XdeEntityColumn column, bool addDefaults);

		IEnumerable<XdeCommand> GetAddColumnCommand(XdeEntityColumn column);
		IEnumerable<XdeCommand> GetAddColumnCommand(string entityName, string columnName, string columnDefinition);
		IEnumerable<XdeCommand> GetChangeColumnCommand(XdeEntityColumn column);
		IEnumerable<XdeCommand> GetChangeColumnCommand(string entityName, string columnName, string columnDefinition);
		IEnumerable<XdeCommand> GetDropColumnCommand(string entityName, string columnName);
		IEnumerable<XdeCommand> GetRenameColumnCommand(string entityName, string columnName, string newColumnName);
		IEnumerable<XdeCommand> GetSaveColumnCommand(XdeEntityColumn column);
		IEnumerable<XdeCommand> GetSaveColumnCommand(string entityName, string columnName, string columnDefinition);

		/// <summary>
		/// Возвращает тип данных, который асоциируется с данной колонкой.
		/// </summary>
		Type GetColumnUnderlyingType(XdeEntityColumn column);

		/// <summary>
		/// Устанавливает тип данных, который асоциируется с данной колонкой.
		/// </summary>
		void SetColumnUndelyingType(XdeEntityColumn column, Type dataType, int scale, int precision);
		//string GetColumnCondensedTypeName(XdeEntityColumn column);

		//string GetEntityExistsSelectString(string entityName);
		//string GetColumnExistsSelectString(string entityName, string columnName);
		//string GetPrimaryKeyExistsSelectString(string primaryKeyName);
		//string GetForeignKeyExistsSelectString(string foreignKeyName);
		#endregion

		#region query
		//bool ProcessBuildQueryEnumerateStaticFields
		//{
		//    get;
		//}

		void ProcessBuildQuerySelectList(XdeQuery query, XdeQueryBuildData buildData);
		void ProcessBuildQueryFromClause(XdeQuery query, XdeQueryBuildData buildData);
		void ProcessBuildQueryClauses(XdeQuery query, XdeQueryBuildData buildData);
		#endregion

		#region data
		IEnumerable<XdeCommand> GetUnitDeleteCommand(XdeUnit unit);
		IEnumerable<XdeCommand> GetUnitSaveCommand(XdeUnit unit);
		IEnumerable<XdeCommand> GetPropertySaveCommand(XdeProperty property);
		#endregion
	}
}
