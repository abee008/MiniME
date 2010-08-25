﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MiniME
{
	// AST Visitor to create the SymbolScope object heirarchy
	//  - scopes are created for functions and catch clauses
	//  - also detects evil and marks scopes as such
	class VisitorScopeBuilder : ast.IVisitor
	{
		// Constructor
		public VisitorScopeBuilder(SymbolScope rootScope)
		{
			m_Scopes.Push(rootScope);
		}

		public bool OnEnterNode(MiniME.ast.Node n)
		{
			// Is it a function?
			if (n.GetType() == typeof(ast.ExprNodeFunction) || n.GetType() == typeof(ast.CatchClause))
			{
				n.Scope = new SymbolScope(n, Accessibility.Private);

				// Add this function to the parent function's list of nested functions
				m_Scopes.Peek().InnerScopes.Add(n.Scope);
				n.Scope.OuterScope = m_Scopes.Peek();

				// Enter scope
				m_Scopes.Push(n.Scope);

				return true;
			}

			// Is it an evil?
			if (n.GetType() == typeof(ast.StatementWith))
			{
				if (n.Bookmark.warnings)
					Console.WriteLine("{0}: warning: use of `with` statement prevents local symbol obfuscation of all containing scopes", n.Bookmark);

				m_Scopes.Peek().DefaultAccessibility = Accessibility.Public;
				return true;
			}

			// More evil
			if (n.GetType() == typeof(ast.ExprNodeIdentifier))
			{
				var m = (ast.ExprNodeIdentifier)n;
				if (m.Lhs == null && m.Name == "eval")
				{
					if (n.Bookmark.warnings)
						Console.WriteLine("{0}: warning: use of `eval` prevents local symbol obfuscation of all containing scopes", n.Bookmark);

					m_Scopes.Peek().DefaultAccessibility = Accessibility.Public;
				}
				return true;
			}

			// Private member declaration?
			if (n.GetType() == typeof(ast.StatementAccessibility))
			{
				var p = (ast.StatementAccessibility)n;
				foreach (var s in p.Specs)
				{
					m_Scopes.Peek().AddAccessibilitySpec(p.Bookmark, s);
				}
			}

			// Try to guess name of function by assignment in variable declaration
			var decl = n as ast.StatementVariableDeclaration;
			if (decl != null)
			{
				foreach (var i in decl.Variables)
				{
					if (i.InitialValue!=null)
					{
						var fn = i.InitialValue.RootNode as ast.ExprNodeFunction;
						if (fn != null)
						{
							fn.AssignedToName = i.Name;
						}
					}
				}
			}

			// For any functions that are assigned to something else, store the target
			// of that assignment. We use this to guess the name of anonymous functions.
			var assignment = n as ast.ExprNodeAssignment;
			if (assignment != null)
			{
				var fn = assignment.Rhs as ast.ExprNodeFunction;
				if (fn != null)
				{
					var id = assignment.Lhs as ast.ExprNodeIdentifier;
					if (id!=null)
						fn.AssignedToName = id.Name;
				}
			}

			// Find functions assigned to object literals
			var objLiteral = n as ast.ExprNodeObjectLiteral;
			if (objLiteral != null)
			{
				foreach (var i in objLiteral.Values)
				{
					var fn = i.Value as ast.ExprNodeFunction;
					if (fn != null)
					{
						var id = i.Key as ast.ExprNodeIdentifier;
						if (id != null)
						{
							fn.AssignedToName = id.Name;
						}
					}
				}
			}

			return true;
		}

		public void OnLeaveNode(MiniME.ast.Node n)
		{
			if (n.Scope!=null)
			{
				System.Diagnostics.Debug.Assert(m_Scopes.Peek() == n.Scope);

				// Check if scope contained evil
				Accessibility innerAccessibility = m_Scopes.Peek().DefaultAccessibility;

				// Pop the stack
				m_Scopes.Pop();

				// Propagate evil
				if (innerAccessibility==Accessibility.Public)
					m_Scopes.Peek().DefaultAccessibility = Accessibility.Public;
			}
		}

		public Stack<SymbolScope> m_Scopes=new Stack<SymbolScope>();
	}
}
