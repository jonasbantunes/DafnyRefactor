namespace DafnyRefactor.MoveMethod
{
    public static class MoveMethodErrorMsg
    {
        public static string DestClassDoesntExist(string methodName)
        {
            return $"Error: \"{methodName}()\" doesn't belong to a defined class.";
        }

        public static string InvalidPosition()
        {
            return "Error: Parameter position is invalid";
        }

        public static string IsBuilIn()
        {
            return "Error: target type is built-in.";
        }

        public static string IsNullable()
        {
            return "Error: target type is nullable.";
        }

        public static string MethodAlreadyExists(string methodName, string className)
        {
            return $"Error: method \"{methodName}()\" already exists on class \"{className}\".";
        }

        public static string NotFoundTarget()
        {
            return "Error: can't locate target's parameter to be moved.";
        }

        public static string WrongPositionSyntax()
        {
            return "Error: Incorrect parameter position syntax.";
        }
    }
}