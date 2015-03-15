using System;
using System.Collections.Generic;
using System.Text;

namespace Dataway.DataAccess
{
	/// <summary>
	/// Represents a group of sorted fields
	/// </summary>
    public class SortFieldCollection : List<SortField>
    {
		/// <summary>
		/// Add a sort field and parse it
		/// </summary>
		/// <param name="sortField">Sort field to be parsed</param>
        public void Add(string sortField)
        {
            this.Add(new SortField(sortField));
        }

		/// <summary>
		/// Add a sort field and parse it along with sort direction
		/// </summary>
		/// <param name="fieldName">Sort field to parse</param>
		/// <param name="order">Sort direction</param>
        public void Add(string fieldName, SortOrder order)
        {
            this.Add(new SortField(fieldName, order));
        }

        public SortFieldCollection() { }

        public SortFieldCollection(string sortFields)
            : this()
        {
            if (!string.IsNullOrEmpty(sortFields.Trim()))
            {
                string[] bits = sortFields.Split(',');
                foreach (string field in bits)
                {
                    this.Add(field.Trim());
                }
            }
        }
    }
}
