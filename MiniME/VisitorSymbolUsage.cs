﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MiniME
{
	class VisitorSymbolUsage : ast.IVisitor
	{
		public VisitorSymbolUsage(SymbolScope rootScope)
		{
			currentScope = rootScope;
		}

		public void OnEnterNode(MiniME.ast.Node n)
		{
			// Descending into an inner scope
			if (n.Scope != null)
			{
				System.Diagnostics.Debug.Assert(n.Scope.OuterScope == currentScope);
				currentScope = n.Scope;
			}

			if (n.GetType() == typeof(ast.ExprNodeMember))
			{
				var m = (ast.ExprNodeMember)n;
				if (m.Lhs == null)
				{
					UseSymbol(m.Name);
				}
			}
		}

		public void OnLeaveNode(MiniME.ast.Node n)
		{
			if (n.Scope != null)
			{
				currentScope = n.Scope.OuterScope;
			}
		}

		void UseSymbol(string str)
		{
			currentScope.Symbols.UseSymbol(str);
		}

		public SymbolScope currentScope = null;
	}
}