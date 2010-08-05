﻿/*
 * MiniME
 * 
 * Copyright (C) 2010 Topten Software. Some Rights Reserved.
 * See http://toptensoftware.com/minime for licensing terms.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MiniME.ast
{
	// Represents a condiditional expression eg: conditions ? true : false
	class ExprNodeConditional : ExprNode
	{
		// Constructor
		public ExprNodeConditional(Bookmark bookmark, ExprNode condition) : base(bookmark)
		{
			Condition = condition;
		}

		// Attributes
		public ExprNode Condition;
		public ExprNode TrueResult;
		public ExprNode FalseResult;

		public override void Dump(int indent)
		{
			writeLine(indent, "if:");
			Condition.Dump(indent + 1);
			writeLine(indent, "then:");
			TrueResult.Dump(indent + 1);
			writeLine(indent, "else:");
			FalseResult.Dump(indent + 1);
		}

		public override OperatorPrecedence GetPrecedence()
		{
			return OperatorPrecedence.conditional;
		}

		public override bool Render(RenderContext dest)
		{
			WrapAndRender(dest, Condition, false);
			dest.Append('?');
			WrapAndRender(dest, TrueResult, false);
			dest.Append(':');
			WrapAndRender(dest, FalseResult, false);
			return true;
		}

		public override void OnVisitChildNodes(IVisitor visitor)
		{
			Condition.Visit(visitor);
			TrueResult.Visit(visitor);
			FalseResult.Visit(visitor);
		}

	}
}
