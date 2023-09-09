using Newtonsoft.Json.Linq;

namespace TodoLists.Tests.Integration
{
    public class TodoItemsTests
    {
        [Test]
        public async Task AddTest01()
        {
            await TestsDecorators.Default(tc =>
            {
                // Keep default setup

                tc.Page.AddTodoItemNameTextBox.Text = "foo";
                tc.Page.AddTodoItemButton.Click();
                tc.Browser.Wait.Until(_ => tc.Page.TodoItemNames.Count == 1);
            
                Assert.That(tc.Page.TodoItemNames, Is.EqualTo(new [] {"foo"}));
            });
        }

        [Test]
        public async Task AddTest02()
        {
            await TestsDecorators.Default(tc =>
            {
                // Keep default setup

                tc.Page.AddTodoItemNameTextBox.Text = "foo";
                tc.Page.AddTodoItemButton.Click();
                tc.Browser.Wait.Until(_ => tc.Page.TodoItemNames.Count == 1);
                tc.Page.AddTodoItemNameTextBox.Text = "bar";
                tc.Page.AddTodoItemButton.Click();
                tc.Browser.Wait.Until(_ => tc.Page.TodoItemNames.Count == 2);
            
                Assert.That(tc.Page.TodoItemNames, Is.EqualTo(new [] {"foo", "bar"}));
            });
        }

        [Test]
        public async Task UpdateTest01()
        {
            await TestsDecorators.Default(new TestDecoratorOptions<MainPage>
            {
                SetUpAsync = async tsc =>
                {
                    var httpClient = await TestDataBuilder.CreateHttpClientAndAuthenticateAsync(tsc.ProfileName, TestDataBuilder.DefaultUserName);
                    tsc.CompositeDisposable.Add(httpClient);
                    var projectId = await GetInboxProjectId(httpClient);
                    await TestDataBuilder.CreateTodoItemAsync(projectId, "foo", false, httpClient);
                },
                Test = tc =>
                {
                    tc.Browser.Wait.Until(_ => tc.Page.TodoItemsDataGrid.Rows.Count == 1);

                    tc.Page.TodoItemsDataGrid.Rows[0].Cells[0].AsCheckBox().Click();
                    tc.Page.Refresh();

                    tc.Browser.WaitAndAssertThat(() => tc.Page.TodoItemsDataGrid.Rows[0].Cells[0].AsCheckBox().Checked, Is.True);
                }
            });
        }

        [Test]
        public async Task UpdateTest02()
        {
            await TestsDecorators.Default(new TestDecoratorOptions<MainPage>
            {
                SetUpAsync = async tsc =>
                {
                    var httpClient = await TestDataBuilder.CreateHttpClientAndAuthenticateAsync(tsc.ProfileName, TestDataBuilder.DefaultUserName);
                    tsc.CompositeDisposable.Add(httpClient);
                    var projectId = await GetInboxProjectId(httpClient);
                    await TestDataBuilder.CreateTodoItemAsync(projectId, "foo", false, httpClient);
                },
                Test = tc =>
                {
                    tc.Browser.Wait.Until(_ => tc.Page.TodoItemsDataGrid.Rows.Count == 1);
            
                    var row = tc.Page.TodoItemsDataGrid.Rows[0];
                    row.Cells[1].Click();
                    tc.Browser.Wait.Until(_ => tc.Page.TodoItemsDataGrid.Rows[0].Cells[1].AsTextBox());
                    tc.Page.TodoItemsDataGrid.Rows[0].Cells[1].AsTextBox().Text = "bar";
                    tc.Page.Refresh();
                    tc.Browser.Wait.Until(_ => tc.Page.TodoItemsDataGrid.Rows.Count == 1);
                
                    Assert.That(tc.Page.TodoItemsDataGrid.Rows[0].Cells[1].Text, Is.EqualTo("bar"));
                },
            });
        }

        private async Task<long> GetInboxProjectId(HttpClient httpClient)
        {
            var response = await httpClient.GetAsync("api/Projects?projectName=Inbox");
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            return JArray.Parse(responseContent)[0]["id"]!.Value<long>();
        }
    }
}