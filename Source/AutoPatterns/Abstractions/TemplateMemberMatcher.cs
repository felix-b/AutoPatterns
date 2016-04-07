using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AutoPatterns.Abstractions
{
    public abstract class TemplateMemberMatcher
    {
        public abstract bool Match(MemberInfo member);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual MemberTypes MemberTypes => MemberTypes.All;
    }
}
