﻿namespace TodoLists.App.Models;

public class TodoItem
{
    public long Id { get; set; }
    public long ProfileId { get; set; } public Profile Profile { get; set; } = null!;
    public string? Name { get; set; }
    public bool IsComplete { get; set; }
}