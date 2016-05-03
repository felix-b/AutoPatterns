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
        /// APT01: Pattern template was not preprocessed.
        /// </summary>
        public const string TemplateWasNotPreprocessed = "APT01";

        /// <summary>
        /// APT02: Preprocessed template might need to be refreshed.
        /// </summary>
        public const string PreprocessedTemplateNeedsRefresh = "APT02";
    }
}
