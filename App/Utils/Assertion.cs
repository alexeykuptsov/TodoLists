using JetBrains.Annotations;

namespace TodoLists.App.Utils;

public static class Assertion
{
    [AssertionMethod]
    public static void Assert([AssertionCondition(AssertionConditionType.IS_TRUE)] bool condition, string conditionMessage)
    {
        if (!condition)
            throw new AssertionException(conditionMessage);
    }
}
 
public class AssertionException : Exception
{
    public AssertionException(string conditionMessage) : base("Assertion failed: " + conditionMessage)
    {
    }
}