namespace TodoLists.App.Templates;

public static class TemplateCatalog
{
    public static readonly IReadOnlyList<Template> All =
    [
        new Template("shopping-list", "Shopping list", ["Milk", "Eggs", "Bread"]),
    ];

    public static Template? FindById(string id) =>
        All.FirstOrDefault(t => t.Id == id);
}
