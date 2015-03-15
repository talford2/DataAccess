using System;
using System.Collections.Generic;
using System.Text;

namespace Dataway.DataAccess.Conditions
{
	/// <summary>
	/// Represents any SQL conditions which will not be parsed
	/// </summary>
    public class CustomCondition : ICondition
    {
        #region Private Members

        private string conditionString;

        #endregion

        #region Public Properties

		/// <summary>
		/// Gets or sets the SQL condition
		/// </summary>
        public string ConditionString
        {
            get { return this.conditionString; }
            set { this.conditionString = value; }
        }

        #endregion

        #region Constructors

		/// <summary>
		/// Construct a new custom condition
		/// </summary>
        public CustomCondition() { }

		/// <summary>
		/// Constructs a new custom condition with the SQL condition
		/// </summary>
		/// <param name="conditionString"></param>
        public CustomCondition(string conditionString)
        {
            this.conditionString = conditionString;
        }

        #endregion

        #region Public Methods

		/// <summary>
		/// Return string representation of the conditions, used for internal processing
		/// </summary>
		/// <param name="ingnoreEmptyValues">Used for internal use</param>
		/// <returns>Return string representation of the condition</returns>
        public string ToString(bool ingnoreEmptyValues)
        {
            return this.conditionString;
        }

        #endregion
    }
}
