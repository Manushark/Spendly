using Moq;
using Spendly.Application.DTOs.Category;
using Spendly.Application.Interfaces;
using Spendly.Application.UseCases.Categories;
using Spendly.Domain.Entities;
using Spendly.Domain.Exceptions;

namespace Spendly.Tests.UseCases.Categories;

public class CreateCategoryUseCaseTests
{
    private readonly Mock<ICategoryRepository> _repo = new();

    private static CreateCategoryDto ValidDto(string name = "Transport") => new()
    {
        Name  = name,
        Icon  = "bi-car-front",
        Color = "#3b82f6"
    };

    [Fact]
    public async Task ExecuteAsync_Should_CreateCategory_When_NameIsUniqueAndLimitNotReached()
    {
        // Arrange — el usuario tiene 10 categorías, aún puede crear más
        _repo.Setup(r => r.CountByUserAsync(1)).ReturnsAsync(10);
        _repo.Setup(r => r.GetByNameAndUserAsync(1, "Transport")).ReturnsAsync((Category?)null);
        _repo.Setup(r => r.AddAsync(It.IsAny<Category>())).Returns(Task.CompletedTask);

        // Act
        await new CreateCategoryUseCase(_repo.Object).ExecuteAsync(userId: 1, ValidDto());

        // Assert
        _repo.Verify(r => r.AddAsync(It.IsAny<Category>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_Should_ThrowInvalidDomain_When_UserHas50Categories()
    {
        // Arrange — ya llegó al límite de 50
        _repo.Setup(r => r.CountByUserAsync(1)).ReturnsAsync(50);

        // Act + Assert
        await Assert.ThrowsAsync<InvalidDomainException>(() =>
            new CreateCategoryUseCase(_repo.Object).ExecuteAsync(userId: 1, ValidDto()));

        // Nunca debe llegar a guardar
        _repo.Verify(r => r.AddAsync(It.IsAny<Category>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_Should_ThrowInvalidDomain_When_CategoryNameAlreadyExists()
    {
        // Arrange — ya existe una categoría con ese nombre
        _repo.Setup(r => r.CountByUserAsync(1)).ReturnsAsync(5);

        var duplicate = Category.Create(1, "Transport", "bi-car-front", "#000");
        _repo.Setup(r => r.GetByNameAndUserAsync(1, "Transport")).ReturnsAsync(duplicate);

        // Act + Assert
        await Assert.ThrowsAsync<InvalidDomainException>(() =>
            new CreateCategoryUseCase(_repo.Object).ExecuteAsync(userId: 1, ValidDto("Transport")));
    }
}

public class DeleteCategoryUseCaseTests
{
    private readonly Mock<ICategoryRepository>         _categoryRepo  = new();
    private readonly Mock<IExpenseRepository>          _expenseRepo   = new();
    private readonly Mock<IBudgetRepository>           _budgetRepo    = new();
    private readonly Mock<IRecurringExpenseRepository> _recurringRepo = new();

    private DeleteCategoryUseCase Build() =>
        new(_categoryRepo.Object, _expenseRepo.Object, _budgetRepo.Object, _recurringRepo.Object);

    private static Category MakeCategory(int userId = 1, int id = 5, string name = "Transport")
    {
        var c = Category.Create(userId, name, "bi-car-front", "#000");
        typeof(Category).GetProperty(nameof(Category.Id))!.SetValue(c, id);
        return c;
    }

    [Fact]
    public async Task ExecuteAsync_Should_DeleteCategory_When_NothingIsAssociated()
    {
        // Arrange — categoría sin gastos, presupuestos ni recurrentes
        var cat = MakeCategory(userId: 1, id: 5);

        _categoryRepo.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(cat);
        _expenseRepo.Setup(r => r.CountByCategoryAsync(1, "Transport")).ReturnsAsync(0);
        _budgetRepo.Setup(r => r.CountByCategoryAsync(1, "Transport")).ReturnsAsync(0);
        _recurringRepo.Setup(r => r.CountByCategoryAsync(1, "Transport")).ReturnsAsync(0);
        _categoryRepo.Setup(r => r.DeleteAsync(5)).ReturnsAsync(true);

        // Act
        var result = await Build().ExecuteAsync(userId: 1, categoryId: 5);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ExecuteAsync_Should_ThrowInvalidDomain_When_CategoryHasExpenses()
    {
        // Arrange — la categoría tiene 3 gastos asociados
        var cat = MakeCategory(userId: 1, id: 5);

        _categoryRepo.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(cat);
        _expenseRepo.Setup(r => r.CountByCategoryAsync(1, "Transport")).ReturnsAsync(3); // tiene gastos
        _budgetRepo.Setup(r => r.CountByCategoryAsync(1, "Transport")).ReturnsAsync(0);
        _recurringRepo.Setup(r => r.CountByCategoryAsync(1, "Transport")).ReturnsAsync(0);

        // Act + Assert
        await Assert.ThrowsAsync<InvalidDomainException>(() =>
            Build().ExecuteAsync(userId: 1, categoryId: 5));

        // No debe borrar la categoría si tiene registros asociados
        _categoryRepo.Verify(r => r.DeleteAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_Should_ThrowUnauthorized_When_CategoryBelongsToDifferentUser()
    {
        // Arrange
        var cat = MakeCategory(userId: 1, id: 5);
        _categoryRepo.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(cat);

        // Act + Assert — usuario 2 intenta borrar categoría del usuario 1
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            Build().ExecuteAsync(userId: 2, categoryId: 5));
    }

    [Fact]
    public async Task ExecuteAsync_Should_ThrowInvalidDomain_When_CategoryNotFound()
    {
        // Arrange
        _categoryRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Category?)null);

        // Act + Assert
        await Assert.ThrowsAsync<InvalidDomainException>(() =>
            Build().ExecuteAsync(userId: 1, categoryId: 99));
    }
}
