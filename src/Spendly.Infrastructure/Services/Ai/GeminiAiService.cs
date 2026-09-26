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

            var model = _config["Gemini:Model"] ?? "gemini-3.5-flash-lite";
            var categoryList = validCategories.ToList();

            // Fallback parsing if no API key is set yet (allows development and offline testing)
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                _logger.LogWarning("Gemini API key is not configured. Utilizing local fallback parser.");
                return FallbackLocalParser(prompt, referenceDate, categoryList);
            }

            try
            {
                var systemInstructionText = $@"You are Spendly AI Copilot, an expert and intuitive personal finance assistant.
Your task is to analyze user commands (received via voice transcription or text) and extract financial expenses and budgets into a structured plan.

CONTEXT:
- Reference Date (Today): {referenceDate:yyyy-MM-dd (dddd)}
- User Timezone: {timeZone}
- Valid Categories for User: [{string.Join(", ", categoryList)}]

STRICT RULES:
1. CURRENCY AGNOSTIC: Extract the pure positive numerical scalar amount. NEVER convert currencies. Ignore words like 'pesos', 'dólares', 'dollars', 'euros', 'lucas'.
2. SEMANTIC CATEGORY INFERENCE:
   Map user intent to the closest matching category from the Valid Categories list [{string.Join(", ", categoryList)}]:
   - 'salón', 'peluquería', 'barbería', 'uñas', 'corte de pelo', 'spa', 'estética' -> Map to 'Health', 'Belleza', 'Cuidado Personal', 'Shopping', or closest match.
   - 'gasolina', 'combustible', 'uber', 'taxi', 'peaje', 'metro', 'pasaje', 'carro', 'taller' -> Map to 'Transportation' or closest match.
   - 'almuerzo', 'cena', 'desayuno', 'café', 'restaurante', 'supermercado', 'comida', 'pizza', 'colmado' -> Map to 'Food & Dining' or closest match.
   - 'cine', 'netflix', 'spotify', 'salida', 'juego', 'concierto', 'fiesta' -> Map to 'Entertainment' or closest match.
   - 'luz', 'agua', 'internet', 'teléfono', 'celular', 'cable', 'factura' -> Map to 'Bills & Utilities' or closest match.
   - 'farmacia', 'medicina', 'doctor', 'dentista', 'análisis', 'clínica' -> Map to 'Health' or closest match.
   - 'ropa', 'zapatos', 'tienda', 'mall', 'compras' -> Map to 'Shopping' or closest match.
   - 'libro', 'curso', 'universidad', 'colegio', 'escuela' -> Map to 'Education' or closest match.
   If totally ambiguous, use 'Other'.
3. INDEPENDENT PER-EXPENSE RELATIVE DATE RESOLUTION:
   Each expense MUST have its own independent date based on its specific clause:
   - If user mentions 'hoy', 'today', 'el día de hoy' for an item, that expense's date MUST be '{referenceDate:yyyy-MM-dd}'.
   - If user mentions 'ayer', 'yesterday', 'el día de ayer' for an item, that expense's date MUST be '{referenceDate.AddDays(-1):yyyy-MM-dd}'.
   - If user mentions 'anteayer', 'antier', 'antes de ayer' for an item, that expense's date MUST be '{referenceDate.AddDays(-2):yyyy-MM-dd}'.
   - If user says 'agrega una cena el día de hoy y el día de ayer agrega un gasto en salón', the first expense MUST be '{referenceDate:yyyy-MM-dd}' and the second expense MUST be '{referenceDate.AddDays(-1):yyyy-MM-dd}'. NEVER put the same date for both if different dates were specified!
4. TAG EXTRACTION: If user mentions 'etiqueta X', 'tag X', '#X', 'con la etiqueta X', or 'etiquetado como X', extract 'X' into the tags array without '#'.
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

                var candidateModels = new[] { "gemini-3.5-flash-lite", model }.Distinct().ToList();

                HttpResponseMessage? response = null;
                string? lastError = null;

                foreach (var currentModel in candidateModels)
                {
                    var url = $"https://generativelanguage.googleapis.com/v1beta/models/{currentModel}:generateContent?key={apiKey}";
                    try
                    {
                        // Strict 5-second timeout per model attempt to prevent hanging on Google Cloud queue spikes
                        using var perAttemptCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                        perAttemptCts.CancelAfter(TimeSpan.FromSeconds(5));

                        response = await _http.PostAsJsonAsync(url, requestBody, perAttemptCts.Token);
                        if (response.IsSuccessStatusCode)
                        {
                            break;
                        }

                        lastError = await response.Content.ReadAsStringAsync(cancellationToken);
                        _logger.LogWarning("Gemini model {Model} returned status {StatusCode}: {ErrorBody}. Trying alternative model...", currentModel, response.StatusCode, lastError);
                    }
                    catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
                    {
                        lastError = $"Model {currentModel} timed out after 5 seconds.";
                        _logger.LogWarning("Gemini API call timed out after 5 seconds for model {Model}. Trying alternative model...", currentModel);
                    }
                    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        lastError = ex.Message;
                        _logger.LogWarning(ex, "Gemini API call failed for model {Model}. Trying alternative model...", currentModel);
                    }
                }

                if (response == null || !response.IsSuccessStatusCode)
                {
                    _logger.LogError("All Gemini model attempts failed. Activating local heuristic fallback. Last error: {LastError}", lastError);
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
                    Source = "Gemini",
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

        private static readonly Dictionary<string, string[]> SemanticSynonyms = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Health"] = ["salón", "salon", "peluqueria", "peluquería", "barberia", "barbería", "uñas", "spa", "peinado", "belleza", "estetica", "estética", "farmacia", "medicina", "doctor", "dentista", "salud", "médico", "clinica", "clínica", "hospital", "corte", "manicura", "pedicura", "masaje"],
            ["Transportation"] = ["uber", "taxi", "gasolina", "combustible", "peaje", "metro", "pasaje", "carro", "vehiculo", "vehículo", "transporte", "taller", "mecanico", "mecánico", "parqueo", "estacionamiento", "guagua"],
            ["Food & Dining"] = ["almuerzo", "cena", "desayuno", "comida", "supermercado", "restaurante", "restaurant", "café", "cafe", "pizza", "hamburguesa", "colmado", "mercado", "abarrotes", "merienda", "snack", "alimentos", "chuleta", "carne", "pollo", "azafran", "azafrán", "arroz", "habichuela", "viveres", "víveres", "pan", "queso", "leche", "huevo", "huevos", "embutidos"],
            ["Entertainment"] = ["cine", "netflix", "spotify", "salida", "juego", "videojuego", "concierto", "fiesta", "bar", "discoteca", "diversion", "diversión", "hobby", "entretenimiento"],
            ["Bills & Utilities"] = ["luz", "agua", "internet", "telefono", "teléfono", "celular", "cable", "factura", "electricidad", "gas", "servicio", "servicios", "facturas"],
            ["Shopping"] = ["ropa", "zapato", "zapatos", "tienda", "mall", "camisa", "pantalon", "pantalón", "vestido", "compra", "compras"],
            ["Education"] = ["curso", "universidad", "colegio", "escuela", "libro", "libros", "estudio", "estudios", "tutor", "matricula", "matrícula", "educación"]
        };

        private static string MatchCategory(string suggested, List<string> validCategories)
        {
            if (string.IsNullOrWhiteSpace(suggested) || validCategories == null || validCategories.Count == 0)
                return validCategories?.FirstOrDefault() ?? "Other";

            var exact = validCategories.FirstOrDefault(c => string.Equals(c, suggested, StringComparison.OrdinalIgnoreCase));
            if (exact != null) return exact;

            var partial = validCategories.FirstOrDefault(c => c.Contains(suggested, StringComparison.OrdinalIgnoreCase) || suggested.Contains(c, StringComparison.OrdinalIgnoreCase));
            if (partial != null) return partial;

            // Semantic dictionary matching
            var lowerSuggested = suggested.ToLowerInvariant();
            foreach (var (standardCategory, keywords) in SemanticSynonyms)
            {
                if (keywords.Any(k => lowerSuggested.Contains(k, StringComparison.OrdinalIgnoreCase)))
                {
                    // Check if user has this standard category or a synonym of it
                    var matchingCategory = validCategories.FirstOrDefault(c =>
                        string.Equals(c, standardCategory, StringComparison.OrdinalIgnoreCase) ||
                        c.Contains(standardCategory, StringComparison.OrdinalIgnoreCase) ||
                        (standardCategory == "Health" && (c.Contains("Belleza", StringComparison.OrdinalIgnoreCase) || c.Contains("Personal", StringComparison.OrdinalIgnoreCase) || c.Contains("Salud", StringComparison.OrdinalIgnoreCase))) ||
                        (standardCategory == "Transportation" && c.Contains("Transport", StringComparison.OrdinalIgnoreCase)) ||
                        (standardCategory == "Food & Dining" && (c.Contains("Comida", StringComparison.OrdinalIgnoreCase) || c.Contains("Aliment", StringComparison.OrdinalIgnoreCase))) ||
                        (standardCategory == "Bills & Utilities" && (c.Contains("Servicio", StringComparison.OrdinalIgnoreCase) || c.Contains("Factura", StringComparison.OrdinalIgnoreCase))));

                    if (matchingCategory != null) return matchingCategory;

                    var directMatch = validCategories.FirstOrDefault(c => string.Equals(c, standardCategory, StringComparison.OrdinalIgnoreCase));
                    if (directMatch != null) return directMatch;
                }
            }

            return validCategories.FirstOrDefault(c =>
                c.Equals("Other", StringComparison.OrdinalIgnoreCase) ||
                c.Equals("Otros", StringComparison.OrdinalIgnoreCase) ||
                c.Equals("General", StringComparison.OrdinalIgnoreCase) ||
                c.Equals("Varios", StringComparison.OrdinalIgnoreCase))
                ?? validCategories.FirstOrDefault()
                ?? "Other";
        }

        private static AiFinancialPlanDto FallbackLocalParser(string prompt, DateTime referenceDate, List<string> validCategories)
        {
            var plan = new AiFinancialPlanDto { Source = "Local" };

            // 1. Extract Budget commands (e.g. "presupuesto de 400 en Comida")
            var budgetMatch = Regex.Match(prompt, @"presupuesto\s+(?:de\s+)?(\d+(?:[.,]\d+)?)\s*(?:pesos|d[oó]lares|usd|dop|\$)?\s*(?:en|para)?\s*([a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+)", RegexOptions.IgnoreCase);
            if (budgetMatch.Success && decimal.TryParse(budgetMatch.Groups[1].Value.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var budgetAmt) && budgetAmt > 0)
            {
                var rawCategory = CleanDescription(budgetMatch.Groups[2].Value);
                plan.Budgets.Add(new AiParsedBudgetDto
                {
                    Category = MatchCategory(rawCategory, validCategories),
                    MonthlyLimit = budgetAmt,
                    Year = referenceDate.Year,
                    Month = referenceDate.Month
                });
            }

            // 2. Split into clauses to evaluate each expense and its independent date
            var clauseDelimiters = new[] {
                @"(?:y\s+)?tambi[eé]n\s+(?:compr[eé]|agr[eé]game|pagu[eé])?\b",
                @"\by\s+el\s+d[ií]a\s+de\b",
                @"\by\s+el\b",
                @"\by\s+ayer\b",
                @"\by\s+hoy\b",
                @"\badem[aá]s\b",
                @"\by\b",
                @"[;,\.\n]+"
            };
            var splitPattern = string.Join("|", clauseDelimiters);
            var rawClauses = Regex.Split(prompt, splitPattern, RegexOptions.IgnoreCase);

            var clauses = new List<string>();
            foreach (var c in rawClauses)
            {
                var trimmed = c.Trim();
                if (trimmed.Length > 0 && !trimmed.StartsWith("presupuesto", StringComparison.OrdinalIgnoreCase))
                {
                    clauses.Add(trimmed);
                }
            }

            if (clauses.Count == 0)
            {
                clauses.Add(prompt);
            }

            foreach (var clause in clauses)
            {
                // Skip if this clause is purely a budget definition
                if (clause.Contains("presupuesto", StringComparison.OrdinalIgnoreCase)) continue;

                // Determine date specifically for THIS clause
                var clauseDate = referenceDate.Date;
                if (Regex.IsMatch(clause, @"\b(?:anteayer|antier|antes de ayer|el d[ií]a antes de ayer)\b", RegexOptions.IgnoreCase))
                {
                    clauseDate = referenceDate.AddDays(-2).Date;
                }
                else if (Regex.IsMatch(clause, @"\b(?:ayer|yesterday|d[ií]a de ayer|el d[ií]a de ayer)\b", RegexOptions.IgnoreCase))
                {
                    clauseDate = referenceDate.AddDays(-1).Date;
                }
                else if (Regex.IsMatch(clause, @"\b(?:hoy|today|d[ií]a de hoy|el d[ií]a de hoy)\b", RegexOptions.IgnoreCase))
                {
                    clauseDate = referenceDate.Date;
                }
                else
                {
                    var daysAgoMatch = Regex.Match(clause, @"\bhace\s+(\d+)\s+d[ií]as\b", RegexOptions.IgnoreCase);
                    if (daysAgoMatch.Success && int.TryParse(daysAgoMatch.Groups[1].Value, out var nDays))
                    {
                        clauseDate = referenceDate.AddDays(-nDays).Date;
                    }
                }

                // Extract tags for this clause
                var clauseTags = new List<string>();
                var tagMatches = Regex.Matches(clause, @"(?:con\s+la\s+etiqueta|con\s+etiqueta|etiqueta|tag|#)\s*:?\s*([a-zA-Z0-9áéíóúÁÉÍÓÚñÑ_-]+)", RegexOptions.IgnoreCase);
                foreach (Match tm in tagMatches)
                {
                    var val = tm.Groups[1].Value.Trim().TrimStart('#');
                    if (!string.IsNullOrWhiteSpace(val) && !clauseTags.Contains(val, StringComparer.OrdinalIgnoreCase))
                    {
                        clauseTags.Add(val);
                    }
                }

                // Extract amount and concept
                decimal amt = 0;
                string rawConcept = "";

                // Check for quantity multiplier like "dos cubetas ... cada una 50 pesos"
                var qtyMatch = Regex.Match(clause, @"\b(?:dos|2)\s+([a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+?)\s+(?:cada una|por)\s*(?:a|me encontraron|me salieron|me costaron)?\s*(\d+(?:[.,]\d+)?)", RegexOptions.IgnoreCase);
                if (qtyMatch.Success && decimal.TryParse(qtyMatch.Groups[2].Value.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var unitPrice))
                {
                    amt = 2 * unitPrice;
                    rawConcept = qtyMatch.Groups[1].Value;
                }
                else
                {
                    // Try "amount before text" (e.g. "500 pesos para pagar la farmacia")
                    var amtFirst = Regex.Match(clause, @"(?:(\d+(?:[.,]\d+)?))\s*(?:pesos|d[oó]lares|usd|dop|\$)?\s*(?:en|de|para|por)?\s*([a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+)", RegexOptions.IgnoreCase);
                    // Try "text before amount" (e.g. "dos azafranes que costaron 200 pesos" / "cocinamos ... que fueron 30 pesos")
                    var amtSecond = Regex.Match(clause, @"([a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+?)\s*(?:de|en|por|para|que fueron|fueron|que costaron|costaron|que me costaron)?\s*(\$?\s*\d+(?:[.,]\d+)?)\s*(?:pesos|d[oó]lares|usd|dop|\$)?", RegexOptions.IgnoreCase);

                    if (amtFirst.Success && decimal.TryParse(amtFirst.Groups[1].Value.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var parsed1) && parsed1 > 0)
                    {
                        amt = parsed1;
                        rawConcept = amtFirst.Groups[2].Value;
                    }
                    else if (amtSecond.Success)
                    {
                        var numStr = Regex.Match(amtSecond.Groups[2].Value, @"\d+(?:[.,]\d+)?").Value;
                        if (decimal.TryParse(numStr.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var parsed2) && parsed2 > 0)
                        {
                            amt = parsed2;
                            rawConcept = amtSecond.Groups[1].Value;
                        }
                    }
                    else
                    {
                        var numOnly = Regex.Match(clause, @"\d+(?:[.,]\d+)?");
                        if (numOnly.Success && decimal.TryParse(numOnly.Value.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var parsed3) && parsed3 > 0)
                        {
                            amt = parsed3;
                            rawConcept = clause.Replace(numOnly.Value, "");
                        }
                    }
                }

                if (amt > 0)
                {
                    var cleanDesc = CleanDescription(rawConcept);
                    if (cleanDesc.Length < 2) cleanDesc = "Gasto";

                    plan.Expenses.Add(new AiParsedExpenseDto
                    {
                        Amount = amt,
                        Description = char.ToUpper(cleanDesc[0]) + cleanDesc[1..],
                        Category = MatchCategory(rawConcept, validCategories),
                        Date = clauseDate,
                        Tags = [.. clauseTags]
                    });
                }
            }

            // Global fallback if no expenses or budgets extracted
            if (plan.Expenses.Count == 0 && plan.Budgets.Count == 0)
            {
                var singleAmtMatch = Regex.Match(prompt, @"(\d+(?:[.,]\d+)?)");
                if (singleAmtMatch.Success && decimal.TryParse(singleAmtMatch.Groups[1].Value.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var singleAmt) && singleAmt > 0)
                {
                    var cleanPrompt = CleanDescription(prompt);
                    plan.Expenses.Add(new AiParsedExpenseDto
                    {
                        Amount = singleAmt,
                        Description = cleanPrompt.Length > 40 ? cleanPrompt[..40] : (cleanPrompt.Length > 2 ? cleanPrompt : "Gasto"),
                        Category = MatchCategory(cleanPrompt, validCategories),
                        Date = referenceDate.Date,
                        Tags = []
                    });
                }
            }

            plan.Summary = plan.HasActions
                ? $"Se detectaron {plan.Expenses.Count} gasto(s) y {plan.Budgets.Count} presupuesto(s)."
                : "No se reconocieron montos específicos en el comando.";

            return plan;
        }

        private static string CleanDescription(string input)
        {
            var s = input;
            s = Regex.Replace(s, @"\b(?:pesos|d[oó]lares|dollars|usd|dop|eur|euros|\$)\b", "", RegexOptions.IgnoreCase);
            s = Regex.Replace(s, @"\b(?:para\s+el\s+d[ií]a\s+de\s+ayer|el\s+d[ií]a\s+de\s+ayer|d[ií]a\s+de\s+ayer|ayer|anteayer|antier|el\s+d[ií]a\s+antes\s+de\s+ayer|antes\s+de\s+ayer|para\s+el\s+d[ií]a\s+de\s+hoy|el\s+d[ií]a\s+de\s+hoy|d[ií]a\s+de\s+hoy|hoy|today|yesterday)\b", "", RegexOptions.IgnoreCase);
            s = Regex.Replace(s, @"\b(?:con\s+la\s+etiqueta|con\s+etiqueta|etiqueta|tag|#)\s*:?\s*[a-zA-Z0-9áéíóúÁÉÍÓÚñÑ_-]+", "", RegexOptions.IgnoreCase);
            s = Regex.Replace(s, @"\b(?:podr[ií]as|por\s+favor|agr[eé]game|agrega|agregame|compr[eé]|comprar|pagar|pagu[eé]|un\s+gasto|gasto|gastos|otra\s+cosa|que\s+me|me\s+encontraron|me\s+salieron|me\s+costaron|as[ií]\s+que|cada\s+una|lo\s+que\s+viene\s+siendo|que\s+fueron|fueron|el\s+cual|me|de|en|para|el|la|los|las|un|una)\b", "", RegexOptions.IgnoreCase);
            s = Regex.Replace(s, @"\s+", " ");
            s = s.Trim(',', '.', ' ', '-', ';', ':', '$');
            return string.IsNullOrWhiteSpace(s) ? "Gasto" : s;
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
