using HelpDesk.Api.Models.Enums;

namespace HelpDesk.Api.Interfaces;

public interface ICurrentUserService
{
    bool IsAuthenticated { get; }

    int Id { get; }

    string Name { get; }

    string Email { get; }

    UserRole Role { get; }
}