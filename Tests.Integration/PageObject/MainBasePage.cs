using System.Collections;
using OpenQA.Selenium;

namespace TodoLists.Tests.Integration.PageObject;

public class MainBasePage : BasePage
{
    public InputElement AddTodoItemNameInput { get; }
    public ButtonElement AddTodoItemButton { get; }

    public MainBasePage(Browser browser) : base(browser)
    {
        AddTodoItemNameInput = new InputElement(Browser, new[] { By.CssSelector("#add-name") });
        AddTodoItemButton = new ButtonElement(Browser, new[] { By.CssSelector(".se-add-todo-item-button") });
    }

    public override void WaitUntilLoaded()
    {
        base.WaitUntilLoaded();
        Browser.Wait.Until(d => d.FindElement(By.Id("se-user-name")));
    }

    public List<string> TodoItemNames => Browser.Driver
        .FindElements(By.CssSelector(".dx-datagrid .dx-data-row td[aria-colindex='2']")).Select(x => x.Text).ToList();
}