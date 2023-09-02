﻿namespace TodoLists.App.Models;

public class TodoItemDto
{
    public long Id { get; set; }
    public long ProjectId { get; set; }
    public string? Name { get; set; }
    public bool IsComplete { get; set; }
}