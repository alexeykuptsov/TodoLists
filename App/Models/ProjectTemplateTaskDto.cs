namespace TodoLists.App.Models;

public class ProjectTemplateTaskDto
{
    public long Id { get; set; }
    public long ProjectTemplateId { get; set; }
    public string? Name { get; set; }
}
