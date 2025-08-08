namespace WebApi.Exceptions;

public abstract class BaseException: Exception
{
    public abstract int StatusCode { get;}
    public abstract string ErrorCode { get;}
    protected BaseException(string message) : base(message) { }
}