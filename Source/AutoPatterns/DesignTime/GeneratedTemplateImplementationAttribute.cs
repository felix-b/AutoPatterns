using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPatterns.DesignTime
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class GeneratedTemplateImplementationAttribute : Attribute
    {
        public int Hash { get; set; }
    }
}
