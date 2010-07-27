﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MiniME.ast
{
	class StatementFor : Statement
	{
		public StatementFor()
		{
		}

		public override void Dump(int indent)
		{
			writeLine(indent, "for loop init:");
			if (Initialize != null)
				Initialize.Dump(indent + 1);
			else
				writeLine(indent+1, "n/a");

			writeLine(indent, "condition:");
			if (Condition!=null)
				Condition.Dump(indent+1);
			else
				writeLine(indent+1, "n/a");

			writeLine(indent, "increment:");
			if (Increment!=null)
				Increment.Dump(indent + 1);
			else
				writeLine(indent+1, "n/a");

			writeLine(indent, "do:");
			CodeBlock.Dump(indent + 1);
		}

		public override bool Render(RenderContext dest)
		{
			dest.Append("for(");
			if (Initialize != null)
				Initialize.Render(dest);
			dest.Append(";");
			if (Condition != null)
				Condition.Render(dest);
			dest.Append(";");
			if (Increment != null)
				Increment.Render(dest);
			dest.Append(")");
			return CodeBlock.RenderIndented(dest);
		}

		public override void OnVisitChildNodes(IVisitor visitor)
		{
			if (Initialize!=null)
				Initialize.Visit(visitor);
			else
				Condition.Visit(visitor);

			Increment.Visit(visitor);
			CodeBlock.Visit(visitor);
		}


		public Statement Initialize;
		public ExpressionNode Condition;
		public ExpressionNode Increment;
		public Statement CodeBlock;
	}
}