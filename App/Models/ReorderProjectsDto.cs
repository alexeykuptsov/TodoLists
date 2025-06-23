namespace TodoLists.App.Models;

public class ReorderProjectsDto
{
    public long[] ProjectIds { get; set; } = Array.Empty<long>();
}