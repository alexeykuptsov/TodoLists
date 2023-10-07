namespace TodoLists.App.Entities;

public class Project
{
    public long Id { get; set; }
    public long ProfileId { get; set; } public Profile Profile { get; set; } = null!;
    public string? Name { get; set; }
    public bool IsDeleted { get; set; }
}