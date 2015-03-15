using System;
using System.Collections.Generic;
using System.Text;

namespace Dataway.DataAccess
{
    public class Field
    {
        #region Private Memebers

        private string fieldName;

        private string table;

        private string alias;

        #endregion

        #region Public Properties

		/// <summary>
		/// Gets or sets the name of the field
		/// </summary>
        public string FieldName
        {
            get { return fieldName; }
            set { fieldName = value; }
        }

		/// <summary>
		/// Gets or sets the fields table which is used to prefix it, eg Customer.FirstName
		/// </summary>
        public string Table
        {
            get { return table; }
            set { table = value; }
        }

		/// <summary>
		/// Gets or sets a fields alias for renaming it, eg FirstName AS GivenName
		/// </summary>
        public string Alias
        {
            get { return alias; }
            set { alias = value; }
        }

        #endregion

        #region Constructors

		/// <summary>
		/// Construct a new field
		/// </summary>
        public Field() { }

		/// <summary>
		/// Construct a field and parse into field object
		/// </summary>
		/// <param name="field"></param>
        public Field(string field)
        {
            if (field.Contains("("))
            {
                this.fieldName = field;
                return;
            }

            this.fieldName = field;
            if (this.fieldName.Contains("."))
            {
                string[] bits = this.fieldName.Split('.');
                this.Table = bits[0];
                this.fieldName = bits[1];
            }
            if (this.fieldName.ToLower().Contains(" as "))
            {
                string[] bits = this.fieldName.Split(new string[] { " as ", " AS ", " As ", " aS " }, StringSplitOptions.RemoveEmptyEntries);
                this.fieldName = bits[0].Trim();
                this.Alias = bits[1].Trim();
            }
        }

		/// <summary>
		/// Construct a field
		/// </summary>
		/// <param name="fieldName">Name of the field</param>
		/// <param name="table">Table field belongs to</param>
        public Field(string fieldName, string table)
            : this()
        {
            this.fieldName = fieldName.Trim();
            this.table = table.Trim();
        }

		/// <summary>
		/// Constructs a field
		/// </summary>
		/// <param name="fieldName">Name of the field</param>
		/// <param name="table">Table field belongs to</param>
		/// <param name="alias">Alias for the field</param>
        public Field(string fieldName, string table, string alias)
            : this(fieldName, table)
        {
            this.alias = alias.Trim();
        }

        #endregion

        #region Public Methods

		/// <summary>
		/// Return the string representation of the field, default prefixes table name
		/// </summary>
		/// <returns>String representation of the field</returns>
        override public string ToString()
        {
            return this.ToString(true);
        }

		/// <summary>
		/// Returns the string representation of the field specifying if the table should to prefixed 
		/// </summary>
		/// <param name="withTable">Whether the prefix the table name or not</param>
		/// <returns>String representation of the field</returns>
        public string ToString(bool withTable)
        {
            if (!string.IsNullOrEmpty(table) && withTable)
            {
                return string.Format("{0}.{1}", table, fieldName);
            }
            return fieldName;
        }

		/// <summary>
		/// Returns the string representation of the field with alias if provided
		/// </summary>
		/// <returns>String representation of the field with alias if provided</returns>
        public string ToSelectString()
        {
            if (string.IsNullOrEmpty(this.Alias))
            {
                return this.ToString();
            }
            else
            {
                return string.Format("{0} AS {1}", this.ToString(), this.Alias);
            }
        }

        #endregion

        #region Public Static Methods

		/// <summary>
		/// Create a new field by parsing
		/// </summary>
		/// <param name="field">Field text to parse</param>
		/// <returns>Field object</returns>
        public static Field Parse(string field)
        {
            if (field.Contains(" ("))
            {
                return new SubQueryField(field);
            }
            return new Field(field);
        }

        #endregion
    }
}
