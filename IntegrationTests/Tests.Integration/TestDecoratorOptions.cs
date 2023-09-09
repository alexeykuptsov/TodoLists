namespace TodoLists.Tests.Integration;

public class TestDecoratorOptions<TPage> where TPage : BasePage
{
    public Func<TestSetUpContext, Task> SetUpAsync { get; set; }
    public Action<TestSetUpContext> SetUp { get; set; }
    public Func<TestContext<TPage>, Task> TestAsync { get; set; }
    public Action<TestContext<TPage>> Test { get; set; }
}