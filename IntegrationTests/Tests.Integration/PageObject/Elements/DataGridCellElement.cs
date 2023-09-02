using OpenQA.Selenium;

namespace TodoLists.Tests.Integration.PageObject.Elements;

public class DataGridCellElement : BaseElement
{
    public DataGridCellElement(Browser browser, IEnumerable<By> webElementLocatorsChain) : base(browser, webElementLocatorsChain)
    {
    }

    public string Text => FindElementByChain().Text;

    public void Click()
    {
        FindElementByChain().Click();
    }

    public CheckBoxElement AsCheckBox()
    {
        return new CheckBoxElement(Browser, WebElementLocatorsChain.Append(By.CssSelector(".dx-checkbox")));
    }

    public TextBoxElement AsTextBox()
    {
        return new TextBoxElement(Browser, WebElementLocatorsChain.Append(By.CssSelector(".dx-textbox input")));
    }
}