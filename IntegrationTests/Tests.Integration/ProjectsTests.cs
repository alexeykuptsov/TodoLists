namespace TodoLists.Tests.Integration;

public class ProjectsTests
{
  [Test]
  public async Task NewProfile01()
  {
    await TestsDecorators.Default(tc =>
    {
      // Keep default setup

      // Do nothing
            
      tc.Browser.Wait.Until(_ => tc.MainPage.ProjectNameLabel.Text == "Inbox");
      tc.Browser.Wait.Until(_ => tc.MainPage.TodoItemsDataGrid.Rows.Count == 0);
    });
  }

  [Test]
  public async Task Delete01()
  {
    await TestsDecorators.Default(tc =>
    {
      // Keep default setup

      tc.MainPage.ProjectsDataGrid.Rows[0].DeleteButton.Click();

      var expectedErrors = new[]
      {
        "It is impossible to delete the last project. There should be at least one.",
      };
      tc.Browser.WaitAndAssertThat(() => tc.MainPage.ErrorMessages, Is.EquivalentTo(expectedErrors));
    });
  }
  
  [Test]
  public async Task Add01()
  {
    await TestsDecorators.Default(tc =>
    {
      // Keep default setup

      tc.MainPage.ProjectsDataGrid.AddRowButton.Click();
      tc.Browser.Wait.Until(_ => tc.MainPage.ProjectsDataGrid.Rows.Count == 2);
      tc.MainPage.ProjectsDataGrid.TextEditor.Text = "Foo";

      tc.Browser.WaitAndAssertThat(() => tc.MainPage.ProjectsDataGrid.Rows.Select(x => x.Cells[1].Text), Is.EqualTo(new[] { "Inbox", "Foo" }));
    });
  }
  
  [Test]
  public async Task MasterDetail01()
  {
    await TestsDecorators.Default(async tc =>
    {
      // Add to-do item "foo"
      tc.MainPage.AddTodoItemNameTextBox.Text = "foo";
      tc.MainPage.AddTodoItemButton.Click();
      tc.Browser.Wait.Until(_ => tc.MainPage.TodoItemNames.Count == 1);
      // Add project "Bar"
      tc.MainPage.ProjectsDataGrid.AddRowButton.Click();
      tc.Browser.Wait.Until(_ => tc.MainPage.ProjectsDataGrid.Rows.Count == 2);
      tc.MainPage.ProjectsDataGrid.TextEditor.Text = "Bar";

      await Task.Delay(500);
      tc.MainPage.ProjectsDataGrid.Rows[1].Cells[1].Click();

      tc.Browser.WaitAndAssertThat(() => tc.MainPage.TodoItemsDataGrid.Rows.Count, Is.EqualTo(0));
    });
  }
}