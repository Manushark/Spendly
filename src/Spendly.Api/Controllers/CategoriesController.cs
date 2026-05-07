using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Spendly.Api.Extensions;
using Spendly.Api.Security;
using Spendly.Application.DTOs.Category;
using Spendly.Application.UseCases.Categories;

namespace Spendly.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly GetCategoriesUseCase _getAll;
        private readonly CreateCategoryUseCase _create;
        private readonly UpdateCategoryUseCase _update;
        private readonly DeleteCategoryUseCase _delete;

        public CategoriesController(
            GetCategoriesUseCase getAll,
            CreateCategoryUseCase create,
            UpdateCategoryUseCase update,
            DeleteCategoryUseCase delete)
        {
            _getAll = getAll;
            _create = create;
            _update = update;
            _delete = delete;
        }

        /// <summary>
        /// GET /api/categories
        /// Returns all categories for the authenticated user
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var categories = await _getAll.ExecuteAsync(User.GetUserId());
            return Ok(categories);
        }

        /// <summary>
        /// POST /api/categories
        /// Creates a new custom category
        /// </summary>
        [EnableRateLimiting(RateLimitPolicies.WriteOperations)]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCategoryDto dto)
        {
            var id = await _create.ExecuteAsync(User.GetUserId(), dto);
            return Ok(new { id, message = "Category created successfully" });
        }

        /// <summary>
        /// PUT /api/categories/{id}
        /// Updates an existing category
        /// </summary>
        [EnableRateLimiting(RateLimitPolicies.WriteOperations)]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoryDto dto)
        {
            await _update.ExecuteAsync(User.GetUserId(), id, dto);
            return Ok(new { message = "Category updated successfully" });
        }

        /// <summary>
        /// DELETE /api/categories/{id}
        /// Deletes a category
        /// </summary>
        [EnableRateLimiting(RateLimitPolicies.WriteOperations)]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _delete.ExecuteAsync(User.GetUserId(), id);
            return deleted ? NoContent() : NotFound();
        }
    }
}
