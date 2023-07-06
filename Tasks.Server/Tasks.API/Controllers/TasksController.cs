using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Tasks.API.Interfaces;
using Tasks.API.Requests;
using Tasks.API.Responses;

namespace Tasks.API.Controllers;
[Authorize]
[Route("api/[controller]")]
[ApiController]
public class TasksController : BaseController
{
    private readonly ITaskService _taskService;

    public TasksController(ITaskService taskService) => _taskService = taskService;

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var getTasksResponse = await _taskService.GetTasks(UserID);

        if (!getTasksResponse.Success)
            return UnprocessableEntity(getTasksResponse);

        var tasksResponse = getTasksResponse.Tasks.ConvertAll(o => new TaskResponse { Id = o.Id, IsCompleted = o.IsCompleted, Name = o.Name, Ts = o.Ts });

        return Ok(tasksResponse);
    }

    [HttpPost]
    public async Task<IActionResult> Post(TaskRequest taskRequest)
    {
        var task = new Entities.Task { IsCompleted = taskRequest.IsCompleted, Ts = taskRequest.Ts, Name = taskRequest.Name, UserId = UserID };

        var saveTaskResponse = await _taskService.SaveTask(task);

        if (!saveTaskResponse.Success)
            return UnprocessableEntity(saveTaskResponse);

        var taskResponse = new TaskResponse { Id = saveTaskResponse.Task.Id, IsCompleted = saveTaskResponse.Task.IsCompleted, Name = saveTaskResponse.Task.Name, Ts = saveTaskResponse.Task.Ts };

        return Ok(taskResponse);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (id == 0)
            return BadRequest(new DeleteTaskResponse { Success = false, ErrorCode = "D01", Error = "Invalid Task id" });

        var deleteTaskResponse = await _taskService.DeleteTask(id, UserID);

        if (!deleteTaskResponse.Success)
            return UnprocessableEntity(deleteTaskResponse);

        return Ok(deleteTaskResponse.TaskId);
    }

    [HttpPut]
    public async Task<IActionResult> Put(TaskRequest taskRequest)
    {
        var task = new Entities.Task { Id = taskRequest.Id, IsCompleted = taskRequest.IsCompleted, Ts = taskRequest.Ts, Name = taskRequest.Name, UserId = UserID };

        var saveTaskResponse = await _taskService.SaveTask(task);

        if (!saveTaskResponse.Success)
            return UnprocessableEntity(saveTaskResponse);

        var taskResponse = new TaskResponse { Id = saveTaskResponse.Task.Id, IsCompleted = saveTaskResponse.Task.IsCompleted, Name = saveTaskResponse.Task.Name, Ts = saveTaskResponse.Task.Ts };

        return Ok(taskResponse);
    }
}