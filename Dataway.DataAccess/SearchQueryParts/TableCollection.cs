using System;
using System.Collections.Generic;
using System.Text;

namespace Dataway.DataAccess
{
	/// <summary>
	/// Represents a group of SQL tables
	/// </summary>
    public class TableCollection : List<Table>
    {
		/// <summary>
		/// Add a table name to the list and parse
		/// </summary>
		/// <param name="table"></param>
        public void Add(string table)
        {
            this.Add(new Table(table));
        }

        public TableCollection() { }

        public TableCollection(string tables)
            : this()
        {
            if (!string.IsNullOrEmpty(tables.Trim()))
            {
                string[] bits = tables.Split(',');
                foreach (string table in bits)
                {
                    this.Add(table.Trim());
                }
            }
        }
    }
}
