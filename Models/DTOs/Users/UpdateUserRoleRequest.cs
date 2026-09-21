using System.ComponentModel.DataAnnotations;
using HelpDesk.Api.Models.Enums;

namespace HelpDesk.Api.Models.DTOs.Users;

public class UpdateUserRoleRequest
{
    [Required(ErrorMessage = "Role is required")]
    [EnumDataType(typeof(UserRole), ErrorMessage = "Invalid role")]
    public UserRole Role { get; set; }
}