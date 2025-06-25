using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;

namespace TodoLists.Tests.Integration.PageObject.Elements;

public class DataGridRowElement : BaseElement
{
    public DataGridRowElement(Browser browser, IEnumerable<By> webElementLocatorsChain)
        : base(browser, webElementLocatorsChain)
    {
        DeleteButton = new ButtonElement(browser, WebElementLocatorsChain.Append(By.CssSelector(".dx-link-delete")));
        EditButton = new ButtonElement(browser, WebElementLocatorsChain.Append(By.CssSelector(".dx-link-edit")));
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
    public ButtonElement EditButton { get; }

    public void Click()
    {
        FindElementByChain().Click();
    }
    
    
    public IWebElement DragHandle
    {
        get
        {
            return FindElementByChain(WebElementLocatorsChain.Append(By.CssSelector(".dx-datagrid-drag-icon")).ToList());
        }
    }

    public void DragBeforeRow(DataGridRowElement targetRow)
    {
        var sourceElement = DragHandle;
        var targetElement = targetRow.FindElementByChain();
        
        // Calculate position at 1/5 from the top of the target row
        var targetSize = targetElement.Size;
        var targetLocation = targetElement.Location;
        var offsetY = (int)(targetSize.Height * 0.2); // 1/5 from top
        
        var actions = new Actions(Browser.Driver);
        actions.ClickAndHold(sourceElement)
            .MoveToElement(targetElement, 0, offsetY - (targetSize.Height / 2)) // Offset from center
            .Release()
            .Perform();
        
        // Wait for the drag operation to complete
        Thread.Sleep(500);
    }

    public void DragAfterRow(DataGridRowElement targetRow)
    {
        var sourceElement = DragHandle;
        var targetElement = targetRow.FindElementByChain();
        
        // Calculate position at 1/5 from the bottom of the target row
        var targetSize = targetElement.Size;
        var targetLocation = targetElement.Location;
        var offsetY = (int)(targetSize.Height * 0.8); // 4/5 from top (1/5 from bottom)
        
        var actions = new Actions(Browser.Driver);
        actions.ClickAndHold(sourceElement)
            .MoveToElement(targetElement, 0, offsetY - (targetSize.Height / 2)) // Offset from center
            .Release()
            .Perform();
        
        // Wait for the drag operation to complete
        Thread.Sleep(500);
    }
}