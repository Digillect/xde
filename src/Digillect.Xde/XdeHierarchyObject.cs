using System;

namespace Digillect.Xde
{
	public abstract class XdeHierarchyObject : XdeObject, IXdeHierarchyObject
	{
		private readonly IXdeHierarchyObject m_owner;

		#region Constructor
		protected XdeHierarchyObject(IXdeHierarchyObject owner, string name)
			: base(name)
		{
			if ( owner == null )
			{
				throw new ArgumentNullException("owner");
			}

			m_owner = owner;
		}
		#endregion

		#region IXdeHierarchyObject Members
		public IXdeHierarchyObject Owner
		{
			get { return m_owner; }
		}
		#endregion
	}
}
