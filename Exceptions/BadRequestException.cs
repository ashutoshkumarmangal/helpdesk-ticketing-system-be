namespace HelpDesk.Api.Exceptions;

public class BadRequestException : ApiException
{
    public BadRequestException(string message) : base(400, message)
    {
    }
}