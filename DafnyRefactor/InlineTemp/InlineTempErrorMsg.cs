namespace DafnyRefactor.InlineTemp
{
    public static class InlineTempErrorMsg
    {
        public static string ContainsMethodCall(string varName, string position)
        {
            return $"Error: variable \"{varName}\" located on {position} contains a method call.";
        }

        public static string InitWithConstructor(string varName, string position)
        {
            return $"Error: variable \"{varName}\" located on {position} is initialized with an object constructor.";
        }

        public static string NotConstant(string varName, string position)
        {
            return $"Error: variable \"{varName}\" located on {position} is not constant.";
        }

        public static string NotConstantBySolver(string varName, string position)
        {
            return
                $"Error: variable \"{varName}\" located on {position} is not constant according with theorem prover.";
        }

        public static string NeverInitialized(string varName, string position)
        {
            return $"Error: variable \"{varName}\" located on {position} is never initialized.";
        }

        public static string NotFoundVariable(string position)
        {
            return $"Error: can't locate local variable on {position}.";
        }

        public static string WrongPositionSyntax()
        {
            return "Error: Incorrect variable position syntax.";
        }

        public static string InvalidPosition()
        {
            return "Error: Variable position is invalid";
        }
    }
}