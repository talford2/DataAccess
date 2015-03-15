using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace Dataway.DataAccess
{
    /// <summary>
    /// Connect and execute a stored procedure against a database.
    /// </summary>
    public class StoredProcedure : DBComm
    {
        #region Public Properties

        /// <summary>
        /// Get the name of the stored procedure to call.
        /// </summary>
        public string Name
        {
            get { return this.SqlCommand.CommandText; }
        }

        #endregion

        #region Constructor(s)

        /// <summary>
        /// Construct a new stored procedure.
        /// </summary>
        /// <param name="name">Name of the stored procedure to execute.</param>
        /// <param name="connectionString">Database connection string.</param>
        public StoredProcedure(string name, string connectionString)
            : base(connectionString)
        {
            this.SqlCommand.CommandType = CommandType.StoredProcedure;
            this.SqlCommand.CommandText = name;
        }

        /// <summary>
        /// Construct a new stored procedure.
        /// </summary>
        /// <param name="name">Name of the stored procedure to execute.</param>
        /// <param name="transaction">Database transaction.</param>
        public StoredProcedure(string name, DatabaseTransaction transaction)
            : base(transaction)
        {
            this.SqlCommand.CommandType = CommandType.StoredProcedure;
            this.SqlCommand.CommandText = name;
        }

        /// <summary>
        /// Construct a new stored procedure.
        /// </summary>
        /// <param name="name">Name of the stored procedure to execute.</param>
        /// <param name="connectionString">Database connection string.</param>
        /// <param name="parameters">Any number of parameter values, parameter names will automatically be generated as @0, @1, @2 etc.  Or an anonymous object with parameter names declared ie new { FirstName = "John" }.</param>
        public StoredProcedure(string name, string connectionString, params object[] parameters)
            : this(name, connectionString)
        {
            this.AddParameters(parameters);
        }

        #endregion

        #region Public Static Methods

        /// <summary>
        /// Get a data set from the executed stored procedure.
        /// </summary>
        /// <param name="name">Name of the stored procedure.</param>
        /// <param name="connectionString">Database connection string.</param>
        /// <returns>DataSet of results from executed stored procedure.</returns>
        public static DataSet GetDataSet(String name, string connectionString)
        {
            StoredProcedure sp = new StoredProcedure(name, connectionString);
            return sp.GetDataSet();
        }

        /// <summary>
        /// Gets the first row in the results.
        /// </summary>
        /// <param name="name">Name of the stored procedure.</param>
        /// <param name="connectionString">Database connection string.</param>
        /// <returns>First DataRow from results.</returns>
        public static DataRow GetFirstRow(string name, string connectionString)
        {
            StoredProcedure sp = new StoredProcedure(name, connectionString);
            return sp.GetFirstRow();
        }

        /// <summary>
        /// Gets the first row in the results.
        /// </summary>
        /// <param name="name">Name of the stored procedure.</param>
        /// <param name="transaction">Database transaction.</param>
        /// <returns>First DataRow from results.</returns>
        public static DataRow GetFirstRow(string name, DatabaseTransaction transaction)
        {
            StoredProcedure sp = new StoredProcedure(name, transaction);
            return sp.GetFirstRow();
        }

        /// <summary>
        /// Gets the first row for results.
        /// </summary>
        /// <param name="name">Name of the stored procedure.</param>
        /// <param name="connectionString">Database connection string.</param>
        /// <param name="parameters">Stored procedures parameters.</param>
        /// <returns>DataRow</returns>
        public static DataRow GetFirstRow(string name, string connectionString, SqlParameter[] parameters)
        {
            StoredProcedure sp = new StoredProcedure(name, connectionString);
            if (parameters != null)
            {
                foreach (SqlParameter param in parameters)
                {
                    sp.AddParameter(param);
                }
            }
            return sp.GetFirstRow();
        }

        /// <summary>
        /// Gets an object of type T from the first DataRow.
        /// </summary>
        /// <typeparam name="T">Object type to return.</typeparam>
        /// <param name="name">Name of the stored procedure.</param>
        /// <param name="connectionString">Database connection string.</param>
        /// <param name="getObjectFromDataRow">Method to create an object of type T from the first DataRow.</param>
        /// <returns>Object of type T from the first DataRow.</returns>
        public static T GetFirst<T>(string name, string connectionString, GetModelFromRow<T> getObjectFromDataRow)
        {
            StoredProcedure sp = new StoredProcedure(name, connectionString);
            return sp.GetFirst<T>(getObjectFromDataRow);
        }

        /// <summary>
        /// Gets an object of type T from the first DataRow.
        /// </summary>
        /// <typeparam name="T">Object type to return.</typeparam>
        /// <param name="name">Name of the stored procedure.</param>
        /// <param name="transaction">Database transaction.</param>
        /// <param name="getObjectFromDataRow">Method to create an object of type T from the first DataRow.</param>
        /// <returns>Object of type T from the first DataRow.</returns>
        public static T GetFirst<T>(string name, DatabaseTransaction transaction, GetModelFromRow<T> getObjectFromDataRow)
        {
            StoredProcedure sp = new StoredProcedure(name, transaction);
            return sp.GetFirst<T>(getObjectFromDataRow);
        }

        /// <summary>
        /// Gets an object of type T from the first DataRow.
        /// </summary>
        /// <typeparam name="T">Object type to return.</typeparam>
        /// <param name="name">Name of the stored procedure.</param>
        /// <param name="connectionString">Database connection string.</param>
        /// <returns>Object of type T from the first DataRow.</returns>
        public static T GetFirst<T>(string name, string connectionString) where T : new()
        {
            StoredProcedure q = new StoredProcedure(name, connectionString);
            return q.GetFirst<T>();
        }

        /// <summary>
        /// Gets an object of type T from the first DataRow.
        /// </summary>
        /// <typeparam name="T">Object type to return.</typeparam>
        /// <param name="name">Name of the stored procedure.</param>
        /// <param name="connectionString">Database connection string.</param>
        /// <param name="parameters">Any number of parameter values, parameter names will automatically be generated as @0, @1, @2 etc.  Or an anonymous object with parameter names declared ie new { FirstName = "John" }.</param>
        /// <returns>Object of type T from the first DataRow.</returns>
        public static T GetFirst<T>(string name, string connectionString, params object[] parameters) where T : new()
        {
            StoredProcedure q = new StoredProcedure(name, connectionString, parameters);
            return q.GetFirst<T>();
        }

        public static List<T> GetList<T>(string name, string connectionString) where T : new()
        {
            StoredProcedure sp = new StoredProcedure(name, connectionString);
            return sp.GetList<T>();
        }

        /// <summary>
        /// Get a list of type T from the stored procedure.
        /// </summary>
        /// <typeparam name="T">Type of objects to get in the list.</typeparam>
        /// <param name="name">Name of the stored procedure.</param>
        /// <param name="connectionString">Database connection string.</param>
        /// <param name="getObjectFromDataRow">Method to create an object of type T from the first DataRow.</param>
        /// <returns>List of type T from the executed stored procedure.</returns>
        public static List<T> GetList<T>(string name, string connectionString, GetModelFromRow<T> getObjectFromDataRow)
        {
            StoredProcedure sp = new StoredProcedure(name, connectionString);
            return sp.GetList<T>(getObjectFromDataRow);
        }

        /// <summary>
        /// Get a list of type T from the stored procedure.
        /// </summary>
        /// <typeparam name="T">Type of objects to get in the list.</typeparam>
        /// <param name="name">Name of the stored procedure.</param>
        /// <param name="connectionString">Database connection string.</param>
        /// <param name="parameters">Any number of parameter values, parameter names will automatically be generated as @0, @1, @2 etc.  Or an anonymous object with parameter names declared ie new { FirstName = "John" }.</param>
        /// <returns>List of type T from the executed stored procedure.</returns>
        public static List<T> GetList<T>(string name, string connectionString, params object[] parameters) where T : new()
        {
            if (parameters is object)
            {
                Query q = new Query(name, connectionString);
                q.AddParameters(parameters);
                return q.GetList<T>();
            }

            var qq = new StoredProcedure(name, connectionString, parameters);
            return qq.GetList<T>();
        }

        /// <summary>
        /// Get a table of results.
        /// </summary>
        /// <param name="name">Name of the stored procedure.</param>
        /// <param name="connectionString">Database connection string.</param>
        /// <returns>DataTable of results.</returns>
        public static DataTable GetTable(string name, string connectionString)
        {
            StoredProcedure sp = new StoredProcedure(name, connectionString);
            return sp.GetTable();
        }

        /// <summary>
        /// Get a table of results.
        /// </summary>
        /// <param name="name">Name of the stored procedure.</param>
        /// <param name="transaction">Database transaction.</param>
        /// <returns>DataTable of results.</returns>
        public static DataTable GetTable(string name, DatabaseTransaction transaction)
        {
            StoredProcedure sp = new StoredProcedure(name, transaction);
            return sp.GetTable();
        }

        /// <summary>
        /// Get a DataTable of the results.
        /// </summary>
        /// <param name="name">Name of the stored procedure.</param>
        /// <param name="connectionString">Database connection string.</param>
        /// <param name="parameters">Stored procedures parameters.</param>
        /// <returns>DataTable of the results.</returns>
        public static DataTable GetTable(string name, string connectionString, SqlParameter[] parameters)
        {
            StoredProcedure sp = new StoredProcedure(name, connectionString);
            if (parameters != null)
            {
                foreach (SqlParameter param in parameters)
                {
                    sp.AddParameter(param);
                }
            }
            return sp.GetTable();
        }

        /// <summary>
        /// Executes and returns the first column of the first row as a typed object.
        /// </summary>
        /// <param name="name">Name of the stored procedure.</param>
        /// <param name="connectionString">Database connection string.</param>
        /// <returns>Object.</returns>
        public static object ExecuteScalar(string name, string connectionString)
        {
            StoredProcedure sp = new StoredProcedure(name, connectionString);
            return sp.ExecuteScalar();
        }

        /// <summary>
        /// Executes and returns the first column of the first row as a typed object.
        /// </summary>
        /// <param name="name">Name of the stored procedure.</param>
        /// <param name="transaction">Database transaction.</param>
        /// <returns>Object.</returns>
        public static object ExecuteScalar(string name, DatabaseTransaction transaction)
        {
            StoredProcedure sp = new StoredProcedure(name, transaction);
            return sp.ExecuteScalar();
        }

        /// <summary>
        /// Executes and returns the first column of the first row as a typed object.
        /// </summary>
        /// <param name="name">Name of the stored procedure.</param>
        /// <param name="connectionString">Database connection string.</param>
        /// <param name="parameters">Stored procedures parameters.</param>
        /// <returns>Object</returns>
        public static object ExecuteScalar(string name, string connectionString, SqlParameter[] parameters)
        {
            StoredProcedure sp = new StoredProcedure(name, connectionString);
            if (parameters != null)
            {
                foreach (SqlParameter param in parameters)
                {
                    sp.AddParameter(param);
                }
            }
            return sp.ExecuteScalar();
        }

        public static T ExecuteScalar<T>(string name, string connectionString)
        {
            return (T)Convert.ChangeType(ExecuteScalar(name, connectionString), typeof(T));
        }

        /// <summary>
        /// Executes and returns the number of rows affected.
        /// </summary>
        /// <param name="name">Name of the stored procedure.</param>
        /// <param name="connectionString">Database connection string.</param>
        /// <returns>Number of records affected.</returns>
        public static int ExecuteNonQuery(string name, string connectionString)
        {
            StoredProcedure sp = new StoredProcedure(name, connectionString);
            return sp.ExecuteNonQuery();
        }

        /// <summary>
        /// Executes and returns the number of rows affected.
        /// </summary>
        /// <param name="name">Name of the stored procedure.</param>
        /// <param name="transaction">Database transaction.</param>
        /// <returns>Number of records affected.</returns>
        public static int ExecuteNonQuery(string name, DatabaseTransaction transaction)
        {
            StoredProcedure sp = new StoredProcedure(name, transaction);
            return sp.ExecuteNonQuery();
        }

        /// <summary>
        /// Executes and returns the number of rows affected.
        /// </summary>
        /// <param name="name">Name of the stored procedure.</param>
        /// <param name="connectionString">Database connection string.</param>
        /// <param name="parameters">Stored procedures parameters.</param>
        /// <returns>Number of records affected.</returns>
        public static int ExecuteNonQuery(string name, string connectionString, SqlParameter[] parameters)
        {
            StoredProcedure sp = new StoredProcedure(name, connectionString);
            if (parameters != null)
            {
                foreach (SqlParameter param in parameters)
                {
                    sp.AddParameter(param);
                }
            }
            return sp.ExecuteNonQuery();
        }

        #endregion
    }
}
