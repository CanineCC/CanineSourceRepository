namespace CanineSourceRepository;


public enum ResultCode
{
  Success = 200,
  Created = 201,
  NoContent = 204,

  BadRequest = 400,
  Unauthorized = 401,
  Forbidden = 403,
  NotFound = 404,

  InternalError = 500,
  ServiceUnavailable = 503
}

public record ValidationResponse(bool IsValid, string InvalidReason, ResultCode ErrorCode);
