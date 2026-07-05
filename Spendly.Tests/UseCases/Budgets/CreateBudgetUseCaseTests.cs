using Moq;
using Spendly.Application.DTOs.Budget;
using Spendly.Application.Interfaces;
using Spendly.Application.UseCases.Budgets;
using Spendly.Domain.Entities;
using Spendly.Domain.Exceptions;

namespace Spendly.Tests.UseCases.Budgets;

/// <summary>
/// Pruebas unitarias para CreateBudgetUseCase.
/// 
/// CONCEPTO CLAVE — ¿Qué probamos aquí?
/// El Use Case tiene 2 caminos posibles:
///   1. Camino FELIZ: la categoría no existe ese mes → crea el presupuesto ✅
///   2. Camino de ERROR: ya existe un presupuesto para esa categoría ese mes → lanza excepción ❌
/// Cada prueba verifica exactamente UNO de esos caminos.
/// </summary>
public class CreateBudgetUseCaseTests
{
    // ──────────────────────────────────────────────────────────────────────────────
    // TEST 1: Camino FELIZ — el presupuesto se crea correctamente
    // ──────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// NOMBRE del test: siempre sigue el patrón:
    ///   [MétodoQueProbo]_Should_[ResultadoEsperado]_When_[Condición]
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_Should_CreateBudget_When_NoBudgetExistsForThatCategoryAndMonth()
    {
        // ══════════════ ARRANGE (Preparar) ══════════════
        // "Arrange" = armar todo lo necesario antes de ejecutar el código real.
        // Aquí creamos un "doble" del repositorio con Moq — es como un actor 
        // que finge ser el repositorio real sin tocar la base de datos.

        var mockRepo = new Mock<IBudgetRepository>();

        // Le decimos al "actor falso" cómo comportarse:
        // Cuando alguien llame GetByUserCategoryAndMonthAsync(...) → devuelve null
        // (null significa: "no existe presupuesto previo para esa categoría")
        mockRepo
            .Setup(r => r.GetByUserCategoryAndMonthAsync(
                It.IsAny<int>(),    // cualquier userId
                It.IsAny<string>(), // cualquier categoría
                It.IsAny<int>(),    // cualquier año
                It.IsAny<int>()))   // cualquier mes
            .ReturnsAsync((Budget?)null);

        // También preparamos que AddAsync no haga nada (simula guardar en BD)
        mockRepo
            .Setup(r => r.AddAsync(It.IsAny<Budget>()))
            .Returns(Task.CompletedTask);

        // Creamos el Use Case pasándole el repositorio falso
        var useCase = new CreateBudgetUseCase(mockRepo.Object);

        // El DTO es el "formulario" que llega desde la UI
        var dto = new CreateBudgetDto
        {
            Category = "Food & Dining",
            MonthlyLimit = 500m,
            Year = 2026,
            Month = 7
        };

        // ══════════════ ACT (Actuar) ══════════════
        // "Act" = ejecutar el código que estamos probando (una sola llamada)
        await useCase.ExecuteAsync(userId: 1, dto);

        // ══════════════ ASSERT (Verificar) ══════════════
        // "Assert" = comprobar que lo que esperábamos realmente ocurrió.
        // Verificamos que AddAsync fue llamado EXACTAMENTE 1 vez.
        // Si no fue llamado, el presupuesto nunca se creó → el test falla.
        mockRepo.Verify(r => r.AddAsync(It.IsAny<Budget>()), Times.Once);
    }

    // ──────────────────────────────────────────────────────────────────────────────
    // TEST 2: Camino de ERROR — lanza excepción si ya existe un presupuesto
    // ──────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_Should_ThrowInvalidDomainException_When_BudgetAlreadyExistsForThatMonth()
    {
        // ══════════════ ARRANGE ══════════════
        var mockRepo = new Mock<IBudgetRepository>();

        // Esta vez el repositorio "encuentra" un presupuesto existente:
        var existingBudget = Budget.Create(
            userId: 1,
            category: "Food & Dining",
            monthlyLimit: 300m,
            year: 2026,
            month: 7
        );

        mockRepo
            .Setup(r => r.GetByUserCategoryAndMonthAsync(1, "Food & Dining", 2026, 7))
            .ReturnsAsync(existingBudget); // ← ahora devuelve un presupuesto real

        var useCase = new CreateBudgetUseCase(mockRepo.Object);

        var dto = new CreateBudgetDto
        {
            Category = "Food & Dining",
            MonthlyLimit = 500m,
            Year = 2026,
            Month = 7
        };

        // ══════════════ ACT + ASSERT ══════════════
        // Cuando el código debe LANZAR una excepción, usamos Assert.ThrowsAsync<T>
        // Le decimos: "espero que este código lance InvalidDomainException"
        await Assert.ThrowsAsync<InvalidDomainException>(async () =>
            await useCase.ExecuteAsync(userId: 1, dto)
        );

        // Verificación extra: si lanzó excepción, AddAsync NUNCA debió llamarse
        // (no tiene sentido guardar si hubo un error de validación)
        mockRepo.Verify(r => r.AddAsync(It.IsAny<Budget>()), Times.Never);
    }
}
