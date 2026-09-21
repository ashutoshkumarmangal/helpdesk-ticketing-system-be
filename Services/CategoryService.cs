using HelpDesk.Api.Data;
using HelpDesk.Api.Interfaces;
using HelpDesk.Api.Mappings;
using HelpDesk.Api.Models.DTOs.Categories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace HelpDesk.Api.Services;

public class CategoryService : ICategoryService
{
    private const string CacheKey = "categories";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);

    private readonly AppDbContext _context;
    private readonly IMemoryCache _cache;

    public CategoryService(AppDbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<List<CategoryResponse>> GetCategoriesAsync()
    {
        if (_cache.TryGetValue(CacheKey, out List<CategoryResponse>? cached) && cached is not null)
        {
            return cached;
        }

        var categories = await _context.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new CategoryResponse
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description
            })
            .ToListAsync();

        _cache.Set(CacheKey, categories, CacheDuration);

        return categories;
    }
}