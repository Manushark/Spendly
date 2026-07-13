using Moq;
using Spendly.Application.Interfaces;
using Spendly.Application.Services;
using Spendly.Domain.Entities;
using Spendly.Domain.Enums;

namespace Spendly.Tests.UseCases.Services;

/// <summary>
/// Pruebas unitarias para BudgetAlertService.
///
/// Este es el servicio MÁS COMPLEJO del proyecto porque:
///   1. Tiene 5 dependencias inyectadas (4 repositorios + 1 proveedor de fecha)
///   2. Tiene lógica de escalación entre dos umbrales: 80% (warning) → 100% (exceeded)
///   3. Previene duplicados consultando si la notificación ya existe
///   4. Respeta la timezone del usuario para calcular el mes actual
///   5. Itera sobre MÚLTIPLES presupuestos en una sola llamada
///
/// Mapa de caminos posibles:
///
///   gasto < 80%     → sin notificación
///   gasto >= 80%    → BudgetWarning  (solo si NO existe warning Y NO existe exceeded)
///   gasto >= 100%   → BudgetExceeded (solo si NO existe exceeded ya)
///   gasto >= 100%   → nada extra     (si ya existe BudgetExceeded)
///   gasto 80-99%    → nada           (si ya hubo warning antes)
///   gasto 80-99%    → nada           (si ya hubo exceeded antes — evita "bajar" el estado)
/// </summary>
public class BudgetAlertServiceTests
{
    // ──────────────────────────────────────────────────────────────────────────────
    // SETUP — mocks reutilizables declarados como campos para no repetirlos
    // ──────────────────────────────────────────────────────────────────────────────

    private readonly Mock<IBudgetRepository>       _budgetRepo   = new();
    private readonly Mock<IExpenseRepository>      _expenseRepo  = new();
    private readonly Mock<INotificationRepository> _notifRepo    = new();
    private readonly Mock<IUserRepository>         _userRepo     = new();
    private readonly Mock<IDateTimeProvider>       _dateTime     = new();

    // Fecha fija para todos los tests: julio 2026
    private static readonly DateTime FixedNow = new(2026, 7, 15);

    /// <summary>
    /// Crea el servicio conectando los 5 mocks.
    /// Así no repetimos el constructor en cada test.
    /// </summary>
    private BudgetAlertService BuildService() =>
        new(_budgetRepo.Object, _expenseRepo.Object, _notifRepo.Object,
            _userRepo.Object, _dateTime.Object);

    /// <summary>
    /// Configura los mocks compartidos que TODOS los tests necesitan:
    /// user sin timezone especial y fecha fija.
    /// </summary>
    private void SetupBaseContext(int userId = 1)
    {
        var user = User.Create($"user{userId}@test.com", "hash");
        _userRepo.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
        _dateTime.Setup(d => d.Now(It.IsAny<string?>())).Returns(FixedNow);
    }

    /// <summary>
    /// Crea un presupuesto usando reflection para poder asignar el Id
    /// (el constructor privado de Budget no expone el Id directamente).
    /// Usamos Budget.Create() que es el factory method público.
    /// </summary>
    private static Budget CreateBudget(int userId, string category, decimal limit, int id = 1)
    {
        var budget = Budget.Create(userId, category, limit, FixedNow.Year, FixedNow.Month);
        // Asignamos el Id usando reflection porque EF lo haría igual en producción
        typeof(Budget)
            .GetProperty(nameof(Budget.Id))!
            .SetValue(budget, id);
        return budget;
    }

    // ══════════════════════════════════════════════════════════════════════════════
    // BLOQUE 1: Sin notificación (por debajo del umbral)
    // ══════════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task CheckAndCreateAlertsAsync_Should_NotCreateNotification_When_SpentIsBelow80Percent()
    {
        // Arrange
        const int userId = 1;
        SetupBaseContext(userId);

        var budget = CreateBudget(userId, "Food & Dining", limit: 1000m, id: 10);

        _budgetRepo
            .Setup(r => r.GetByUserAndMonthAsync(userId, FixedNow.Year, FixedNow.Month))
            .ReturnsAsync(new List<Budget> { budget });

        // Gastado: $750 = 75% → por debajo del 80%
        _expenseRepo
            .Setup(r => r.GetTotalByCategoryAsync(userId, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new Dictionary<string, decimal> { ["Food & Dining"] = 750m });

        var service = BuildService();

        // Act
        await service.CheckAndCreateAlertsAsync(userId);

        // Assert — AddAsync nunca debe llamarse
        _notifRepo.Verify(r => r.AddAsync(It.IsAny<Notification>()), Times.Never);
    }

    // ══════════════════════════════════════════════════════════════════════════════
    // BLOQUE 2: Warning al 80%
    // ══════════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task CheckAndCreateAlertsAsync_Should_CreateBudgetWarning_When_SpentReaches80Percent()
    {
        // Arrange
        const int userId = 1;
        SetupBaseContext(userId);

        var budget = CreateBudget(userId, "Shopping", limit: 500m, id: 20);

        _budgetRepo
            .Setup(r => r.GetByUserAndMonthAsync(userId, FixedNow.Year, FixedNow.Month))
            .ReturnsAsync(new List<Budget> { budget });

        // Gastado: $420 = 84% → zona de WARNING
        _expenseRepo
            .Setup(r => r.GetTotalByCategoryAsync(userId, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new Dictionary<string, decimal> { ["Shopping"] = 420m });

        // Ninguna notificación previa para este presupuesto
        _notifRepo
            .Setup(r => r.ExistsForBudgetThisMonthAsync(userId, budget.Id, NotificationType.BudgetWarning, FixedNow.Year, FixedNow.Month))
            .ReturnsAsync(false);
        _notifRepo
            .Setup(r => r.ExistsForBudgetThisMonthAsync(userId, budget.Id, NotificationType.BudgetExceeded, FixedNow.Year, FixedNow.Month))
            .ReturnsAsync(false);
        _notifRepo.Setup(r => r.AddAsync(It.IsAny<Notification>())).Returns(Task.CompletedTask);

        var service = BuildService();

        // Act
        await service.CheckAndCreateAlertsAsync(userId);

        // Assert — debe crear exactamente UNA notificación de tipo BudgetWarning
        _notifRepo.Verify(r => r.AddAsync(
            It.Is<Notification>(n => n.Type == NotificationType.BudgetWarning)),
            Times.Once);
    }

    [Fact]
    public async Task CheckAndCreateAlertsAsync_Should_NotCreateWarning_When_WarningAlreadyExists()
    {
        // Arrange
        const int userId = 1;
        SetupBaseContext(userId);

        var budget = CreateBudget(userId, "Shopping", limit: 500m, id: 20);

        _budgetRepo
            .Setup(r => r.GetByUserAndMonthAsync(userId, FixedNow.Year, FixedNow.Month))
            .ReturnsAsync(new List<Budget> { budget });

        _expenseRepo
            .Setup(r => r.GetTotalByCategoryAsync(userId, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new Dictionary<string, decimal> { ["Shopping"] = 420m }); // 84%

        // Ya existe un BudgetWarning previo → el servicio NO debe crear otro
        _notifRepo
            .Setup(r => r.ExistsForBudgetThisMonthAsync(userId, budget.Id, NotificationType.BudgetWarning, FixedNow.Year, FixedNow.Month))
            .ReturnsAsync(true); // ← DIFERENCIA respecto al test anterior
        _notifRepo
            .Setup(r => r.ExistsForBudgetThisMonthAsync(userId, budget.Id, NotificationType.BudgetExceeded, FixedNow.Year, FixedNow.Month))
            .ReturnsAsync(false);

        var service = BuildService();

        // Act
        await service.CheckAndCreateAlertsAsync(userId);

        // Assert — no se debe crear ninguna notificación duplicada
        _notifRepo.Verify(r => r.AddAsync(It.IsAny<Notification>()), Times.Never);
    }

    // ══════════════════════════════════════════════════════════════════════════════
    // BLOQUE 3: Exceeded al 100%
    // ══════════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task CheckAndCreateAlertsAsync_Should_CreateBudgetExceeded_When_SpentReaches100Percent()
    {
        // Arrange
        const int userId = 1;
        SetupBaseContext(userId);

        var budget = CreateBudget(userId, "Health", limit: 300m, id: 30);

        _budgetRepo
            .Setup(r => r.GetByUserAndMonthAsync(userId, FixedNow.Year, FixedNow.Month))
            .ReturnsAsync(new List<Budget> { budget });

        // Gastado: $360 = 120% → EXCEEDED
        _expenseRepo
            .Setup(r => r.GetTotalByCategoryAsync(userId, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new Dictionary<string, decimal> { ["Health"] = 360m });

        // No existe ningún exceeded previo
        _notifRepo
            .Setup(r => r.ExistsForBudgetThisMonthAsync(userId, budget.Id, NotificationType.BudgetExceeded, FixedNow.Year, FixedNow.Month))
            .ReturnsAsync(false);
        _notifRepo.Setup(r => r.AddAsync(It.IsAny<Notification>())).Returns(Task.CompletedTask);

        var service = BuildService();

        // Act
        await service.CheckAndCreateAlertsAsync(userId);

        // Assert — debe crear exactamente UNA notificación de tipo BudgetExceeded
        _notifRepo.Verify(r => r.AddAsync(
            It.Is<Notification>(n => n.Type == NotificationType.BudgetExceeded)),
            Times.Once);
    }

    [Fact]
    public async Task CheckAndCreateAlertsAsync_Should_NotCreateExceeded_When_ExceededAlreadyExists()
    {
        // Arrange — mismo escenario de exceeded pero con notificación previa
        const int userId = 1;
        SetupBaseContext(userId);

        var budget = CreateBudget(userId, "Health", limit: 300m, id: 30);

        _budgetRepo
            .Setup(r => r.GetByUserAndMonthAsync(userId, FixedNow.Year, FixedNow.Month))
            .ReturnsAsync(new List<Budget> { budget });

        _expenseRepo
            .Setup(r => r.GetTotalByCategoryAsync(userId, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new Dictionary<string, decimal> { ["Health"] = 360m }); // 120%

        // Ya existe un BudgetExceeded → no debe crear uno más
        _notifRepo
            .Setup(r => r.ExistsForBudgetThisMonthAsync(userId, budget.Id, NotificationType.BudgetExceeded, FixedNow.Year, FixedNow.Month))
            .ReturnsAsync(true); // ← DIFERENCIA

        var service = BuildService();

        // Act
        await service.CheckAndCreateAlertsAsync(userId);

        // Assert
        _notifRepo.Verify(r => r.AddAsync(It.IsAny<Notification>()), Times.Never);
    }

    // ══════════════════════════════════════════════════════════════════════════════
    // BLOQUE 4: Escalación — el caso más sutil
    // Un gasto que antes tenía Warning ahora supera el 100%.
    // El servicio debe crear Exceeded pero NO un Warning adicional.
    // ══════════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task CheckAndCreateAlertsAsync_Should_NotCreateWarning_When_AlreadyExceeded_EvenIfAt80Percent()
    {
        // Arrange — este es el caso de ANTI-REGRESIÓN:
        // Imagina que el usuario ya superó el 100% en el pasado y luego borró un gasto.
        // Ahora está al 85%, pero ya tiene notificación Exceeded.
        // El servicio NO debe bajar el estado a Warning.
        const int userId = 1;
        SetupBaseContext(userId);

        var budget = CreateBudget(userId, "Education", limit: 200m, id: 40);

        _budgetRepo
            .Setup(r => r.GetByUserAndMonthAsync(userId, FixedNow.Year, FixedNow.Month))
            .ReturnsAsync(new List<Budget> { budget });

        // Ahora está al 85% (bajó del 100%)
        _expenseRepo
            .Setup(r => r.GetTotalByCategoryAsync(userId, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new Dictionary<string, decimal> { ["Education"] = 170m }); // 85%

        // Warning: no existe. Exceeded: SÍ existe (escaló antes)
        _notifRepo
            .Setup(r => r.ExistsForBudgetThisMonthAsync(userId, budget.Id, NotificationType.BudgetWarning, FixedNow.Year, FixedNow.Month))
            .ReturnsAsync(false);
        _notifRepo
            .Setup(r => r.ExistsForBudgetThisMonthAsync(userId, budget.Id, NotificationType.BudgetExceeded, FixedNow.Year, FixedNow.Month))
            .ReturnsAsync(true); // ← ya superó antes

        var service = BuildService();

        // Act
        await service.CheckAndCreateAlertsAsync(userId);

        // Assert — no debe crear Warning porque ya hubo Exceeded (el estado no retrocede)
        _notifRepo.Verify(r => r.AddAsync(It.IsAny<Notification>()), Times.Never);
    }

    // ══════════════════════════════════════════════════════════════════════════════
    // BLOQUE 5: Múltiples presupuestos en un mismo mes
    // El servicio itera sobre varios presupuestos. Este test verifica que
    // cada presupuesto se evalúa de forma INDEPENDIENTE.
    // ══════════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task CheckAndCreateAlertsAsync_Should_CreateIndependentNotifications_ForEachBudget()
    {
        // Arrange — 3 presupuestos con distintos estados:
        //   - Food:          60% → sin notificación
        //   - Shopping:      90% → BudgetWarning
        //   - Entertainment: 110% → BudgetExceeded
        const int userId = 1;
        SetupBaseContext(userId);

        var foodBudget    = CreateBudget(userId, "Food & Dining",  limit: 1000m, id: 1);
        var shopBudget    = CreateBudget(userId, "Shopping",       limit: 500m,  id: 2);
        var entBudget     = CreateBudget(userId, "Entertainment",  limit: 200m,  id: 3);

        _budgetRepo
            .Setup(r => r.GetByUserAndMonthAsync(userId, FixedNow.Year, FixedNow.Month))
            .ReturnsAsync(new List<Budget> { foodBudget, shopBudget, entBudget });

        _expenseRepo
            .Setup(r => r.GetTotalByCategoryAsync(userId, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new Dictionary<string, decimal>
            {
                ["Food & Dining"]  = 600m,  // 60%  → nada
                ["Shopping"]       = 450m,  // 90%  → warning
                ["Entertainment"]  = 220m,  // 110% → exceeded
            });

        // Food: ninguna notificación (no se consulta ExistsForBudget para éste)
        // Shopping: no existe warning ni exceeded previos
        _notifRepo
            .Setup(r => r.ExistsForBudgetThisMonthAsync(userId, shopBudget.Id, NotificationType.BudgetWarning, FixedNow.Year, FixedNow.Month))
            .ReturnsAsync(false);
        _notifRepo
            .Setup(r => r.ExistsForBudgetThisMonthAsync(userId, shopBudget.Id, NotificationType.BudgetExceeded, FixedNow.Year, FixedNow.Month))
            .ReturnsAsync(false);

        // Entertainment: no existe exceeded previo
        _notifRepo
            .Setup(r => r.ExistsForBudgetThisMonthAsync(userId, entBudget.Id, NotificationType.BudgetExceeded, FixedNow.Year, FixedNow.Month))
            .ReturnsAsync(false);

        _notifRepo.Setup(r => r.AddAsync(It.IsAny<Notification>())).Returns(Task.CompletedTask);

        var service = BuildService();

        // Act
        await service.CheckAndCreateAlertsAsync(userId);

        // Assert — en total deben crearse EXACTAMENTE 2 notificaciones
        _notifRepo.Verify(r => r.AddAsync(It.IsAny<Notification>()), Times.Exactly(2));

        // Y cada una del tipo correcto
        _notifRepo.Verify(r => r.AddAsync(
            It.Is<Notification>(n => n.Type == NotificationType.BudgetWarning)),
            Times.Once);

        _notifRepo.Verify(r => r.AddAsync(
            It.Is<Notification>(n => n.Type == NotificationType.BudgetExceeded)),
            Times.Once);
    }

    // ══════════════════════════════════════════════════════════════════════════════
    // BLOQUE 6: Sin presupuestos — el servicio debe salir sin hacer nada
    // ══════════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task CheckAndCreateAlertsAsync_Should_DoNothing_When_UserHasNoBudgets()
    {
        // Arrange
        const int userId = 1;
        SetupBaseContext(userId);

        _budgetRepo
            .Setup(r => r.GetByUserAndMonthAsync(userId, FixedNow.Year, FixedNow.Month))
            .ReturnsAsync(new List<Budget>()); // lista vacía

        var service = BuildService();

        // Act
        await service.CheckAndCreateAlertsAsync(userId);

        // Assert — si no hay presupuestos, no hay nada que evaluar
        _expenseRepo.Verify(r => r.GetTotalByCategoryAsync(
            It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()),
            Times.Never);

        _notifRepo.Verify(r => r.AddAsync(It.IsAny<Notification>()), Times.Never);
    }
}
