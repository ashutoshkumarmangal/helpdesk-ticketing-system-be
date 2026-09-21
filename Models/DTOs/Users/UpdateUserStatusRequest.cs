using System.ComponentModel.DataAnnotations;

namespace HelpDesk.Api.Models.DTOs.Users;

public class UpdateUserStatusRequest
{
    [Required(ErrorMessage = "IsActive is required")]
    public bool IsActive { get; set; }
}