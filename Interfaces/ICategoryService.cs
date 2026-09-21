using HelpDesk.Api.Models.DTOs.Categories;

namespace HelpDesk.Api.Interfaces;

public interface ICategoryService
{
    Task<List<CategoryResponse>> GetCategoriesAsync();
}