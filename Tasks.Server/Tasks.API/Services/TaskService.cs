using Microsoft.EntityFrameworkCore;

using Tasks.API.Interfaces;
using Tasks.API.Responses;

namespace Tasks.API.Services;

public class TaskService : ITaskService
{
    private readonly TasksDbContext _tasksDbContext;

    public TaskService(TasksDbContext tasksDbContext) => _tasksDbContext = tasksDbContext;

    public async Task<DeleteTaskResponse> DeleteTask(int taskId, int userId)
    {
        var task = await _tasksDbContext.Tasks.FindAsync(taskId);

        if (task is null)
        {
            return new DeleteTaskResponse
            {
                Success = false,
                Error = "Task not found",
                ErrorCode = "T01"
            };
        }

        if (task.UserId != userId)
        {
            return new DeleteTaskResponse
            {
                Success = false,
                Error = "You don't have access to delete this task",
                ErrorCode = "T02"
            };
        }

        _tasksDbContext.Tasks.Remove(task);

        var saveResponse = await _tasksDbContext.SaveChangesAsync();

        if (saveResponse >= 0)
        {
            return new DeleteTaskResponse
            {
                Success = true,
                TaskId = task.Id
            };
        }

        return new DeleteTaskResponse
        {
            Success = false,
            Error = "Unable to delete task",
            ErrorCode = "T03"
        };
    }

    public async Task<GetTasksResponse> GetTasks(int userId)
    {
        var tasks = await _tasksDbContext.Tasks.Where(o => o.UserId == userId).ToListAsync();
        return new GetTasksResponse { Success = true, Tasks = tasks };
    }

    public async Task<SaveTaskResponse> SaveTask(Entities.Task task)
    {
        if (task.Id == 0)
            await _tasksDbContext.Tasks.AddAsync(task);
        else
        {
            var taskRecord = await _tasksDbContext.Tasks.FindAsync(task.Id);

            taskRecord!.IsCompleted = task.IsCompleted;
            taskRecord.Ts = task.Ts;
        }

        var saveResponse = await _tasksDbContext.SaveChangesAsync();

        if (saveResponse >= 0)
        {
            return new SaveTaskResponse
            {
                Success = true,
                Task = task
            };
        }
        return new SaveTaskResponse
        {
            Success = false,
            Error = "Unable to save task",
            ErrorCode = "T05"
        };
    }
}