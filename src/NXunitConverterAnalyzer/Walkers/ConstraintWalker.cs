using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NXunitConverterAnalyzer.Walkers
{
    public enum ContraintType
    {
        None = 0,
        Is = 1,
        Throws = 2
    }

    public class ConstraintData
    {
        public ContraintType Constraint { get; set; } = ContraintType.None;
        public string ConstraintMode { get; set; }
        public bool IsInverted { get; set; }
        public TypeSyntax ConstraintGenericType { get; set; }
        public ArgumentSyntax ConstraintArgument { get; set; }
    }


    public class ConstraintWalker : CSharpSyntaxWalker
    {
        private SyntaxWalkContext<ConstraintData> _constratintContext;

        public ConstraintWalker()
        {
            _constratintContext = new SyntaxWalkContext<ConstraintData>();
        }

        public ConstraintData GetConstraintData(SyntaxNode invocationExpression)
        {
            using (_constratintContext.Enter())
            {
                Visit(invocationExpression);
                return _constratintContext.Current;
            }
        }

        public override void VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
        {
            base.VisitMemberAccessExpression(node);

            if (node.Expression is IdentifierNameSyntax identifierExpression)
            {
                if (identifierExpression.Identifier.ValueText == "Is")
                    _constratintContext.Current.Constraint = ContraintType.Is;

                if (identifierExpression.Identifier.ValueText == "Throws")
                {
                    _constratintContext.Current.Constraint = ContraintType.Throws;
                    // If Nothing we have no Assert statement.
                    //Otherwise Throws.ArgumentNullException case
                    if (node.Name.Identifier.ValueText != "Nothing")
                    {
                        _constratintContext.Current.ConstraintMode = "Throws";
                        _constratintContext.Current.ConstraintGenericType = SyntaxFactory.ParseTypeName(node.Name.Identifier.ValueText);
                    }
                    // For example Throws.TypeOf<...>() case
                    if (node.Name is GenericNameSyntax genericName
                        && genericName.TypeArgumentList.Arguments.First() is IdentifierNameSyntax closedType) // It is never a predefined type, all Exception dervice from Exception
                    {
                        _constratintContext.Current.ConstraintMode = "Throws";
                        _constratintContext.Current.ConstraintGenericType = SyntaxFactory.ParseTypeName(closedType.Identifier.ValueText);
                    }
                }

            }

            if (_constratintContext.Current.ConstraintMode == null
                && _constratintContext.Current.Constraint == ContraintType.Is)
            {
                if (node.Name.Identifier.ValueText == "Not")
                    _constratintContext.Current.IsInverted = true;

                if (node.Name.Identifier.ValueText == "EqualTo")
                    _constratintContext.Current.ConstraintMode = _constratintContext.Current.IsInverted ?
                        "NotEqual" : "Equal";

                if (node.Name.Identifier.ValueText == "True")
                    _constratintContext.Current.ConstraintMode = _constratintContext.Current.IsInverted ?
                        "False" : "True";

                if (node.Name.Identifier.ValueText == "False")
                    _constratintContext.Current.ConstraintMode = _constratintContext.Current.IsInverted ?
                        "True" : "False";

                if (node.Name.Identifier.ValueText == "TypeOf")
                    _constratintContext.Current.ConstraintMode = _constratintContext.Current.IsInverted ?
                        "IsNotType" : "IsType";

                if (node.Name.Identifier.ValueText == "Empty")
                    _constratintContext.Current.ConstraintMode = _constratintContext.Current.IsInverted ?
                        "NotEmpty" : "Empty";

                if (node.Name.Identifier.ValueText == "Null")
                    _constratintContext.Current.ConstraintMode = _constratintContext.Current.IsInverted ?
                        "NotNull" : "Null";

                if (_constratintContext.Current.ConstraintMode != null
                    && _constratintContext.Current.ConstraintGenericType == null
                    && node.Name is GenericNameSyntax genericIdentifier
                    && genericIdentifier.TypeArgumentList.Arguments.First() is TypeSyntax closedTypeName)
                {
                    // Re-parse the type so it converts int to Int32
                    _constratintContext.Current.ConstraintGenericType = closedTypeName;
                }

                // For example EqualsTo(5), we capture the argument.
                if (_constratintContext.Current.ConstraintMode != null
                    && _constratintContext.Current.ConstraintArgument == null
                    && node.Parent is InvocationExpressionSyntax methodContraint
                    && methodContraint.ArgumentList.Arguments.Any())
                {
                    _constratintContext.Current.ConstraintArgument = methodContraint.ArgumentList.Arguments.First();
                }
            }
        }
    }
}
