namespace TodoLists.App.Entities;

public class ProjectTemplateTask
{
    public long Id { get; set; }
    public long ProfileId { get; set; }
    public Profile Profile { get; set; } = null!;
    public long ProjectTemplateId { get; set; }
    public ProjectTemplate ProjectTemplate { get; set; } = null!;
    public string? Name { get; set; }
}
