using Tasks.API.Responses;

namespace Tasks.API.Interfaces;

public interface ITaskService
{
    Task<GetTasksResponse> GetTasks(int userId);
    Task<SaveTaskResponse> SaveTask(Entities.Task task);
    Task<DeleteTaskResponse> DeleteTask(int taskId, int userId);
}