using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Spendly.Application.Interfaces;
using Spendly.Domain.Entities;

namespace Spendly.Application.Services
{
    /// <summary>
    /// Servicio de Insights con IA para análisis de gastos
    /// Integración con OpenAI GPT-4 o Anthropic Claude
    /// </summary>
    public class AIInsightsService
    {
        private readonly IExpenseRepository _expenseRepo;
        private readonly IBudgetRepository _budgetRepo;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly ILogger<AIInsightsService> _logger;

        public AIInsightsService(
            IExpenseRepository expenseRepo,
            IBudgetRepository budgetRepo,
            IConfiguration configuration,
            HttpClient httpClient,
            ILogger<AIInsightsService> logger)
        {
            _expenseRepo = expenseRepo;
            _budgetRepo = budgetRepo;
            _configuration = configuration;
            _httpClient = httpClient;
            _logger = logger;
        }

        /// <summary>
        /// Genera insights personalizados para un usuario
        /// </summary>
        public async Task<InsightsSummaryDto> GenerateInsightsAsync(int userId)
        {
            // Obtener datos del usuario
            var expenses = _expenseRepo.GetByDateRange(userId,
                DateTime.Now.AddMonths(-3),
                DateTime.Now).ToList();

            var budgets = _budgetRepo.GetByUserAndMonth(userId,
                DateTime.Now.Year,
                DateTime.Now.Month);

            // Preparar datos para análisis
            var analysisData = PrepareAnalysisData(expenses, budgets);

            // Generar insights con IA
            var aiInsights = await GenerateAIInsightsAsync(analysisData);

            // Calcular métricas
            var insights = new InsightsSummaryDto
            {
                OverallScore = CalculateFinancialScore(expenses, budgets),
                Insights = aiInsights,
                Predictions = GeneratePredictions(expenses),
                Recommendations = GenerateRecommendations(expenses, budgets),
                Patterns = DetectPatterns(expenses),
                Anomalies = DetectAnomalies(expenses)
            };

            return insights;
        }

        /// <summary>
        /// Prepara datos para análisis de IA
        /// </summary>
        private string PrepareAnalysisData(List<Expense> expenses, List<Budget> budgets)
        {
            var summary = new
            {
                TotalExpenses = expenses.Sum(e => e.Amount.Value),
                ExpenseCount = expenses.Count,
                Categories = expenses.GroupBy(e => e.Category)
                    .Select(g => new
                    {
                        Category = g.Key,
                        Total = g.Sum(e => e.Amount.Value),
                        Count = g.Count(),
                        Average = g.Average(e => e.Amount.Value)
                    }),
                Budgets = budgets.Select(b => new
                {
                    Category = b.Category,
                    Limit = b.MonthlyLimit,
                    Spent = expenses.Where(e => e.Category == b.Category)
                        .Sum(e => e.Amount.Value)
                }),
                DateRange = $"{DateTime.Now.AddMonths(-3):yyyy-MM-dd} to {DateTime.Now:yyyy-MM-dd}"
            };

            return JsonSerializer.Serialize(summary);
        }

        /// <summary>
        /// Genera insights usando OpenAI GPT-4
        /// </summary>
        private async Task<List<InsightDto>> GenerateAIInsightsAsync(string data)
        {
            try
            {
                var apiKey = _configuration["OpenAI:ApiKey"];
                if (string.IsNullOrEmpty(apiKey))
                {
                    _logger.LogWarning("OpenAI API key not configured, using fallback insights");
                    return GetFallbackInsights(data);
                }

                var prompt = $@"Analyze this financial data and provide 3-5 actionable insights in JSON format:

{data}

Response format:
[
  {{
    ""title"": ""Insight title"",
    ""description"": ""Detailed explanation"",
    ""type"": ""warning|info|success"",
    ""actionable"": true/false,
    ""impact"": ""high|medium|low""
  }}
]

Focus on: spending patterns, budget adherence, savings opportunities, unusual transactions.";

                var request = new
                {
                    model = "gpt-4",
                    messages = new[]
                    {
                        new { role = "system", content = "You are a financial advisor analyzing spending data." },
                        new { role = "user", content = prompt }
                    },
                    temperature = 0.7,
                    max_tokens = 1000
                };

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

                var response = await _httpClient.PostAsJsonAsync(
                    "https://api.openai.com/v1/chat/completions",
                    request);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("OpenAI API error: {Status}", response.StatusCode);
                    return GetFallbackInsights(data);
                }

                var result = await response.Content.ReadFromJsonAsync<OpenAIResponse>();
                var content = result?.Choices?.FirstOrDefault()?.Message?.Content;

                if (string.IsNullOrEmpty(content))
                    return GetFallbackInsights(data);

                return JsonSerializer.Deserialize<List<InsightDto>>(content)
                    ?? GetFallbackInsights(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating AI insights");
                return GetFallbackInsights(data);
            }
        }

        /// <summary>
        /// Insights de respaldo cuando IA no está disponible
        /// </summary>
        private List<InsightDto> GetFallbackInsights(string data)
        {
            return new List<InsightDto>
            {
                new()
                {
                    Title = "Spending Analysis",
                    Description = "Your spending data has been analyzed. Upgrade to Pro for AI-powered insights.",
                    Type = "info",
                    Actionable = true,
                    Impact = "medium"
                }
            };
        }

        /// <summary>
        /// Calcula score de salud financiera (0-100)
        /// </summary>
        private int CalculateFinancialScore(List<Expense> expenses, List<Budget> budgets)
        {
            if (!expenses.Any()) return 100;

            var score = 100;

            // Penalizar por exceder budgets (-20 puntos)
            var budgetsExceeded = budgets.Count(b =>
            {
                var spent = expenses.Where(e => e.Category == b.Category)
                    .Sum(e => e.Amount.Value);
                return spent > b.MonthlyLimit;
            });

            score -= budgetsExceeded * 20;

            // Penalizar por variabilidad alta (-15 puntos)
            var dailyTotals = expenses.GroupBy(e => e.Date.Date)
                .Select(g => g.Sum(e => e.Amount.Value))
                .ToList();

            if (dailyTotals.Any())
            {
                var stdDev = CalculateStandardDeviation(dailyTotals);
                var mean = dailyTotals.Average();

                if (mean > 0 && stdDev / mean > 0.5) // Coeficiente de variación > 50%
                    score -= 15;
            }

            // Bonus por gastar menos del budget (+10 puntos)
            var budgetsUnderLimit = budgets.Count(b =>
            {
                var spent = expenses.Where(e => e.Category == b.Category)
                    .Sum(e => e.Amount.Value);
                return spent < b.MonthlyLimit * 0.8m; // Menos del 80%
            });

            score += budgetsUnderLimit * 10;

            return Math.Clamp(score, 0, 100);
        }

        /// <summary>
        /// Genera predicciones de gasto
        /// </summary>
        private List<PredictionDto> GeneratePredictions(List<Expense> expenses)
        {
            var predictions = new List<PredictionDto>();

            // Predicción por categoría
            var categories = expenses.GroupBy(e => e.Category);
            foreach (var category in categories)
            {
                var monthlyAvg = category.Average(e => e.Amount.Value);
                var trend = CalculateTrend(category.ToList());

                predictions.Add(new PredictionDto
                {
                    Category = category.Key,
                    PredictedAmount = monthlyAvg * (1 + trend),
                    Confidence = CalculateConfidence(category.ToList()),
                    Trend = trend > 0 ? "increasing" : trend < 0 ? "decreasing" : "stable"
                });
            }

            return predictions;
        }

        /// <summary>
        /// Genera recomendaciones personalizadas
        /// </summary>
        private List<RecommendationDto> GenerateRecommendations(
            List<Expense> expenses,
            List<Budget> budgets)
        {
            var recommendations = new List<RecommendationDto>();

            // Recomendación por categorías sin budget
            var categoriesWithoutBudget = expenses
                .Select(e => e.Category)
                .Distinct()
                .Where(c => !budgets.Any(b => b.Category == c))
                .ToList();

            if (categoriesWithoutBudget.Any())
            {
                recommendations.Add(new RecommendationDto
                {
                    Title = "Create Budgets",
                    Description = $"You have {categoriesWithoutBudget.Count} categories without budgets. Set limits to control spending.",
                    Priority = "high",
                    ActionUrl = "/budgets/create"
                });
            }

            // Recomendación por gastos grandes recientes
            var largeExpenses = expenses
                .Where(e => e.Date >= DateTime.Now.AddDays(-7))
                .OrderByDescending(e => e.Amount.Value)
                .Take(3)
                .Where(e => e.Amount.Value > 100)
                .ToList();

            if (largeExpenses.Any())
            {
                recommendations.Add(new RecommendationDto
                {
                    Title = "Review Recent Large Expenses",
                    Description = $"You had {largeExpenses.Count} large transactions this week totaling ${largeExpenses.Sum(e => e.Amount.Value):N2}",
                    Priority = "medium",
                    ActionUrl = "/expenses"
                });
            }

            return recommendations;
        }

        /// <summary>
        /// Detecta patrones de gasto
        /// </summary>
        private List<PatternDto> DetectPatterns(List<Expense> expenses)
        {
            var patterns = new List<PatternDto>();

            // Patrón: Día de la semana más caro
            var byDayOfWeek = expenses.GroupBy(e => e.Date.DayOfWeek)
                .Select(g => new { Day = g.Key, Total = g.Sum(e => e.Amount.Value) })
                .OrderByDescending(x => x.Total)
                .FirstOrDefault();

            if (byDayOfWeek != null)
            {
                patterns.Add(new PatternDto
                {
                    Type = "weekly",
                    Description = $"You spend most on {byDayOfWeek.Day}s (${byDayOfWeek.Total:N2} average)",
                    Frequency = "weekly"
                });
            }

            // Patrón: Hora del día (si tenemos esa info)
            // Esto requeriría guardar timestamps completos

            return patterns;
        }

        /// <summary>
        /// Detecta anomalías en gastos
        /// </summary>
        private List<AnomalyDto> DetectAnomalies(List<Expense> expenses)
        {
            var anomalies = new List<AnomalyDto>();

            var amounts = expenses.Select(e => (double)e.Amount.Value).ToList();
            if (!amounts.Any()) return anomalies;

            var mean = amounts.Average();
            var stdDev = CalculateStandardDeviation(amounts);

            // Gastos > 2 desviaciones estándar = anomalía
            var unusualExpenses = expenses
                .Where(e => Math.Abs((double)e.Amount.Value - mean) > 2 * stdDev)
                .OrderByDescending(e => e.Date)
                .Take(5)
                .ToList();

            foreach (var expense in unusualExpenses)
            {
                anomalies.Add(new AnomalyDto
                {
                    ExpenseId = expense.Id,
                    Description = expense.Description,
                    Amount = expense.Amount.Value,
                    Date = expense.Date,
                    Severity = expense.Amount.Value > (decimal)mean ? "high" : "low",
                    Reason = $"${expense.Amount.Value:N2} is unusual for your spending pattern"
                });
            }

            return anomalies;
        }

        // Helper methods
        private double CalculateStandardDeviation(IEnumerable<decimal> values)
        {
            var vals = values.Select(v => (double)v).ToList();
            return CalculateStandardDeviation(vals);
        }

        private double CalculateStandardDeviation(IEnumerable<double> values)
        {
            var enumerable = values.ToList();
            if (!enumerable.Any()) return 0;

            var avg = enumerable.Average();
            var sumOfSquares = enumerable.Sum(v => Math.Pow(v - avg, 2));
            return Math.Sqrt(sumOfSquares / enumerable.Count);
        }

        private decimal CalculateTrend(List<Expense> expenses)
        {
            if (expenses.Count < 2) return 0;

            var sorted = expenses.OrderBy(e => e.Date).ToList();
            var firstHalf = sorted.Take(sorted.Count / 2).Sum(e => e.Amount.Value);
            var secondHalf = sorted.Skip(sorted.Count / 2).Sum(e => e.Amount.Value);

            if (firstHalf == 0) return 0;
            return (secondHalf - firstHalf) / firstHalf;
        }

        private decimal CalculateConfidence(List<Expense> expenses)
        {
            // Más datos = mayor confianza
            var count = expenses.Count;
            return count switch
            {
                < 5 => 0.3m,
                < 10 => 0.6m,
                < 20 => 0.8m,
                _ => 0.95m
            };
        }
    }

    // DTOs
    public class InsightsSummaryDto
    {
        public int OverallScore { get; set; }
        public List<InsightDto> Insights { get; set; } = new();
        public List<PredictionDto> Predictions { get; set; } = new();
        public List<RecommendationDto> Recommendations { get; set; } = new();
        public List<PatternDto> Patterns { get; set; } = new();
        public List<AnomalyDto> Anomalies { get; set; } = new();
    }

    public class InsightDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Type { get; set; } // warning, info, success
        public bool Actionable { get; set; }
        public string Impact { get; set; } // high, medium, low
    }

    public class PredictionDto
    {
        public string Category { get; set; }
        public decimal PredictedAmount { get; set; }
        public decimal Confidence { get; set; }
        public string Trend { get; set; }
    }

    public class RecommendationDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Priority { get; set; }
        public string ActionUrl { get; set; }
    }

    public class PatternDto
    {
        public string Type { get; set; }
        public string Description { get; set; }
        public string Frequency { get; set; }
    }

    public class AnomalyDto
    {
        public int ExpenseId { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string Severity { get; set; }
        public string Reason { get; set; }
    }

    // OpenAI Response Model
    public class OpenAIResponse
    {
        public List<Choice> Choices { get; set; }
    }

    public class Choice
    {
        public Message Message { get; set; }
    }

    public class Message
    {
        public string Content { get; set; }
    }
}
