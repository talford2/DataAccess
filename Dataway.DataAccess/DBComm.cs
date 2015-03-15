using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;
using System.Linq;

namespace Dataway.DataAccess
{
    /// <summary>
    /// Database execution base, used by Query and StoredProcedure.
    /// </summary>
    public abstract class DBComm
    {
        #region Private Members

        private SqlConnection connection;

        #endregion

        #region Protected Properties

        /// <summary>
        /// Returns true if the current command has a transaction associated with it.
        /// </summary>
        protected bool HasTransaction
        {
            get
            {
                return this.Command.Transaction != null;
            }
        }

        #endregion

        #region Protected Members

        /// <summary>
        /// SQL command to execute.
        /// </summary>
        protected SqlCommand SqlCommand = new SqlCommand();

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the database connection string.
        /// </summary>
        public string ConnectionString
        {
            get { return this.connection.ConnectionString; }
        }

        /// <summary>
        /// Gets or sets the command timeout.
        /// </summary>
        public int CommandTimeout
        {
            get
            {
                return this.SqlCommand.CommandTimeout;
            }
            set
            {
                this.SqlCommand.CommandTimeout = value;
            }
        }

        /// <summary>
        /// Get the connection timeout.
        /// </summary>
        public int ConnectionTimeout
        {
            get
            {
                return this.connection.ConnectionTimeout;
            }
        }

        /// <summary>
        /// Get the SQL command.
        /// </summary>
        public SqlCommand Command
        {
            get
            {
                return this.SqlCommand;
            }
        }

        #endregion

        #region Constructor(s)

        /// <summary>
        /// Construct a DBComm object with a database connection string.
        /// </summary>
        /// <param name="connectionString">Connection string.</param>
        public DBComm(string connectionString)
        {
            this.connection = new SqlConnection(connectionString);
            this.SqlCommand.Connection = this.connection;
        }

        /// <summary>
        /// Construct a DBComm object with a database transaction.
        /// </summary>
        /// <param name="transaction">Database transaction.</param>
        public DBComm(DatabaseTransaction transaction)
        {
            this.SqlCommand.Transaction = transaction.Transaction;
            this.SqlCommand.Connection = transaction.Connection;
            this.connection = transaction.Connection;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Add a parameter to for executing with the SQL command.
        /// </summary>
        /// <param name="param">SqlParameter to add.</param>
        public void AddParameter(SqlParameter param)
        {
            this.SqlCommand.Parameters.Add(param);
        }

        /// <summary>
        /// Add a parameter to for executing with the SQL command.
        /// </summary>
        /// <param name="paramName">Name of the parameter.</param>
        /// <param name="value">Value for the parameter.</param>
        public void AddParameter(string paramName, object value)
        {
            if (value == null)
            {
                value = DBNull.Value;
            }
            AddParameter(new SqlParameter(paramName, value));
        }

        /// <summary>
        /// Add a parameter to for executing with the SQL command.
        /// </summary>
        /// <param name="paramName">Name of the parameter.</param>
        /// <param name="value">Value for the parameter.</param>
        /// <param name="paramDirection">ParameterDirection of the parameter.</param>
        public void AddParameter(string paramName, object value, ParameterDirection paramDirection)
        {
            if (value == null)
            {
                value = DBNull.Value;
            }
            SqlParameter param = new SqlParameter(paramName, value);
            param.Direction = paramDirection;
            AddParameter(param);
        }

        /// <summary>
        /// Add multiple parameter vals using an anonymous object for example 'new { FirstName = "John" }'.
        /// </summary>
        /// <param name="parameters">Any number of parameter values, parameter names will automatically be generated as @0, @1, @2 etc.  Or an anonymous object with parameter names declared ie new { FirstName = "John" }.</param>
        public void AddParameters(object[] parameters)
        {
            int index = 0;
            foreach (var obj in parameters)
            {
                if (IsObjectAnonymous(obj))
                {
                    var properties = obj.GetType().GetProperties();
                    foreach (var property in properties)
                    {
                        this.AddParameter(property.Name, property.GetValue(obj, null));
                    }
                }
                else
                {
                    this.AddParameter(index.ToString(), obj);
                    index++;
                }
            }
        }

        /// <summary>
        /// Get a parameter by name, can be used after results have been collected.
        /// </summary>
        /// <param name="paramName">Name of the parameter.</param>
        /// <returns>Object value for the specified parameter.</returns>
        public object GetParamValue(string paramName)
        {
            return this.SqlCommand.Parameters[paramName].Value;
        }

        /// <summary>
        /// Get a DataTable of the results.
        /// </summary>
        /// <returns>DataTable with the results.</returns>
        public virtual DataTable GetTable()
        {
            SqlDataAdapter adapter = new SqlDataAdapter(this.SqlCommand);
            DataTable dtData = new DataTable();

            try
            {
                this.OpenConnection();
                adapter.Fill(dtData);
            }
            catch (Exception ex)
            {
                this.LogError(ex);
                throw ex;
            }
            finally
            {
                this.CloseConnection();
            }

            return dtData;
        }

        /// <summary>
        /// Gets a collection of tables from executed SQL command.
        /// </summary>
        /// <returns>A collection of tables from the executed SQL command.</returns>
        public DataTableCollection GetTables()
        {
            return GetDataSet().Tables;
        }

        /// <summary>
        /// Get a data set from the executed SQL command.
        /// </summary>
        /// <returns>DataSet of results from executed SQL command.</returns>
        public virtual DataSet GetDataSet()
        {
            SqlDataAdapter adapter = new SqlDataAdapter(this.SqlCommand);
            DataSet ds = new DataSet();

            try
            {
                this.OpenConnection();
                adapter.Fill(ds);
            }
            catch (Exception ex)
            {
                this.LogError(ex);
                throw ex;
            }
            finally
            {
                this.CloseConnection();
            }

            return ds;
        }

        /// <summary>
        /// Delegate to get an object of type T from a DataRow.
        /// </summary>
        /// <typeparam name="T">Object type to return.</typeparam>
        /// <param name="row">Row with values.</param>
        /// <returns>Object of type T representation of row.</returns>
        public delegate T GetModelFromRow<T>(DataRow row);

        /// <summary>
        /// Get a list of type T from the executed SQL command.
        /// </summary>
        /// <typeparam name="T">Type of objects to get in the list.</typeparam>
        /// <param name="getObjectFromDataRow">Method to create an object of type T from a DataRow.</param>
        /// <returns>List of type T from the executed SQL command.</returns>
        public List<T> GetList<T>(GetModelFromRow<T> getObjectFromDataRow)
        {
            List<T> myList = new List<T>();
            DataTable t = this.GetTable();
            foreach (DataRow row in t.Rows)
            {
                myList.Add(getObjectFromDataRow(row));
            }
            return myList;
        }

        /// <summary>
        /// Get a list of type T from the executed SQL command.
        /// </summary>
        /// <typeparam name="T">Type of objects to get in the list.</typeparam>
        /// <returns>List of type T from the executed SQL command.</returns>
        public List<T> GetList<T>() where T : new()
        {
            var table = this.GetTable();

            var list = new List<T>();

            foreach (DataRow row in table.Rows)
            {
                list.Add(GetBound<T>(row));
            }

            return list;
        }

        /// <summary>
        /// Gets an object of type T from the first DataRow.
        /// </summary>
        /// <typeparam name="T">Object type to return.</typeparam>
        /// <param name="getObjectFromDataRow">Method to create an object of type T from the first DataRow.</param>
        /// <returns>Object of type T from the first DataRow.</returns>
        public T GetFirst<T>(GetModelFromRow<T> getObjectFromDataRow)
        {
            return getObjectFromDataRow(this.GetFirstRow());
        }

        /// <summary>
        /// Gets an object of type T using automatic property binding.
        /// </summary>
        /// <typeparam name="T">Object type to return.</typeparam>
        /// <returns>Object of type T with auto bound property values.</returns>
        public T GetFirst<T>() where T : new()
        {
            return GetBound<T>(this.GetFirstRow());
        }

        /// <summary>
        /// Gets an object of type T from the first DataRow, if none exists returns default value of T.
        /// </summary>
        /// <typeparam name="T">Object type to return.</typeparam>
        /// <param name="getObjectFromDataRow">Method to create an object of type T from the first DataRow.</param>
        /// <returns>Object of type T from the first DataRow, if none exists returns default value of T.</returns>
        public T GetFirstOrDefault<T>(GetModelFromRow<T> getObjectFromDataRow)
        {
            DataRow first = this.GetFirstRow();
            if (first == null)
            {
                return default(T);
            }
            return getObjectFromDataRow(first);
        }

        /// <summary>
        /// Gets the first row from the executed SQL commands results.
        /// </summary>
        /// <returns>DataRow first row from results.</returns>
        public DataRow GetFirstRow()
        {
            DataTable dt = GetTable();
            if (dt != null)
            {
                if (dt.Rows != null)
                {
                    if (dt.Rows.Count > 0)
                    {
                        return GetTable().Rows[0];
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Executes and returns the number of rows affected.
        /// </summary>
        /// <returns>Number of rows affected.</returns>
        public virtual int ExecuteNonQuery()
        {
            int rowsAffected = -1;
            try
            {
                this.OpenConnection();
                rowsAffected = this.SqlCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                this.LogError(ex);
                throw ex;
            }
            finally
            {
                this.CloseConnection();
            }

            return rowsAffected;
        }

        /// <summary>
        /// Executes and returns the first column value of the first row.
        /// </summary>
        /// <returns>The first column value of the first row.</returns>
        public virtual object ExecuteScalar()
        {
            object scalarValue;
            try
            {
                this.OpenConnection();
                scalarValue = this.SqlCommand.ExecuteScalar();
            }
            catch (Exception ex)
            {
                this.LogError(ex);
                throw ex;
            }
            finally
            {
                this.CloseConnection();
            }
            return scalarValue;
        }

        /// <summary>
        /// Executes and returns the first column value of the first row as a specified type.
        /// </summary>
        /// <typeparam name="T">Type to be returned.</typeparam>
        /// <returns>First column value of the first row as type T.</returns>
        public T ExecuteScalar<T>()
        {
            Type t = typeof(T);
            t = Nullable.GetUnderlyingType(t) ?? t;

            object value = this.ExecuteScalar();

            if (value == null || DBNull.Value.Equals(value))
            {
                return default(T);
            }
            else
            {
                return (T)Convert.ChangeType(value, t);
            }
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Open the database connection.
        /// </summary>
        protected void OpenConnection()
        {
            if (!this.HasTransaction)
            {
                this.connection.Open();

                if (Logger.HasLogging)
                {
                    Logger.Start = DateTime.Now;
                }
            }
        }

        /// <summary>
        /// Close the database connection.
        /// </summary>
        protected void CloseConnection()
        {
            if (!this.HasTransaction)
            {
                this.Log();

                if (this.connection.State == ConnectionState.Open)
                {
                    this.connection.Close();
                }

                if (this.connection.State != ConnectionState.Closed)
                {
                    throw new Exception(string.Format("Failed to close connection, connection state is \"{0}\"", this.connection.State));
                }
            }
        }

        private void LogError(Exception ex)
        {
            if (Logger.HasLogging)
            {
                DateTime end = DateTime.Now;

                StringBuilder sb = new StringBuilder("ERROR : " + this.Command.CommandText);

                sb.Append(" (");

                foreach (SqlParameter parameter in this.Command.Parameters)
                {
                    sb.AppendFormat("@{0}={1},", parameter.ParameterName, parameter.Value);
                }

                sb.Append(ex.Message);

                Logger.Log("[" + end.Subtract(Logger.Start).TotalMilliseconds + "] - " + sb.ToString().TrimEnd(',') + ") ");
            }
        }

        private void Log()
        {
            if (Logger.HasLogging && Logger.LogEverything)
            {
                DateTime end = DateTime.Now;

                StringBuilder sb = new StringBuilder(this.Command.CommandText);

                sb.Append(" (");

                foreach (SqlParameter parameter in this.Command.Parameters)
                {
                    sb.AppendFormat("@{0}={1},", parameter.ParameterName, parameter.Value);
                }

                Logger.Log(sb.ToString().TrimEnd(',') + ") ");
            }
        }

        /// <summary>
        /// Get an object of T type from a DataRow, setting the properties using automatic binding.
        /// </summary>
        /// <typeparam name="T">Object type.</typeparam>
        /// <param name="row">DataRow of data.</param>
        /// <returns>An object of type T.</returns>
        protected T GetBound<T>(DataRow row) where T : new()
        {
            if (row == null)
            {
                return default(T);
            }

            // Look for constructor with DataRow parameter
            ConstructorInfo[] c = typeof(T).GetConstructors();
            foreach (var constructor in c)
            {
                if (constructor.GetParameters().Length == 1 && constructor.GetParameters()[0].ParameterType == typeof(DataRow))
                {
                    return (T)Activator.CreateInstance(typeof(T), row);
                }
            }

            // Automatic binding
            var item = new T();
            foreach (DataColumn col in row.Table.Columns)
            {
                PropertyInfo info = item.GetType().GetProperty(col.ColumnName, BindingFlags.SetProperty | BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                if (info != null)
                {
                    object value = row[col.ColumnName];

                    if (value is DBNull)
                    {
                        value = null;
                    }

                    info.SetValue(item, value, null);
                }
            }

            return item;
        }

        #endregion

        #region Private Methods

        private bool IsObjectAnonymous(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            Type type = obj.GetType();

            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            return Attribute.IsDefined(type, typeof(CompilerGeneratedAttribute), false)
                && type.IsGenericType && type.Name.Contains("AnonymousType")
                && (type.Name.StartsWith("<>") || type.Name.StartsWith("VB$"))
                && (type.Attributes & TypeAttributes.NotPublic) == TypeAttributes.NotPublic;
        }

        #endregion
    }
}