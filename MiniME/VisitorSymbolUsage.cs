﻿// 
//   MiniME - http://www.toptensoftware.com/minime
// 
//   The contents of this file are subject to the license terms as 
//	 specified at the web address above.
//  
//   Software distributed under the License is distributed on an 
//   "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either express or
//   implied. See the License for the specific language governing
//   rights and limitations under the License.
// 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MiniME
{
	// Walk the AST counting usage frequency of symbols
	//  - this is part 2 of what was started by VisitorSymbolDeclaration
	class VisitorSymbolUsage : ast.IVisitor
	{
		// Constructor
		public VisitorSymbolUsage(SymbolScope rootScope)
		{
			currentScope = rootScope;
		}

		public bool OnEnterNode(MiniME.ast.Node n)
		{
			// Define name of function in outer scope, before descending
			if (n.GetType() == typeof(ast.ExprNodeFunction))
			{
				var fn = (ast.ExprNodeFunction)n;

				// Define a symbol for the new function
				if (!String.IsNullOrEmpty(fn.Name))
				{
					currentScope.Symbols.UseSymbol(fn.Name);
				}
			}

			// Descending into an inner scope
			if (n.Scope != null)
			{
				System.Diagnostics.Debug.Assert(n.Scope.OuterScope == currentScope);
				currentScope = n.Scope;
			}

			// Identifier?
			if (n.GetType() == typeof(ast.ExprNodeIdentifier))
			{
				var m = (ast.ExprNodeIdentifier)n;
				if (m.Lhs == null)
				{
					currentScope.Symbols.UseSymbol(m.Name);
				}
				else
				{
					currentScope.Members.UseSymbol(m.Name);
				}
			}

			// Use catch clause exception variables in the inner scope
			if (n.GetType() == typeof(ast.CatchClause))
			{
				var cc = (ast.CatchClause)n;
				currentScope.Symbols.UseSymbol(cc.ExceptionVariable);
				return true;
			}

			// Use variables in the current scope
			if (n.GetType() == typeof(ast.StatementVariableDeclaration))
			{
				var vardecl = (ast.StatementVariableDeclaration)n;
				foreach (var v in vardecl.Variables)
				{
					currentScope.Symbols.UseSymbol(v.Name);
				}
				return true;
			}

			// Use parameters in the current scope
			if (n.GetType() == typeof(ast.Parameter))
			{
				var p = (ast.Parameter)n;
				currentScope.Symbols.UseSymbol(p.Name);
				return true;
			}

			// Look for assignment to undefined variable
			if (n.GetType() == typeof(ast.ExprNodeAssignment))
			{
				var rtlOp = (ast.ExprNodeAssignment)n;
				if (rtlOp.Op == Token.assign)
				{
					if (rtlOp.Lhs.GetType() == typeof(ast.ExprNodeIdentifier))
					{
						var identifier = (ast.ExprNodeIdentifier)rtlOp.Lhs;
						if (identifier.Lhs == null)
						{
							// Assignment to an identifier
							if (currentScope.FindSymbol(identifier.Name) == null)
							{
								currentScope.Compiler.RecordWarning(identifier.Bookmark, "assignment to undeclared variable `{0}` introduces new global variable", identifier.Name);
							}
						}
					}
				}
			}

			return true;

		}

		public void OnLeaveNode(MiniME.ast.Node n)
		{
			if (n.Scope != null)
			{
				currentScope = n.Scope.OuterScope;
			}
		}

		public SymbolScope currentScope = null;
	}
}
