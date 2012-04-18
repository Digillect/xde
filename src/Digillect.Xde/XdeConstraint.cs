using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Digillect.Xde
{
	public class XdeConstraint : XdeHierarchyObject
	{
		protected XdeConstraint(XdeEntity owner, string name)
			: base(owner, name)
		{
		}

		public override IEnumerable<XdeCommand> GetCommand()
		{
			return Enumerable.Empty<XdeCommand>();
		}
	}

#if false // Not yet used/supported
	public class XdeCheckConstraint : XdeConstraint
	{
		public XdeCheckConstraint(XdeEntity owner, string name)
			: base(owner, name)
		{
		}
	}
#endif

	public class XdeForeignKeyConstraint : XdeConstraint
	{
		private readonly string m_referencedColumnName;
		private readonly string m_referencingEntityName;
		private readonly string m_referencingColumnName;

		public XdeForeignKeyConstraint(XdeEntity owner, string name, string referencedColumnName, string referencingEntityName, string referencingColumnName)
			: base(owner, name)
		{
			m_referencedColumnName = referencedColumnName;
			m_referencingEntityName = referencingEntityName;
			m_referencingColumnName = referencingColumnName;
		}

		public string ReferencedColumnName
		{
			get { return m_referencedColumnName; }
		}

		public string ReferencingEntityName
		{
			get { return m_referencingEntityName; }
		}

		public string ReferencingColumnName
		{
			get { return m_referencingColumnName; }
		}
	}

	[Flags]
	public enum XdeForeignKeyConstraintRules
	{
		None = 0,
		CascadeUpdate = 1,
		CascadeDelete = 2
	}
}
