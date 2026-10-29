
using System.Collections.Concurrent;
using G3_StudentTaskManagementAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace G3_StudentTaskManagementAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    // Tasks are stored in memory and cleared when the API restarts.
    private static readonly ConcurrentDictionary<Guid, TaskItem> _tasks = new();

    // READ ALL: GET /api/tasks
    [HttpGet]
    public ActionResult<IEnumerable<TaskItem>> GetAll(
        [FromQuery] string? status,
        [FromQuery] string? search,
        [FromQuery] string? sort)
    {
        IEnumerable<TaskItem> query = _tasks.Values;

        if (status == "pending")
        {
            query = query.Where(t => !t.Completed);
        }
        else if (status == "completed")
        {
            query = query.Where(t => t.Completed);
        }
        else if (!string.IsNullOrEmpty(status) && status != "all")
        {
            return BadRequest(new
            {
                message = "status must be pending, completed, or all."
            });
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(t =>
                t.Title.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                t.Description.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        query = sort switch
        {
            "newest" => query.OrderByDescending(t => t.CreatedAt),
            "priority" => query.OrderBy(t => t.Completed)
                               .ThenByDescending(t => t.Priority)
                               .ThenBy(t => t.Deadline),
            _ => query.OrderBy(t => t.Completed)
                      .ThenBy(t => t.Deadline)
                      .ThenByDescending(t => t.CreatedAt)
        };

        return Ok(query.ToList());
    }

    // READ ONE: GET /api/tasks/{id}
    [HttpGet("{id:guid}")]
    public ActionResult<TaskItem> GetById(Guid id)
    {
        if (!_tasks.TryGetValue(id, out var task))
        {
            return NotFound(new { message = "Task not found." });
        }

        return Ok(task);
    }

    // CREATE: POST /api/tasks
    [HttpPost]
    public ActionResult<TaskItem> Create(TaskRequest request)
    {
        if (request.Deadline!.Value <
            DateOnly.FromDateTime(DateTime.Today))
        {
            ModelState.AddModelError(
                nameof(request.Deadline),
                "Deadline must be today or a later date.");

            return ValidationProblem(ModelState);
        }

        var now = DateTime.UtcNow;

        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = request.Title!.Trim(),
            Description = request.Description?.Trim() ?? string.Empty,
            Deadline = request.Deadline.Value,
            Priority = request.Priority,
            Completed = request.Completed,
            CreatedAt = now,
            UpdatedAt = now
        };

        _tasks[task.Id] = task;

        return CreatedAtAction(
            nameof(GetById),
            new { id = task.Id },
            task);
    }

    // UPDATE: PUT /api/tasks/{id}
    [HttpPut("{id:guid}")]
    public ActionResult<TaskItem> Update(Guid id, TaskRequest request)
    {
        if (!_tasks.TryGetValue(id, out var task))
        {
            return NotFound(new { message = "Task not found." });
        }

        task.Title = request.Title!.Trim();
        task.Description = request.Description?.Trim() ?? string.Empty;
        task.Deadline = request.Deadline!.Value;
        task.Priority = request.Priority;
        task.Completed = request.Completed;
        task.UpdatedAt = DateTime.UtcNow;

        return Ok(task);
    }

    // COMPLETE TASK: PATCH /api/tasks/{id}/complete
    [HttpPatch("{id:guid}/complete")]
    public ActionResult<TaskItem> Complete(Guid id)
    {
        if (!_tasks.TryGetValue(id, out var task))
        {
            return NotFound(new { message = "Task not found." });
        }

        task.Completed = true;
        task.UpdatedAt = DateTime.UtcNow;

        return Ok(task);
    }

    // DELETE: DELETE /api/tasks/{id}
    [HttpDelete("{id:guid}")]
    public IActionResult Delete(Guid id)
    {
        if (!_tasks.TryRemove(id, out _))
        {
            return NotFound(new { message = "Task not found." });
        }

        return NoContent();
    }
}
