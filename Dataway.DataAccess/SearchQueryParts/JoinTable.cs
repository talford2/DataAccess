using System;
using System.Collections.Generic;
using System.Text;
using Dataway.DataAccess.Extensions;

namespace Dataway.DataAccess
{
	/// <summary>
	/// Represents a joined tabled in SQL eg INNER JOIN CustomerOrders co ON co.CustomerId = c.CustomerId
	/// </summary>
    public class JoinTable
    {
        #region Private Members

        //private string table;
        private Table table;
        private JoinType join;
        private Field localField;
        private Field foreignField;

        #endregion

        #region Public Properties

		/// <summary>
		/// Gets or sets the table to join
		/// </summary>
        public Table Table
        {
            get { return this.table; }
            set { this.table = value; }
        }

		/// <summary>
		/// Gets or sets the joining type eg INNER JOIN
		/// </summary>
        public JoinType Join
        {
            get { return this.join; }
            set { this.join = value; }
        }

		/// <summary>
		/// Gets or sets the local field for the joining condition
		/// </summary>
        public Field LocalField
        {
            get { return this.localField; }
            set { this.localField = value; }
        }

		/// <summary>
		/// Gets or sets the foreign field for the joining condition
		/// </summary>
        public Field ForeignField
        {
            get { return this.foreignField; }
            set { this.foreignField = value; }
        }

        #endregion

        #region Constructors

		/// <summary>
		/// Construct a new join table
		/// </summary>
        public JoinTable() { }

		/// <summary>
		/// Construct a new join table and parse join input
		/// </summary>
		/// <param name="join"></param>
        public JoinTable(string join)
        {
            string[] bits = join.Split(new string[] { " ON ", " on ", " On ", " oN " }, StringSplitOptions.RemoveEmptyEntries);

            string frontSec = bits[0];

            if (frontSec.ToLower().Contains("inner join"))
            {
                this.join = JoinType.InnerJoin;
                frontSec = frontSec.Replace("inner join", "");
                frontSec = frontSec.Replace("INNER JOIN", "");
                frontSec = frontSec.Replace("Inner Join", "");

            }
            else if (frontSec.ToLower().Contains("left outer join"))
            {
                this.join = JoinType.LeftOuterJoin;
                frontSec = frontSec.Replace("left outer join", "");
                frontSec = frontSec.Replace("LEFT OUTER JOIN", "");
                frontSec = frontSec.Replace("Left Outer Join", "");
            }
            else
            {
                throw new NotSupportedException("Unknown join type");
            }

            frontSec = frontSec.Trim();
            this.table = new Table(frontSec);
            string backSec = bits[1];

            var backWords = backSec.Split(" ");
            int nestedDepth = 0;

            List<string> mainParts = new List<string>();
            mainParts.Add("");
            int index = 0;

            foreach (string word in backWords)
            {
                nestedDepth += word.GetCharCount('(');
                nestedDepth -= word.GetCharCount(')');
                if (nestedDepth == 0)
                {
                    if (word.Contains("="))
                    {
                        mainParts.Add("");
                        index++;
                    }
                }



                if (nestedDepth == 0)
                {
                    mainParts[index] += " " + word.TrimStart('=');
                    //mainParts[index] = mainParts[index]
                }
                else
                {
                    mainParts[index] += " " + word;
                }
            }

            if (mainParts.Count != 2)
            {
                throw new NotSupportedException("Invalid join.");
            }

            this.localField = Field.Parse(mainParts[0].Trim());
            this.foreignField = Field.Parse(mainParts[1].Trim());
        }

		/// <summary>
		/// Construct a new join table
		/// </summary>
		/// <param name="table">Join table name</param>
		/// <param name="join">Type of join eg INNER JOIN or LEFT OUTER JOIN</param>
		/// <param name="localField">Location field for join condition to be parsed</param>
		/// <param name="foreignField">Foreign field for join condition to be parsed</param>
        public JoinTable(string table, JoinType join, string localField, string foreignField)
        {
            this.table = new Table(table);
            this.join = join;
            this.localField = Field.Parse(localField);
            this.foreignField = Field.Parse(foreignField);
        }

		/// <summary>
		/// Construct a new join table
		/// </summary>
		/// <param name="table">Join table name</param>
		/// <param name="join">Type of join eg INNER JOIN or LEFT OUTER JOIN</param>
		/// <param name="localField">Location field for join condition</param>
		/// <param name="foreignField">Foreign field for join condition</param>
        public JoinTable(Table table, JoinType join, Field localField, Field foreignField)
        {
            this.table = table;
            this.join = join;
            this.localField = localField;
            this.foreignField = foreignField;
        }

        #endregion

        #region Public Methods

		/// <summary>
		/// Gets the SQL representation of the join
		/// </summary>
		/// <returns></returns>
        override public string ToString()
        {
            StringBuilder sb = new StringBuilder();
            if (this.join == JoinType.InnerJoin)
            {
                sb.Append("INNER JOIN ");
            }
            else if (this.join == JoinType.LeftOuterJoin)
            {
                sb.Append("LEFT OUTER JOIN ");
            }

            sb.AppendFormat("{0} ON ", this.table.ToString(true));

            sb.AppendFormat("{0} = {1}", this.LocalField.ToString(), this.ForeignField.ToString());

            return sb.ToString();
        }

        #endregion
    }
}
