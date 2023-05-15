using OpenQA.Selenium;

namespace TodoLists.Tests.Integration.PageObject;

public class DataGridElement : BaseElement
{
    public DataGridElement(Browser browser, IEnumerable<By> webElementLocatorsChain)
        : base(browser, webElementLocatorsChain)
    {
    }

    public List<DataGridRowElement> Rows
    {
        get
        {
            var rowItemCssSelectorText = ".dx-data-row";
            var rowElements = FindElementsByChain(WebElementLocatorsChain.Append(By.CssSelector(rowItemCssSelectorText)).ToList());
            var result = new List<DataGridRowElement>();
            for (int i = 0; i < rowElements.Count; i++)
            {
                var nthRowSelector = By.CssSelector(rowItemCssSelectorText + $":nth-child({i + 1})");
                var locatorsChain = WebElementLocatorsChain.Append(nthRowSelector).ToList();
                result.Add(new DataGridRowElement(Browser, locatorsChain));
            }
            return result;
        }
    }
}

public class DataGridRowElement : BaseElement
{
    public DataGridRowElement(Browser browser, IEnumerable<By> webElementLocatorsChain)
        : base(browser, webElementLocatorsChain)
    {
    }

    public List<DataGridCellElement> Cells
    {
        get
        {
            var cellItemCssSelectorText = "td";
            var cellElements = FindElementsByChain(WebElementLocatorsChain.Append(By.CssSelector(cellItemCssSelectorText)).ToList());
            var result = new List<DataGridCellElement>();
            for (int i = 0; i < cellElements.Count; i++)
            {
                var cellRowSelector = By.CssSelector(cellItemCssSelectorText + $":nth-child({i + 1})");
                var locatorsChain = WebElementLocatorsChain.Append(cellRowSelector).ToList();
                result.Add(new DataGridCellElement(Browser, locatorsChain));
            }
            return result;
        }
    }
}

public class DataGridCellElement : BaseElement
{
    public DataGridCellElement(Browser browser, IEnumerable<By> webElementLocatorsChain) : base(browser, webElementLocatorsChain)
    {
    }

    public void Click()
    {
        FindElementByChain().Click();
    }
}