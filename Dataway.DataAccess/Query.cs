using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace Dataway.DataAccess
{
    /// <summary>
    /// Connect and execute a query against a database.
    /// </summary>
    public class Query : DBComm
    {
        #region Constructors

        /// <summary>
        /// Construct a new query passing the connection string.
        /// </summary>
        /// <param name="connectionString">Database connection string.</param>
        public Query(string connectionString) : base(connectionString) { }

        /// <summary>
        /// Construct a new query using a SearchQuery object to construct the SQL.
        /// </summary>
        /// <param name="searchQuery">SearchQuery object to construct the SQL.</param>
        /// <param name="connectionString">Database connection string.</param>
		public Query(SearchQuery searchQuery, string connectionString) : base(connectionString) {
            this.SearchQuery = searchQuery;
        }

        /// <summary>
        /// Construct a new query passing the transaction.
        /// </summary>
        /// <param name="transaction"></param>
        public Query(DatabaseTransaction transaction) : base(transaction) { }

        /// <summary>
        /// Construct a new query passing the query and connection string.
        /// </summary>
        /// <param name="query">Query to execute.</param>
        /// <param name="connectionString">Database connection string.</param>
        public Query(string query, string connectionString)
            : base(connectionString)
        {
            this.SqlCommand.CommandType = CommandType.Text;
            this.SqlCommand.CommandText = query;
        }

        /// <summary>
        /// Construct a new query passing the query and database transaction.
        /// </summary>
        /// <param name="query">Query to execute.</param>
        /// <param name="transaction">Database transaction.</param>
        public Query(string query, DatabaseTransaction transaction)
            : base(transaction)
        {
            this.SqlCommand.CommandType = CommandType.Text;
            this.SqlCommand.CommandText = query;
        }

        /// <summary>
        /// Constructs a new query passing the query, connection string and parameters.
        /// </summary>
        /// <param name="query">Query to execute.</param>
        /// <param name="connectionString">Database connection string.</param>
        /// <param name="parameters">Array of parameters for the query.</param>
        public Query(string query, string connectionString, SqlParameter[] parameters)
            : this(query, connectionString)
        {
            this.SqlCommand.Parameters.AddRange(parameters);
        }

        /// <summary>
        /// Constructs a new query passing the query, database transaction and parameters.
        /// </summary>
        /// <param name="query">Query to execute.</param>
        /// <param name="transaction">Database transaction.</param>
        /// <param name="parameters">Array of parameters for the query.</param>
        public Query(string query, DatabaseTransaction transaction, SqlParameter[] parameters)
            : this(query, transaction)
        {
            this.SqlCommand.Parameters.AddRange(parameters);
        }

        /// <summary>
        /// Constructs a new query passing the query, connection string and parameters.
        /// </summary>
        /// <param name="query">Query to execute.</param>
        /// <param name="connectionString">Database connection string.</param>
        /// <param name="parameters">List of parameters for the query.</param>
        public Query(string query, string connectionString, List<SqlParameter> parameters)
            : this(query, connectionString, parameters.ToArray())
        {
        }

        /// <summary>
        /// Construct a new query passing a single parameter.
        /// </summary>
        /// <param name="query">Query to execute.</param>
        /// <param name="connectionString">Database connection string.</param>
        /// <param name="parameter">Single parameter.</param>
        private Query(string query, string connectionString, SqlParameter parameter)
            : this(query, connectionString)
        {
            this.AddParameter(parameter);
        }

        /// <summary>
        /// Construct a new query passing a single parameter.
        /// </summary>
        /// <param name="query">Query to execute.</param>
        /// <param name="transaction">Database transaction.</param>
        /// <param name="parameter">Single parameter.</param>
        private Query(string query, DatabaseTransaction transaction, SqlParameter parameter)
            : this(query, transaction)
        {
            this.AddParameter(parameter);
        }

        /// <summary>
        /// Construct a new query passing the query, connection string and any number of parameters, parameter names will be @0, @1, @2 etc.
        /// </summary>
        /// <param name="query">Query to execute.</param>
        /// <param name="connectionString">Database connection string.</param>
        /// <param name="parameters">Any number of parameter values, parameter names will automatically be generated as @0, @1, @2 etc.  Or an anonymous object with parameter names declared ie new { FirstName = "John" }.</param>
        private Query(string query, string connectionString, params object[] parameters)
            : base(connectionString)
        {
            this.SqlCommand.CommandType = CommandType.Text;
            this.SqlCommand.CommandText = query;
            this.AddParameters(parameters);
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the query to be executed.
        /// </summary>
        public string QueryCommand
        {
            get
            {
                if (this.SearchQuery != null)
                {
                    return this.SearchQuery.ToString();
                }
                return this.SqlCommand.CommandText;
            }
            set
            {
                this.SqlCommand.CommandText = value;
            }
        }

        /// <summary>
        /// Use a SearchQuery object to constructor the query rather than standard text.
        /// </summary>
        public SearchQuery SearchQuery { get; set; }

        #endregion

        #region Public Static Methods

        /// <summary>
        /// Get a data set from the executed query.
        /// </summary>
        /// <param name="query">Query to execute.</param>
        /// <param name="connectionString">Database connection string.</param>
        /// <returns>DataSet of results from executed query.</returns>
        public static DataSet GetDataSet(string query, string connectionString)
        {
            Query q = new Query(query, connectionString);
            return q.GetDataSet();
        }

        /// <summary>
        /// Get a data set from the executed query.
        /// </summary>
        /// <param name="query">Query to execute.</param>
        /// <param name="connectionString">Database connection string.</param>
        /// <param name="parameters">Any number of parameter values, parameter names will automatically be generated as @0, @1, @2 etc.  Or an anonymous object with parameter names declared ie new { FirstName = "John" }.</param>
        /// <returns>DataSet of results from executed query.</returns>
        public static DataSet GetDataSet(string query, string connectionString, params object[] parameters)
        {
            Query q = new Query(query, connectionString, parameters);
            return q.GetDataSet();
        }

        /// <summary>
        /// Get a table of the results.
        /// </summary>
        /// <param name="query">Query to execute.</param>
        /// <param name="connectionString">Database connection string.</param>
        /// <returns>DataTable of results.</returns>
        public static DataTable GetTable(string query, string connectionString)
        {
            Query q = new Query(query, connectionString);
            return q.GetTable();
        }

        /// <summary>
        /// Get a table of the results.
        /// </summary>
        /// <param name="query">Query to execute.</param>
        /// <param name="transaction">Database connection string.</param>
        /// <returns>DataTable of results.</returns>
        public static DataTable GetTable(string query, DatabaseTransaction transaction)
        {
            Query q = new Query(query, transaction);
            return q.GetTable();
        }

        /// <summary>
        /// Gets a table of results.
        /// </summary>
        /// <param name="query">Query to execute.</param>
        /// <param name="connectionString">Database connection string.</param>
        /// <param name="parameters">Array of parameters for the query.</param>
        /// <returns>DataTable of results.</returns>
        public static DataTable GetTable(string query, string connectionString, SqlParameter[] parameters)
        {
            Query q = new Query(query, connectionString);
            q.SqlCommand.Parameters.AddRange(parameters);
            return q.GetTable();
        }

        /// <summary>
        /// Gets a table of results.
        /// </summary>
        /// <param name="query">Query to execute.</param>
        /// <param name="connectionString">Database connection string.</param>
        /// <param name="parameters">List of parameters for the query.</param>
        /// <returns>DataTable of results.</returns>
        public static DataTable GetTable(string query, string connectionString, List<SqlParameter> parameters)
        {
            return Query.GetTable(query, connectionString, parameters.ToArray());
        }

        /// <summary>
        /// Get a table of results.
        /// </summary>
        /// <param name="query">Query to execute.</param>
        /// <param name="connectionString">Database connection string.</param>
        /// <param name="parameters">Any number of parameter values, parameter names will automatically be generated as @0, @1, @2 etc.  Or an anonymous object with parameter names declared ie new { FirstName = "John" }.</param>
        /// <returns>DataTable of results.</returns>
        public static DataTable GetTable(string query, string connectionString, params object[] parameters)
        {
            Query q = new Query(query, connectionString, parameters);
            return q.GetTable();
        }

        /// <summary>
        /// Get a table of results.
        /// </summary>
        /// <param name="searchQuery">SearchQuery object which represents a query.</param>
        /// <param name="connstionString">Database connection string.</param>
        /// <returns>DataTable of results.</returns>
        public static DataTable GetTable(SearchQuery searchQuery, string connstionString)
        {
            return GetTable(searchQuery.ToString(), connstionString);
        }

        /// <summary>
        /// Gets the first row in the results.
        /// </summary>
        /// <param name="query">Query to execute.</param>
        /// <param name="connectionString">Database connection string.</param>
        /// <returns>First DataRow from results.</returns>
        public static DataRow GetFirstRow(string query, string connectionString)
        {
            Query q = new Query(query, connectionString);
            return q.GetFirstRow();
        }

        /// <summary>
        /// Gets the first row in the results.
        /// </summary>
        /// <param name="query">Query to execute.</param>
        /// <param name="connectionString">Database connection string.</param>
        /// <param name="parameters">Array of parameters for the query.</param>
        /// <returns>First DataRow from results.</returns>
        public static DataRow GetFirstRow(string query, string connectionString, SqlParameter[] parameters)
        {
            Query q = new Query(query, connectionString);
            q.SqlCommand.Parameters.AddRange(parameters);
            return q.GetFirstRow();
        }

        /// <summary>
        /// Gets the first row in the results.
        /// </summary>
        /// <param name="query">Query to execute.</param>
        /// <param name="connectionString">Database connection string.</param>
        /// <param name="parameters">List of parameters for the query.</param>
        /// <returns>First DataRow from results.</returns>
        public static DataRow GetFirstRow(string query, string connectionString, List<SqlParameter> parameters)
        {
            return Query.GetFirstRow(query, connectionString, parameters.ToArray());
        }

        /// <summary>
        /// Gets the first row in the results.
        /// </summary>
        /// <param name="query">Query to execute.</param>
        /// <param name="connectionString">Database connection string.</param>
        /// <param name="parameterValues">Any number of parameter values, parameter names will automatically be generated as @0, @1, @2 etc.  Or an anonymous object with parameter names declared ie new { FirstName = "John" }.</param>
        /// <returns>First DataRow from results.</returns>
        public static DataRow GetFirstRow(string query, string connectionString, params object[] parameterValues)
        {
            Query q = new Query(query, connectionString, parameterValues);
            return q.GetFirstRow();
        }

        /// <summary>
        /// Gets an object of type T from the first DataRow.
        /// </summary>
        /// <typeparam name="T">Object type to return.</typeparam>
        /// <param name="query">Query to execute.</param>
        /// <param name="connctionString">Database connection string.</param>
        /// <returns>Object of type T from the first DataRow, automatically generated using reflection.</returns>
        public static T GetFirst<T>(string query, string connctionString) where T : new()
        {
            Query q = new Query(query, connctionString);
            return q.GetFirst<T>();
        }

        /// <summary>
        /// Gets an object of type T from the first DataRow.
        /// </summary>
        /// <typeparam name="T">Object type to return.</typeparam>
        /// <param name="query">Query to execute.</param>
        /// <param name="connectionString">Database connection string.</param>
        /// <param name="getObjectFromDataRow">Method to create an object of type T from the first DataRow.</param>
        /// <returns>Object of type T from the first DataRow.</returns>
        public static T GetFirst<T>(string query, string connectionString, GetModelFromRow<T> getObjectFromDataRow)
        {
            Query q = new Query(query, connectionString);
            return q.GetFirst<T>(getObjectFromDataRow);
        }

        /// <summary>
        /// Gets an object of type T from the first DataRow.
        /// </summary>
        /// <typeparam name="T">Object type to return.</typeparam>
        /// <param name="query">Query to execute.</param>
        /// <param name="transaction">Database transaction.</param>
        /// <param name="getObjectFromDataRow">Method to create an object of type T from the first DataRow.</param>
        /// <returns>Object of type T from the first DataRow.</returns>
        public static T GetFirst<T>(string query, DatabaseTransaction transaction, GetModelFromRow<T> getObjectFromDataRow)
        {
            Query q = new Query(query, transaction);
            return q.GetFirst<T>(getObjectFromDataRow);
        }

        /// <summary>
        /// Gets an object of type T from the first DataRow.
        /// </summary>
        /// <typeparam name="T">Object type to return.</typeparam>
        /// <param name="query">Query to execute.</param>
        /// <param name="connectionString">Database connection string.</param>
        /// <param name="getObjectFromDataRow">Method to create an object of type T from the first DataRow.</param>
        /// <param name="parameters">Any number of parameter values, parameter names will automatically be generated as @0, @1, @2 etc.  Or an anonymous object with parameter names declared ie new { FirstName = "John" }.</param>
        /// <returns>Object of type T from the first DataRow.</returns>
        public static T GetFirst<T>(string query, string connectionString, GetModelFromRow<T> getObjectFromDataRow, params object[] parameters)
        {
            Query q = new Query(query, connectionString, parameters);
            return q.GetFirst<T>(getObjectFromDataRow);
        }

        /// <summary>
        /// Gets an object of type T from the first DataRow.
        /// </summary>
        /// <typeparam name="T">Object type to return.</typeparam>
        /// <param name="query">Query to execute.</param>
        /// <param name="connectionString">Database connection string.</param>
        /// <param name="parameters">Any number of parameter values, parameter names will automatically be generated as @0, @1, @2 etc.  Or an anonymous object with parameter names declared ie new { FirstName = "John" }.</param>
        /// <returns>Object of type T from the first DataRow, automatically generated using reflection.</returns>
        public static T GetFirst<T>(string query, string connectionString, params object[] parameters) where T : new()
        {
            Query q = new Query(query, connectionString, parameters);
            return q.GetFirst<T>();
        }

        /// <summary>
        /// Get a list of type T from the query.
        /// </summary>
        /// <typeparam name="T">Type of objects to get in the list.</typeparam>
        /// <param name="query">Query to execute.</param>
        /// <param name="connectionString">Database connection string.</param>
        /// <returns>List of type T from the executed query, automatically generates objects using reflection.</returns>
        public static List<T> GetList<T>(string query, string connectionString) where T : new()
        {
            Query q = new Query(query, connectionString);
            return q.GetList<T>();
        }

        /// <summary>
        /// Get a list of type T from the query.
        /// </summary>
        /// <typeparam name="T">Type of objects to get in the list.</typeparam>
        /// <param name="query">Query to execute.</param>
        /// <param name="connectionString">Database connection string.</param>
        /// <param name="parameters">Any number of parameter values, parameter names will automatically be generated as @0, @1, @2 etc.  Or an anonymous object with parameter names declared ie new { FirstName = "John" }.</param>
        /// <returns>List of type T from the executed query.</returns>
        public static List<T> GetList<T>(string query, string connectionString, params object[] parameters) where T : new()
        {
            if (parameters is object)
            {
                Query q = new Query(query, connectionString);
                q.AddParameters(parameters);
                return q.GetList<T>();
            }

            Query qq = new Query(query, connectionString, parameters);
            return qq.GetList<T>();
        }

        /// <summary>
        /// Get a list of type T from the query.
        /// </summary>
        /// <typeparam name="T">Type of objects to get in the list.</typeparam>
        /// <param name="query">Query to execute.</param>
        /// <param name="connectionString">Database connection string.</param>
        /// <param name="getObjectFromDataRow">Method to create an object of type T from a DataRow.</param>
        /// <returns>List of type T from the executed query.</returns>
        public static List<T> GetList<T>(string query, string connectionString, GetModelFromRow<T> getObjectFromDataRow)
        {
            Query q = new Query(query, connectionString);
            return q.GetList<T>(getObjectFromDataRow);
        }

        /// <summary>
        /// Get a list of type T from the query.
        /// </summary>
        /// <typeparam name="T">Type of objects to get in the list.</typeparam>
        /// <param name="query">Query to execute.</param>
        /// <param name="connectionString">Database connection string.</param>
        /// <param name="getObjectFromDataRow">Method to create an object of type T from a DataRow.</param>
        /// <param name="parameters">Any number of parameter values, parameter names will automatically be generated as @0, @1, @2 etc.  Or an anonymous object with parameter names declared ie new { FirstName = "John" }.</param>
        /// <returns>Any number of parameter values, parameter names will automatically be generated as @0, @1, @2 etc.</returns>
        public static List<T> GetList<T>(string query, string connectionString, GetModelFromRow<T> getObjectFromDataRow, params object[] parameters)
        {
            Query q = new Query(query, connectionString, parameters);
            return q.GetList<T>(getObjectFromDataRow);
        }

        /// <summary>
        /// Executes and returns the first column value of the first row.
        /// </summary>
        /// <param name="query">Query to execute.</param>
        /// <param name="connectionString">Database connection string.</param>
        /// <returns>First column value of the first row as an object type.</returns>
        public static object ExecuteScalar(string query, string connectionString)
        {
            Query q = new Query(query, connectionString);
            return q.ExecuteScalar();
        }

        /// <summary>
        /// Executes and returns the first column value of the first row.
        /// </summary>
        /// <param name="query">Query to execute.</param>
        /// <param name="transaction">Database transaction.</param>
        /// <returns>First column value of the first row as an object type.</returns>
        public static object ExecuteScalar(string query, DatabaseTransaction transaction)
        {
            Query q = new Query(query, transaction);
            return q.ExecuteScalar();
        }

        /// <summary>
        /// Executes and returns the first column value of the first row.
        /// </summary>
        /// <param name="query">Query to execute.</param>
        /// <param name="connectionString">Database connection string.</param>
        /// <param name="parameters">Any number of parameter values, parameter names will automatically be generated as @0, @1, @2 etc.  Or an anonymous object with parameter names declared ie new { FirstName = "John" }.</param>
        /// <returns></returns>
        public static object ExecuteScalar(String query, string connectionString, params object[] parameters)
        {
            Query q = new Query(query, connectionString, parameters);
            return q.ExecuteScalar();
        }

        /// <summary>
        /// Executes and returns the first column value of the first row as specified type.
        /// </summary>
        /// <typeparam name="T">Type of object to return.</typeparam>
        /// <param name="query">Query to execute.</param>
        /// <param name="connectionString">Database connection string.</param>
        /// <returns>First column value of the first row as type T.</returns>
        public static T ExecuteScalar<T>(string query, string connectionString)
        {
            return (T)Convert.ChangeType(ExecuteScalar(query, connectionString), typeof(T));
        }

        /// <summary>
        /// Executes and returns the first column value of the first row as specified type.
        /// </summary>
        /// <typeparam name="T">Type of object to return.</typeparam>
        /// <param name="query">Query to execute.</param>
        /// <param name="transaction">Database transaction.</param>
        /// <returns>First column value of the first row as type T.</returns>
        public static T ExecuteScalar<T>(string query, DatabaseTransaction transaction)
        {
            return (T)Convert.ChangeType(ExecuteScalar(query, transaction), typeof(T));
        }

        /// <summary>
        /// Executes and returns the first column value of the first row as specified type.
        /// </summary>
        /// <typeparam name="T">Type of object to return.</typeparam>
        /// <param name="query">Query to execute.</param>
        /// <param name="connectionString">Database connection string.</param>
        /// <param name="parameters">Any number of parameter values, parameter names will automatically be generated as @0, @1, @2 etc.  Or an anonymous object with parameter names declared ie new { FirstName = "John" }.</param>
        /// <returns></returns>
        public static T ExecuteScalar<T>(string query, string connectionString, params object[] parameters)
        {
            Query q = new Query(query, connectionString, parameters);
            return q.ExecuteScalar<T>();
        }

        /// <summary>
        /// Executes and returns the number of row affected.
        /// </summary>
        /// <param name="query">Query to execute.</param>
        /// <param name="connectionString">Database connection string.</param>
        /// <returns>Number of records affected.</returns>
        public static int ExecuteNonQuery(string query, string connectionString)
        {
            Query q = new Query(query, connectionString);
            return q.ExecuteNonQuery();
        }

        /// <summary>
        /// Executes and returns the number of row affected.
        /// </summary>
        /// <param name="query">Query to execute.</param>
        /// <param name="transaction">Database transaction.</param>
        /// <returns>Number of records affected.</returns>
        public static int ExecuteNonQuery(string query, DatabaseTransaction transaction)
        {
            Query q = new Query(query, transaction);
            return q.ExecuteNonQuery();
        }

        /// <summary>
        /// Executes and return the number of rows affected.
        /// </summary>
        /// <param name="query">Query to execute.</param>
        /// <param name="connectionString">Database connection string.</param>
        /// <param name="parameters">Any number of parameter values, parameter names will automatically be generated as @0, @1, @2 etc.  Or an anonymous object with parameter names declared ie new { FirstName = "John" }.</param>
        /// <returns>Number of records affected.</returns>
        public static int ExecuteNonQuery(string query, string connectionString, params object[] parameters)
        {
            Query q = new Query(query, connectionString, parameters);
            return q.ExecuteNonQuery();
        }

        #endregion

        #region Public Methods

		/// <summary>
		/// Execute the query and return the results in a DataTable
		/// </summary>
		/// <returns>DataTable of results</returns>
        public override DataTable GetTable()
        {
            if (this.SearchQuery != null)
            {
                base.SqlCommand.CommandText = this.SearchQuery.ToString();
            }
            return base.GetTable();
        }

		/// <summary>
		/// Execute a query and return the number of rows affected
		/// </summary>
		/// <returns>The number of rows affected</returns>
        public override int ExecuteNonQuery()
        {
            if (this.SearchQuery != null)
            {
                base.SqlCommand.CommandText = this.SearchQuery.ToString();
            }
            return base.ExecuteNonQuery();
        }

		/// <summary>
		/// Execute a query and return the first value from the first row
		/// </summary>
		/// <returns>Object result</returns>
        public override object ExecuteScalar()
        {
            if (this.SearchQuery != null)
            {
                base.SqlCommand.CommandText = this.SearchQuery.ToString();
            }
            return base.ExecuteScalar();
        }

		/// <summary>
		/// Execute the query and return a DataSet of the results
		/// </summary>
		/// <returns>DataSet of the results</returns>
        public override DataSet GetDataSet()
        {
            if (this.SearchQuery != null)
            {
                base.SqlCommand.CommandText = this.SearchQuery.ToString();
            }
            return base.GetDataSet();
        }

        #endregion
    }
}