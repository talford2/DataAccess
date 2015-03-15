using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;

namespace Dataway.DataAccess
{
    /// <summary>
    /// Execute multiple sql commands together, results can be rolled back.
    /// </summary>
    public class DatabaseTransaction : IDisposable
    {
        #region Private Members

        private SqlConnection conn;

        #endregion

        #region Internal Properties

        internal SqlTransaction Transaction { get; set; }

        internal SqlConnection Connection
        {
            get
            {
                return this.conn;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Construct a new transaction passing the database connection string.
        /// </summary>
        /// <param name="connectionString">Database connection string.</param>
        public DatabaseTransaction(string connectionString)
        {
            conn = new SqlConnection(connectionString);
            conn.Open();
            Transaction = conn.BeginTransaction();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Rollback the transaction commands.
        /// </summary>
        public void RollBack()
        {
            this.Transaction.Rollback();
            conn.Close();
        }

        /// <summary>
        /// Commit the transaction commands.
        /// </summary>
        public void Commit()
        {
            this.Transaction.Commit();
            conn.Close();
        }

        /// <summary>
        /// Dispose of the object and close the database connection.
        /// </summary>
        public void Dispose()
        {
            conn.Close();
        }

        #endregion
    }
}
