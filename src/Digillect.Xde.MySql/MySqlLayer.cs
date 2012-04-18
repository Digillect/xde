using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Digillect.Xde.Layers
{
	public abstract class MySqlLayer : Sql92Layer
	{
		#region ddl
		/*
		public override XdeEntityMetadata GetEntityMetaData(IXdeAdapter adapter, IDataReader rs)
		{
			XdeEntityMetadata rc = new XdeEntityMetadata();

			rc.TableName = rs.GetString(0);
			rc.TableType = "TABLE";
			rc.Type = XdeEntityType.Table;
			rc.Name = rc.TableName.Substring(3);

			return rc;

		}
		*/

		public override XdeEntityColumnMetadata GetColumnMetaData(IXdeAdapter adapter, IDataReader rs)
		{
			XdeEntityColumnMetadata rc = new XdeEntityColumnMetadata();

			rc.ColumnName = (string) adapter.GetValue(rs, "Field");
			rc.DataTypeName = (string) adapter.GetValue(rs, "Type");
			//rc.Precision = 0;
			//rc.Length = 0;
			//rc.Scale = 0;
			rc.Nullable = "yes".Equals((string) adapter.GetValue(rs, "Null"), StringComparison.OrdinalIgnoreCase);
			//rc.DefaultValue = null;
			rc.Name = rc.ColumnName.Substring(3);

			return rc;
		}

		public override XdeCommand GetEntitiesSelectCommand()
		{
			return new XdeCommand("show full tables like 'XT_%'");
		}

		public override XdeCommand GetColumnsSelectCommand(string entityName)
		{
			return new XdeCommand("show columns from " + GetTableName(entityName) + " like 'XM_%'");

		}
		#endregion

		#region entity
		#endregion

		#region column
		public override IEnumerable<XdeCommand> GetChangeColumnCommand(string entityName, string field_name, string field_description)
		{
			//string rc= string.Format("alter table {0} alter column {1} {2}", this.GetTableName(entityName), GetColumnName(field_name), field_description);	
			return null;
		}

		public override IEnumerable<XdeCommand> GetDropColumnCommand(string entityName, string field_name)
		{
			//string rc= string.Format("alter table {0} drop column {1}", GetTableName(entityName), GetColumnName(field_name));
			return null;
		}

		public override IEnumerable<XdeCommand> GetRenameColumnCommand(string entityName, string field_name, string new_field_name)
		{
			//string rc= string.Format("exec sp_rename '{0}.{1}', '{2}', 'COLUMN'", this.GetTableName(entityName), GetColumnName(field_name), ("xm_"+ new_field_name).ToUpper());	
			return null;
		}
		#endregion

		#region query
		protected override bool ProcessBuildQueryEnumerateStaticFields
		{
			get { return true; }
		}
		#endregion
	}
}
