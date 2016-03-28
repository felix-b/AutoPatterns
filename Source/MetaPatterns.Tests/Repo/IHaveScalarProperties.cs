using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetaPatterns.Tests.Repo
{
    public interface IHaveScalarProperties
    {
        int IntValue { get; set; }
        string StringValue { get; set; }
        DayOfWeek EnumValue { get; set; }
        TimeSpan TimeSpanValue { get; set; }
    }
}
