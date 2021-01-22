namespace DafnyRefactor.ExtractVariable
{
    public static class ExtractVariableErrorMsg
    {
        public static string CantReplaceOccurrences()
        {
            return "Error: Can't replace all occurrences of selected expression.";
        }

        public static string EndsWithBinExp()
        {
            return "Error: selected expression ends with a operand.";
        }

        public static string ExprInvalid()
        {
            return "Error: Selected expression is invalid";
        }

        public static string NegationSlice()
        {
            return "Error: start of selection is slicing a negation operator.";
        }

        public static string NotAnExpr()
        {
            return "Error: selection is not an expression.";
        }

        public static string NotFoundStmt()
        {
            return "Error: Couldn't find selected expression.";
        }

        public static string ObjectSlice()
        {
            return "Error: EvUserSelection should start on beginning of object";
        }

        public static string RangeOutOfBounds()
        {
            return "Error: Selection range out of bounds from source code.";
        }

        public static string StartsWithBinExp()
        {
            return "Error: selected expression starts with a binary operand.";
        }

        public static string WrongRange()
        {
            return "Error: Selection range is invalid.";
        }

        public static string WrongRangeSyntax()
        {
            return "Error: Incorrect selection range syntax.";
        }
    }
}