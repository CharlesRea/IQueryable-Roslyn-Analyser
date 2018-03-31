using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace IQueryableToIEnumerableAnalyser
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class IQueryableToIEnumerableAnalyser : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "IQueryableToIEnumerableAnalyser";
        
        private static readonly LocalizableString Title = "IQueryable implicitly converted to IEnumerable";
        private static readonly LocalizableString MessageFormat = "{0} is an {1}, but is being implicitly converted to an {2}";
        private const string Category = "Usage";

        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyseInvocationExpression, SyntaxKind.InvocationExpression);
        }

        private static readonly IReadOnlyCollection<string> AllowedConversionMethodNames = new [] { nameof(Enumerable.ToList), nameof(Enumerable.AsEnumerable) };
        
        private static void AnalyseInvocationExpression(SyntaxNodeAnalysisContext context)
        {
            var invocationExpressionSyntax = context.Node as InvocationExpressionSyntax;
            
            if (invocationExpressionSyntax?.Expression is MemberAccessExpressionSyntax memberAccess)
            {
                var memberAccessTypeInfo = context.SemanticModel.GetTypeInfo(memberAccess.Expression);

                if (!Equals(memberAccessTypeInfo.Type, memberAccessTypeInfo.ConvertedType) &&
                    IsQueryable(memberAccessTypeInfo.Type, context) &&
                    IsEnumerable(memberAccessTypeInfo.ConvertedType, context) &&
                    !AllowedConversionMethodNames.Contains((memberAccess.Name as IdentifierNameSyntax)?.Identifier.ValueText))
                {
                    ReportIQueryableImplicitConversionDiagnostic(context, memberAccess, memberAccessTypeInfo);
                }
            }
        }

        private static bool IsQueryable(ITypeSymbol type, SyntaxNodeAnalysisContext context)
        {
            var queryableType = context.Compilation.GetTypeByMetadataName("System.Linq.IQueryable`1");
            return Equals(type?.OriginalDefinition, queryableType);
        }

        private static bool IsEnumerable(ITypeSymbol type, SyntaxNodeAnalysisContext context)
        {
            var enumerableType = context.Compilation.GetTypeByMetadataName("System.Collections.Generic.IEnumerable`1");
            return Equals(type?.OriginalDefinition, enumerableType);
        }

        private static void ReportIQueryableImplicitConversionDiagnostic(SyntaxNodeAnalysisContext context,
            MemberAccessExpressionSyntax memberAccessExpressionSyntax, TypeInfo memberAccessTypeInfo)
        {
            var memberAccessName = (memberAccessExpressionSyntax.Expression as IdentifierNameSyntax)?.Identifier.ValueText;
            var originalTypeDisplayString = memberAccessTypeInfo.Type.ToDisplayString();
            var convertedTypeDisplayString = memberAccessTypeInfo.ConvertedType.ToDisplayString();

            var diagnostic = Diagnostic.Create(Rule, memberAccessExpressionSyntax.GetLocation(), memberAccessName,
                originalTypeDisplayString, convertedTypeDisplayString);

            context.ReportDiagnostic(diagnostic);
        }
    }
}
