using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;

namespace TodoLists.Tests.Integration.PageObject.Elements;

public class DataGridRowElement : BaseElement
{
    public DataGridRowElement(Browser browser, IEnumerable<By> webElementLocatorsChain)
        : base(browser, webElementLocatorsChain)
    {
        DeleteButton = new ButtonElement(browser, WebElementLocatorsChain.Append(By.CssSelector(".se-delete-button")));
        EditButton = new ButtonElement(browser, WebElementLocatorsChain.Append(By.CssSelector(".se-edit-button")));
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
            return FindElementByChain(WebElementLocatorsChain.Append(By.CssSelector(".se-drag-handle")).ToList());
        }
    }

    public void DragBeforeRow(DataGridRowElement targetRow)
    {
        var sourceElement = DragHandle;
        var targetElement = targetRow.FindElementByChain();

        // SortableJS with forceFallback:true needs a pause after mousedown to register drag start
        var targetSize = targetElement.Size;
        var offsetY = (int)(targetSize.Height * 0.2) - targetSize.Height / 2;

        var actions = new Actions(Browser.Driver);
        actions.ClickAndHold(sourceElement)
            .Pause(TimeSpan.FromMilliseconds(400))
            .MoveByOffset(0, 2)
            .MoveToElement(targetElement, 0, offsetY)
            .Pause(TimeSpan.FromMilliseconds(100))
            .Release()
            .Perform();

        Thread.Sleep(800);
    }

    public void DragAfterRow(DataGridRowElement targetRow)
    {
        var sourceElement = DragHandle;
        var targetElement = targetRow.FindElementByChain();

        // SortableJS with forceFallback:true needs a pause after mousedown to register drag start
        var targetSize = targetElement.Size;
        var offsetY = (int)(targetSize.Height * 0.8) - targetSize.Height / 2;

        var actions = new Actions(Browser.Driver);
        actions.ClickAndHold(sourceElement)
            .Pause(TimeSpan.FromMilliseconds(400))
            .MoveByOffset(0, 2)
            .MoveToElement(targetElement, 0, offsetY)
            .Pause(TimeSpan.FromMilliseconds(100))
            .Release()
            .Perform();

        Thread.Sleep(800);
    }
}