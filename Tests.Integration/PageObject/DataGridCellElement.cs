using OpenQA.Selenium;

namespace TodoLists.Tests.Integration.PageObject;

public class DataGridCellElement : BaseElement
{
    public DataGridCellElement(Browser browser, IEnumerable<By> webElementLocatorsChain) : base(browser, webElementLocatorsChain)
    {
    }

    public void Click()
    {
        FindElementByChain().Click();
    }

    public CheckBoxElement AsCheckBox()
    {
        return new CheckBoxElement(Browser, WebElementLocatorsChain.Append(By.CssSelector(".dx-checkbox")));
    }
}