namespace TodoLists.App.Entities;

public class ProjectTemplate
{
    public long Id { get; set; }
    public long ProfileId { get; set; }
    public Profile Profile { get; set; } = null!;
    public string? Name { get; set; }
    public ICollection<ProjectTemplateTask> Tasks { get; set; } = null!;
}
