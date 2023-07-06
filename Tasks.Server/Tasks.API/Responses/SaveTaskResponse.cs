namespace Tasks.API.Responses;

public class SaveTaskResponse : BaseResponse
{
    public Entities.Task Task { get; set; }
}