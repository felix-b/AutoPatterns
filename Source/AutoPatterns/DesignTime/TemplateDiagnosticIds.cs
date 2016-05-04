using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPatterns.DesignTime
{
    public static class TemplateDiagnosticIds
    {
        /// <summary>
        /// APT01: Pattern template implementation was not generated.
        /// </summary>
        public const string TemplateIsNotImplemented = "APT01";

        /// <summary>
        /// APT02: Pattern template implementation may be out of date and needs to be regenerated.
        /// </summary>
        public const string TemplateImplementationIsOutOfDate = "APT02";
    }
}
