using System;
using System.Collections.Generic;
using System.Text;
using Dataway.DataAccess.Extensions;

namespace Dataway.DataAccess
{
	/// <summary>
	/// Represents a group of SQL joins
	/// </summary>
    public class JoinCollection : List<JoinTable>
    {
		/// <summary>
		/// Parse and add a new join
		/// </summary>
		/// <param name="join"></param>
        public void Add(string join)
        {
            this.Add(new JoinTable(join));
        }

		/// <summary>
		/// Construct a new join collection
		/// </summary>
        public JoinCollection() { }

		/// <summary>
		/// Construct a new join collection and parse a joins string
		/// </summary>
		/// <param name="joins">String joins to be parsed into objects</param>
        public JoinCollection(string joins)
            : this()
        {
            int index = -1;
            var words = joins.Split(" ");
            List<string> joinStrs = new List<string>();

            foreach (string word in words)
            {
                if (word.ToLower() + " join" == "inner join" || word.ToLower() + " outer join" == "left outer join")
                {
                    index++;
                    joinStrs.Add("");
                }
                joinStrs[index] += " " + word;
            }

            foreach (var fullStr in joinStrs)
            {

                //this.Add(fullStr.SplitKeepingBrackets("="));

                //this.Add(fullStr.Replace("=", " = ").Replace("  ", " "));
                this.Add(fullStr.Replace("  ", " "));
            }
        }
    }
}
