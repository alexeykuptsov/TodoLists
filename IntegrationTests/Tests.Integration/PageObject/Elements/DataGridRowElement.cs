using OpenQA.Selenium;

namespace TodoLists.Tests.Integration.PageObject.Elements;

public class DataGridRowElement : BaseElement
{
    public DataGridRowElement(Browser browser, IEnumerable<By> webElementLocatorsChain)
        : base(browser, webElementLocatorsChain)
    {
        DeleteButton = new ButtonElement(browser, WebElementLocatorsChain.Append(By.CssSelector(".dx-link-delete")));
    }

    public List<DataGridCellElement> Cells
    {
        get
        {
            var cellItemLocator = By.CssSelector("td");
            var cellElements = FindElementsByChain(WebElementLocatorsChain.Append(cellItemLocator).ToList());
            var result = new List<DataGridCellElement>();
            for (int i = 0; i < cellElements.Count; i++)
            {
                var cellRowSelector = By.CssSelector("td" + $":nth-child({i + 1})");
                var locatorsChain = WebElementLocatorsChain.Append(cellRowSelector).ToList();
                result.Add(new DataGridCellElement(Browser, locatorsChain));
            }
            return result;
        }
    }

    public ButtonElement DeleteButton { get; }

    public void Click()
    {
        FindElementByChain().Click();
    }
}