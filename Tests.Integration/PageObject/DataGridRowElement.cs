using OpenQA.Selenium;

namespace TodoLists.Tests.Integration.PageObject;

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