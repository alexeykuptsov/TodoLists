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
            
      tc.Browser.Wait.Until(_ => tc.Page.ProjectNameLabel.Text == "Inbox");
      tc.Browser.Wait.Until(_ => tc.Page.TodoItemsDataGrid.Rows.Count == 0);
    });
  }

  [Test]
  public async Task Delete01()
  {
    await TestsDecorators.Default(tc =>
    {
      // Keep default setup

      tc.Page.ProjectsDataGrid.Rows[0].DeleteButton.Click();

      var expectedErrors = new[]
      {
        "It is impossible to delete the last project. There should be at least one.",
      };
      tc.Browser.WaitAndAssertThat(() => tc.Page.ErrorMessages, Is.EquivalentTo(expectedErrors));
    });
  }

  [Test]
  public async Task Delete02()
  {
    await TestsDecorators.Default(new TestDecoratorOptions<MainPage>
    {
      SetUpAsync = async tsc =>
      {
        var httpClient = await TestDataBuilder.CreateHttpClientAndAuthenticateAsync(tsc.ProfileName, TestDataBuilder.DefaultUserName);
        tsc.CompositeDisposable.Add(httpClient);
        await TestDataBuilder.CreateProjectAsync("Foo", httpClient);
      },
      Test = tc =>
      {
        tc.Page.ProjectsDataGrid.Rows[1].DeleteButton.Click();
        tc.Browser.Wait.Until(_ => tc.Page.DeleteDialog.Displayed && tc.Page.DeleteDialog.YesButton.Displayed);
        tc.Page.DeleteDialog.YesButton.Click();
        tc.Browser.Wait.Until(_ => tc.Page.ProjectsDataGrid.Rows.Count == 1);
        tc.Page.Refresh();

        tc.Browser.WaitAndAssertThat(() => tc.Page.ProjectsDataGrid.Rows.Select(x => x.Cells[1].Text), Is.EqualTo(new[] { "Inbox" }));
      },
    });
  }
  
  [Test]
  public async Task Add01()
  {
    await TestsDecorators.Default(tc =>
    {
      // Keep default setup

      tc.Page.ProjectsDataGrid.AddRowButton.Click();
      tc.Browser.Wait.Until(_ => tc.Page.ProjectsDataGrid.Rows.Count == 2);
      tc.Page.ProjectsDataGrid.TextEditor.Text = "Foo";

      tc.Browser.WaitAndAssertThat(
        () => tc.Page.ProjectsDataGrid.Rows.Select(x => x.Cells[1].Text),
        Is.EqualTo(new[] { "Inbox", "Foo" }));
    });
  }
  
  [Test]
  public async Task MasterDetail01()
  {
    await TestsDecorators.Default(new TestDecoratorOptions<MainPage>
    {
      TestAsync = async tc =>
      {
        // Add to-do item "foo"
        tc.Page.AddTodoItemNameTextBox.Text = "foo";
        tc.Page.AddTodoItemButton.Click();
        tc.Browser.Wait.Until(_ => tc.Page.TodoItemNames.Count == 1);
        // Add project "Bar"
        tc.Page.ProjectsDataGrid.AddRowButton.Click();
        tc.Browser.Wait.Until(_ => tc.Page.ProjectsDataGrid.Rows.Count == 2);
        tc.Page.ProjectsDataGrid.TextEditor.Text = "Bar";

        await Task.Delay(500);
        tc.Page.ProjectsDataGrid.Rows[1].Cells[1].Click();

        tc.Browser.WaitAndAssertThat(() => tc.Page.TodoItemsDataGrid.Rows.Count, Is.EqualTo(0));
      },
    });
  }
}