using System.ComponentModel.DataAnnotations;

namespace G3_StudentTaskManagementAPI.Models;

// Used for both Create (POST) and Update (PUT)
public class TaskRequest
{
    [Required(ErrorMessage = "Title is required.")]
    [StringLength(100, ErrorMessage = "Title must be 100 characters or fewer.")]
    public string? Title { get; set; }

    [StringLength(1000, ErrorMessage = "Description must be 1000 characters or fewer.")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Deadline is required.")]
    public DateOnly? Deadline { get; set; }

    public TaskPriority Priority { get; set; } = TaskPriority.Medium;

    public bool Completed { get; set; }
}