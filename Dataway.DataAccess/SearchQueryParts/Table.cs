using System;
using System.Collections.Generic;
using System.Text;

namespace Dataway.DataAccess
{
	/// <summary>
	/// Represents an SQL table
	/// </summary>
    public class Table
    {
        #region Private Members

        private string tableName;

        private string alias;

        #endregion

        #region Public Properties

		/// <summary>
		/// Gets or sets the name of the table
		/// </summary>
        public string TableName
        {
            get { return tableName; }
            set { tableName = value; }
        }

		/// <summary>
		/// Gets or sets the tables alias eg CustomersTable AS ct
		/// </summary>
        public string Alias
        {
            get { return alias; }
            set { alias = value; }
        }

        #endregion

        #region Constructors

        public Table() { }

        public Table(string table)
            : this()
        {
            this.tableName = table;

            if (this.tableName.Trim().Contains(" "))
            {
                string[] bits = this.tableName.Split(' ');

                if (bits.Length > 1)
                {
                    this.tableName = bits[0];
                    this.alias = bits[1];
                }
            }
        }

        public Table(string tableName, string alias)
            : this(tableName)
        {
            this.alias = alias;
        }

        #endregion

        #region Public Methods

        override public string ToString()
        {
            return tableName;
        }

        public string ToString(bool withAlias)
        {
            if (!string.IsNullOrEmpty(alias))
            {
                return string.Format("{0} {1}", tableName, alias);
            }
            else
            {
                return ToString();
            }
        }

        #endregion
    }
}
