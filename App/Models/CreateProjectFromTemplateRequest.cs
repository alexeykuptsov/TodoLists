namespace TodoLists.App.Models;

public class CreateProjectFromTemplateRequest
{
    public string Name { get; set; } = "";
    public long TemplateId { get; set; }
}
