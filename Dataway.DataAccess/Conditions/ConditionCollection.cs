using System;
using System.Collections.Generic;
using System.Text;

namespace Dataway.DataAccess.Conditions
{
	/// <summary>
	/// Represents a group of SQL conditions
	/// </summary>
    public class ConditionCollection : List<ICondition>
    {
		/// <summary>
		/// Add a condition using a string eg FirstName='John'
		/// </summary>
		/// <param name="condition">The SQL condition</param>
        public void Add(string condition)
        {
            ICondition newCondition = new SearchCondition(condition);

            string xyz = newCondition.ToString(false);

            if (string.IsNullOrEmpty(newCondition.ToString(false)))
            {
                newCondition = new CustomCondition(condition);
            }

            this.Add(newCondition);
        }

		/// <summary>
		/// Construct a new condition collection
		/// </summary>
        public ConditionCollection()
        {
        }

		/// <summary>
		/// Construct with a condition
		/// </summary>
		/// <param name="where"></param>
        public ConditionCollection(string where)
            : this()
        {
			this.Add(where);
        }
    }
}
