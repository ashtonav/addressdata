namespace AddressData.Core.Models.ApiResponse;

public record ErrorModelApiResponse
{
    public ErrorModelApiResponse(Exception ex)
    {
        Type = ex.GetType().Name;
        Message = ex.Message;
        StackTrace = ex.ToString();
    }

    public string Type { get; init; }
    public string Message { get; init; }
    public string StackTrace { get; init; }
}
