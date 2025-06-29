using Newtonsoft.Json.Linq;
using OpenQA.Selenium;

namespace TodoLists.Tests.Integration.Tests.MainPage;

public class TodoItemRenameTests
{
    [Test]
    public async Task RenameTodoItem_ClickCellAndChangeNameThenPressEnter_ShouldUpdateTodoItemName()
    {
        await TestsDecorators.Default(new TestDecoratorOptions<PageObject.MainPage>
        {
            SetUpAsync = async context =>
            {
                using var httpClient = await TestDataBuilder.CreateHttpClientAndAuthenticateAsync(context.ProfileName, TestDataBuilder.DefaultUserName);
                
                var projectId = await GetInboxProjectId(httpClient);
                await TestDataBuilder.CreateTodoItemAsync(projectId, "Original Todo Item", false, httpClient);
            },
            Test = context =>
            {
                // Wait for todo item to appear
                context.Browser.Wait.Until(_ => context.Page.TodoItemsDataGrid.Rows.Count == 1);
                
                // Verify initial todo item name
                var initialTodoItemNames = context.Page.TodoItemsDataGrid.Rows.Select(x => x.Cells[1].Text).ToList();
                Assert.That(initialTodoItemNames, Is.EqualTo(new[] { "Original Todo Item" }));
                
                // Click on the name cell to start editing
                var firstRow = context.Page.TodoItemsDataGrid.Rows[0];
                firstRow.Cells[1].Click();
                
                // Wait for the text editor to appear and become available
                context.Browser.Wait.Until(_ =>
                {
                    var textBox = firstRow.Cells[1].AsTextBox();
                    return textBox.FindElementByChain().Displayed;
                });
                
                // Edit the todo item name
                var textBox = firstRow.Cells[1].AsTextBox();
                textBox.Text = "Renamed Todo Item";
                
                // Wait for the UI to update and verify the name change
                context.Browser.Wait.Until(_ =>
                {
                    try
                    {
                        var updatedTodoItemNames =
                            context.Page.TodoItemsDataGrid.Rows.Select(x => x.Cells[1].Text).ToList();
                        return updatedTodoItemNames.Count == 1 && updatedTodoItemNames[0] == "Renamed Todo Item";
                    }
                    catch
                    {
                        return false;
                    }
                });
                
                context.Page.Refresh();
                
                // Final verification
                var finalTodoItemNames = context.Page.TodoItemsDataGrid.Rows.Select(x => x.Cells[1].Text).ToList();
                Assert.That(finalTodoItemNames, Is.EqualTo(new[] { "Renamed Todo Item" }));
            }
        });
    }

    [Test]
    public async Task RenameTodoItem_WithMultipleTodoItems_ShouldOnlyUpdateSelectedTodoItem()
    {
        await TestsDecorators.Default(new TestDecoratorOptions<PageObject.MainPage>
        {
            SetUpAsync = async context =>
            {
                using var httpClient = await TestDataBuilder.CreateHttpClientAndAuthenticateAsync(context.ProfileName, TestDataBuilder.DefaultUserName);
                
                var projectId = await GetInboxProjectId(httpClient);
                await TestDataBuilder.CreateTodoItemAsync(projectId, "Todo Item A", false, httpClient);
                await TestDataBuilder.CreateTodoItemAsync(projectId, "Todo Item B", false, httpClient);
                await TestDataBuilder.CreateTodoItemAsync(projectId, "Todo Item C", false, httpClient);
            },
            Test = context =>
            {
                // Wait for todo items to appear
                context.Browser.Wait.Until(_ => context.Page.TodoItemsDataGrid.Rows.Count == 3);
                
                // Verify initial todo item names
                var initialTodoItemNames = context.Page.TodoItemsDataGrid.Rows.Select(x => x.Cells[1].Text).ToList();
                Assert.That(initialTodoItemNames, Is.EqualTo(new[] { "Todo Item A", "Todo Item B", "Todo Item C" }));
                
                // Edit the second todo item (Todo Item B)
                var secondRow = context.Page.TodoItemsDataGrid.Rows[1];
                secondRow.Cells[1].Click();
                
                // Wait for the text editor to appear
                context.Browser.Wait.Until(_ =>
                {
                    var textBox = secondRow.Cells[1].AsTextBox();
                    return textBox.FindElementByChain().Displayed;
                });
                
                // Edit the todo item name
                var textBox = secondRow.Cells[1].AsTextBox();
                textBox.Text = "Modified Todo Item B";
                
                // Wait for the UI to update and verify only the selected todo item was renamed
                context.Browser.Wait.Until(_ =>
                {
                    var updatedTodoItemNames = context.Page.TodoItemsDataGrid.Rows.Select(x => x.Cells[1].Text).ToList();
                    return updatedTodoItemNames.Count == 3 && updatedTodoItemNames[1] == "Modified Todo Item B";
                });
                
                context.Page.Refresh();

                // Final verification - only Todo Item B should be renamed
                var finalTodoItemNames = context.Page.TodoItemsDataGrid.Rows.Select(x => x.Cells[1].Text).ToList();
                Assert.That(finalTodoItemNames, Is.EqualTo(new[] { "Todo Item A", "Modified Todo Item B", "Todo Item C" }));
            }
        });
    }

    [Test]
    public async Task RenameTodoItem_VeryLongName_ShouldHandleGracefully()
    {
        await TestsDecorators.Default(new TestDecoratorOptions<PageObject.MainPage>
        {
            SetUpAsync = async context =>
            {
                using var httpClient = await TestDataBuilder.CreateHttpClientAndAuthenticateAsync(context.ProfileName, TestDataBuilder.DefaultUserName);
                
                var projectId = await GetInboxProjectId(httpClient);
                await TestDataBuilder.CreateTodoItemAsync(projectId, "Short Name", false, httpClient);
            },
            Test = context =>
            {
                // Wait for todo item to appear
                context.Browser.Wait.Until(_ => context.Page.TodoItemsDataGrid.Rows.Count == 1);
                
                // Verify initial todo item name
                var initialTodoItemNames = context.Page.TodoItemsDataGrid.Rows.Select(x => x.Cells[1].Text).ToList();
                Assert.That(initialTodoItemNames, Is.EqualTo(new[] { "Short Name" }));
                
                // Click on the name cell to start editing
                var firstRow = context.Page.TodoItemsDataGrid.Rows[0];
                firstRow.Cells[1].Click();
                
                // Wait for the text editor to appear
                context.Browser.Wait.Until(_ =>
                {
                    var textBox = firstRow.Cells[1].AsTextBox();
                    return textBox.FindElementByChain().Displayed;
                });
                
                // Set a very long name (500 characters)
                var longName = new string('A', 500);
                var textBox = firstRow.Cells[1].AsTextBox();
                textBox.Text = longName;
                
                // Wait for the UI to update
                Thread.Sleep(1000);
                
                context.Page.Refresh();
                
                // Verify the name was either accepted or truncated/rejected appropriately
                var finalTodoItemNames = context.Page.TodoItemsDataGrid.Rows.Select(x => x.Cells[1].Text).ToList();
                Assert.That(finalTodoItemNames[0], Is.Not.Empty, "Todo item name should not be empty");
                // The system should handle this gracefully - either accept, truncate, or reject with validation
            }
        });
    }

    [Test]
    public async Task RenameTodoItem_SpecialCharacters_ShouldHandleCorrectly()
    {
        await TestsDecorators.Default(new TestDecoratorOptions<PageObject.MainPage>
        {
            SetUpAsync = async context =>
            {
                using var httpClient = await TestDataBuilder.CreateHttpClientAndAuthenticateAsync(context.ProfileName, TestDataBuilder.DefaultUserName);
                
                var projectId = await GetInboxProjectId(httpClient);
                await TestDataBuilder.CreateTodoItemAsync(projectId, "Normal Name", false, httpClient);
            },
            Test = context =>
            {
                // Wait for todo item to appear
                context.Browser.Wait.Until(_ => context.Page.TodoItemsDataGrid.Rows.Count == 1);
                
                // Verify initial todo item name
                var initialTodoItemNames = context.Page.TodoItemsDataGrid.Rows.Select(x => x.Cells[1].Text).ToList();
                Assert.That(initialTodoItemNames, Is.EqualTo(new[] { "Normal Name" }));
                
                // Click on the name cell to start editing
                var firstRow = context.Page.TodoItemsDataGrid.Rows[0];
                firstRow.Cells[1].Click();
                
                // Wait for the text editor to appear
                context.Browser.Wait.Until(_ =>
                {
                    var textBox = firstRow.Cells[1].AsTextBox();
                    return textBox.FindElementByChain().Displayed;
                });
                
                // Set name with special characters
                var specialName = "Todo with @#$%^&*()_+-=[]{}|;':\",./<>? characters";
                var textBox = firstRow.Cells[1].AsTextBox();
                textBox.Text = specialName;
                
                // Wait for the UI to update
                context.Browser.Wait.Until(_ =>
                {
                    var updatedTodoItemNames = context.Page.TodoItemsDataGrid.Rows.Select(x => x.Cells[1].Text).ToList();
                    return updatedTodoItemNames.Count == 1 && updatedTodoItemNames[0] == specialName;
                });
                
                context.Page.Refresh();
                
                // Final verification
                var finalTodoItemNames = context.Page.TodoItemsDataGrid.Rows.Select(x => x.Cells[1].Text).ToList();
                Assert.That(finalTodoItemNames, Is.EqualTo(new[] { specialName }));
            }
        });
    }

    [Test]
    public async Task RenameTodoItem_EscapeKey_ShouldCancelEditingAndRevertToOriginalName()
    {
        await TestsDecorators.Default(new TestDecoratorOptions<PageObject.MainPage>
        {
            SetUpAsync = async context =>
            {
                using var httpClient = await TestDataBuilder.CreateHttpClientAndAuthenticateAsync(context.ProfileName, TestDataBuilder.DefaultUserName);
                
                var projectId = await GetInboxProjectId(httpClient);
                await TestDataBuilder.CreateTodoItemAsync(projectId, "Original Todo", false, httpClient);
            },
            Test = context =>
            {
                // Wait for todo item to appear
                context.Browser.Wait.Until(_ => context.Page.TodoItemsDataGrid.Rows.Count == 1);
                
                // Verify initial todo item name
                var initialTodoItemNames = context.Page.TodoItemsDataGrid.Rows.Select(x => x.Cells[1].Text).ToList();
                Assert.That(initialTodoItemNames, Is.EqualTo(new[] { "Original Todo" }));
                
                // Click on the name cell to start editing
                var firstRow = context.Page.TodoItemsDataGrid.Rows[0];
                firstRow.Cells[1].Click();
                
                // Wait for the text editor to appear
                context.Browser.Wait.Until(_ =>
                {
                    var textBox = firstRow.Cells[1].AsTextBox();
                    return textBox.FindElementByChain().Displayed;
                });
                
                // Start editing but then cancel with Escape
                var textBox = firstRow.Cells[1].AsTextBox();
                var textBoxElement = textBox.FindElementByChain();
                textBoxElement.Clear();
                textBoxElement.SendKeys("Changed Text");
                textBoxElement.SendKeys(Keys.Escape);
                
                // Wait a moment for the cancel operation
                Thread.Sleep(500);
                
                // Verify the name reverted to original
                var finalTodoItemNames = context.Page.TodoItemsDataGrid.Rows.Select(x => x.Cells[1].Text).ToList();
                Assert.That(finalTodoItemNames, Is.EqualTo(new[] { "Original Todo" }));
            }
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