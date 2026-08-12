using OpenQA.Selenium;

namespace TodoLists.Tests.Integration.PageObject.Elements;

public class DataGridElement : BaseElement
{
    public ButtonElement AddRowButton { get; }
    public TextBoxElement TextEditor { get; }

    public DataGridElement(Browser browser, IEnumerable<By> webElementLocatorsChain)
        : base(browser, webElementLocatorsChain)
    {
        AddRowButton = new ButtonElement(Browser, WebElementLocatorsChain.Append(By.CssSelector(".se-add-row-button")));
        TextEditor = new TextBoxElement(Browser, WebElementLocatorsChain.Append(By.CssSelector(".se-text-editor-input")));
        CloneButton = new ButtonElement(browser, WebElementLocatorsChain.Append(By.CssSelector(".se-clone-button")));
    }

    public List<DataGridRowElement> Rows
    {
        get
        {
            var rowItemCssSelectorText = ".se-data-row";
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

    public ButtonElement CloneButton { get; }

    public void DragRowBeforePosition(int sourceRowIndex, int targetRowIndex)
    {
        var draggableRows = Rows;
        if (sourceRowIndex >= draggableRows.Count || targetRowIndex >= draggableRows.Count)
        {
            throw new ArgumentException("Row index out of bounds");
        }

        var sourceRow = draggableRows[sourceRowIndex];
        var targetRow = draggableRows[targetRowIndex];

        sourceRow.DragBeforeRow(targetRow);
    }

    public void DragRowAfterPosition(int sourceRowIndex, int targetRowIndex)
    {
        var draggableRows = Rows;
        if (sourceRowIndex >= draggableRows.Count || targetRowIndex >= draggableRows.Count)
        {
            throw new ArgumentException("Row index out of bounds");
        }

        var sourceRow = draggableRows[sourceRowIndex];
        var targetRow = draggableRows[targetRowIndex];

        sourceRow.DragAfterRow(targetRow);
    }

}
