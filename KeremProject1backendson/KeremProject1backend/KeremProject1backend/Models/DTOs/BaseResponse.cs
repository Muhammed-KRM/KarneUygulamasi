namespace KeremProject1backend.Models.DTOs;

public class BaseResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Error { get; set; }

    public static BaseResponse<T> SuccessResponse(T data)
    {
        return new BaseResponse<T>
        {
            Success = true,
            Data = data,
            Error = null
        };
    }

    public static BaseResponse<T> ErrorResponse(string error)
    {
        return new BaseResponse<T>
        {
            Success = false,
            Data = default,
            Error = error
        };
    }
}
