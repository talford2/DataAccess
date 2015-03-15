using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
namespace Dataway.DataAccess.Conditions
{
	/// <summary>
	/// Represents a group of conditions separated by a operand eg FirstName='John' AND LastName='Smith'
	/// </summary>
	public class OperandConditionGroup : ICondition
	{
		#region Private Members

		private ConditionOperand operand;

		private ConditionCollection conditions;

		#endregion

		#region Public Properties

		/// <summary>
		/// Gets or sets the operand
		/// </summary>
		public ConditionOperand Operand
		{
			get { return operand; }
			set { operand = value; }
		}

		/// <summary>
		/// Gets or sets the collection of conditions
		/// </summary>
		public ConditionCollection Conditions
		{
			get
			{
				if (conditions == null)
				{
					conditions = new ConditionCollection();
				}
				return conditions;
			}
			set { conditions = value; }
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Construct a new group of conditions
		/// </summary>
		public OperandConditionGroup() { }

		/// <summary>
		/// Construct a new group of conditions 
		/// </summary>
		/// <param name="operand">The separator logic joining the condition AND or OR</param>
		public OperandConditionGroup(ConditionOperand operand)
			: this()
		{
			this.operand = operand;
		}

		/// <summary>
		/// Construct a new group of conditions
		/// </summary>
		/// <param name="operand">The separator logic joining the condition AND or OR</param>
		/// <param name="conditions">Conditions in group</param>
		public OperandConditionGroup(ConditionOperand operand, ConditionCollection conditions)
			: this(operand)
		{
			this.conditions = conditions;
		}

		#endregion

		#region Public Properties

		/// <summary>
		/// Gets the SQL representation of the group
		/// </summary>
		/// <param name="ignoreEmptyValues">Remove items where null values are used</param>
		/// <returns>SQL representation of the group</returns>
		public string ToString(bool ignoreEmptyValues)
		{
			StringBuilder sb = new StringBuilder();
			if (this.Conditions.Count == 0)
			{
				return string.Empty;
			}

			for (int i = 0; i < Conditions.Count; i++)
			{
				int curLen = sb.Length;

				string resultCondition = null;

				if (this.Conditions[i] is Conditions.OperandConditionGroup && ((Conditions.OperandConditionGroup)this.Conditions[i]).Conditions.Count > 1)
				{
					string condStr = Conditions[i].ToString(ignoreEmptyValues);
					if (!string.IsNullOrEmpty(condStr))
					{
						resultCondition = string.Format("({0})", condStr);
					}
				}
				else
				{
					resultCondition = Conditions[i].ToString(ignoreEmptyValues);
				}

				if (!string.IsNullOrEmpty(resultCondition))
				{
					sb.Append(resultCondition);
					//if (curLen != sb.Length)
					//{
					if (i < Conditions.Count - 1)
					{
						switch (Operand)
						{
							case ConditionOperand.And:
								sb.Append(" AND ");
								break;
							case ConditionOperand.Or:
								sb.Append(" OR ");
								break;
							default:
								throw new NotImplementedException();
						}
					}
					//}
				}


			}

			return sb.ToString();
		}

		/// <summary>
		/// Returns SQL of condition groups
		/// </summary>
		/// <returns>String SQL of condition groups</returns>
		public override string ToString()
		{
			return ToString(false);
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Add a condition
		/// </summary>
		/// <param name="condition">Condition to add</param>
		public void AddCondition(ICondition condition)
		{
			this.Conditions.Add(condition);
		}

		/// <summary>
		/// Add a search condition
		/// </summary>
		/// <param name="condition">Typed search condition</param>
		public void AddCondition(SearchCondition condition)
		{
			this.Conditions.Add(condition);
		}

		/// <summary>
		/// Add a condition as a string to be parsed
		/// </summary>
		/// <param name="condition">Condition string to be parsed</param>
		public void AddCondition(string condition)
		{
			this.Conditions.Add(new SearchCondition(condition));
		}

		#endregion
	}
}
