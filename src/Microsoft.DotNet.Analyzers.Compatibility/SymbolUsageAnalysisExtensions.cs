﻿using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

// This class isn't actually defining an analyzer. It's simply providing
// extension methods for real analyzers.
//
// Thus, we supress
//
//      RS1012: Start action has no registered actions.
//
#pragma warning disable RS1012

namespace Microsoft.DotNet.Analyzers.Compatibility
{
    public static class SymbolUsageAnalysisExtensions
    {
        public static void RegisterSymbolUsageAction(this AnalysisContext context, Action<SymbolUsageAnalysisContext> action)
        {
            RegisterSyntaxNodeAction(context.RegisterSyntaxNodeAction, action);
        }

        public static void RegisterSymbolUsageAction(this CompilationStartAnalysisContext context, Action<SymbolUsageAnalysisContext> action)
        {
            RegisterSyntaxNodeAction(context.RegisterSyntaxNodeAction, action);
        }

        private static void RegisterSyntaxNodeAction(Action<Action<SyntaxNodeAnalysisContext>, SyntaxKind[]> registrationAction, Action<SymbolUsageAnalysisContext> action)
        {
            registrationAction(
                nodeContext => Handle(nodeContext, action),
                new[]
                {
                    SyntaxKind.IdentifierName,
                    SyntaxKind.ObjectCreationExpression,

                    // These are the list of operators that can result in
                    // custom operators:

                    SyntaxKind.AddExpression,
                    SyntaxKind.SubtractExpression,
                    SyntaxKind.MultiplyExpression,
                    SyntaxKind.DivideExpression,
                    SyntaxKind.ModuloExpression,
                    SyntaxKind.LeftShiftExpression,
                    SyntaxKind.RightShiftExpression,
                    SyntaxKind.LogicalOrExpression,
                    SyntaxKind.LogicalAndExpression,
                    SyntaxKind.BitwiseOrExpression,
                    SyntaxKind.BitwiseAndExpression,
                    SyntaxKind.ExclusiveOrExpression,
                    SyntaxKind.EqualsExpression,
                    SyntaxKind.NotEqualsExpression,
                    SyntaxKind.LessThanExpression,
                    SyntaxKind.LessThanOrEqualExpression,
                    SyntaxKind.GreaterThanExpression,
                    SyntaxKind.GreaterThanOrEqualExpression,

                    SyntaxKind.AddAssignmentExpression,
                    SyntaxKind.SubtractAssignmentExpression,
                    SyntaxKind.MultiplyAssignmentExpression,
                    SyntaxKind.DivideAssignmentExpression,
                    SyntaxKind.ModuloAssignmentExpression,
                    SyntaxKind.AndAssignmentExpression,
                    SyntaxKind.ExclusiveOrAssignmentExpression,
                    SyntaxKind.OrAssignmentExpression,
                    SyntaxKind.LeftShiftAssignmentExpression,
                    SyntaxKind.RightShiftAssignmentExpression,

                    SyntaxKind.UnaryPlusExpression,
                    SyntaxKind.UnaryMinusExpression,
                    SyntaxKind.BitwiseNotExpression,
                    SyntaxKind.LogicalNotExpression,
                    SyntaxKind.PreIncrementExpression,
                    SyntaxKind.PreDecrementExpression,
                    SyntaxKind.PostIncrementExpression,
                    SyntaxKind.PostDecrementExpression
                }
            );
        }

        private static void Handle(SyntaxNodeAnalysisContext context, Action<SymbolUsageAnalysisContext> action)
        {
            SyntaxKind syntaxKind = context.Node.Kind();
            switch (syntaxKind)
            {
                case SyntaxKind.IdentifierName:
                case SyntaxKind.ObjectCreationExpression:
                case SyntaxKind.AddExpression:
                case SyntaxKind.SubtractExpression:
                case SyntaxKind.MultiplyExpression:
                case SyntaxKind.DivideExpression:
                case SyntaxKind.ModuloExpression:
                case SyntaxKind.LeftShiftExpression:
                case SyntaxKind.RightShiftExpression:
                case SyntaxKind.LogicalOrExpression:
                case SyntaxKind.LogicalAndExpression:
                case SyntaxKind.BitwiseOrExpression:
                case SyntaxKind.BitwiseAndExpression:
                case SyntaxKind.ExclusiveOrExpression:
                case SyntaxKind.EqualsExpression:
                case SyntaxKind.NotEqualsExpression:
                case SyntaxKind.LessThanExpression:
                case SyntaxKind.LessThanOrEqualExpression:
                case SyntaxKind.GreaterThanExpression:
                case SyntaxKind.GreaterThanOrEqualExpression:
                case SyntaxKind.AddAssignmentExpression:
                case SyntaxKind.SubtractAssignmentExpression:
                case SyntaxKind.MultiplyAssignmentExpression:
                case SyntaxKind.DivideAssignmentExpression:
                case SyntaxKind.ModuloAssignmentExpression:
                case SyntaxKind.AndAssignmentExpression:
                case SyntaxKind.ExclusiveOrAssignmentExpression:
                case SyntaxKind.OrAssignmentExpression:
                case SyntaxKind.LeftShiftAssignmentExpression:
                case SyntaxKind.RightShiftAssignmentExpression:
                case SyntaxKind.UnaryPlusExpression:
                case SyntaxKind.UnaryMinusExpression:
                case SyntaxKind.BitwiseNotExpression:
                case SyntaxKind.LogicalNotExpression:
                case SyntaxKind.PreIncrementExpression:
                case SyntaxKind.PreDecrementExpression:
                case SyntaxKind.PostIncrementExpression:
                case SyntaxKind.PostDecrementExpression:
                    Handle(context, (ExpressionSyntax)context.Node, action);
                    break;
                default:
                    throw new NotImplementedException($"Unexpected node. Kind = {context.Node.Kind()}");
            }
        }

        private static void Handle(SyntaxNodeAnalysisContext context, ExpressionSyntax node, Action<SymbolUsageAnalysisContext> action)
        {
            var symbolInfo = context.SemanticModel.GetSymbolInfo(node);
            var symbol = symbolInfo.Symbol;

            // No point in checking unresolved symbols.
            if (symbol == null)
                return;

            // We don't want to check symbols that aren't best matches.
            if (symbolInfo.CandidateReason != CandidateReason.None)
                return;

            // We don't want to handle generic instantiations, we only
            // care about the original definitions.
            symbol = symbol.OriginalDefinition;

            // We don't want handle synthetic extensions, we only care
            // about the original static declaration.
            if (symbol.Kind == SymbolKind.Method && symbol is IMethodSymbol methodSymbol && methodSymbol.ReducedFrom != null)
            {
                symbol = methodSymbol.ReducedFrom;
            }
            else if (symbol.Kind == SymbolKind.Property && symbol is IPropertySymbol propertySymbol)
            {
                // For properties figure out if it is the getter or setter and then use the corresponding method
                // Many different expressions can have the property as a getter, so the code below attempts to identify all setters.
                var isSetter = false;
                SyntaxNode parent = node.Parent;
                if (parent is MemberAccessExpressionSyntax)
                {
                    isSetter = parent.Parent is AssignmentExpressionSyntax assignmentExpression && assignmentExpression.Left == parent;
                }
                else if ((parent is AssignmentExpressionSyntax assigmentExpression && assigmentExpression.Left == node) || parent is NameEqualsSyntax)
                {
                    isSetter = true;
                }

                // It is possible that neither the setter nor the getter actually exists but the null check after this block takes care of that.
                // For the cases that we are interested the method must exist, direct operations on backing fields are not interesting to our analysis.
                symbol = isSetter ? propertySymbol.SetMethod : propertySymbol.GetMethod;
                if (symbol == null)
                    return;
            }

            // We don't want to check symbols defined in source.
            if (symbol.DeclaringSyntaxReferences.Any())
                return;

            var symbolUsageContext = new SymbolUsageAnalysisContext(context, symbol);
            action(symbolUsageContext);
        }
    }
}
