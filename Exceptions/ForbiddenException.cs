namespace HelpDesk.Api.Exceptions;

public class ForbiddenException : ApiException
{
    public ForbiddenException(string message) : base(403, message)
    {
    }
}