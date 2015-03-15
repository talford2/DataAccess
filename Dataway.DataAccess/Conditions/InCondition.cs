using System;
using System.Collections.Generic;
using System.Text;

namespace Dataway.DataAccess.Conditions
{
	/// <summary>
	/// Represents an SQL in condition eg WHERE CustomerID IN (SELECT CustomerID FROM Orders)
	/// </summary>
    public class InCondition : ICondition
    {
		#region Private Members

		private List<Object> values;

		#endregion

		#region Public Properties
		
		/// <summary>
		/// Get or set the field to check in inner query
		/// </summary>
        public Field Field { get; set; }

		/// <summary>
		/// Gets or sets the query to check the field is in
		/// </summary>
        public SearchQuery InnerQuery { get; set; }

		/// <summary>
		/// Gets or sets a list of values to check a field is in
		/// </summary>
		public List<object> Values
		{
			get
        {
				if (this.values == null)
            {
					this.values = new List<object>();
            }
				return this.values;
        }
			set
        {
				this.values = value;
			}
        }

		/// <summary>
		/// Get or set whether field is not in the query
		/// </summary>
		public bool IsNotIn { get; set; }

		#endregion

		#region Constructors

		/// <summary>
		/// Construct a new in condition
		/// </summary>
		public InCondition() { }

		/// <summary>
		/// Construct a new in condition passing the checking field
		/// </summary>
		/// <param name="field">Field to check in</param>
		public InCondition(Field field)
        {
			this.Field = field;
        }

		/// <summary>
		/// Construct a new in condition passing the field and inner query
		/// </summary>
		/// <param name="field">Field to check in</param>
		/// <param name="innerQuery">Query to check field is in</param>
        public InCondition(Field field, SearchQuery innerQuery)
        {
            this.Field = field;
            this.InnerQuery = innerQuery;
        }

		/// <summary>
		/// Construct a new in condition padding the field and list of values
		/// </summary>
		/// <param name="field">Field to check in</param>
		/// <param name="values">List of values to check field is in</param>
		public InCondition(Field field, List<object> values)
        {
			this.Field = field;
			this.Values = values;
		}

		/// <summary>
		/// Construct a new in condition passing the field and list of values
		/// </summary>
		/// <param name="field">Field to check in which is parsed</param>
		/// <param name="values">Query to check field is in</param>
		public InCondition(string field, List<object> values) : this(new Field(field), values) { }

		/// <summary>
		/// Construct a new in condition passing the field and list of values
		/// </summary>
		/// <param name="field">Field to check in which id parsed</param>
		/// <param name="values">List of values to check field is in</param>
		public InCondition(string field, List<int> values)
		{
			this.Field = new Field(field);
			foreach (var v in values)
			{
				this.Values.Add(v);
			}
        }

		/// <summary>
		/// Construct a new in condition passing the field and inner query
		/// </summary>
		/// <param name="field">Field to check in which is parsed</param>
		/// <param name="innerQuery">Inner query to check field is in which is parsed</param>
		public InCondition(string field, string innerQuery) : this(new Field(field), new SearchQuery(innerQuery)) { }

		/// <summary>
		/// Construct a new in condition passing the condition
		/// </summary>
		/// <param name="condition">Condition string to be parsed</param>
        public InCondition(string condition)
        {
            string[] parts = condition.Split(new string[] { " IN ", " in ", " In ", " iN " }, new StringSplitOptions());

            if (parts.Length != 2)
            {
                throw new NotSupportedException("Bad format : " + condition);
            }

            this.Field = new Field(parts[0]);
            this.InnerQuery = new SearchQuery(parts[1].TrimStart('(').TrimEnd(')'));
        }

		#endregion

		#region Public Methods

		/// <summary>
		/// Gets the string representation of the in condition
		/// </summary>
		/// <param name="ingnoreEmptyValues">If set to true conditions with empty values will return an empty string</param>
		/// <returns>String Representation of the in condition</returns>
		public string ToString(bool ingnoreEmptyValues)
		{
			if (this.Field != null)
			{
				string format = "{0} IN ({1})";
				if (this.IsNotIn)
				{
					format = "{0} NOT IN ({1})";
				}

				if (this.InnerQuery != null)
				{
					return string.Format(format, this.Field.ToString(true), this.InnerQuery.ToString(false));
				}
				else if (this.Values != null && this.Values.Count > 0)
				{
					string valuesStr = "";
					foreach (var value in this.Values)
					{
						valuesStr += value + ",";
					}
					valuesStr = valuesStr.TrimEnd(',');
					return string.Format(format, this.Field.ToString(true), valuesStr);
				}
			}
			return null;
		}

		/// <summary>
		/// Gets the string representation of the in condition
		/// </summary>
		/// <returns>String Representation of the in condition</returns>
		public override string ToString()
		{
			return this.ToString(false);
		}

		#endregion
    }
}
