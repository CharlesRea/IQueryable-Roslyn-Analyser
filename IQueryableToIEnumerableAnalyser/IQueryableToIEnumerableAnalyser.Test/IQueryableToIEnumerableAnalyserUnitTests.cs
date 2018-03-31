using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace IQueryableToIEnumerableAnalyser.Test
{
    [TestClass]
    public class IQueryableToIEnumerableAnalyserTests : CodeFixVerifier
    {
        [TestMethod]
        public void EmptySource_NoDiagnostics()
        {
            var test = @"";

            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void ImplicitConversionQueryableToEnumerable_ReturnsDiagnostic()
        {
            var test = @"
using System.Linq;

namespace ExampleProject
{
    public class ExampleClass
    {
        private readonly IQueryable<int> queryable = new[] {1, 3, 4}.AsQueryable();

        public void ImplicitlyConvertToEnumerable()
        {
            var enumerable = queryable.ToDictionary(i => i);
        }
    }
}";

            var expected = BuildIQueryableDiagnostic(12, 30, "queryable", "System.Linq.IQueryable<int>", "System.Collections.Generic.IEnumerable<int>");
            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void ExplicitConversionQueryableToEnumerableToList_NoDiagnostics()
        {
            var test = @"
using System.Linq;
using System.Collections.Generic;

namespace ExampleProject
{
    public class ExampleClass
    {
        private readonly IQueryable<int> queryable = new[] {1, 3, 4}.AsQueryable();

        public void ImplicitlyConvertToEnumerable()
        {
            var enumerable = queryable.Select(i => i*2).ToList();
        }
    }
}";

            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void ExplicitConversionQueryableToEnumerableAsEnumerable_NoDiagnostics()
        {
            var test = @"
using System.Linq;
using System.Collections.Generic;

namespace ExampleProject
{
    public class ExampleClass
    {
        private readonly IQueryable<int> queryable = new[] {1, 3, 4}.AsQueryable();

        public void ImplicitlyConvertToEnumerable()
        {
            var enumerable = queryable.Select(i => i*2).AsEnumerable();
        }
    }
}";

            VerifyCSharpDiagnostic(test);
        }

        private DiagnosticResult BuildIQueryableDiagnostic(int line, int column,
            string variableName, string queryableType, string enumerableType)
        {
            return new DiagnosticResult
            {
                Id = "IQueryableToIEnumerableAnalyser",
                Message = $"{variableName} is an {queryableType}, but is being implicitly converted to an {enumerableType}",
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                        new DiagnosticResultLocation("Test0.cs", line, column)
                    }
            };
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new IQueryableToIEnumerableAnalyser();
        }
    }
}
