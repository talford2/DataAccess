using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using Dataway.DataAccess.Conditions;
using Dataway.DataAccess.Extensions;
using System.Linq;

namespace Dataway.DataAccess
{
    /// <summary>
    /// Construct an SQL query using objects.
    /// </summary>
    public class SearchQuery
    {
        #region Private Members

        private TableCollection fromTables;
        private FieldCollection selectedFields;
        private SortFieldCollection orderBy;
        private FieldCollection groupBy;
        private bool isSelectAll;
        private int? top;
        private ICondition condition;
        private ICondition having;
        private bool ignoreEmptyConditionValue;
        private JoinCollection joins;

        #endregion

        #region Constructors

        /// <summary>
        /// Construct a new SearchQuery.
        /// </summary>
        public SearchQuery() { }

        /// <summary>
        /// Construct a new SearchQuery parsing an SQL query to populate objects.
        /// </summary>
        /// <param name="query">SQL query to parse.</param>
        public SearchQuery(string query)
        {
            query = this.GetCulledComments(query);
            query = query.Replace(Environment.NewLine, " ").Replace("\t", " ");

            QueryMode currentMode = QueryMode.Select;

            string selectSection = "";
            string fromSection = "";
            string joinSection = "";
            string orderBySection = "";
            string whereSection = "";
            string groupBySection = "";
            string intoSection = "";

            int nestingDepth = 0;

            var words = query.Split(" ");

            foreach (string word in words)
            {
                nestingDepth += word.GetCharCount('(');
                nestingDepth -= word.GetCharCount(')');
                if (word.Contains("("))
                {
                    //    nestingDepth++;
                }
                if (word.Contains(")"))
                {
                    //    nestingDepth--;
                }

                QueryMode? mode = null;

                if (nestingDepth == 0)
                {
                    mode = this.GetModeFromWord(word);
                }
                string val = string.Format("{0} ", word);

                if (mode.HasValue)
                {
                    currentMode = mode.Value;
                    val = "";
                }

                switch (word.ToLower())
                {
                    case "inner":
                        val = "INNER JOIN ";
                        break;
                    case "left":
                        val = "LEFT OUTER JOIN ";
                        break;
                }

                switch (currentMode)
                {
                    case QueryMode.Select:
                        selectSection += val;
                        break;
                    case QueryMode.Into:
                        intoSection += val;
                        break;
                    case QueryMode.From:
                        fromSection += val;
                        break;
                    case QueryMode.Join:
                        joinSection += val;
                        break;
                    case QueryMode.Where:
                        whereSection += val;
                        break;
                    case QueryMode.OrderBy:
                        orderBySection += val;
                        break;
                    case QueryMode.GroupBy:
                        groupBySection += val;
                        break;
                }
            }


            if (selectSection.ToLower().TrimStart().StartsWith("top "))
            {
                string topVal = selectSection.ToLower().Replace("top ", null).Trim().Split(' ')[0];
                this.Top = int.Parse(topVal);
                selectSection = selectSection.Replace(string.Format("{0} {1}", selectSection.TrimStart().Substring(0, 3), this.Top), null);
            }

            if (selectSection != null && selectSection.Trim() == "*")
            {
                this.IsSelectAll = true;
            }
            else
            {
                this.Select = new FieldCollection(selectSection.Trim());
            }

            this.Into = intoSection.Trim();
            this.fromTables = new TableCollection(fromSection.Trim());

            this.condition = new CustomCondition(whereSection.Trim());
            //this.Condition
            this.OrderBy = new SortFieldCollection(orderBySection.Trim());
            this.groupBy = new FieldCollection(groupBySection.Trim());
            this.Joins = new JoinCollection(joinSection.Trim());
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the tables that represent the FROM in a SQL SELECT query.
        /// </summary>
        public TableCollection From
        {
            get
            {
                if (this.fromTables == null)
                {
                    this.fromTables = new TableCollection();
                }
                return this.fromTables;
            }
            set { this.fromTables = value; }
        }

        /// <summary>
        /// Gets or sets the fields to select in an SQL query.
        /// </summary>
        public FieldCollection Select
        {
            get
            {
                if (this.selectedFields == null)
                {
                    this.selectedFields = new FieldCollection();
                }
                return this.selectedFields;
            }
            set { this.selectedFields = value; }
        }

        public SortFieldCollection OrderBy
        {
            get
            {
                if (this.orderBy == null)
                {
                    this.orderBy = new SortFieldCollection();
                }
                return this.orderBy;
            }
            set { this.orderBy = value; }
        }

        public FieldCollection GroupBy
        {
            get
            {
                if (this.groupBy == null)
                {
                    this.groupBy = new FieldCollection();
                }
                return this.groupBy;
            }
            set { this.groupBy = value; }
        }

        public bool IsSelectAll
        {
            get { return this.isSelectAll; }
            set { this.isSelectAll = value; }
        }

        public int? Top
        {
            get { return this.top; }
            set { this.top = value; }
        }

        public ICondition Where
        {
            get
            {
                return this.condition;
            }
            set { this.condition = value; }
        }

        public ICondition Having
        {
            get
            {
                return this.having;
            }
            set
            {
                this.having = value;
            }
        }

        public bool IgnoreEmptyConditionValue
        {
            get { return this.ignoreEmptyConditionValue; }
            set { this.ignoreEmptyConditionValue = value; }
        }

        public JoinCollection Joins
        {
            get
            {
                if (this.joins == null)
                {
                    this.joins = new JoinCollection();
                }
                return this.joins;
            }
            set { this.joins = value; }
        }

        public string Into { get; set; }

        #endregion

        #region Public Methods

        public string ToString(bool includeOrderBy)
        {
            if (!this.From.Any())
            {
                throw new NotSupportedException("No from tables specified");
            }
            if (!this.IsSelectAll && !this.Select.Any())
            {
                throw new NotSupportedException("No select fields specified.");
            }

            StringBuilder sb = new StringBuilder();

            //Select
            sb.Append("SELECT ");
            if (this.top.HasValue)
            {
                sb.Append(string.Format("TOP {0} ", this.top.Value));
            }
            if (this.isSelectAll)
            {
                sb.Append("* ");
            }
            else
            {
                sb.Append(this.GetFieldListString(this.selectedFields, true));
                sb.Append(" ");
            }

            //Into table
            if (!string.IsNullOrEmpty(this.Into))
            {
                sb.Append(string.Format("INTO #{0} ", this.Into.Replace("#", "")));
            }

            //From
            sb.Append("FROM ");
            sb.Append(this.GetTableListString(this.From));

            //Join
            if (this.Joins.Any())
            {
                sb.Append(" ");
                sb.Append(this.GetJoinString(this.Joins));
            }

            //Where
            if (this.Where != null)
            {
                string conditionStr = this.Where.ToString(this.ignoreEmptyConditionValue);
                if (!string.IsNullOrEmpty(conditionStr))
                {
                    sb.Append(" ");
                    sb.Append("WHERE ");
                    sb.Append(conditionStr);
                }
            }

            //Group by
            if (this.GroupBy.Any())
            {
                sb.Append(" ");
                sb.Append("GROUP BY ");
                sb.Append(this.GetFieldListString(this.GroupBy, false));
            }

            //Having
            if (this.Having != null)
            {
                sb.Append(" ");
                sb.Append("HAVING ");
                sb.Append(this.Having.ToString(this.ignoreEmptyConditionValue));
            }

            if (includeOrderBy)
            {
                //Order by
                if (this.OrderBy.Any())
                {
                    sb.Append(" ");
                    sb.Append("ORDER BY ");
                    sb.Append(this.GetSortFieldString(this.OrderBy));
                }
            }

            return sb.ToString();
        }

        override public string ToString()
        {
            return this.ToString(true);
        }

        public string GetCountString()
        {
            return this.GetCountString(null);
        }

        public string GetCountString(string countInput)
        {
            if (string.IsNullOrEmpty(countInput))
            {
                countInput = "*";
            }

            if (groupBy.Any())
            {
                return string.Format(string.Format("SELECT COUNT({0}) FROM ({1}) as count", countInput, this.ToString(false)));
            }

            string fromStr = this.GetTableListString(this.From);
            string whereStr = "";
            string joinStr = "";

            if (this.Joins.Any())
            {
                joinStr = string.Format(" {0}", this.GetJoinString(this.Joins));
            }

            if (this.Where != null)
            {
                whereStr = string.Format(" WHERE {0}", this.Where.ToString(true));
            }

            return string.Format("SELECT COUNT({0}) as count FROM {1}{2}{3}", countInput, fromStr, joinStr, whereStr);
        }

        public string GetPaginationString(int rowsPerPage, int pageIndex)
        {
            return this.GetPaginationString(rowsPerPage, pageIndex, false);
        }

        public string GetPaginationString(int rowsPerPage, int pageIndex, bool withTotalCount)
        {
            if (this.GroupBy.Any())
            {
                return this.GroupGetPaginatedString(rowsPerPage, pageIndex, withTotalCount);
            }

            int startIndex = pageIndex * rowsPerPage;
            string format = "SELECT TOP {0} * FROM (SELECT ROW_NUMBER() OVER (ORDER BY {2}) AS RowNumber, {1} FROM {3}{4}{5}) AS a WHERE RowNumber > {6}";

            string selectStr = "";
            string whereStr = "";

            if (this.Where != null)
            {
                whereStr = " WHERE " + this.Where.ToString(true);
            }

            if (this.IsSelectAll)
            {
                selectStr = "*";
            }
            else
            {
                selectStr = this.GetFieldListString(this.Select, false);
            }

            if (!this.OrderBy.Any())
            {
                throw new NotSupportedException("OrderBy fields required for pagination.");
            }

            string joinStr = "";

            if (this.Joins.Any())
            {
                StringBuilder sbJoins = new StringBuilder(" ");
                foreach (JoinTable join in this.Joins)
                {
                    sbJoins.AppendFormat("{0} ", join.ToString());
                }
                joinStr = sbJoins.ToString();
            }

            string sss = string.Format(
                    format,
                    rowsPerPage,
                    selectStr,
                    this.GetSortFieldString(this.OrderBy),
                    this.GetTableListString(this.From),
                    joinStr,
                    whereStr,
                    startIndex
                    );

            if (withTotalCount)
            {
                sss += " " + this.GetCountString(null);
            }

            return sss;
        }

        public string GetPaginationString(int rowsPerPage, int pageIndex, bool withTotalCount, Field pagingGroupByField)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("SELECT * FROM (");
            sb.Append(this.ToString(false));
            sb.Append(") AS results WHERE ");
            sb.Append(pagingGroupByField.FieldName);
            sb.Append(" IN (");
            sb.Append(this.GetPaginationGroupByString(rowsPerPage, pageIndex, pagingGroupByField.FieldName));
            sb.Append(")");

            sb.Append(string.Format(" ORDER BY {0}", this.GetSortFieldString(this.OrderBy, false)));

            if (withTotalCount)
            {
                sb.Append(string.Format(" {0}", this.GetCountString(string.Format("DISTINCT {0}", pagingGroupByField))));
            }

            return sb.ToString();
        }

        public bool IsComplete()
        {
            bool isComplete = true;
            if (this.Select == null || !this.Select.Any())
            {
                isComplete = false;
            }
            if (this.From == null || !this.From.Any())
            {
                isComplete = false;
            }
            return isComplete;
        }

        #endregion

        #region Public Static Methods

        public static SearchQuery Parse(string query)
        {
            return new SearchQuery(query);
        }

        public static SearchQuery Build(string select, string from, string where, string orderBy)
        {
            SearchQuery sq = new SearchQuery();
            sq.Select = new FieldCollection(select);
            sq.From = new TableCollection(from);
            sq.Where = new CustomCondition(where);
            sq.OrderBy = new SortFieldCollection(orderBy);
            return sq;
        }

        #endregion

        #region Private Static Methods

        private static string FindString(string value, string[] strings)
        {
            for (int i = 0; i < strings.Length; i++)
            {
                string stringPart = strings[i];
                if (value.Contains(stringPart))
                {
                    return stringPart;
                }
            }
            return null;
        }

        private string GetFieldListString(FieldCollection fields, bool withAliases)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < fields.Count; i++)
            {
                if (withAliases)
                {
                    sb.Append(fields[i].ToSelectString());
                }
                else
                {
                    sb.Append(fields[i].ToString());
                }
                if (i < fields.Count - 1)
                {
                    sb.Append(", ");
                }
            }

            return sb.ToString();
        }

        private string GetSortFieldString(SortFieldCollection sorts)
        {
            return this.GetSortFieldString(sorts, true);
        }

        private string GetSortFieldString(SortFieldCollection sorts, bool withTables)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < sorts.Count; i++)
            {
                sb.Append(sorts[i].ToString(withTables));

                if (i < sorts.Count - 1)
                {
                    sb.Append(", ");
                }
            }

            return sb.ToString();
        }

        private string GetTableListString(TableCollection tables)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < tables.Count; i++)
            {
                sb.Append(tables[i].ToString(true));
                if (i < tables.Count - 1)
                {
                    sb.Append(", ");
                }
            }

            return sb.ToString();
        }

        private string GetJoinString(List<JoinTable> joins)
        {
            StringBuilder sb = new StringBuilder();

            foreach (JoinTable join in this.Joins)
            {
                sb.AppendFormat("{0} ", join.ToString());
            }

            return sb.ToString();
        }

        private string GroupGetPaginatedString(int rowsPerPage, int pageIndex, bool withTotalCount)
        {
            if (this.OrderBy == null || !this.OrderBy.Any())
            {
                throw new NotSupportedException("Pagination requires order by columns.");
            }
            if (rowsPerPage <= 0)
            {
                throw new NotSupportedException("RowsPerPage cannot be less than 1.");
            }

            StringBuilder sb = new StringBuilder();

            string tempTableName = string.Format("#paginationTempTable{0}", DateTime.Now.Ticks);

            sb.Append(this.ToString(true));
            sb.AppendFormat(" SELECT TOP {0} * FROM ", rowsPerPage);
            sb.Append("(");
            sb.AppendFormat("SELECT ROW_NUMBER() OVER (ORDER BY {0}) AS RowNumber, * FROM {1}", this.GetSortFieldString(this.OrderBy), tempTableName);
            sb.AppendFormat(") r WHERE RowNumber > ({0} * {1} )", pageIndex, rowsPerPage);

            sb.AppendFormat(" DROP TABLE {0}", tempTableName);

            if (withTotalCount)
            {
                sb.Append(" " + this.GetCountString(null));
            }

            return sb.ToString();
        }

        private string GetPaginationGroupByString(int rowsPerPage, int pageIndex, string groupByField)
        {
            if (this.GroupBy.Any())
            {
                return this.GroupGetPaginatedString(rowsPerPage, pageIndex, false);
            }

            int startIndex = pageIndex * rowsPerPage;
            string format = "SELECT TOP {0} * FROM (SELECT ROW_NUMBER() OVER (ORDER BY {2}) AS RowNumber, {1} FROM {3}{4}{5}) AS a WHERE RowNumber > {6}";

            if (!string.IsNullOrEmpty(groupByField))
            {
                format = "SELECT TOP {0} " + groupByField + " FROM (SELECT ROW_NUMBER() OVER (ORDER BY {2}) AS RowNumber, {1} FROM {3}{4}{5}) AS a WHERE RowNumber > {6}";
            }

            string selectStr = "";
            string whereStr = "";

            if (this.Where != null)
            {
                whereStr = " WHERE " + this.Where.ToString(true);
            }

            if (this.IsSelectAll)
            {
                selectStr = "*";
            }
            else
            {
                selectStr = this.GetFieldListString(this.Select, false);
            }

            if (!this.OrderBy.Any())
            {
                throw new NotSupportedException("OrderBy fields required for pagination.");
            }

            string joinStr = "";

            if (this.Joins.Any())
            {
                joinStr = " ";
                foreach (JoinTable join in this.Joins)
                {
                    joinStr = string.Format("{0}{1} ", joinStr, join.ToString());
                }
            }

            string sss = string.Format(
                    format,
                    rowsPerPage,
                    selectStr,
                    this.GetSortFieldString(this.OrderBy),
                    this.GetTableListString(this.From),
                    joinStr,
                    whereStr,
                    startIndex
                    );

            if (!string.IsNullOrEmpty(groupByField))
            {
                sss += " GROUP BY " + groupByField;
            }

            return sss;
        }

        #endregion

        #region Private Methods

        private QueryMode? GetModeFromWord(string word)
        {
            QueryMode? mode = null;

            switch (word.ToLower())
            {
                case "select":
                    mode = QueryMode.Select;
                    break;
                case "from":
                    mode = QueryMode.From;
                    break;
                case "where":
                    mode = QueryMode.Where;
                    break;
                case "outer":
                case "join":
                case "left":
                case "inner":
                    mode = QueryMode.Join;
                    break;
                case "order":
                case "by":
                    mode = QueryMode.OrderBy;
                    break;
                case "group":
                    mode = QueryMode.GroupBy;
                    break;
                case "into":
                    mode = QueryMode.Into;
                    break;
            }

            return mode;
        }

        private string GetCulledComments(string query)
        {
            var lines = query.Split(Environment.NewLine);

            List<string> validLines = new List<string>();
            foreach (var line in lines)
            {
                if (!line.Trim().StartsWith("--"))
                {
                    string newLine = line;

                    if (line.Contains("--"))
                    {
                        newLine = line.Split("--")[0];
                    }

                    validLines.Add(newLine);
                }
            }

            query = "";
            foreach (var validLine in validLines)
            {
                query += validLine + Environment.NewLine;
            }

            //query = query.Replace(Environment.NewLine, " ").Replace("\t", " ");

            return query;
        }

        #endregion

        private enum QueryMode
        {
            Unknown,
            Select,
            Into,
            From,
            OrderBy,
            Join,
            Where,
            GroupBy
        }
    }

    #region Enums

    /// <summary>
    /// Condition comparison type
    /// </summary>
    public enum ConditionType
    {
        Equal,
        GreaterThan,
        GreaterThanOrEqualTo,
        LessThan,
        LessThanOrEqualTo,
        NotEqual,
        Like
    }

    /// <summary>
    /// SQL condition grouping eg FisrtName='John' AND LastName='Smith'
    /// </summary>
    public enum ConditionOperand
    {
        And,
        Or
    }

    /// <summary>
    /// Database joining type INNER JOIN and LEFT OUTER JOIN
    /// </summary>
    public enum JoinType
    {
        InnerJoin,
        LeftOuterJoin
    }

    /// <summary>
    /// Database sorting directions, represent ASC and DESC
    /// </summary>
    public enum SortOrder
    {
        Ascending,
        Descending
    }

    #endregion
}