﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using NXunitConverterAnalyzer.Data;
using NXunitConverterAnalyzer.Recognizers;
using NXunitConverterAnalyzer.Walkers;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace NXunitConverterAnalyzer.Rewriters
{
    public class XunitRewriter : CSharpSyntaxRewriter
    {
        private SemanticModel _semanticModel;
        private Document _originalDocument;
        private SyntaxWalkContext<MethodDeclarationData, MethodDeclarationSyntax> _methodDeclarationContext;
        private SyntaxWalkContext<ClassDeclarationData, ClassDeclarationSyntax> _classDeclarationContext;

        public XunitRewriter()
        {
            _methodDeclarationContext = new SyntaxWalkContext<MethodDeclarationData, MethodDeclarationSyntax>(InitializeMethodDeclarationData);
            _classDeclarationContext = new SyntaxWalkContext<ClassDeclarationData, ClassDeclarationSyntax>(InitializeClassDeclarationData);
        }

        public async Task<Document> UpdateToXUnitAsync(Document document, SemanticModel semanticModel, TextSpan diagnosticSpan, CancellationToken cancellationToken)
        {
            _semanticModel = semanticModel;
            _originalDocument = document;
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

            var newRoot = Visit(root);
            return document.WithSyntaxRoot(newRoot);
        }

        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            using (_classDeclarationContext.Enter(node))
                return base.VisitClassDeclaration(node);
        }

        public override SyntaxNode VisitAttribute(AttributeSyntax node)
        {
            var symbolInfo = _semanticModel.GetSymbolInfo(node).Symbol;
            var newAttribute = base.VisitAttribute(node) as AttributeSyntax;

            if (AttributesRecognizer.IsTestAttribute(symbolInfo))
            {
                var newAttributeName = _methodDeclarationContext.Current.HasTestCase || _methodDeclarationContext.Current.HasTestCaseSourceAttribute ? "Theory" : "Fact";
                return SyntaxFactory.Attribute(SyntaxFactory.IdentifierName(newAttributeName));
            }
            if (AttributesRecognizer.IsTestCaseAttribute(symbolInfo))
            {
                return newAttribute.WithName(SyntaxFactory.IdentifierName("InlineData"));
            }
            if (AttributesRecognizer.IsTestCaseSourceAttribute(symbolInfo) && newAttribute.ArgumentList.Arguments.Count == 1)
            {
                return newAttribute.WithName(SyntaxFactory.IdentifierName("MemberData"));
            }
            return newAttribute;
        }

        public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            using (_methodDeclarationContext.Enter(node))
            {
                var newDeclaration = base.VisitMethodDeclaration(node) as MethodDeclarationSyntax;

                // Add Theory if no Test attribute to replace.
                if ((_methodDeclarationContext.Current.HasTestCase || _methodDeclarationContext.Current.HasTestCaseSourceAttribute)
                    && !_methodDeclarationContext.Current.HasTestAttribute)
                {
                    var theoryAttribute = SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("Theory"))));
                    newDeclaration = newDeclaration.WithAttributeLists(newDeclaration.AttributeLists.Insert(0, theoryAttribute));
                }
                newDeclaration = ReplaceBlocks(newDeclaration);
                return newDeclaration;
            }
        }

        public override SyntaxNode VisitUsingDirective(UsingDirectiveSyntax node)
        {
            var symbolInfo = _semanticModel.GetSymbolInfo(node.Name).Symbol;
            var inner = base.VisitUsingDirective(node);

            if (AttributesRecognizer.IsNUnitUsingDirective(symbolInfo))
            {
                return SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName("Xunit"));
            }
            return inner;
        }

        public override SyntaxNode VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            var newProperty = base.VisitPropertyDeclaration(node) as PropertyDeclarationSyntax;
            if (_classDeclarationContext.Current.TestCaseSources.Any(x => x.ValueText == newProperty.Identifier.ValueText))
            {
                if (node.Type is GenericNameSyntax genericType
                    && genericType.TypeArgumentList.Arguments.Count == 1
                    && genericType.TypeArgumentList.Arguments.First() is IdentifierNameSyntax closedType)
                {
                    var genericSymbol = _semanticModel.GetSymbolInfo(closedType).Symbol;
                    if (!AttributesRecognizer.IsTestCaseData(genericSymbol))
                        return newProperty;

                    newProperty = newProperty.WithType(genericType.WithTypeArgumentList(
                        SyntaxFactory.TypeArgumentList(
                          SyntaxFactory.SingletonSeparatedList<TypeSyntax>(
                                 SyntaxFactory.ArrayType(
                                     SyntaxFactory.PredefinedType(
                                         SyntaxFactory.Token(SyntaxKind.ObjectKeyword)
                                     ), SyntaxFactory.SingletonList(SyntaxFactory.ArrayRankSpecifier(
                                         SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>(SyntaxFactory.OmittedArraySizeExpression())))
                                 )
                             )
                          )
                        )
                    );
                }


            }

            return newProperty;
        }

        public override SyntaxNode VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
        {
            var newObjectCreation = base.VisitObjectCreationExpression(node) as ObjectCreationExpressionSyntax;

            if (!AttributesRecognizer.IsTestCaseDataCtor(_semanticModel.GetSymbolInfo(newObjectCreation).Symbol))
                return newObjectCreation;

            var arrayExprssion = SyntaxFactory.ArrayType(
                                     SyntaxFactory.PredefinedType(
                                         SyntaxFactory.Token(SyntaxKind.ObjectKeyword)
                                     ), SyntaxFactory.SingletonList(SyntaxFactory.ArrayRankSpecifier(
                                         SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>(SyntaxFactory.OmittedArraySizeExpression())))
                                 );

            IEnumerable<ExpressionSyntax> args = newObjectCreation.ArgumentList.Arguments.Select(x => x.Expression);

            var arrayInitializerExpression = SyntaxFactory.InitializerExpression(SyntaxKind.ArrayInitializerExpression,
                SyntaxFactory.SeparatedList(args));
            var arrayCreationExpression = SyntaxFactory.ArrayCreationExpression(arrayExprssion, arrayInitializerExpression);
            return arrayCreationExpression;
        }

        public override SyntaxNode VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            var symbol = _semanticModel.GetSymbolInfo(node).Symbol as IMethodSymbol;
            var innerInvocation = base.VisitInvocationExpression(node) as InvocationExpressionSyntax;
            var invocationMember = innerInvocation.Expression as MemberAccessExpressionSyntax;

            if (AssertRecognizer.IsTrueMethod(symbol) && innerInvocation.ArgumentList.Arguments.Count < 3)
                return innerInvocation.WithExpression(invocationMember.WithName(SyntaxFactory.IdentifierName("True")));

            if (AssertRecognizer.IsFalseMethod(symbol) && innerInvocation.ArgumentList.Arguments.Count < 3)
                return innerInvocation.WithExpression(invocationMember.WithName(SyntaxFactory.IdentifierName("False")));

            if (AssertRecognizer.AreEqualMethod(symbol) && innerInvocation.ArgumentList.Arguments.Count == 2)
                return innerInvocation.WithExpression(invocationMember.WithName(SyntaxFactory.IdentifierName("Equal")));

            if (AssertRecognizer.AreNotEqualMethod(symbol) && innerInvocation.ArgumentList.Arguments.Count == 2)
                return innerInvocation.WithExpression(invocationMember.WithName(SyntaxFactory.IdentifierName("NotEqual")));

            if (AssertRecognizer.IsNullMethod(symbol) && innerInvocation.ArgumentList.Arguments.Count == 1)
                return innerInvocation.WithExpression(invocationMember.WithName(SyntaxFactory.IdentifierName("Null")));

            if (AssertRecognizer.IsNotNullMethod(symbol) && innerInvocation.ArgumentList.Arguments.Count == 1)
                return innerInvocation.WithExpression(invocationMember.WithName(SyntaxFactory.IdentifierName("NotNull")));

            if (AssertRecognizer.AreSameMethod(symbol) && innerInvocation.ArgumentList.Arguments.Count == 2)
                return innerInvocation.WithExpression(invocationMember.WithName(SyntaxFactory.IdentifierName("Same")));

            if (AssertRecognizer.AreNotSameMethod(symbol) && innerInvocation.ArgumentList.Arguments.Count == 2)
                return innerInvocation.WithExpression(invocationMember.WithName(SyntaxFactory.IdentifierName("NotSame")));

            if (AssertRecognizer.IsEmptyMethod(symbol)
                && innerInvocation.ArgumentList.Arguments.Count == 1
                && symbol is IMethodSymbol isEmptyMethodSymbol)
            {
                if (NetStandardRecognizer.IsIEnumerableParameter(isEmptyMethodSymbol.Parameters.First().Type))
                    return innerInvocation.WithExpression(invocationMember.WithName(SyntaxFactory.IdentifierName("Empty")));
                if (NetStandardRecognizer.IsStringParameter(isEmptyMethodSymbol.Parameters.First().Type))
                {
                    var invocation = innerInvocation.WithExpression(invocationMember.WithName(SyntaxFactory.IdentifierName("Equal")));
                    invocation = invocation.WithArgumentList(invocation.ArgumentList.WithArguments(invocation.ArgumentList.Arguments.Add(
                        SyntaxFactory.Argument(GetStringEmpty()))));
                    return invocation;
                }
            }

            if (AssertRecognizer.IsNotEmptyMethod(symbol)
                && innerInvocation.ArgumentList.Arguments.Count == 1
                && symbol is IMethodSymbol isNotEmptyMethodSymbol)
            {
                if (NetStandardRecognizer.IsIEnumerableParameter(isNotEmptyMethodSymbol.Parameters.First().Type))
                    return innerInvocation.WithExpression(invocationMember.WithName(SyntaxFactory.IdentifierName("NotEmpty")));
                if (NetStandardRecognizer.IsStringParameter(isNotEmptyMethodSymbol.Parameters.First().Type))
                {
                    var invocation = innerInvocation.WithExpression(invocationMember.WithName(SyntaxFactory.IdentifierName("NotEqual")));
                    invocation = invocation.WithArgumentList(invocation.ArgumentList.WithArguments(invocation.ArgumentList.Arguments.Add(
                        SyntaxFactory.Argument(GetStringEmpty()))));
                    return invocation;
                }
            }

            if (AssertRecognizer.ZeroMethod(symbol) && innerInvocation.ArgumentList.Arguments.Count == 1)
                return WithRenameWithFirstParamter(innerInvocation, invocationMember, "Equal", GetZero());

            if (AssertRecognizer.NotZeroMethod(symbol) && innerInvocation.ArgumentList.Arguments.Count == 1)
                return WithRenameWithFirstParamter(innerInvocation, invocationMember, "NotEqual", GetZero());

            if (AssertRecognizer.PassMethod(symbol) && innerInvocation.ArgumentList.Arguments.Count < 2)
                return WithRenameWithFirstParamter(innerInvocation, invocationMember, "True", GetTrue());

            if (AssertRecognizer.FailMethod(symbol) && innerInvocation.ArgumentList.Arguments.Count < 2)
                return WithRenameWithFirstParamter(innerInvocation, invocationMember, "True", GetFalse());

            if (AssertRecognizer.ThrowsMethod(symbol) && innerInvocation.ArgumentList.Arguments.Count == 1)
                return WrapInAction(innerInvocation.ArgumentList.Arguments.First().Expression, innerInvocation);

            if (AssertRecognizer.DoesNotThrowMethod(symbol) && innerInvocation.ArgumentList.Arguments.Count == 1)
                return GetInnerLambda(innerInvocation.ArgumentList.Arguments.First().Expression, innerInvocation);

            if (AssertRecognizer.ThrowsAsyncMethod(symbol) && innerInvocation.ArgumentList.Arguments.Count == 1)
                return SyntaxFactory.AwaitExpression(innerInvocation);

            if (AssertRecognizer.DoesNotThrowAsyncMethod(symbol) && innerInvocation.ArgumentList.Arguments.Count == 1)
                return GetInnerLambda(innerInvocation.ArgumentList.Arguments.First().Expression, innerInvocation);

            if (AssertRecognizer.IsInstanceOfMethod(symbol) && innerInvocation.ArgumentList.Arguments.Count == 1)
                return RewriteInstanceOf(innerInvocation, invocationMember);

            if (AssertRecognizer.IsNotInstanceOfMethod(symbol) && innerInvocation.ArgumentList.Arguments.Count == 1)
                return RewriteNotInstanceOf(innerInvocation, invocationMember);

            if (AssertRecognizer.IsAssignableFromMethod(symbol) && innerInvocation.ArgumentList.Arguments.Count == 1)
                return RewriteIsAssignableFromMethod(innerInvocation, invocationMember, "True");

            if (AssertRecognizer.IsNotAssignableFromMethod(symbol) && innerInvocation.ArgumentList.Arguments.Count == 1)
                return RewriteIsAssignableFromMethod(innerInvocation, invocationMember, "False");

            return innerInvocation;
        }


        private InvocationExpressionSyntax RewriteIsAssignableFromMethod(InvocationExpressionSyntax invocationExpression, MemberAccessExpressionSyntax invocationMember, string assertMethod)
        {
            if (invocationMember.Name is GenericNameSyntax genericMethod)
            {
                var genericType = genericMethod.TypeArgumentList.Arguments.First();
                var argumentExpression = invocationExpression.ArgumentList.Arguments.First().Expression;

                var getTypeIvocation = SyntaxFactory.InvocationExpression(SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                    argumentExpression, SyntaxFactory.IdentifierName("GetType")));

                var isAssignableInvocation = SyntaxFactory.InvocationExpression(SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                    getTypeIvocation, SyntaxFactory.IdentifierName("IsAssignableFrom")),
                    SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Argument(
                        SyntaxFactory.TypeOfExpression(genericType)))));

                var newArg = SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Argument(isAssignableInvocation)));

                return invocationExpression
                    .WithExpression(invocationMember.WithName(SyntaxFactory.IdentifierName(assertMethod)))
                    .WithArgumentList(newArg);
            }
            return invocationExpression;
        }

        private InvocationExpressionSyntax RewriteNotInstanceOf(InvocationExpressionSyntax invocationExpression, MemberAccessExpressionSyntax invocationMember)
        {
            if (invocationMember.Name is GenericNameSyntax genericMethod)
            {
                var genericType = genericMethod.TypeArgumentList.Arguments.First();
                var argumentExpression = invocationExpression.ArgumentList.Arguments.First().Expression;

                var newArg = SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Argument(
                SyntaxFactory.BinaryExpression(SyntaxKind.IsExpression, argumentExpression, genericType))));

                return invocationExpression
                    .WithExpression(invocationMember.WithName(SyntaxFactory.IdentifierName("False")))
                    .WithArgumentList(newArg);
            }
            return invocationExpression;
        }

        private InvocationExpressionSyntax RewriteInstanceOf(InvocationExpressionSyntax invocationExpression, MemberAccessExpressionSyntax invocationMember)
        {
            if (invocationMember.Name is GenericNameSyntax genericMethod)
            {
                var newGenericMethod = genericMethod.WithIdentifier(SyntaxFactory.Identifier("IsAssignableFrom"));
                return invocationExpression.WithExpression(invocationMember.WithName(newGenericMethod));
            }
            return invocationExpression;
        }

        private SyntaxNode WrapInAction(ExpressionSyntax expression, InvocationExpressionSyntax invocationExpression)
        {
            var wrappedAction = SyntaxFactory.ObjectCreationExpression(SyntaxFactory.ParseTypeName("Action"),
                SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(
                    SyntaxFactory.Argument(expression))), null);

            return invocationExpression.WithArgumentList(SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(
                SyntaxFactory.Argument(wrappedAction))));
        }

        private SyntaxNode GetInnerLambda(ExpressionSyntax expression, InvocationExpressionSyntax invocationExpression)
        {
            if (expression is ParenthesizedLambdaExpressionSyntax lambda)
            {
                if (lambda.ExpressionBody != null)
                    return lambda.ExpressionBody.WithLeadingTrivia(invocationExpression.GetLeadingTrivia());
                if (lambda.Block != null)
                {
                    return invocationExpression;
                }
            }
            return invocationExpression;
        }

        private InvocationExpressionSyntax WithRenameWithFirstParamter(InvocationExpressionSyntax innerInvocation, MemberAccessExpressionSyntax isAssertFailMemberAccess, string name, ExpressionSyntax argumentExpression)
        {
            var invocation = innerInvocation.WithExpression(isAssertFailMemberAccess.WithName(SyntaxFactory.IdentifierName(name)));
            invocation = invocation.WithArgumentList(invocation.ArgumentList.WithArguments(invocation.ArgumentList.Arguments.Insert(0,
                SyntaxFactory.Argument(argumentExpression))));
            return invocation;
        }

        private MemberAccessExpressionSyntax GetStringEmpty()
        {
            return SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword)),
                SyntaxFactory.IdentifierName(nameof(string.Empty)));
        }

        private LiteralExpressionSyntax GetZero()
        {
            return SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(0));
        }

        private LiteralExpressionSyntax GetTrue() => SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression);

        private LiteralExpressionSyntax GetFalse() => SyntaxFactory.LiteralExpression(SyntaxKind.FalseLiteralExpression);

        private MethodDeclarationData InitializeMethodDeclarationData(MethodDeclarationSyntax node)
        {
            var initializer = new MethodDeclarationWalker(_semanticModel);
            return initializer.GetMethodDeclarationData(node);
        }

        private ClassDeclarationData InitializeClassDeclarationData(ClassDeclarationSyntax node)
        {
            var initializer = new ClassDeclarationWalker(_semanticModel);
            return initializer.GetClassDeclarationData(node);
        }

        private MethodDeclarationSyntax ReplaceBlocks(MethodDeclarationSyntax node)
        {
            var statements = node.Body.Statements;
            foreach (var replacement in _methodDeclarationContext.Current.BlockReplace)
            {
                var removableText = replacement.Key.ToString();
                var expressionStatementToRemove = statements.OfType<ExpressionStatementSyntax>().FirstOrDefault(x => x.Expression is InvocationExpressionSyntax && x.Expression.ToString() == removableText);
                if (expressionStatementToRemove == null)
                    continue;
                var indexOfReplacement = statements.IndexOf(expressionStatementToRemove);
                statements = statements.Remove(expressionStatementToRemove);
                statements = statements.InsertRange(indexOfReplacement, replacement.Value);
            }
            node = node.WithBody(node.Body.WithStatements(statements));
            return node;
        }

    }
}
