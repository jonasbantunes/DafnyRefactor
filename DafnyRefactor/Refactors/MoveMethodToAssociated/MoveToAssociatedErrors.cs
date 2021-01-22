namespace DafnyRefactor.MoveMethodToAssociated
{
    public class MoveToAssociatedErrors
    {
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

        public static string MethodHasNoClass(string methodName)
        {
            return $"Error: \"{methodName}()\" doesn't belong to a defined class.";
        }

        public static string NotConstantBySolver()
        {
            return
                $"Error: field is not constant according with theorem prover.";
        }

        public static string NotFoundField()
        {
            return "Error: can't locate field to move method.";
        }

        public static string NotFoundMethod()
        {
            return "Error: can't locate method to be moved.";
        }

        public static string WrongPositionSyntax()
        {
            return "Error: Incorrect parameter position syntax.";
        }
    }
}