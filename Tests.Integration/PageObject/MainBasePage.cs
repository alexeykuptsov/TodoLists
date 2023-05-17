using OpenQA.Selenium;

namespace TodoLists.Tests.Integration.PageObject;

public class MainBasePage : BasePage
{
    public TextBoxElement AddTodoItemNameTextBox { get; }
    public ButtonElement AddTodoItemButton { get; }
    public DataGridElement TodoItemsDataGrid { get; }

    public MainBasePage(Browser browser) : base(browser)
    {
        AddTodoItemNameTextBox = new TextBoxElement(Browser, new[] { By.CssSelector("#add-name") });
        AddTodoItemButton = new ButtonElement(Browser, new[] { By.CssSelector(".se-add-todo-item-button") });
        TodoItemsDataGrid = new DataGridElement(Browser, new[] { By.CssSelector(".se-todo-item-data-grid") });
    }

    public override void WaitUntilLoaded()
    {
        base.WaitUntilLoaded();
        Browser.Wait.Until(d => d.FindElements(By.Id("se-user-name")).Count > 0);
    }

    public List<string> TodoItemNames => Browser.Driver
        .FindElements(By.CssSelector(".dx-datagrid .dx-data-row td[aria-colindex='2']")).Select(x => x.Text).ToList();
}