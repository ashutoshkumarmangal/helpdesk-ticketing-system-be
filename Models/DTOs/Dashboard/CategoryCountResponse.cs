namespace HelpDesk.Api.Models.DTOs.Dashboard;

public class CategoryCountResponse
{
    public string Category { get; set; } = string.Empty;

    public int Count { get; set; }
}