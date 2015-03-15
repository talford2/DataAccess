using System;
using System.Collections.Generic;
using System.Text;

namespace Dataway.DataAccess
{
	/// <summary>
	/// Represents a collection of fields
	/// </summary>
    public class FieldCollection : List<Field>
    {
		/// <summary>
		/// Add a new field and parse it
		/// </summary>
		/// <param name="field">Text representation of the field to be parsed</param>
        public void Add(string field)
        {
            this.Add(Field.Parse(field));
        }

		/// <summary>
		/// Construct a new collection of fields
		/// </summary>
        public FieldCollection() { }


		/// <summary>
		/// Construct a new collection of fields with a comma delimited list of fields to parse
		/// </summary>
		/// <param name="fields">Comma delimited list of fields to be parsed</param>
        public FieldCollection(string fields)
            : this()
        {
            if (!string.IsNullOrEmpty(fields))
            {
                string[] bits = fields.Split(',');
                foreach (string field in bits)
                {
                    this.Add(field.Trim());
                }
            }
        }
    }
}
