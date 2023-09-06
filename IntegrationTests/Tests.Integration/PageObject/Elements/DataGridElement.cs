using OpenQA.Selenium;
using OpenQA.Selenium.DevTools.V85.Debugger;

namespace TodoLists.Tests.Integration.PageObject.Elements;

public class DataGridElement : BaseElement
{
    public ButtonElement AddRowButton { get; }
    public TextBoxElement TextEditor { get; } 

    public DataGridElement(Browser browser, IEnumerable<By> webElementLocatorsChain)
        : base(browser, webElementLocatorsChain)
    {
        AddRowButton = new ButtonElement(Browser, WebElementLocatorsChain.Append(By.CssSelector(".dx-datagrid-addrow-button")));
        TextEditor = new TextBoxElement(Browser, WebElementLocatorsChain.Append(By.CssSelector(".dx-texteditor-input")));
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