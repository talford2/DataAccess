using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;

namespace Dataway.DataAccess.Conditions
{
	/// <summary>
	/// Represents a single simple SQL condition eg FirstName='John'
	/// </summary>
	public class SearchCondition : ICondition
	{
		#region Private Members

		private ConditionType conditionOperator;
		private Field field;
		private string value;
		private bool isStringType;

		#endregion

		#region Public Properties

		/// <summary>
		/// Gets or sets the comparing operator
		/// </summary>
		public ConditionType Operator
		{
			get { return this.conditionOperator; }
			set { this.conditionOperator = value; }
		}

		/// <summary>
		/// Gets or sets the field the condition relates to
		/// </summary>
		public Field Field
		{
			get { return this.field; }
			set { this.field = value; }
		}

		/// <summary>
		/// Gets or sets the value the condition is comparing against
		/// </summary>
		public string Value
		{
			get { return this.value; }
			set { this.value = value; }
		}

		/// <summary>
		/// Gets or sets whether the condition is for a string, if true '' will be added around the value unless its a parametter
		/// </summary>
		public bool IsStringType
		{
			get { return this.isStringType; }
			set { this.isStringType = value; }
		}

		/// <summary>
		/// Gets or sets whether the condition is compared to a parameter
		/// </summary>
		public bool IsParameter { get; set; }

		#endregion

		#region Constructors

		/// <summary>
		/// Construct a search condition
		/// </summary>
		public SearchCondition() { }

		/// <summary>
		/// Construct 
		/// </summary>
		/// <param name="searchCondition"></param>
		public SearchCondition(string searchCondition)
		{
			string[] bits = searchCondition.Trim().Split(' ');
			if (bits.Length == 3)
			{
				this.field = new Field(bits[0]);
				switch (bits[1].ToLower())
				{
					case "=":
						this.conditionOperator = ConditionType.Equal;
						break;
					case ">":
						this.conditionOperator = ConditionType.GreaterThan;
						break;
					case ">=":
						this.conditionOperator = ConditionType.GreaterThanOrEqualTo;
						break;
					case "<":
						this.conditionOperator = ConditionType.LessThan;
						break;
					case "<=":
						this.conditionOperator = ConditionType.LessThanOrEqualTo;
						break;
					case "like":
						this.conditionOperator = ConditionType.Like;
						break;
					case "<>":
						this.conditionOperator = ConditionType.NotEqual;
						break;
				}

				this.value = bits[2];

				if (this.value.StartsWith("@"))
				{
					this.IsParameter = true;
				}
				else
				{
					if (this.value.Contains("'"))
					{
						this.isStringType = true;
					}

					if (this.value.ToLower() == "true")
					{
						this.value = "1";
					}
					else if (this.value.ToLower() == "false")
					{
						this.value = "0";
					}

					this.value = this.value.Replace("'", "");
				}
			}
		}

		public SearchCondition(Field field, ConditionType conditionOperator, object value, bool isParameter)
		{
			this.field = field;
			this.conditionOperator = conditionOperator;

			this.IsParameter = isParameter;

			if (!isParameter)
			{
				if (value != null)
				{
					if (value is bool)
					{
						bool boolVal = (bool)value;

						if (boolVal)
						{
							this.value = "1";
						}
						else
						{
							this.value = "0";
						}
					}
					else if (value is DateTime)
					{
						this.isStringType = true;
						this.value = value.ToString();
					}
					else if (value is string)
					{
						string str = value as string;
						this.isStringType = true;
						this.value = str.Replace("'", "");
					}
					else if (value is SqlParameter)
					{
						this.value = "@" + ((SqlParameter)value).ParameterName.Replace("@", string.Empty);
					}
					else
					{
						this.value = value.ToString();
					}
				}
				else
				{
					this.value = null;
				}
			}
			else
			{
				this.value = value.ToString();
			}
		}

		//public SearchCondition(Field field, ConditionType condition, object value, bool isStringType)
		//    : this(field, condition, value)
		//{
		//    this.isStringType = isStringType;
		//}

		//public SearchCondition(string field, ConditionType condition, object value, bool isStringType)
		//    : this(new Field(field), condition, value, isStringType)
		//{

		//}

		public SearchCondition(string fieldName, ConditionType conditionOperator, object value, bool isParameter)
			: this(new Field(fieldName), conditionOperator, value, isParameter)
		{
		}

		public SearchCondition(string fieldName, ConditionType conditionOperator, object value)
			: this(fieldName, conditionOperator, value, false)
		{
		}

		#endregion

		#region Public Methods

		public string ToString(bool ignoreEmptyValues)
		{
			bool render = true;

			if (ignoreEmptyValues)
			{
				if (string.IsNullOrEmpty((string)this.Value))
				{
					render = false;
				}
			}

			if (render)
			{
				if (field == null)
				{
					return null;
				}

				StringBuilder sb = new StringBuilder();
				sb.AppendFormat("{0} ", field.ToString());

				switch (conditionOperator)
				{
					case ConditionType.Equal:
						sb.Append("=");
						break;
					case ConditionType.GreaterThan:
						sb.Append(">");
						break;
					case ConditionType.GreaterThanOrEqualTo:
						sb.Append(">=");
						break;
					case ConditionType.LessThan:
						sb.Append("<");
						break;
					case ConditionType.LessThanOrEqualTo:
						sb.Append("<=");
						break;
					case ConditionType.Like:
						sb.Append("LIKE");
						break;
					case ConditionType.NotEqual:
						sb.Append("<>");
						break;
				}
				sb.Append(" ");

				string queryValue = value;// value.Replace("'", "''");

				if (!IsParameter)
				{
					queryValue = value.Replace("'", "''");
					if (conditionOperator == ConditionType.Like)
					{
						if (!queryValue.Contains("%"))
						{
							sb.Append(string.Format("'%{0}%'", queryValue));
						}
						else
						{
							sb.Append(string.Format("'{0}'", queryValue));
						}
					}
					else
					{
						if (isStringType)
						{
							sb.Append(string.Format("'{0}'", queryValue));
						}
						else
						{
							sb.Append(queryValue);
						}
					}

				}
				else
				{
					sb.Append(queryValue);
				}

				return sb.ToString();
			}
			return null;
		}

		public override string ToString()
		{
			return this.ToString(false);
		}

		#endregion
	}
}
