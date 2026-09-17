using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Spendly.Application.DTOs.Ai;
using Spendly.Application.Interfaces;

namespace Spendly.Infrastructure.Services.Ai
{
    public class GeminiAiService : IAiAssistantService
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _config;
        private readonly ILogger<GeminiAiService> _logger;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public GeminiAiService(
            HttpClient http,
            IConfiguration config,
            ILogger<GeminiAiService> logger)
        {
            _http = http;
            _config = config;
            _logger = logger;
        }

        public async Task<AiFinancialPlanDto> ParseCommandAsync(
            string prompt,
            DateTime referenceDate,
            string timeZone,
            IEnumerable<string> validCategories,
            CancellationToken cancellationToken = default)
        {
            var apiKey = _config["Gemini:ApiKey"]
                         ?? Environment.GetEnvironmentVariable("Gemini__ApiKey")
                         ?? Environment.GetEnvironmentVariable("GEMINI_API_KEY")
                         ?? string.Empty;

            var model = _config["Gemini:Model"] ?? "gemini-1.5-flash";
            var categoryList = validCategories.ToList();

            // Fallback parsing if no API key is set yet (allows development and offline testing)
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                _logger.LogWarning("Gemini API key is not configured. Utilizing local fallback parser.");
                return FallbackLocalParser(prompt, referenceDate, categoryList);
            }

            try
            {
                var systemInstructionText = $@"You are Spendly AI Copilot, a precise personal finance assistant.
Your task is to analyze user commands (received via voice transcription or text) and extract financial expenses and budgets into a structured plan.

CONTEXT:
- Reference Date (Today): {referenceDate:yyyy-MM-dd (dddd)}
- User Timezone: {timeZone}
- Valid Categories for User: [{string.Join(", ", categoryList)}]

STRICT RULES:
1. CURRENCY AGNOSTIC: Extract the pure positive numerical scalar amount. NEVER convert currencies. Ignore words like 'pesos', 'dólares', 'dollars', 'euros', 'lucas'.
2. CATEGORY MATCHING: Always assign each expense to the closest match in the Valid Categories list. If completely unrelated, assign to 'Other' or 'Food & Dining'.
3. DATE RESOLUTION: If the user says 'hoy' or 'today', use {referenceDate:yyyy-MM-dd}. If 'ayer' or 'yesterday', use {referenceDate.AddDays(-1):yyyy-MM-dd}. Calculate exact YYYY-MM-DD for any mentioned day.
4. TAG EXTRACTION: If user mentions 'etiqueta X' or 'tag X', extract 'X' into tags array.
5. BUDGETS: If user sets a spending limit (e.g. 'presupuesto de 400 en Comida'), extract category, monthlyLimit, and default year={referenceDate.Year}, month={referenceDate.Month}.
6. SUMMARY: Write a brief, friendly 1-sentence confirmation summary in the language of the prompt.";

                var requestBody = new
                {
                    systemInstruction = new
                    {
                        parts = new[] { new { text = systemInstructionText } }
                    },
                    contents = new[]
                    {
                        new
                        {
                            parts = new[] { new { text = prompt } }
                        }
                    },
                    generationConfig = new
                    {
                        responseMimeType = "application/json",
                        responseSchema = new
                        {
                            type = "OBJECT",
                            properties = new
                            {
                                expenses = new
                                {
                                    type = "ARRAY",
                                    items = new
                                    {
                                        type = "OBJECT",
                                        properties = new
                                        {
                                            amount = new { type = "NUMBER" },
                                            category = new { type = "STRING" },
                                            description = new { type = "STRING" },
                                            date = new { type = "STRING" },
                                            tags = new
                                            {
                                                type = "ARRAY",
                                                items = new { type = "STRING" }
                                            }
                                        },
                                        required = new[] { "amount", "category", "description", "date" }
                                    }
                                },
                                budgets = new
                                {
                                    type = "ARRAY",
                                    items = new
                                    {
                                        type = "OBJECT",
                                        properties = new
                                        {
                                            category = new { type = "STRING" },
                                            monthlyLimit = new { type = "NUMBER" },
                                            year = new { type = "INTEGER" },
                                            month = new { type = "INTEGER" }
                                        },
                                        required = new[] { "category", "monthlyLimit", "year", "month" }
                                    }
                                },
                                summary = new { type = "STRING" }
                            },
                            required = new[] { "expenses", "budgets", "summary" }
                        }
                    }
                };

                var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";
                var response = await _http.PostAsJsonAsync(url, requestBody, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogError("Gemini API call failed with status {StatusCode}: {ErrorBody}", response.StatusCode, errorBody);
                    return FallbackLocalParser(prompt, referenceDate, categoryList);
                }

                var jsonResult = await response.Content.ReadFromJsonAsync<GeminiApiResponse>(JsonOptions, cancellationToken);
                var rawText = jsonResult?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;

                if (string.IsNullOrWhiteSpace(rawText))
                {
                    _logger.LogWarning("Gemini API returned empty text. Falling back to local parser.");
                    return FallbackLocalParser(prompt, referenceDate, categoryList);
                }

                var parsed = JsonSerializer.Deserialize<GeminiFinancialResponse>(rawText, JsonOptions);
                if (parsed == null)
                {
                    return FallbackLocalParser(prompt, referenceDate, categoryList);
                }

                return new AiFinancialPlanDto
                {
                    Expenses = parsed.Expenses.Select(e => new AiParsedExpenseDto
                    {
                        Amount = e.Amount,
                        Category = MatchCategory(e.Category, categoryList),
                        Description = e.Description,
                        Date = DateTime.TryParse(e.Date, out var d) ? d : referenceDate.Date,
                        Tags = e.Tags ?? []
                    }).ToList(),
                    Budgets = parsed.Budgets.Select(b => new AiParsedBudgetDto
                    {
                        Category = MatchCategory(b.Category, categoryList),
                        MonthlyLimit = b.MonthlyLimit,
                        Year = b.Year > 0 ? b.Year : referenceDate.Year,
                        Month = b.Month >= 1 && b.Month <= 12 ? b.Month : referenceDate.Month
                    }).ToList(),
                    Summary = parsed.Summary
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception communicating with Gemini API. Activating local heuristic fallback.");
                return FallbackLocalParser(prompt, referenceDate, categoryList);
            }
        }

        private static string MatchCategory(string suggested, List<string> validCategories)
        {
            if (string.IsNullOrWhiteSpace(suggested))
                return validCategories.FirstOrDefault() ?? "Other";

            var exact = validCategories.FirstOrDefault(c => string.Equals(c, suggested, StringComparison.OrdinalIgnoreCase));
            if (exact != null) return exact;

            var partial = validCategories.FirstOrDefault(c => c.Contains(suggested, StringComparison.OrdinalIgnoreCase) || suggested.Contains(c, StringComparison.OrdinalIgnoreCase));
            return partial ?? validCategories.FirstOrDefault() ?? "Other";
        }

        private static AiFinancialPlanDto FallbackLocalParser(string prompt, DateTime referenceDate, List<string> validCategories)
        {
            var plan = new AiFinancialPlanDto();
            var matches = Regex.Matches(prompt, @"(?:(\d+(?:[.,]\d+)?))\s*(?:pesos|d[oó]lares|usd|dop|\$)?\s*(?:en|de|para)?\s*([a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+)", RegexOptions.IgnoreCase);

            var fallbackCategory = validCategories.FirstOrDefault(c => c.Contains("Food", StringComparison.OrdinalIgnoreCase)) ?? "Food & Dining";

            foreach (Match m in matches)
            {
                if (m.Groups.Count >= 3 && decimal.TryParse(m.Groups[1].Value.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var amt) && amt > 0)
                {
                    var desc = m.Groups[2].Value.Trim();
                    if (desc.Length > 2)
                    {
                        plan.Expenses.Add(new AiParsedExpenseDto
                        {
                            Amount = amt,
                            Description = desc,
                            Category = MatchCategory(desc, validCategories),
                            Date = referenceDate.Date,
                            Tags = []
                        });
                    }
                }
            }

            if (plan.Expenses.Count == 0)
            {
                // Single amount extraction fallback
                var singleAmtMatch = Regex.Match(prompt, @"(\d+(?:[.,]\d+)?)");
                if (singleAmtMatch.Success && decimal.TryParse(singleAmtMatch.Groups[1].Value.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var singleAmt) && singleAmt > 0)
                {
                    plan.Expenses.Add(new AiParsedExpenseDto
                    {
                        Amount = singleAmt,
                        Description = prompt.Length > 40 ? prompt[..40] : prompt,
                        Category = fallbackCategory,
                        Date = referenceDate.Date,
                        Tags = []
                    });
                }
            }

            plan.Summary = plan.Expenses.Count > 0
                ? $"Se detectaron {plan.Expenses.Count} elemento(s) para registrar."
                : "No se reconocieron montos específicos en el comando.";

            return plan;
        }

        // Inner DTOs for Gemini API response serialization
        private class GeminiApiResponse
        {
            public List<GeminiCandidate>? Candidates { get; set; }
        }

        private class GeminiCandidate
        {
            public GeminiContent? Content { get; set; }
        }

        private class GeminiContent
        {
            public List<GeminiPart>? Parts { get; set; }
        }

        private class GeminiPart
        {
            public string? Text { get; set; }
        }

        private class GeminiFinancialResponse
        {
            public List<GeminiExpenseItem> Expenses { get; set; } = [];
            public List<GeminiBudgetItem> Budgets { get; set; } = [];
            public string Summary { get; set; } = string.Empty;
        }

        private class GeminiExpenseItem
        {
            public decimal Amount { get; set; }
            public string Category { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public string Date { get; set; } = string.Empty;
            public List<string>? Tags { get; set; }
        }

        private class GeminiBudgetItem
        {
            public string Category { get; set; } = string.Empty;
            public decimal MonthlyLimit { get; set; }
            public int Year { get; set; }
            public int Month { get; set; }
        }
    }
}
