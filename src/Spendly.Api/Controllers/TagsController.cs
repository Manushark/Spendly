using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Spendly.Api.Extensions;
using Spendly.Api.Security;
using Spendly.Application.DTOs.Tag;
using Spendly.Application.UseCases.Tags;

namespace Spendly.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class TagsController : ControllerBase
    {
        private readonly CreateTagUseCase _create;
        private readonly UpdateTagUseCase _update;
        private readonly DeleteTagUseCase _delete;
        private readonly ListTagsUseCase _list;
        private readonly SetExpenseTagsUseCase _setTags;

        public TagsController(
            CreateTagUseCase create,
            UpdateTagUseCase update,
            DeleteTagUseCase delete,
            ListTagsUseCase list,
            SetExpenseTagsUseCase setTags)
        {
            _create = create;
            _update = update;
            _delete = delete;
            _list = list;
            _setTags = setTags;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var tags = await _list.ExecuteAsync(User.GetUserId());
            return Ok(tags);
        }

        [EnableRateLimiting(RateLimitPolicies.WriteOperations)]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTagDto dto)
        {
            var id = await _create.ExecuteAsync(User.GetUserId(), dto);
            return Ok(new { id });
        }

        [EnableRateLimiting(RateLimitPolicies.WriteOperations)]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateTagDto dto)
        {
            await _update.ExecuteAsync(User.GetUserId(), id, dto);
            return NoContent();
        }

        [EnableRateLimiting(RateLimitPolicies.WriteOperations)]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _delete.ExecuteAsync(User.GetUserId(), id);
            return NoContent();
        }

        /// <summary>
        /// PUT /api/tags/expense/{expenseId} — Set tags for an expense
        /// </summary>
        [EnableRateLimiting(RateLimitPolicies.WriteOperations)]
        [HttpPut("expense/{expenseId:int}")]
        public async Task<IActionResult> SetExpenseTags(int expenseId, [FromBody] TagExpenseDto dto)
        {
            await _setTags.ExecuteAsync(User.GetUserId(), expenseId, dto.TagIds);
            return Ok(new { message = "Tags updated" });
        }
    }
}
