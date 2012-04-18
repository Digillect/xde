using System;

namespace Digillect.Xde
{
	/// <summary>
	/// Теги полей для формирования/декодирования resultset. 
	/// </summary>
	public static class XdeQueryTags
	{
		/// <summary>
		/// Теги полей для формирования/декодирования resultset. 
		/// </summary>
		public static class Header
		{
			public const int None = 0;
			public const int Object = 1;
			public const int Properties = 2;
			public const int Joins = 3;
		}

		/// <summary>
		/// Теги полей для формирования/декодирования resultset. 
		/// </summary>
		public static class Object
		{
			public const int None = 0;
			public const int Alias = 1;
			public const int Id = 2;
			public const int Entity = 4;
			public const int Database = 8;
			public const int Tag = 0x10;
			//public const int InsDate = 0x20;
			//public const int UpdDate = 0x40;
			//public const int Root = Id | Entity | RegName | Tag | InsDate | UpdDate;
			//public const int All = Alias | Root;
		}

		/// <summary>
		/// Теги полей для формирования/декодирования resultset. 
		/// </summary>
		public static class Property
		{
			public const int None = 0;
			public const int Id = 1;
			public const int Type = 2;
			public const int Name = 4;
			public const int Value = 8;
			public const int Root = Name | Value;
		}
	}
}
