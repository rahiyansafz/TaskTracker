namespace Tasks.API.Responses;

public class GetTasksResponse : BaseResponse
{
    public List<Entities.Task> Tasks { get; set; }
}