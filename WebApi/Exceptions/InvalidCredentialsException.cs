
namespace WebApi.Exceptions;

public class InvalidCredentialsException: BaseException
{
    public override int StatusCode => StatusCodes.Status401Unauthorized;
    public override string ErrorCode => "INVALID_CREDENTIALS";
    
    public InvalidCredentialsException() : base("Invalid credentials!"){}
}