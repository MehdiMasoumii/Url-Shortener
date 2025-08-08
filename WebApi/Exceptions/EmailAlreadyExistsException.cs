namespace WebApi.Exceptions;

public class EmailAlreadyExistsException: BaseException
{
    public override int StatusCode => StatusCodes.Status409Conflict;
    public override string ErrorCode => "EMAIL_ALREADY_EXISTS";

    public EmailAlreadyExistsException(string email) : base($"Email {email} already exists!"){}
}