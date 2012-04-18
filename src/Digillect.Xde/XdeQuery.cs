using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;

namespace Digillect.Xde
{
	/// <summary>
	/// Дескриптор запроса данных.
	/// </summary>
	/// <remarks> 
	/// <para><b>Типы запросов</b></para>
	/// Запросы могут группироваться в иерархическую структуру посредством присоединения других запросов в виде джойнов, 
	/// являющихся полным аналогом joins в SQL.<br/>
	/// При этом являющийся джойном запрос имеет свойство <see cref="XdeQuery.Mode"/> равное <see cref="XdeQueryMode.Join"/>.<br/>
	/// Запрос, имеющий джойны, может быть джойном другого запроса.<br/>
	/// 
	/// <para>Xde-совместимый оператор SELECT.</para>
	/// <code>
	/// SELECT
	/// 	-- registry section
	/// 	1, 7, '{alias}', {row_id | cast(null as uniqueidentifier)}, '{entityName}',
	/// 	-- properties section
	/// 	2, {properties_count},
	/// 		[[12, ]'{property_name}', {property_value}] [, ...]
	/// 		-- joins section        
	/// 		3, {joins_count},
	/// 			[joins] [, ...]
	/// [other SELECT clauses]
	/// </code>
	/// <para>где :</para>
	/// <list type="bullet">
	/// <item><i>alias</i> - алиас сущности или наименование джойна, обычно алиас сущности является пустой строкой (не NULL);
	/// наменование джойна должна быть непустая строка, уникальная внутри одного уровня иерархии;</item>
	/// <item><i>row_id</i> - идентификатор объекта данных;</item>
	/// <item><i>entityName</i> - наименование сущности объекта данных, не может быть NULL;</item>
	/// <item><i>properties_count</i> - количество возвращаемых свойств;</item>
	/// <item><i>property_name</i> - наименование свойства, должна быть непустая строка, уникальная внутри группы свойств;</item>
	/// <item><i>property_value</i> - значение свойства, любое value SQL выражение, может быть NULL;</item>
	/// <item><i>joins_count</i> - количество джойнов, если джойнов нет, то обязательно 0.</item>
	/// </list>
	/// 
	/// <para>Синтаксис псевдоимен полей при построении SQL выражений.</para>
	/// 
	/// <para>
	/// синтаксис описания первичных ключей:<br/>
	/// \[.<i>join_alias</i>[. ...]]ID
	/// <code>
	/// // запрос по ID "PRODUCT"
	/// var qs = session.NewQuery("PRODUCT", @"\ID = ?", some_id);
	///
	/// // запрос по ID "VENDOR"
	/// var qs = session.NewQuery("PRODUCT", @"\VEND.ID = ?", some_id);
	/// var vendor_join = qs.Add("VEND", "VENDOR", "$VENDOR_ID", "\\ID");
	///
	/// // запрос по ID "COUNTRY"
	/// var qs = session.NewQuery("PRODUCT", @"\VEND.CNTRY.ID = ?", some_id);
	/// var vendor_join = qs.Add("VEND", "VENDOR", "$VENDOR_ID", "\\ID", QS.Constants.JoinType.Left);
	/// var country_join = vendor_join.Add("CNTRY", "COUNTRY", "$COUNTRY_ID", "\\ID");
	/// </code>
	/// </para>
	/// 
	/// <para>
	/// синтаксис описания полей (свойств):<br/>
	/// $[.<i>join_alias</i>[. ...]]<i>field</i>
	/// <code>
	/// // запрос по NAME "PRODUCT"
	/// var qs = session.NewQuery("PRODUCT", @"$NAME = ?", product_name);
	///
	/// // запрос по NAME "VENDOR"
	/// var qs = session.NewQuery("PRODUCT", @"$VEND.NAME = ?", vendor_name);
	/// var vendor_join = qs.Add("VEND", "VENDOR", "$VENDOR_ID", "\\ID");
	///
	/// // запрос по NAME "COUNTRY"
	/// var qs = session.NewQuery("PRODUCT", @"\VEND.CNTRY.NAME = ?", country_name);
	/// var vendor_join = qs.Add("VEND", "VENDOR", "$VENDOR_ID", "\\ID", QS.Constants.JoinType.Left);
	/// var country_join = vendor_join.Add("CNTRY", "COUNTRY", "$COUNTRY_ID", "\\ID");
	/// </code>
	/// </para>
	/// </remarks>
	/// <example>
	/// В качестве логической модели для примеров будет рассматриваться модель иерархического каталога товаров с несколькими справочниками.<br/>
	/// <para>Физическая модель для примеров</para>
	/// 
	/// сущность ПАПКА ПРОДУКТОВ, имеет наименование и ссылку на родительскую папку<br/>
	/// <b>XT_FOLDER</b>
	/// <list type="bullet">
	///	<item>M_ID uniqueidentifier primary key</item>
	///	<item>XM_NAME nvarchar(500)</item>
	///	<item>XM_PID uniqueidentifier</item>
	///	</list>
	///	
	/// сущность ПРОДУКТ<br/>
	/// <b>XT_PRODUCT</b>
	/// <list type="bullet">
	///	<item>M_ID uniqueidentifier primary key</item>
	///	<item>XM_NAME nvarchar(500)</item>
	///	<item>XM_DESCRIPTION ntext</item>
	///	<item>XM_AMOUNT int</item>
	///	<item>XM_PRICE decimal(16, 2)</item>
	///	<item>XM_IMAGE image</item>
	///	<item>XM_VENDOR_ID uniqueidentifier</item>
	///	</list>
	///	
	/// связка м2м ПАПКА ПРОДУКТ<br/>
	/// <b>XT_PRODUCT_FOLDER_MAP</b>
	/// <list type="bullet">
	///	<item>M_ID uniqueidentifier primary key</item>
	///	<item>XM_FOLDER_ID uniqueidentifier</item>
	///	<item>XM_PRODUCT_ID uniqueidentifier</item>
	///	</list>
	///	
	/// словарь ПРОИЗВОДИТЕЛЬ<br/>
	/// <b>XT_VENDOR</b>
	/// <list type="bullet">
	///	<item>M_ID uniqueidentifier primary key</item>
	///	<item>XM_NAME nvarchar(500)</item>
	///	<item>XM_COUNTRY_ID uniqueidentifier -- ссылка на страну производитель</item>
	///	</list>
	///	
	/// словарь СТРАНА<br/>
	/// <b>XT_COUNTRY</b>
	/// <list type="bullet">
	///	<item>M_ID uniqueidentifier primary key</item>
	///	<item>XM_NAME nvarchar(500)</item>
	///	</list>
	///	
	/// 1. Запрос всех продуктов
	/// <code>
	/// // выбрать все продукты без фильтрации и сортировки
	/// var qs = session.NewQuery("PRODUCT");
	/// </code>
	/// 
	/// 2. Запрос продукта по идентификатору
	/// <code>
	/// // выбрать продукт по идентификатору
	/// var qs = session.NewQuery("PRODUCT", "\\id = ?", product_id);
	/// </code>
	/// 
	/// 3. Запрос всех продуктов вместе со справочниками
	/// <code>
	/// // выбрать все продукты без фильтрации и сортировки с присоединенными производителями и странами
	/// var qs = session.NewQuery("PRODUCT");
	/// var vendor_join = qs.Add("VEND", "VENDOR", "$VENDOR_ID", "\\ID");
	/// var country_join = vendor_join.Add("CNTRY", "COUNTRY", "$COUNTRY_ID", "\\ID");
	/// </code>
	/// 
	/// 4. Запрос продуктов с фильтрацией
	/// <code>
	/// // выбрать все продукты с присоединенными производителями и странами
	/// // для которых наименование продукта подобно "%name_filter%", количество > 0, наименование страны равно "country name"
	/// var qs = session.NewQuery("PRODUCT", "$NAME like ? and $AMOUNT > ? and $VEND.CNTRY.NAME = ?", "%name_filter%", 0, "country name");
	/// var vendor_join = qs.Add("VEND", "VENDOR", "$VENDOR_ID", "\\ID", QS.Constants.JoinType.Left);
	/// var country_join = vendor_join.Add("CNTRY", "COUNTRY", "$COUNTRY_ID", "\\ID");
	/// </code>
	/// 
	/// 5. Запрос продуктов с фильтрацией и сортировкой
	/// <code>
	/// // выбрать все продукты с присоединенными производителями и странами
	/// // для которых наименование продукта подобно "%name_filter%", количество > 0, наименование страны равно "country name"
	/// // отсортированные по цене по уменьшению и по наименованию производителя
	/// var qs = session.NewQuery("PRODUCT", "$NAME like ? and $AMOUNT > ? and $VEND.CNTRY.NAME = ?", "%name_filter%", 0, "country name");
	/// var vendor_join = qs.Add("VEND", "VENDOR", "$VENDOR_ID", "\\ID", QS.Constants.JoinType.Left);
	/// var country_join = vendor_join.Add("CNTRY", "COUNTRY", "$COUNTRY_ID", "\\ID");
	/// qs.OrderBy = "$PRICE desc, $VEND.NAME";
	/// </code>
	/// 
	/// 6. Запрос корневых папок
	/// <code>
	/// var qs = session.NewQuery("FOLDER", "$PID is null");
	/// </code>
	/// 
	/// 7. Запрос дочерних папок
	/// <code>
	/// var qs = session.NewQuery("FOLDER", "$PID = ?", parent_id);
	/// </code>
	/// 
	/// 8. Запрос с заданием строкового шаблона select list.
	/// <code>
	/// // выбрать продукты с запросом только NAME, AMOUNT, PRICE
	/// var qs = session.NewQuery("PRODUCT");
	/// qs.SelectBuffer("$NAME, $AMOUNT, $PRICE");
	/// </code>
	/// 
	/// 9. Запрос с select list.
	/// <code>
	/// // выбрать продукты с запросом только NAME и произведения AMOUNT и PRICE
	/// var qs = session.NewQuery("PRODUCT");
	/// qs.SelectItems.Add("NAME");
	/// qs.SelectItems.Add("TOTAL", "isnull($AMOUNT, 0) * isnull($PRICE, 0.0)");
	/// </code>
	/// 
	/// 10. Запрос с аггрегацией.
	/// <code>
	/// // выбрать продукты с запросом максимального произведения AMOUNT и PRICE при условии, что оно больше некой величины limit
	/// var qs = session.NewQuery("PRODUCT");
	/// qs.SelectItems.Add("VENDOR_ID");
	/// qs.SelectItems.Add("TOTAL", "max(isnull($AMOUNT, 0) * isnull($PRICE, 0.0)");
	/// qs.GroupBy = "$VENDOR_ID";
	/// qs.Having = "max(isnull($AMOUNT, 0) * isnull($PRICE, 0.0) > ?";
	/// qs.Parameters.Add(limit);
	/// </code>
	/// 
	/// 11. Запрос с непосредственной командой.
	/// <code>
	/// // выполнить хранимую процедуру
	/// var qs = session.NewQuery();
	/// qs.ImmediateCommand = "EXEC SOME_STORED_PROCEDURE ?, ?, ?";
	/// qs.Parameters.Add(par1);
	/// qs.Parameters.Add(par2);
	/// qs.Parameters.Add(par3);
	/// </code>
	/// </example>
	public class XdeQuery : XdeHierarchyObject
	{
		private int m_commandTimeout = -1;
		private readonly IList m_parameters = new ArrayList();
		private readonly SelectItemCollection m_selectItems = new SelectItemCollection();
		private readonly XdeObjectCollection<XdeJoin> m_joins;
		//private readonly XdeObjectCollection<XdeUnion> m_unions;
		private string m_select = "*";
		//private bool m_unionAll = true;
		private readonly string m_entityName;
		private XdeExecutionOperation m_operation = XdeExecutionOperation.Select;

		#region ctor
		protected XdeQuery(IXdeHierarchyObject owner, string name, string entityName)
			: base(owner, name)
		{
			m_joins = new XdeObjectCollection<XdeJoin>(this);
			//m_unions = new XdeObjectCollection<XdeUnion>(this);
			m_entityName = entityName;
		}

		internal XdeQuery(XdeSession owner, string name, string entityName, string whereClause, params object[] parameters)
			: this(owner, name, entityName)
		{
			this.Where = whereClause;

			Array.ForEach(parameters, x => m_parameters.Add(x));
		}
		#endregion

		#region properties
		/// <summary>
		/// Возвращает или устанавливает наименование сущности, для которой делается запрос или формируется джойн.
		/// </summary>
		public string EntityName
		{
			get { return m_entityName; }
		}

		/// <summary>
		/// Возвращает коллекцию параметров запроса.
		/// </summary>
		public IList Parameters
		{
			get { return m_parameters; }
		}

		/// <summary>
		/// Возвращает коллекцию выражений select list запроса.
		/// </summary>
		/// <remarks>
		/// Данная коллекция используется для задания сложных выражений в select list.
		/// В случае, если количество записей <see cref="SelectItems"/> больше 0 (список не пуст), 
		/// то в определении select list участвует <see cref="SelectItems"/>, а <see cref="Select"/> игнорируется.
		/// Следует помнить, что при работе с <see cref="SelectItems"/> вместо <see cref="Select"/>
		/// необходимо перечислить все необходимые поля - конструкция типа "*" здесь не поддерживается.
		/// </remarks>
		/// <example>
		/// <code>
		/// var query = session.NewQuery("PRODUCT");
		/// query.SelectItems.Add("TOTAL", "ISNULL($AMOUNT, 0) * ISNULL($PRICE, 0.0)");
		/// </code>
		/// </example>
		public SelectItemCollection SelectItems
		{
			get { return m_selectItems; }
		}

		/*
		/// <summary>
		/// Возвращает коллекцию запросов, которые объединяются в итоговом SELECT по UNION или UNION ALL.
		/// </summary>
		public XdeObjectCollection<XdeUnion> Unions
		{
			get { return m_unions; }
		}
		*/

		/// <summary>
		/// Наименование сервера и базы данных.
		/// </summary>
		public string Database
		{
			get;
			set;
		}

		/// <summary>
		/// Выражение, добавляемое к ON.
		/// </summary>
		public string Join
		{
			get;
			set;
		}

		/// <summary>
		/// Команда непосредственного выполнения.
		/// </summary>
		/// <remarks>
		/// Если данное свойство - непустая строка, то оно интерпретируется как SQL выражение и непосредственно выполняется, все другие свойсва,
		/// участвующие в построении запроса, кроме <see cref="Parameters"/>, игнорирутся.
		/// Следеует помнить, что если SQL выражение возвращает какой либо select, то он должен быть Xde-совместимым (<see cref="XdeQuery">Xde compatible resultset.</see>).
		/// </remarks>
		public string ImmediateCommand
		{
			get;
			set;
		}

		/// <summary>
		/// Возвращает или устанавливает комментарий. 
		/// </summary>
		/// <remarks>
		/// Показывается в дампах и/или логах.
		/// </remarks>
		public string Comment
		{
			get;
			set;
		}

		/// <summary>
		/// Возвращает или устанавливает строковое выражение, добавляемое между SELECT и select list.
		/// </summary>
		/// <example>
		/// <code>
		/// XdeQuery query = some initializer(s)
		/// query.SelectHint= "TOP 1";
		/// </code>
		/// эквивалентно SELECT <i>TOP 1</i> ...
		/// </example>
		public string SelectHint
		{
			get;
			set;
		}

		/// <summary>
		/// Строковое представление списка полей в select list. 
		/// </summary>
		/// <remarks>
		/// Для получения всех свойств должно иметь значение "*" - устанавливается по умолчанию, 
		/// для игнорирования всех полей кроме идентификатора и наименования сущности - пустую строку или <b>null</b>.
		/// </remarks>
		/// <seealso cref="SelectItems"/>
		/// <seealso cref="Execute(String)"/>
		public string Select
		{
			get { return m_select; }
			set { m_select = value; }
		}

		public string SelectExclude
		{
			get;
			set;
		}

		/// <summary>
		/// Возвращает или устанавливает выражение WHERE в синтаксисе псевдоимен.
		/// </summary>
		public string Where
		{
			get;
			set;
		}

		/// <summary>
		/// Возвращает или устанавливает выражение ORDER BY в синтаксисе псевдоимен.
		/// </summary>
		public string OrderBy
		{
			get;
			set;
		}

		/// <summary>
		/// Возвращает или устанавливает выражение GROUP BY в синтаксисе псевдоимен.
		/// </summary>
		public string GroupBy
		{
			get;
			set;
		}

		/// <summary>
		/// Возвращает или устанавливает выражение HAVING в синтаксисе псевдоимен.
		/// </summary>
		public string Having
		{
			get;
			set;
		}

		/*
		public bool UnionAll
		{
			get { return m_unionAll; }
			set { m_unionAll = value; }
		}
		*/

		/// <summary>
		/// Возвращает или устанавливает признак формирования запроса вида select count(*) from ....
		/// </summary>
		/// <remarks>
		/// Если включен этот флаг, то количество записей передается через свойство <see cref="XdeUnit.Tag"/>.
		/// </remarks>
		/// <example>
		/// <code>
		/// using ( var query = session.NewQuery("PRODUCT") )
		/// {
		/// 	query.IsCountOnly = true;
		/// 	query.Open();
		/// 	int rowsCount = query.Unit.Tag;
		/// 	// using rows count
		/// }
		/// </code>
		/// </example>
		public bool CountOnly
		{
			get;
			set;
		}

		/// <summary>
		/// Возвращает или устанавливает признак невключения идентификатора в запрос.
		/// </summary>
		/// <remarks>
		/// Используется, например, при формировании запросов с агрегацией. Значением идентификатора <see cref="XdeUnit"/> будет <see cref="Guid.Empty"/>.
		/// </remarks>
		/// <example>
		/// <code>
		/// var query = session.NewQuery("PRODUCT");
		/// query.SelectItems.Add("TOTAL", "sum(isnull($AMOUNT, 0) * isnull($PRICE, 0.0)));
		/// query.EliminateId = true;
		/// </code>
		/// </example>
		public bool EliminateId
		{
			get;
			set;
		}

		public XdeExecutionOperation Operation
		{
			get { return m_operation; }
			set
			{
				if ( value != XdeExecutionOperation.Select && value != XdeExecutionOperation.Delete )
				{
					throw new ArgumentException("Only Select or Delete operations are supported.", "value");
				}

				m_operation = value;
			}
		}

		public int CommandTimeout
		{
			get { return m_commandTimeout; }
			set { m_commandTimeout = value; }
		}
		#endregion

		#region paging
		public const int UnlimitedPageSize = 0;
		private int m_pageSize;
		// Negative values (including zero) mean absolute start offset while positive values are page numbers
		private int m_offset;

		/// <summary>
		/// Возвращает или устанавливает размер страницы.
		/// </summary>
		/// <value>Размер страницы.</value>
		/// <remarks>
		/// Если данное свойство равно <see cref="UnlimitedPageSize"/> то считается, что страница бесконечного размера.
		/// </remarks>
		public int PageSize
		{
			get { return m_pageSize; }
			set
			{
				if ( value < 0 )
				{
					throw new ArgumentOutOfRangeException("value", value, "Must be non-negative integer.");
				}

				m_pageSize = value;
			}
		}
		public int PageNumber
		{
			get { return Math.Max(1, m_offset); }
			set
			{
				if ( value <= 0 )
				{
					throw new ArgumentOutOfRangeException("value", value, "Must be positive integer.");
				}

				m_offset = value;
			}
		}
		public int Offset
		{
			get { return Math.Max(0, -m_offset); }
			set
			{
				if ( value < 0 )
				{
					throw new ArgumentOutOfRangeException("value", value, "Must be non-negative integer.");
				}

				m_offset = -value;
			}
		}
		private int Skipped
		{
			get
			{
				if ( m_offset < 0 )
					return -m_offset;

				if ( m_offset > 0 )
					return (m_offset - 1) * m_pageSize;

				return 0;
			}
		}
		#endregion

		#region methods
		/// <summary>
		/// Добавляет джойн к запросу.
		/// </summary>
		/// <param name="joinName"></param>
		/// <param name="entityName"></param>
		/// <param name="masterKey"></param>
		/// <param name="detailKey"></param>
		public XdeJoin AddJoin(string joinName, string entityName, string masterKey, string detailKey)
		{
			return AddJoin(joinName, entityName, masterKey, detailKey, XdeJoinType.Inner);
		}

		/// <summary>
		/// Добавляет джойн к запросу.
		/// </summary>
		/// <param name="joinName"></param>
		/// <param name="entityName"></param>
		/// <param name="masterKey"></param>
		/// <param name="detailKey"></param>
		/// <param name="joinType"></param>
		public XdeJoin AddJoin(string joinName, string entityName, string masterKey, string detailKey, XdeJoinType joinType)
		{
			XdeJoin join = new XdeJoin(this, joinName, entityName, KeyOf(masterKey), KeyOf(detailKey));

			join.JoinType = joinType;

			m_joins.Add(join);

			return join;
		}

		private ISet<string> GetSelectExcludeItems()
		{
			ISet<string> items = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

			if ( !String.IsNullOrWhiteSpace(this.SelectExclude) )
			{
				foreach ( string columnName in this.SelectExclude.Split(',') )
				{
					string buffer = columnName.Trim();

					if ( buffer.Length != 0 )
					{
						if ( buffer[0] == '$' )
						{
							buffer = buffer.Substring(1);
						}

						items.Add(buffer);
					}
				}
			}

			return items;
		}

		/// <summary>
		/// For internal using only.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public ICollection<SelectItem> GetFinalSelectItems()
		{
			if ( m_selectItems.Count != 0 )
				return new ReadOnlyCollection<SelectItem>(m_selectItems);

			List<SelectItem> selectList = new List<SelectItem>();

			if ( !String.IsNullOrWhiteSpace(m_select) )
			{
				var excludeList = GetSelectExcludeItems();

				if ( m_select.Trim() == "*" )
				{
					foreach ( XdeEntityColumn field in this.GetSession().Entities[this.EntityName].Columns.Where(x => !excludeList.Contains(x.Name)) )
					{
						selectList.Add(new SelectItem() { Alias = field.Name });
					}
				}
				else
				{
					foreach ( string columnName in m_select.Split(',') )
					{
						string buffer = columnName.Trim();

						if ( buffer.Length != 0 )
						{
							if ( buffer[0] == '$' )
							{
								buffer = buffer.Substring(1);
							}

							if ( !excludeList.Contains(buffer) )
							{
								selectList.Add(new SelectItem() { Alias = buffer });
							}
						}
					}
				}
			}

			return selectList.AsReadOnly();
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public IEnumerable<string> GetFieldsListBySelect(bool eliminateId)
		{
			ICollection<string> rc = new List<string>();

			if ( !eliminateId )
			{
				rc.Add(@"\id");
			}

			if ( !String.IsNullOrWhiteSpace(m_select) )
			{
				if ( m_select.Trim() == "*" )
				{
					foreach ( XdeEntityColumn field in this.GetSession().Entities[this.EntityName].Columns.Where(x => !String.IsNullOrEmpty(x.Name)) )
					{
						rc.Add("$" + field.Name);
					}
				}
				else
				{
					foreach ( string s in m_select.Split(',') )
					{
						string column = s.Trim();

						if ( column.StartsWith("$") )
						{
							rc.Add(column);
						}
					}
				}
			}

			return rc;
		}

		protected override void ProcessDump(StringBuilder buffer, string prefix)
		{
			base.ProcessDump(buffer, prefix);

			XdeDump.ProcessDumpItems(m_joins, buffer, prefix, "Joins");
		}
		#endregion

		#region joins support
		public XdeJoin this[string joinName]
		{
			get { return m_joins.FirstOrDefault(join => join.Name.Equals(joinName, StringComparison.InvariantCultureIgnoreCase)); }
		}

		public virtual XdeQueryMode Mode
		{
			get { return XdeQueryMode.Query; }
		}

		public XdeQuery RootQuery
		{
			get
			{
				if ( this.Mode == XdeQueryMode.Query )
				{
					return this;
				}

				for ( IXdeHierarchyObject parentItem = this.Owner; parentItem != null; parentItem = parentItem.Owner )
				{
					XdeQuery parentQuery = parentItem as XdeQuery;

					if ( parentQuery != null && parentQuery.Mode == XdeQueryMode.Query )
					{
						return parentQuery;
					}
				}

				return null;
			}
		}

#if false // Not used
		public string GetFieldPrefix()
		{
			return AppendFieldPrefix(new StringBuilder()).ToString();
		}

		protected internal virtual StringBuilder AppendFieldPrefix(StringBuilder buffer)
		{
			return buffer;
		}

		public string GetPrefixT()
		{
		    return AppendPrefixT(new StringBuilder()).ToString();
		}
#endif

		protected internal virtual StringBuilder AppendPrefixT(StringBuilder buffer)
		{
			return buffer.Append('T');
		}

		public string GetPrefixTT()
		{
			return AppendPrefixT(new StringBuilder("T")).ToString();
		}

		private static string KeyOf(string value)
		{
			if ( String.IsNullOrEmpty(value) )
				return null;

			value = value.Trim();

			if ( value.Length == 0 )
				return null;

			if ( value[0] != '\\' && value[0] != '$' )
				value = '$' + value;

			return value;
		}
		#endregion

		#region command build
		public override IEnumerable<XdeCommand> GetCommand()
		{
			yield return BuildCommand();
		}

		private XdeCommand BuildCommand()
		{
			if ( this.ImmediateCommand != null )
			{
				StringBuilder immediateCommand = new StringBuilder(this.ImmediateCommand);
				IList immediateCommandPararameters = XdeUtil.ExpandCommandTextAndParameters(immediateCommand, m_parameters);

				return new XdeCommand(immediateCommand.ToString(), immediateCommandPararameters) { CommandTimeout = m_commandTimeout };
			}

			XdeQueryBuildData buildData = new XdeQueryBuildData();

			ProcessBuildQuery(buildData);

			IList parameters = XdeUtil.ExpandCommandTextAndParameters(buildData.WhereBuffer, m_parameters);

			if ( buildData.WhereBuffer.Length > 0 )
				buildData.WhereBuffer.Insert(0, "\r\nWHERE\r\n\t");

			if ( buildData.GroupByBuffer.Length > 0 )
				buildData.GroupByBuffer.Insert(0, "\r\nGROUP BY\r\n\t");

			if ( buildData.HavingBuffer.Length > 0 )
				buildData.HavingBuffer.Insert(0, "\r\nHAVING\r\n\t");

			if ( buildData.OrderByBuffer.Length > 0 )
				buildData.OrderByBuffer.Insert(0, "\r\nORDER BY\r\n\t");

			/*if (Unions.Count > 0)
			{
				foreach (XdeQuery queryUnion in Unions)
				{
					XdeCommand unionCommand = queryUnion.GetCommand(null, XdeCommand.Operations.Select, new BuildData(true, false));
					buildData.UnionBuffer.Append(queryUnion.UnionAll ? "\nUNION ALL\n" : "\nUNION\n").Append(unionCommand[0].CommandText);
					foreach (object unionParameter in unionCommand[0].Parameters)
						pararameters.Add(unionParameter);
				}
			}*/

			StringBuilder buffer = new StringBuilder();

			if ( m_operation == XdeExecutionOperation.Select )
			{
				buffer.Append(Environment.NewLine).Append(Environment.NewLine)
					.Append("SELECT ").Append(this.SelectHint).Append(' ').Append(buildData.SelectBuffer.ToString())
					.Append(buildData.FromBuffer.ToString())
					.Append(buildData.WhereBuffer.ToString())
					.Append(buildData.GroupByBuffer.ToString())
					.Append(buildData.HavingBuffer.ToString())
					//.Append(buildData.UnionBuffer.ToString())
					.Append(buildData.OrderByBuffer).Append(Environment.NewLine);
			}
			else if ( m_operation == XdeExecutionOperation.Delete )
			{
				buffer.Append("\r\n\r\nDELETE FROM ").Append(this.GetSession().Layer.GetQualifiedTableName(this.Database, this.EntityName)).Append(Environment.NewLine)
					.Append(buildData.FromBuffer.ToString()).Append(Environment.NewLine)
					.Append(buildData.WhereBuffer.ToString()).Append(Environment.NewLine);
			}

			return new XdeCommand(Replace(buffer), parameters) { CommandTimeout = m_commandTimeout };
		}

		private void ProcessBuildQuery(XdeQueryBuildData buildData)
		{
			IXdeLayer layer = this.GetSession().Layer;

			layer.ProcessBuildQuerySelectList(this, buildData);
			layer.ProcessBuildQueryClauses(this, buildData);
			layer.ProcessBuildQueryFromClause(this, buildData);

			if ( m_joins.Count > 0 )
			{
				buildData.SelectBuffer.Append(",\r\n\t").Append(XdeQueryTags.Header.Joins).Append(", ").Append(m_joins.Count);

				foreach ( var query in m_joins )
				{
					query.ProcessBuildQuery(buildData);
				}
			}

			if ( !String.IsNullOrWhiteSpace(this.Join) )
			{
				buildData.FromBuffer.Append(' ').Append(this.Join).Append(Environment.NewLine);
			}
		}

		private static string Replace(StringBuilder source)
		{
			if ( source.Length == 0 )
				return String.Empty;

			StringBuilder buffer = new StringBuilder();
			int length = source.Length;
			int startIndex = -1;
			char startChar = '\0';
			char currentChar = '\0';
			int[] dotIndexes = new int[20];
			int dotIndex = 0;
			for ( int charIndex = 0; charIndex <= length; charIndex++ )
			{
				if ( charIndex < length )
				{
					currentChar = source[charIndex];
				}
				if ( startIndex >= 0 )
				{
					// Мы находимся в режиме набора идентификатора
					if ( charIndex < length )
					{
						// [A-Za-z0-9] or [_]
						if ( char.IsLetterOrDigit(currentChar) || currentChar == '_' )
							continue;
						if ( currentChar == '.' )
						{
							dotIndexes[dotIndex++] = charIndex;
							continue;
						}
					}
					// В текущий момент мы имеем потенциально заполненный массив
					// позиций точек и позицию последнего символа.
					buffer.Append("TT");
					// Сначала пытаемся разобраться с алиасами
					if ( dotIndex > 0 )
					{
						// Их есть у нас...
						for ( int i = 0; i < dotIndex; ++i )
						{
							buffer.Append('_');
							buffer.Append(source.ToString(startIndex, dotIndexes[i] - startIndex));
							startIndex = dotIndexes[i] + 1;
						}
					}
					buffer.Append('.');
					// Если последовательность началась с символа '$', то префикс будет "XM_", иначе "M_".
					if ( startChar == '$' )
						buffer.Append("XM_");
					else
						buffer.Append("M_");
					buffer.Append(source.ToString(startIndex, charIndex - startIndex));
					startIndex = -1;
				}
				if ( charIndex >= length )
					break;
				// Встретился спецсимвол
				if ( currentChar == '$' || currentChar == '\\' )
				{
					// Если в исходной строке два одинаковых спецсимвола идут подряд,
					// то они не считаются спецсимволами а заменяются на одиночный символ.
					// Пример: "\\" -> "\", "$$" -> "$".
					if ( charIndex < length - 1 && source[charIndex + 1] == currentChar )
					{
						buffer.Append(currentChar);
						charIndex++;
						continue;
					}
					// Это всё-таки спецсимвол. Запоминаем его и позицию, с которой
					// начинался разбор.
					startChar = currentChar;
					startIndex = charIndex + 1; // реально начинается со следующего символа
					dotIndex = 0;
				}
				else
				{
					// Добавляем символ к результирующей строке
					buffer.Append(currentChar);
				}
			}
			return buffer.ToString();
		}
		#endregion

		#region Execute
		public bool IsExists()
		{
			return IsExists(null);
		}

		public bool IsExists(IDbTransaction outerTransaction)
		{
			return ExecuteSingle(outerTransaction) != null;
		}

		private void CheckSelectOperation()
		{
			if ( m_operation != XdeExecutionOperation.Select )
				throw new InvalidOperationException(String.Format("Xde query operation {0} is incompatible with Execute or Open methods.", m_operation));
		}

		private XdeUnitCollection ProcessExecute(XdeCommand command, IDbConnection connection, IDbTransaction outerTransaction)
		{
			DateTime startDate = DateTime.Now;

			XdeSession session = this.GetSession();
			XdeUnitCollection units = session.NewUnits();

			try
			{
				using ( IDbCommand dbCommand = session.Adapter.GetCommand(connection, command) )
				{
					if ( outerTransaction != null )
						dbCommand.Transaction = outerTransaction;

					if ( command.CommandTimeout >= 0 )
						dbCommand.CommandTimeout = command.CommandTimeout;
					else if ( m_commandTimeout >= 0 )
						dbCommand.CommandTimeout = m_commandTimeout;
					else if ( session.CommandTimeout >= 0 )
						dbCommand.CommandTimeout = session.CommandTimeout;

					using ( IDataReader dataReader = dbCommand.ExecuteReader() )
					{
						bool endOfReader = false;
						int skipped = this.Skipped;

						if ( skipped > 0 )
						{
							for ( int i = 0; i < skipped; i++ )
							{
								if ( !dataReader.Read() )
								{
									endOfReader = true;
									break;
								}
							}
						}

						TimeSpan openTime = DateTime.Now - startDate;

						if ( !endOfReader )
						{
							if ( m_pageSize > UnlimitedPageSize )
							{
								for ( int counter = 0; counter < m_pageSize && dataReader.Read(); counter++ )
								{
									units.Add(new XdeUnit(session, dataReader));
								}
							}
							else
							{
								while ( dataReader.Read() )
								{
									units.Add(new XdeUnit(session, dataReader));
								}
							}
						}

						dbCommand.Cancel();

						TimeSpan processTime = DateTime.Now - startDate;

						if ( XdeDump.Action )
							XdeDump.Write("I: query <{0}> started at {1} and processed in {2} ({3} + {4}) ms.", this.Comment, startDate, Math.Round(processTime.TotalMilliseconds), Math.Round(openTime.TotalMilliseconds), Math.Round((processTime - openTime).TotalMilliseconds));
					}
				}
			}
			catch
			{
				if ( XdeDump.Error )
				{
					XdeDump.Writer.WriteLine(command.ToString());
					XdeDump.Writer.Flush();
				}

				if ( XdeDump.Action )
					XdeDump.Write("E: query <{0}> started at {1} and failed.", this.Comment, startDate);

				throw;
			}

			return units;
		}

		public XdeUnitCollection Execute()
		{
			return Execute((IDbTransaction) null);
		}

		public XdeUnitCollection Execute(IDbTransaction outerTransaction)
		{
			CheckSelectOperation();
			XdeCommand command = BuildCommand();
			command.Dump();
			if ( outerTransaction != null )
			{
				return ProcessExecute(command, outerTransaction.Connection, outerTransaction);
			}
			else
			{
				XdeSession session = this.GetSession();

				using ( IDbConnection dbConnection = session.Adapter.GetConnection(session.Registration.ConnectionString) )
				{
					dbConnection.Open();
					return ProcessExecute(command, dbConnection, null);
				}
			}
		}

		public XdeUnitCollection Execute(string select)
		{
			return Execute(select, null);
		}

		public XdeUnitCollection Execute(string select, IDbTransaction outerTransaction)
		{
			string ownSelect = m_select;
			try
			{
				m_select = select;
				return Execute(outerTransaction);
			}
			finally
			{
				m_select = ownSelect;
			}
		}

		private XdeUnit ProcessExecuteSingle(XdeCommand command, IDbConnection connection, IDbTransaction outerTransaction)
		{
			DateTime startDate = DateTime.Now;

			XdeSession session = this.GetSession();
			XdeUnit unit = null;

			try
			{
				using ( IDbCommand dbCommand = session.Adapter.GetCommand(connection, command) )
				{
					if ( outerTransaction != null )
						dbCommand.Transaction = outerTransaction;

					if ( command.CommandTimeout >= 0 )
						dbCommand.CommandTimeout = command.CommandTimeout;
					else if ( m_commandTimeout >= 0 )
						dbCommand.CommandTimeout = m_commandTimeout;
					else if ( session.CommandTimeout >= 0 )
						dbCommand.CommandTimeout = session.CommandTimeout;

					using ( IDataReader dataReader = dbCommand.ExecuteReader() )
					{
						if ( dataReader.Read() )
						{
							unit = new XdeUnit(session, dataReader);
						}

						dbCommand.Cancel();
					}
				}

				TimeSpan processTime = DateTime.Now - startDate;

				if ( XdeDump.Action )
					XdeDump.Write("I: query <{0}> started at {1} and processed in {2} ms.", this.Comment, startDate, Math.Round(processTime.TotalMilliseconds));
			}
			catch
			{
				if ( XdeDump.Error )
				{
					XdeDump.Writer.WriteLine(command.ToString());
				}

				if ( XdeDump.Action )
					XdeDump.Write("E: query <{0}> started at {1} and failed.", this.Comment, startDate);

				throw;
			}

			return unit;
		}

		public XdeUnit ExecuteSingle()
		{
			return ExecuteSingle((IDbTransaction) null);
		}

		public XdeUnit ExecuteSingle(IDbTransaction outerTransaction)
		{
			CheckSelectOperation();
			string ownSelectHint = this.SelectHint;
			this.SelectHint = "TOP 1";
			XdeCommand command = BuildCommand();
			this.SelectHint = ownSelectHint;
			command.Dump();
			if ( outerTransaction != null )
			{
				return ProcessExecuteSingle(command, outerTransaction.Connection, outerTransaction);
			}
			else
			{
				XdeSession session = this.GetSession();

				using ( IDbConnection dbConnection = session.Adapter.GetConnection(session.Registration.ConnectionString) )
				{
					dbConnection.Open();
					return ProcessExecuteSingle(command, dbConnection, null);
				}
			}
		}

		public XdeUnit ExecuteSingle(string select)
		{
			return ExecuteSingle(select, null);
		}

		public XdeUnit ExecuteSingle(string select, IDbTransaction outerTransaction)
		{
			string ownSelect = m_select;
			try
			{
				m_select = select;
				return ExecuteSingle(outerTransaction);
			}
			finally
			{
				m_select = ownSelect;
			}
		}

		public XdeQueryResultSet Open()
		{
			CheckSelectOperation();

			XdeCommand command = BuildCommand();

			command.Dump();

			XdeSession session = this.GetSession();
			XdeQueryResultSet context = new XdeQueryResultSet(session, m_pageSize);

			try
			{
				context.StartDate = DateTime.Now;

#if false
				if ( outerTransaction != null )
				{
					context.DbCommand = session.Adapter.GetCommand(outerTransaction.Connection, command);
					context.DbCommand.Transaction = outerTransaction;
				}
				else
#endif
				{
					context.DbConnection = session.Adapter.GetConnection(session.Registration.ConnectionString);
					context.DbConnection.Open();
					context.DbCommand = session.Adapter.GetCommand(context.DbConnection, command);
				}

				if ( command.CommandTimeout >= 0 )
					context.DbCommand.CommandTimeout = command.CommandTimeout;
				else if ( m_commandTimeout >= 0 )
					context.DbCommand.CommandTimeout = m_commandTimeout;
				else if ( session.CommandTimeout >= 0 )
					context.DbCommand.CommandTimeout = session.CommandTimeout;

				context.DataReader = context.DbCommand.ExecuteReader();
				context.OpenTime = DateTime.Now - context.StartDate;

				if ( XdeDump.Action )
					XdeDump.Write("I: query <{0}> openened at {1}.", this.Comment, context.StartDate);

				int skipped = this.Skipped;

				if ( skipped > 0 )
				{
					for ( int index = 0; index < skipped; index++ )
					{
						if ( !context.DataReader.Read() )
						{
							break;
						}
					}
				}
			}
			catch
			{
				context.Close();

				throw;
			}

			return context;
		}
		public XdeQueryResultSet Open(string select)
		{
			string ownSelect = m_select;
			try
			{
				m_select = select;
				return Open();
			}
			finally
			{
				m_select = ownSelect;
			}
		}
		#endregion

		#region class SelectItem
		/// <summary>
		/// Описатель поля в запросе <see cref="XdeQuery.SelectItems"/>.
		/// </summary>
		public sealed class SelectItem
		{
			internal SelectItem()
			{
			}

			public string Alias
			{
				get;
				internal set;
			}

			public string Expression
			{
				get;
				internal set;
			}
		}
		#endregion

		#region class SelectItemCollection
		public sealed class SelectItemCollection : Collection<SelectItem>
		{
			internal SelectItemCollection()
			{
			}

			public void Add(string alias, string expression)
			{
				Add(new SelectItem() { Alias = alias, Expression = expression });
			}

			public void Add(string field)
			{
				if ( !String.IsNullOrEmpty(field) )
				{
					if ( field[0] == '$' )
						Add(field.Substring(1), null);
					else
						Add(field, null);
				}
			}
		}
		#endregion
	}

	#region enum XdeQueryMode
	/// <summary>
	/// Тип запроса.
	/// </summary>
	public enum XdeQueryMode
	{
		/// <summary>
		/// Основной запрос.
		/// </summary>
		Query,

		/// <summary>
		/// Объединенный запрос.
		/// </summary>
		Union,

		/// <summary>
		/// Джойн к запросу.
		/// </summary>
		Join,
	}
	#endregion

	#region enum XdeJoinType
	public enum XdeJoinType
	{
		Inner,
		Left,
		Right,
		Full
	}
	#endregion

	#region class XdeJoin
	public class XdeJoin : XdeQuery
	{
		private readonly string m_masterKey;
		private readonly string m_detailKey;

		public XdeJoin(XdeQuery owner, string name, string entityName, string masterKey, string detailKey)
			: base(owner, name, entityName)
		{
			m_masterKey = masterKey;
			m_detailKey = detailKey;
		}

		#region Properties
		public string DetailKey
		{
			get { return m_detailKey; }
		}

		public string MasterKey
		{
			get { return m_masterKey; }
		}

		public XdeJoinType JoinType
		{
			get;
			set;
		}

		public sealed override XdeQueryMode Mode
		{
			get { return XdeQueryMode.Join; }
		}
		#endregion

		public override IEnumerable<XdeCommand> GetCommand()
		{
			throw new NotSupportedException("XdeJoin can't be represented as a standalone command.");
		}

#if false // Not used
		protected internal override StringBuilder AppendFieldPrefix(StringBuilder buffer)
		{
			XdeQuery parentQuery = this.Owner as XdeQuery;

			if ( parentQuery == null )
			{
				throw new InvalidOperationException("Xde.Query.AppendFieldPrefix failed due : null ParentQuery");
			}

			return parentQuery.AppendFieldPrefix(buffer).Append(this.Name).Append('.');
		}
#endif

		protected internal override StringBuilder AppendPrefixT(StringBuilder buffer)
		{
			XdeQuery parentQuery = this.Owner as XdeQuery;

			if ( parentQuery == null )
			{
				throw new InvalidOperationException("Xde.Query.AppendPrefixT failed due : null ParentQuery");
			}

			return parentQuery.AppendPrefixT(buffer).Append('_').Append(this.Name);
		}
	}
	#endregion
}
