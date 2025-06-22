using TodoLists.Tests.Integration.PageObject.Elements;

namespace TodoLists.Tests.Integration;

public class ProjectDragAndDropTests
{
    [Test]
    public async Task DragAndDrop_MoveProjectUp_ShouldReorderProjectsInUI()
    {
        await TestsDecorators.Default(new TestDecoratorOptions<MainPage>
        {
            SetUpAsync = async context =>
            {
                using var httpClient = await TestDataBuilder.CreateHttpClientAndAuthenticateAsync(context.ProfileName, TestDataBuilder.DefaultUserName);
                
                // Create test projects in specific order
                await TestDataBuilder.RenameProjectAsync("Inbox", "Project A", httpClient);
                await TestDataBuilder.CreateProjectAsync("Project B", httpClient);
                await TestDataBuilder.CreateProjectAsync("Project C", httpClient);
            },
            Test = context =>
            {
                // Verify initial order
                var initialOrder = context.Page.ProjectsDataGrid.Rows.Select(x => x.Cells[1].Text).ToList();
                Assert.That(initialOrder, Is.EqualTo(new[] { "Project A", "Project B", "Project C" }));
                
                // Drag Project C before Project A (move up)
                DragRowByNameBefore(context.Page.ProjectsDataGrid, "Project C", "Project A");
                
                // Wait for UI to update
                context.Browser.Wait.Until(_ =>
                {
                    var newOrder = context.Page.ProjectsDataGrid.Rows.Select(x => x.Cells[1].Text).ToList();
                    return newOrder.Count == 3 && newOrder[0] == "Project C";
                });
                
                // Verify new order
                var finalOrder = context.Page.ProjectsDataGrid.Rows.Select(x => x.Cells[1].Text).ToList();
                Assert.That(finalOrder, Is.EqualTo(new[] { "Project C", "Project A", "Project B" }));
            }
        });
    }

    [Test]
    public async Task DragAndDrop_MoveProjectDown_ShouldReorderProjectsInUI()
    {
        await TestsDecorators.Default(new TestDecoratorOptions<MainPage>
        {
            SetUpAsync = async context =>
            {
                using var httpClient = await TestDataBuilder.CreateHttpClientAndAuthenticateAsync(context.ProfileName, TestDataBuilder.DefaultUserName);
                
                // Create test projects in specific order
                await TestDataBuilder.RenameProjectAsync("Inbox", "First Project", httpClient);
                await TestDataBuilder.CreateProjectAsync("Second Project", httpClient);
                await TestDataBuilder.CreateProjectAsync("Third Project", httpClient);
            },
            Test = context =>
            {
                // Verify initial order
                var initialOrder = context.Page.ProjectsDataGrid.Rows.Select(x => x.Cells[1].Text).ToList();
                Assert.That(initialOrder, Is.EqualTo(new[] { "First Project", "Second Project", "Third Project" }));
                
                // Drag First Project after Third Project (move down)
                DragRowByNameAfter(context.Page.ProjectsDataGrid, "First Project", "Third Project");
                
                // Wait for UI to update
                context.Browser.Wait.Until(_ =>
                {
                    var newOrder = context.Page.ProjectsDataGrid.Rows.Select(x => x.Cells[1].Text).ToList();
                    return newOrder.Count == 3 && newOrder[2] == "First Project";
                });
                
                // Verify new order
                var finalOrder = context.Page.ProjectsDataGrid.Rows.Select(x => x.Cells[1].Text).ToList();
                Assert.That(finalOrder, Is.EqualTo(new[] { "Second Project", "Third Project", "First Project" }));
            }
        });
    }

    [Test]
    public async Task DragAndDrop_MoveProjectToMiddle_ShouldReorderProjectsInUI()
    {
        await TestsDecorators.Default(new TestDecoratorOptions<MainPage>
        {
            SetUpAsync = async context =>
            {
                using var httpClient = await TestDataBuilder.CreateHttpClientAndAuthenticateAsync(context.ProfileName, TestDataBuilder.DefaultUserName);
                
                // Create test projects in specific order
                await TestDataBuilder.RenameProjectAsync("Inbox", "Alpha", httpClient);
                await TestDataBuilder.CreateProjectAsync("Beta", httpClient);
                await TestDataBuilder.CreateProjectAsync("Gamma", httpClient);
                await TestDataBuilder.CreateProjectAsync("Delta", httpClient);
            },
            Test = context =>
            {
                // Verify initial order
                var initialOrder = context.Page.ProjectsDataGrid.Rows.Select(x => x.Cells[1].Text).ToList();
                Assert.That(initialOrder, Is.EqualTo(new[] { "Alpha", "Beta", "Gamma", "Delta" }));
                
                // Drag Delta before Gamma (move to middle)
                DragRowByNameBefore(context.Page.ProjectsDataGrid, "Delta", "Gamma");
                
                // Wait for UI to update
                context.Browser.Wait.Until(_ =>
                {
                    var newOrder = context.Page.ProjectsDataGrid.Rows.Select(x => x.Cells[1].Text).ToList();
                    return newOrder.Count == 4 && newOrder[2] == "Delta";
                });
                
                // Verify new order
                var finalOrder = context.Page.ProjectsDataGrid.Rows.Select(x => x.Cells[1].Text).ToList();
                Assert.That(finalOrder, Is.EqualTo(new[] { "Alpha", "Beta", "Delta", "Gamma" }));
            }
        });
    }

    [Test]
    public async Task DragAndDrop_MultipleOperations_ShouldMaintainCorrectOrder()
    {
        await TestsDecorators.Default(new TestDecoratorOptions<MainPage>
        {
            SetUpAsync = async context =>
            {
                using var httpClient = await TestDataBuilder.CreateHttpClientAndAuthenticateAsync(context.ProfileName, TestDataBuilder.DefaultUserName);
                
                // Create test projects
                await TestDataBuilder.RenameProjectAsync("Inbox", "Project 1", httpClient);
                await TestDataBuilder.CreateProjectAsync("Project 2", httpClient);
                await TestDataBuilder.CreateProjectAsync("Project 3", httpClient);
                await TestDataBuilder.CreateProjectAsync("Project 4", httpClient);
                await TestDataBuilder.CreateProjectAsync("Project 5", httpClient);
            },
            Test = context =>
            {
                // Verify initial order
                var initialOrder = context.Page.ProjectsDataGrid.Rows.Select(x => x.Cells[1].Text).ToList();
                Assert.That(initialOrder, Is.EqualTo(new[] { "Project 1", "Project 2", "Project 3", "Project 4", "Project 5" }));
                
                // First operation: Move Project 5 to the beginning
                DragRowByNameBefore(context.Page.ProjectsDataGrid, "Project 5", "Project 1");
                
                // Wait for first operation to complete
                context.Browser.Wait.Until(_ =>
                {
                    var newOrder = context.Page.ProjectsDataGrid.Rows.Select(x => x.Cells[1].Text).ToList();
                    return newOrder.Count == 5 && newOrder[0] == "Project 5";
                });
                
                // Verify intermediate order
                var intermediateOrder = context.Page.ProjectsDataGrid.Rows.Select(x => x.Cells[1].Text).ToList();
                Assert.That(intermediateOrder, Is.EqualTo(new[] { "Project 5", "Project 1", "Project 2", "Project 3", "Project 4" }));
                
                // Second operation: Move Project 2 after Project 4
                DragRowByNameAfter(context.Page.ProjectsDataGrid, "Project 2", "Project 4");
                
                // Wait for second operation to complete
                context.Browser.Wait.Until(_ =>
                {
                    var newOrder = context.Page.ProjectsDataGrid.Rows.Select(x => x.Cells[1].Text).ToList();
                    return newOrder.Count == 5 && newOrder[4] == "Project 2";
                });
                
                // Verify final order
                var finalOrder = context.Page.ProjectsDataGrid.Rows.Select(x => x.Cells[1].Text).ToList();
                Assert.That(finalOrder, Is.EqualTo(new[] { "Project 5", "Project 1", "Project 3", "Project 4", "Project 2" }));
            }
        });
    }

    [Test]
    public async Task DragAndDrop_WithIndexBasedOperations_ShouldReorderCorrectly()
    {
        await TestsDecorators.Default(new TestDecoratorOptions<MainPage>
        {
            SetUpAsync = async context =>
            {
                using var httpClient = await TestDataBuilder.CreateHttpClientAndAuthenticateAsync(context.ProfileName, TestDataBuilder.DefaultUserName);
                
                // Create test projects
                await TestDataBuilder.RenameProjectAsync("Inbox", "Item A", httpClient);
                await TestDataBuilder.CreateProjectAsync("Item B", httpClient);
                await TestDataBuilder.CreateProjectAsync("Item C", httpClient);
            },
            Test = context =>
            {
                // Verify initial order
                var initialOrder = context.Page.ProjectsDataGrid.Rows.Select(x => x.Cells[1].Text).ToList();
                Assert.That(initialOrder, Is.EqualTo(new[] { "Item A", "Item B", "Item C" }));
                
                // Move item at index 2 (Item C) before item at index 0 (Item A)
                context.Page.ProjectsDataGrid.DragRowBeforePosition(2, 0);
                
                // Wait for UI to update
                context.Browser.Wait.Until(_ =>
                {
                    var newOrder = context.Page.ProjectsDataGrid.Rows.Select(x => x.Cells[1].Text).ToList();
                    return newOrder.Count == 3 && newOrder[0] == "Item C";
                });
                
                // Verify new order
                var finalOrder = context.Page.ProjectsDataGrid.Rows.Select(x => x.Cells[1].Text).ToList();
                Assert.That(finalOrder, Is.EqualTo(new[] { "Item C", "Item A", "Item B" }));
            }
        });
    }


    private void DragRowByNameBefore(DataGridElement dataGrid, string sourceProjectName, string targetProjectName)
    {
        var projectNames = dataGrid.Rows.Select(x => x.Cells[1].Text).ToList();
        var sourceIndex = projectNames.IndexOf(sourceProjectName);
        var targetIndex = projectNames.IndexOf(targetProjectName);

        if (sourceIndex == -1)
            throw new ArgumentException($"Source project '{sourceProjectName}' not found");
        if (targetIndex == -1)
            throw new ArgumentException($"Target project '{targetProjectName}' not found");

        dataGrid.DragRowBeforePosition(sourceIndex, targetIndex);
    }

    private void DragRowByNameAfter(DataGridElement dataGrid, string sourceProjectName, string targetProjectName)
    {
        var projectNames = dataGrid.Rows.Select(x => x.Cells[1].Text).ToList();
        var sourceIndex = projectNames.IndexOf(sourceProjectName);
        var targetIndex = projectNames.IndexOf(targetProjectName);

        if (sourceIndex == -1)
            throw new ArgumentException($"Source project '{sourceProjectName}' not found");
        if (targetIndex == -1)
            throw new ArgumentException($"Target project '{targetProjectName}' not found");

        dataGrid.DragRowAfterPosition(sourceIndex, targetIndex);
    }
}