using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dataway.DataAccess.Extensions;

namespace Dataway.DataAccess
{
    public class SubQueryField : Field
    {
        public SearchQuery SubQuery { get; set; }

        public SubQueryField(string field)
        {
            var bits = field.SplitKeepingBrackets("=", true);
            this.FieldName = bits[0];

            string subQuery;
            try
            {
                subQuery = bits[1].TrimStart("(".ToCharArray()).TrimEnd(")".ToCharArray());
                this.SubQuery = new SearchQuery(subQuery);
            }
            catch
            {
                this.FieldName = field;
            }
        }

        public override string ToString()
        {
            return base.ToString() + " = (" + this.SubQuery.ToString() + ")";
        }
    }
}
