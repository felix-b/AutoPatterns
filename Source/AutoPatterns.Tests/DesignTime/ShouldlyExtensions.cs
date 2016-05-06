using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Formatting;
using NUnit.Framework;
using Shouldly;

namespace AutoPatterns.Tests.DesignTime
{
    public static class ShouldlyExtensions
    {
        public static void ShouldNotContainAnalyzerDiagnostics(
            this IEnumerable<Diagnostic> actual,
            DiagnosticAnalyzer analyzer)
        {
            ShouldMatchAnalyzerDiagnostics(actual, analyzer, new DesignTimeUnitTestBase.DiagnosticExpectation[0]);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Checks each of the actual Diagnostics found and compares them with the corresponding DiagnosticResult in the array of expected results.
        /// Diagnostics are considered equal only if the DiagnosticResultLocation, Id, Severity, and Message of the DiagnosticResult match the actual diagnostic.
        /// </summary>
        /// <param name="actual">The Diagnostics found by the compiler after running the analyzer on the source code</param>
        /// <param name="analyzer">The analyzer that was being run on the sources</param>
        /// <param name="expected">Diagnostic Results that should have appeared in the code</param>
        public static void ShouldMatchAnalyzerDiagnostics(
            this IEnumerable<Diagnostic> actual,
            DiagnosticAnalyzer analyzer,
            params DesignTimeUnitTestBase.DiagnosticExpectation[] expected)
        {
            int expectedCount = expected.Count();
            int actualCount = actual.Count();

            actualCount.ShouldBe(expectedCount, () => {
                string diagnosticsOutput = actual.Any() ? FormatDiagnostics(analyzer, actual.ToArray()) : "NONE.";
                return $"Mismatch between number of diagnostics returned. Diagnostics:\r\n\t{diagnosticsOutput}\r\n";
            });

            for (int i = 0 ; i < expected.Length ; i++)
            {
                var actualItem = actual.ElementAt(i);
                var expectedItem = expected[i];

                if (expectedItem.Line == -1 && expectedItem.Column == -1)
                {
                    actualItem.Location.ShouldBe(
                        Location.None, 
                        $"Expected project diagnostic with no location, but found: \r\n" +
                        $"\t{FormatDiagnostics(analyzer, actualItem)}.\r\n");
                }
                else
                {
                    VerifyDiagnosticLocation(analyzer, actualItem, actualItem.Location, expectedItem.Locations.First());
                    var additionalLocations = actualItem.AdditionalLocations.ToArray();

                    additionalLocations.Length.ShouldBe(
                        expectedItem.Locations.Length - 1,
                        $"Expected {expectedItem.Locations.Length - 1} additional locations but got {additionalLocations.Length} for Diagnostic:\r\n" +
                        $"\t{FormatDiagnostics(analyzer, actualItem)}\r\n");

                    for (int j = 0 ; j < additionalLocations.Length ; ++j)
                    {
                        VerifyDiagnosticLocation(analyzer, actualItem, additionalLocations[j], expectedItem.Locations[j + 1]);
                    }
                }

                actualItem.Id.ShouldBe(
                    expectedItem.Id,
                    $"Expected diagnostic id to be \"{expectedItem.Id}\", but was \"{actualItem.Id}\"\r\n" +
                    $"\r\nDiagnostic:\r\n\t{FormatDiagnostics(analyzer, actualItem)}\r\n");

                actualItem.Severity.ShouldBe(
                    expectedItem.Severity,
                    $"Expected diagnostic severity to be \"{expectedItem.Severity}\", but was \"{actualItem.Severity}\"\r\n" +
                    $"\r\nDiagnostic:\r\n\t{FormatDiagnostics(analyzer, actualItem)}\r\n");

                if (expectedItem.Message != null)
                { 
                    actualItem.GetMessage().ShouldBe(
                        expectedItem.Message,
                        $"Expected diagnostic message to be \"{expectedItem.Message}\", but was \"{actualItem.GetMessage()}\"\r\n" +
                        $"\r\nDiagnostic:\r\n\t{FormatDiagnostics(analyzer, actualItem)}\r\n");
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static void ShouldBeSourceCode(this SyntaxNode actualCode, string expectedCode)
        {
            ShouldBeSourceCode(actualCode.ToFullString(), expectedCode);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static void ShouldBeSourceCode(this string actualCode, string expectedCode)
        {
            using (var actualReader = new StringReader(actualCode))
            {
                using (var expectedReader = new StringReader(expectedCode))
                {
                    int actualLineNumber = 0;
                    int expectedLineNumber = 0;

                    while (true)
                    {
                        var actualLine = ReadNextSignificantLineOfCode(actualReader, ref actualLineNumber);
                        var expectedLine = ReadNextSignificantLineOfCode(expectedReader, ref expectedLineNumber);

                        if (actualLine == null && expectedLine == null)
                        {
                            TestLibrary.Platform.ConsoleWriteLine(GetSourceCodePrinting(actualCode));
                            return;
                        }

                        actualLine.ShouldNotBeNull(
                            $"Expected EOF, but was extra line {actualLineNumber}:\r\n\t{actualLine}\r\n" +
                            GetSourceCodeAssertPrinting(actualCode, expectedCode));
                        actualLine.ShouldNotBeNull(
                            $"Expected line {actualLineNumber}, but was EOF. Expected line was:\r\n\t{expectedLine}\r\n" +
                            GetSourceCodeAssertPrinting(actualCode, expectedCode));
                        expectedLine.ShouldBe(
                            actualLine, 
                            $"At line {actualLineNumber},\r\n\r\n\texpected\r\n\r\n{expectedLine}\r\n\r\n\tbut was:\r\n\r\n{actualLine}\r\n" +
                            GetSourceCodeAssertPrinting(actualCode, expectedCode));
                    }
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static string GetSourceCodeAssertPrinting(string actualCode, string expectedCode)
        {
            return (
                "\r\n\r\n------ EXPECTED SOURCE CODE ------\r\n\r\n" +
                GetSourceCodePrinting(expectedCode) + 
                "\r\n\r\n------ ACTUAL SOURCE CODE ------\r\n\r\n" +
                GetSourceCodePrinting(actualCode) +
                "\r\n\r\n------ END OFSOURCE CODE ------\r\n\r\n");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static string GetSourceCodePrinting(string code)
        {
            var printing = new StringBuilder();

            using (var reader = new StringReader(code))
            {
                string line;
                int lineNumber = 0;

                while ((line = reader.ReadLine()) != null)
                {
                    lineNumber++;
                    printing.AppendLine($"{lineNumber:000}: {line}");
                }
            }

            return printing.ToString();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static string ReadNextSignificantLineOfCode(StringReader reader, ref int lineNumber)
        {
            string s;

            do
            {
                s = reader.ReadLine()?.Trim();

                if (s != null)
                {
                    lineNumber++;
                }
            } while (s == string.Empty);

            return s;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static void VerifyDiagnosticLocation(
            DiagnosticAnalyzer analyzer, 
            Diagnostic diagnostic, 
            Location actual, 
            DesignTimeUnitTestBase.DiagnosticResultLocation expected)
        {
            var actualSpan = actual.GetLineSpan();

            var locationsMatch = (
                actualSpan.Path == expected.Path || 
                (actualSpan.Path != null && actualSpan.Path.Contains("Test0.") && expected.Path.Contains("Test.")));

            locationsMatch.ShouldBeTrue(
                $"Expected diagnostic to be in file \"{expected.Path}\", but was actually in file \"{actualSpan.Path}\"\r\n" +
                $"\r\nDiagnostic:\r\n\t{FormatDiagnostics(analyzer, diagnostic)}\r\n");

            var actualLinePosition = actualSpan.StartLinePosition;

            // Only check line position if there is an actual line in the real diagnostic
            if (actualLinePosition.Line > 0)
            {
                actualLinePosition.Line.ShouldBe(
                    expected.Line,
                    $"Expected diagnostic to be on line \"{expected.Line}\", but was actually on line \"{actualLinePosition.Line + 1}\"\r\n" +
                    $"\r\nDiagnostic:\r\n\t{FormatDiagnostics(analyzer, diagnostic)}\r\n");
            }

            // Only check column position if there is an actual column position in the real diagnostic
            if (expected.Column > 0 && actualLinePosition.Character > 0)
            {
                (actualLinePosition.Character + 1).ShouldBe(
                    expected.Column,
                    $"Expected diagnostic to start at column \"{expected.Column}\", but was actually at column \"{actualLinePosition.Character + 1}\"\r\n" +
                    $"\r\nDiagnostic:\r\n\t{FormatDiagnostics(analyzer, diagnostic)}\r\n");
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Helper method to format a Diagnostic into an easily readable string
        /// </summary>
        /// <param name="analyzer">The analyzer that this verifier tests</param>
        /// <param name="diagnostics">The Diagnostics to be formatted</param>
        /// <returns>The Diagnostics formatted as a string</returns>
        private static string FormatDiagnostics(DiagnosticAnalyzer analyzer, params Diagnostic[] diagnostics)
        {
            var builder = new StringBuilder();
            for (int i = 0 ; i < diagnostics.Length ; ++i)
            {
                builder.AppendLine("// " + diagnostics[i].ToString());

                var analyzerType = analyzer.GetType();
                var rules = analyzer.SupportedDiagnostics;

                foreach (var rule in rules)
                {
                    if (rule != null && rule.Id == diagnostics[i].Id)
                    {
                        var location = diagnostics[i].Location;
                        if (location == Location.None)
                        {
                            builder.AppendFormat("GetGlobalResult({0}.{1})", analyzerType.Name, rule.Id);
                        }
                        else
                        {
                            Assert.IsTrue(
                                location.IsInSource,
                                $"Test base does not currently handle diagnostics in metadata locations. Diagnostic in metadata: {diagnostics[i]}\r\n");

                            string resultMethodName = diagnostics[i].Location.SourceTree.FilePath.EndsWith(".cs") ? "GetCSharpResultAt" : "GetBasicResultAt";
                            var linePosition = diagnostics[i].Location.GetLineSpan().StartLinePosition;

                            builder.AppendFormat(
                                "{0}({1}, {2}, {3}.{4})",
                                resultMethodName,
                                linePosition.Line + 1,
                                linePosition.Character + 1,
                                analyzerType.Name,
                                rule.Id);
                        }

                        if (i != diagnostics.Length - 1)
                        {
                            builder.Append(',');
                        }

                        builder.AppendLine();
                        break;
                    }
                }
            }
            return builder.ToString();
        }
    }
}