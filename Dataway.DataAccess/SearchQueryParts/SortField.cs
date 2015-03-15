using System;
using System.Collections.Generic;
using System.Text;

namespace Dataway.DataAccess
{
	/// <summary>
	/// Represents SQL sorting of a field eg ORDER BY LastName DESC
	/// </summary>
    public class SortField : Field
    {
        #region Public Properties

		/// <summary>
		/// Gets or sets the order direction
		/// </summary>
        public SortOrder Order
        {
            get;
            set;
        }

        #endregion

        #region Constructors

        public SortField(string sortField)
            : this(sortField, SortOrder.Ascending)
        {
            string[] bits = this.FieldName.Split(' ');

            if (bits.Length > 1)
            {
                this.FieldName = bits[0];
                if (bits[1].ToLower() == "desc")
                {
                    this.Order = SortOrder.Descending;
                }
            }
        }

        public SortField(string fieldName, SortOrder order)
            : base(fieldName)
        {
            this.Order = order;
        }

        public SortField(string fieldName, string table, SortOrder order)
            : base(fieldName, table)
        {
            this.Order = order;
        }

        #endregion

        public override string ToString()
        {
            if (this.Order == SortOrder.Ascending)
            {
                return string.Format("{0} ASC", base.ToString());
            }
            return string.Format("{0} DESC", base.ToString());
        }

        public new string ToString(bool withTables)
        {
            if (this.Order == SortOrder.Ascending)
            {
                return string.Format("{0} ASC", base.ToString(withTables));
            }

            return string.Format("{0} DESC", base.ToString(withTables));
        }
    }
}
