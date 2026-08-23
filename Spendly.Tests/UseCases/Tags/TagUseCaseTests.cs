using Moq;
using Spendly.Application.DTOs.Tag;
using Spendly.Application.Interfaces;
using Spendly.Application.UseCases.Tags;
using Spendly.Domain.Entities;

namespace Spendly.Tests.UseCases.Tags;

public class TagUseCaseTests
{
    private readonly Mock<ITagRepository> _repo = new();

    private static Tag MakeTag(int userId = 1, int id = 5, string name = "food")
    {
        var tag = Tag.Create(userId, name, "#FF5733");
        typeof(Tag).GetProperty(nameof(Tag.Id))!.SetValue(tag, id);
        return tag;
    }

    // ── CreateTagUseCase ──────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateTag_Should_CreateAndReturnId_When_NameIsUnique()
    {
        _repo.Setup(r => r.GetByNameAsync(1, "food")).ReturnsAsync((Tag?)null);
        _repo.Setup(r => r.AddAsync(It.IsAny<Tag>())).Returns(Task.CompletedTask);

        await new CreateTagUseCase(_repo.Object).ExecuteAsync(1, new CreateTagDto { Name = "food", Color = "#FF5733" });

        _repo.Verify(r => r.AddAsync(It.IsAny<Tag>()), Times.Once);
    }

    [Fact]
    public async Task CreateTag_Should_ThrowInvalidOperation_When_TagAlreadyExists()
    {
        _repo.Setup(r => r.GetByNameAsync(1, "food")).ReturnsAsync(MakeTag());

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            new CreateTagUseCase(_repo.Object).ExecuteAsync(1, new CreateTagDto { Name = "food", Color = "#000" }));
    }

    // ── UpdateTagUseCase ──────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateTag_Should_UpdateTag_When_OwnerAndDataAreValid()
    {
        var tag = MakeTag(userId: 1, id: 5);
        _repo.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(tag);
        _repo.Setup(r => r.UpdateAsync(It.IsAny<Tag>())).Returns(Task.CompletedTask);

        await new UpdateTagUseCase(_repo.Object).ExecuteAsync(1, 5, new UpdateTagDto { Name = "travel", Color = "#0000FF" });

        _repo.Verify(r => r.UpdateAsync(It.IsAny<Tag>()), Times.Once);
    }

    [Fact]
    public async Task UpdateTag_Should_ThrowKeyNotFound_When_TagDoesNotExist()
    {
        _repo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Tag?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            new UpdateTagUseCase(_repo.Object).ExecuteAsync(1, 99, new UpdateTagDto { Name = "x", Color = "#fff" }));
    }

    [Fact]
    public async Task UpdateTag_Should_ThrowUnauthorized_When_TagBelongsToDifferentUser()
    {
        var tag = MakeTag(userId: 1, id: 5); // pertenece al usuario 1
        _repo.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(tag);

        // usuario 2 intenta editar la tag del usuario 1
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            new UpdateTagUseCase(_repo.Object).ExecuteAsync(2, 5, new UpdateTagDto { Name = "x", Color = "#fff" }));
    }

    // ── DeleteTagUseCase ──────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteTag_Should_ReturnTrue_When_Deleted()
    {
        var tag = MakeTag(userId: 1, id: 5);
        _repo.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(tag);
        _repo.Setup(r => r.DeleteAsync(5)).ReturnsAsync(true);

        var result = await new DeleteTagUseCase(_repo.Object).ExecuteAsync(1, 5);

        Assert.True(result);
    }

    [Fact]
    public async Task DeleteTag_Should_ThrowKeyNotFound_When_TagDoesNotExist()
    {
        _repo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Tag?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            new DeleteTagUseCase(_repo.Object).ExecuteAsync(1, 99));
    }

    [Fact]
    public async Task DeleteTag_Should_ThrowUnauthorized_When_TagBelongsToDifferentUser()
    {
        var tag = MakeTag(userId: 1, id: 5);
        _repo.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(tag);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            new DeleteTagUseCase(_repo.Object).ExecuteAsync(2, 5));
    }

    // ── ListTagsUseCase ───────────────────────────────────────────────────────────

    [Fact]
    public async Task ListTags_Should_ReturnMappedList()
    {
        var tags = new List<Tag> { MakeTag(name: "food"), MakeTag(id: 6, name: "travel") };
        // Sin ExpenseTags relacionados (null-safe)
        _repo.Setup(r => r.GetAllByUserAsync(1)).ReturnsAsync(tags);

        var result = await new ListTagsUseCase(_repo.Object).ExecuteAsync(1);

        Assert.Equal(2, result.Count);
        Assert.Contains(result, r => r.Name == "food");
        Assert.Contains(result, r => r.Name == "travel");
    }

    // ── SetExpenseTagsUseCase ─────────────────────────────────────────────────────

    [Fact]
    public async Task SetExpenseTags_Should_CallRepository()
    {
        _repo.Setup(r => r.SetExpenseTagsAsync(1, 10, It.IsAny<List<int>>())).Returns(Task.CompletedTask);

        await new SetExpenseTagsUseCase(_repo.Object).ExecuteAsync(1, 10, new List<int> { 1, 2 });

        _repo.Verify(r => r.SetExpenseTagsAsync(1, 10, It.IsAny<List<int>>()), Times.Once);
    }
}
