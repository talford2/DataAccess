using System;
using System.Collections.Generic;
using System.Text;

namespace Dataway.DataAccess.Conditions
{
	/// <summary>
	/// Interface required for SearchQuery conditions
	/// </summary>
    public interface ICondition
    {
		/// <summary>
		/// Return string representation of the condition
		/// </summary>
		/// <returns>String representation of the condition</returns>
        string ToString();

		/// <summary>
		/// Returns the string representation of the condition, return empty string if condition doesn't have a value
		/// </summary>
		/// <param name="ingnoreEmptyValues">Return empty string if value is empty and this is set to true</param>
		/// <returns>String representation of the condition</returns>
        string ToString(bool ingnoreEmptyValues);
    }
}
