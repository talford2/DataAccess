using System;
using System.Collections.Generic;
using System.Text;

namespace Dataway.DataAccess.Conditions
{
    public class CustomCondition : ICondition
    {
        #region Private Members

        private string conditionString;

        #endregion

        #region Public Properties

        public string ConditionString
        {
            get { return this.conditionString; }
            set { this.conditionString = value; }
        }

        #endregion

        #region Constructors

        public CustomCondition() { }

        public CustomCondition(string conditionString)
        {
            this.conditionString = conditionString;
        }

        #endregion

        #region Public Methods

        public string ToString(bool ingnoreEmptyValues)
        {
            return conditionString;
        }

        #endregion
    }
}
