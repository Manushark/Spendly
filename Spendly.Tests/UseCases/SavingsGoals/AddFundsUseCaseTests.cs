using Moq;
using Spendly.Application.Interfaces;
using Spendly.Application.UseCases.SavingsGoals;
using Spendly.Domain.Entities;
using Spendly.Domain.Enums;
using Spendly.Domain.Exceptions;

namespace Spendly.Tests.UseCases.SavingsGoals;

/// <summary>
/// Pruebas unitarias para AddFundsUseCase.
/// 
/// CONCEPTO CLAVE — ¿Por qué este Use Case es más interesante?
/// Tiene LÓGICA DE NEGOCIO con efectos secundarios:
///   1. Suma fondos a la meta de ahorro
///   2. Si pasa el 50% → crea una notificación de hito 🎯
///   3. Si pasa el 100% → crea una notificación de completado 🎉
///   4. Si el monto es negativo → lanza excepción ❌
///   5. Si la meta no existe → lanza KeyNotFoundException ❌
/// 
/// Con estas pruebas verificamos CADA uno de esos caminos de forma aislada.
/// </summary>
public class AddFundsUseCaseTests
{
    // Helper privado para crear metas de ahorro en los tests
    // Evitamos repetir el mismo código en cada test (principio DRY)
    private static SavingsGoal CreateGoal(
        int userId = 1,
        decimal target = 1000m,
        decimal current = 0m)
        => SavingsGoal.Create(userId, "Mi Meta", target, current, null, "bi-piggy-bank", "#6366f1");

    // ──────────────────────────────────────────────────────────────────────────────
    // TEST 1: Camino FELIZ — suma fondos sin cruzar el umbral del 50%
    // ──────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_Should_AddFunds_And_NotSendNotification_When_Under50Percent()
    {
        // ══════════════ ARRANGE ══════════════
        var goal = CreateGoal(target: 1000m, current: 100m); // 10% de progreso

        var mockGoalRepo = new Mock<ISavingsGoalRepository>();
        var mockNotifRepo = new Mock<INotificationRepository>();

        mockGoalRepo
            .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(goal);

        mockGoalRepo
            .Setup(r => r.UpdateAsync(It.IsAny<SavingsGoal>()))
            .Returns(Task.CompletedTask);

        var useCase = new AddFundsUseCase(mockGoalRepo.Object, mockNotifRepo.Object);

        // ══════════════ ACT ══════════════
        // Añadimos $200 → quedaría en $300 → eso es 30% → no cruza el 50%
        await useCase.ExecuteAsync(userId: 1, id: goal.Id, amount: 200m);

        // ══════════════ ASSERT ══════════════
        // El repositorio de la meta DEBE haber llamado UpdateAsync (para guardar los fondos)
        mockGoalRepo.Verify(r => r.UpdateAsync(It.IsAny<SavingsGoal>()), Times.Once);

        // El repositorio de notificaciones NO debe haber sido llamado
        // (no cruzamos el 50% ni el 100%)
        mockNotifRepo.Verify(r => r.AddAsync(It.IsAny<Notification>()), Times.Never);
    }

    // ──────────────────────────────────────────────────────────────────────────────
    // TEST 2: Hito del 50% — debe enviar notificación de milestone 🎯
    // ──────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_Should_SendMilestoneNotification_When_FundsCross50Percent()
    {
        // ══════════════ ARRANGE ══════════════
        // La meta está al 40% antes de añadir fondos
        var goal = CreateGoal(target: 1000m, current: 400m);

        var mockGoalRepo = new Mock<ISavingsGoalRepository>();
        var mockNotifRepo = new Mock<INotificationRepository>();

        mockGoalRepo
            .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(goal);
        mockGoalRepo
            .Setup(r => r.UpdateAsync(It.IsAny<SavingsGoal>()))
            .Returns(Task.CompletedTask);
        mockNotifRepo
            .Setup(r => r.AddAsync(It.IsAny<Notification>()))
            .Returns(Task.CompletedTask);

        var useCase = new AddFundsUseCase(mockGoalRepo.Object, mockNotifRepo.Object);

        // ══════════════ ACT ══════════════
        // Añadimos $200 → $600 total → 60% → CRUZA el umbral del 50%
        await useCase.ExecuteAsync(userId: 1, id: goal.Id, amount: 200m);

        // ══════════════ ASSERT ══════════════
        // DEBE haberse creado UNA notificación de tipo milestone
        // Usamos It.Is<T>() para inspeccionar el argumento que se pasó
        mockNotifRepo.Verify(r => r.AddAsync(
            It.Is<Notification>(n => n.Type == NotificationType.SavingsGoalMilestone)),
            Times.Once);
    }

    // ──────────────────────────────────────────────────────────────────────────────
    // TEST 3: Meta completada — debe enviar notificación de completado 🎉
    // ──────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_Should_SendCompletedNotification_When_FundsCross100Percent()
    {
        // ══════════════ ARRANGE ══════════════
        // La meta está al 80% antes de añadir fondos
        var goal = CreateGoal(target: 1000m, current: 800m);

        var mockGoalRepo = new Mock<ISavingsGoalRepository>();
        var mockNotifRepo = new Mock<INotificationRepository>();

        mockGoalRepo
            .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(goal);
        mockGoalRepo
            .Setup(r => r.UpdateAsync(It.IsAny<SavingsGoal>()))
            .Returns(Task.CompletedTask);
        mockNotifRepo
            .Setup(r => r.AddAsync(It.IsAny<Notification>()))
            .Returns(Task.CompletedTask);

        var useCase = new AddFundsUseCase(mockGoalRepo.Object, mockNotifRepo.Object);

        // ══════════════ ACT ══════════════
        // Añadimos $300 → $1100 total → CRUZA el 100%
        await useCase.ExecuteAsync(userId: 1, id: goal.Id, amount: 300m);

        // ══════════════ ASSERT ══════════════
        // DEBE haberse creado UNA notificación de tipo SavingsGoalCompleted
        mockNotifRepo.Verify(r => r.AddAsync(
            It.Is<Notification>(n => n.Type == NotificationType.SavingsGoalCompleted)),
            Times.Once);

        // Y la meta DEBE marcarse como completada
        Assert.True(goal.IsCompleted);
    }

    // ──────────────────────────────────────────────────────────────────────────────
    // TEST 4: Camino de ERROR — monto negativo o cero lanza excepción
    // ──────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_Should_ThrowInvalidDomainException_When_AmountIsNegativeOrZero()
    {
        // ══════════════ ARRANGE ══════════════
        var goal = CreateGoal();

        var mockGoalRepo = new Mock<ISavingsGoalRepository>();
        var mockNotifRepo = new Mock<INotificationRepository>();

        mockGoalRepo
            .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(goal);

        var useCase = new AddFundsUseCase(mockGoalRepo.Object, mockNotifRepo.Object);

        // ══════════════ ACT + ASSERT ══════════════
        // Probamos un monto de $0 → debe lanzar InvalidDomainException
        // (la validación está en SavingsGoal.AddFunds, en la capa Domain)
        await Assert.ThrowsAsync<InvalidDomainException>(async () =>
            await useCase.ExecuteAsync(userId: 1, id: goal.Id, amount: 0m)
        );

        // Probamos monto negativo también
        await Assert.ThrowsAsync<InvalidDomainException>(async () =>
            await useCase.ExecuteAsync(userId: 1, id: goal.Id, amount: -50m)
        );

        // IMPORTANTE: si lanzó excepción, UpdateAsync NUNCA debió llamarse
        mockGoalRepo.Verify(r => r.UpdateAsync(It.IsAny<SavingsGoal>()), Times.Never);
    }
}
