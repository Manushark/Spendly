using Spendly.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Spendly.Application.UseCases.Exports
{
    /// <summary>
    /// Generates a premium monthly report in HTML format designed to be printed as PDF from the browser.
    /// Provides full translation of default categories to Spanish and custom visual styling.
    /// </summary>
    public class ExportMonthlyReportUseCase
    {
        private readonly IExpenseRepository _expenseRepo;
        private readonly IIncomeRepository _incomeRepo;

        public ExportMonthlyReportUseCase(IExpenseRepository expenseRepo, IIncomeRepository incomeRepo)
        {
            _expenseRepo = expenseRepo;
            _incomeRepo = incomeRepo;
        }

        private static string TranslateCategory(string categoryName)
        {
            if (string.IsNullOrEmpty(categoryName))
                return categoryName;

            return categoryName.ToLowerInvariant() switch
            {
                "food & dining" => "Comida y Restaurantes",
                "transportation" => "Transporte",
                "entertainment" => "Entretenimiento",
                "shopping" => "Compras",
                "health" => "Salud",
                "education" => "Educación",
                "bills & utilities" => "Servicios y Facturas",
                "other" => "Otros",
                _ => categoryName
            };
        }

        private static (string Color, string Icon) GetCategoryStyle(string categoryName)
        {
            return categoryName.ToLowerInvariant() switch
            {
                "food & dining" => ("#FF6B6B", "bi-cup-hot"),
                "transportation" => ("#4ECDC4", "bi-car-front"),
                "entertainment" => ("#9B59B6", "bi-controller"),
                "shopping" => ("#F39C12", "bi-bag"),
                "health" => ("#E74C3C", "bi-heart-pulse"),
                "education" => ("#3498DB", "bi-book"),
                "bills & utilities" => ("#1ABC9C", "bi-lightning"),
                "other" => ("#95A5A6", "bi-three-dots"),
                _ => ("#6c757d", "bi-tag")
            };
        }

        public async Task<string> ExecuteAsync(int userId, int year, int month)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var expenses = (await _expenseRepo.GetByDateRangeAsync(userId, startDate, endDate)).ToList();
            var totalExpenses = expenses.Sum(e => e.Amount.Value);
            var totalIncome = await _incomeRepo.GetTotalAmountAsync(userId, startDate, endDate);
            var balance = totalIncome - totalExpenses;

            var categoryBreakdown = expenses
                .GroupBy(e => e.Category)
                .Select(g => new { Category = g.Key, Total = g.Sum(e => e.Amount.Value), Count = g.Count() })
                .OrderByDescending(c => c.Total)
                .ToList();

            var topExpenses = expenses
                .OrderByDescending(e => e.Amount.Value)
                .Take(10)
                .ToList();

            var culture = new CultureInfo("es-ES");
            var monthName = startDate.ToString("MMMM yyyy", culture);
            monthName = char.ToUpper(monthName[0]) + monthName.Substring(1);

            var html = $@"<!DOCTYPE html>
<html lang='es'>
<head>
    <meta charset='utf-8'>
    <title>Reporte de Spendly - {monthName}</title>
    <style>
        @import url('https://fonts.googleapis.com/css2?family=Inter:wght@300;400;500;600;700;800&display=swap');
        @import url('https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css');

        body {{ 
            font-family: 'Inter', -apple-system, sans-serif; 
            max-width: 800px; 
            margin: 0 auto; 
            padding: 40px 24px; 
            color: #0f172a; 
            background-color: #f8fafc;
            line-height: 1.5;
        }}
        
        .header-container {{
            display: flex;
            justify-content: space-between;
            align-items: center;
            border-bottom: 2px solid #e2e8f0;
            padding-bottom: 20px;
            margin-bottom: 30px;
        }}
        
        .brand {{
            display: flex;
            align-items: center;
            gap: 8px;
            font-size: 20px;
            font-weight: 800;
            color: #4f46e5;
        }}

        .brand i {{
            font-size: 24px;
        }}
        
        h1 {{ 
            font-size: 26px;
            font-weight: 800;
            color: #0f172a; 
            margin: 0;
            letter-spacing: -0.5px;
        }}
        
        .report-subtitle {{
            font-size: 14px;
            color: #64748b;
            margin-top: 4px;
        }}
        
        h2 {{ 
            font-size: 18px;
            font-weight: 700;
            color: #1e293b; 
            margin-top: 40px; 
            margin-bottom: 16px;
            display: flex;
            align-items: center;
            gap: 8px;
        }}
        
        table {{ 
            width: 100%; 
            border-collapse: separate; 
            border-spacing: 0;
            margin: 15px 0; 
            background: #ffffff;
            border: 1px solid #e2e8f0;
            border-radius: 12px;
            overflow: hidden;
            box-shadow: 0 1px 3px 0 rgb(0 0 0 / 0.05);
        }}
        
        th {{ 
            background: #f8fafc; 
            text-align: left; 
            padding: 14px 16px; 
            font-size: 11px;
            text-transform: uppercase;
            font-weight: 600;
            color: #64748b;
            letter-spacing: 0.5px;
            border-bottom: 1px solid #e2e8f0;
        }}
        
        td {{ 
            padding: 12px 16px; 
            border-bottom: 1px solid #f1f5f9; 
            font-size: 14px;
            color: #334155;
        }}
        
        tr:last-child td {{
            border-bottom: none;
        }}
        
        .summary-grid {{ 
            display: grid; 
            grid-template-columns: repeat(3, 1fr); 
            gap: 20px; 
            margin: 24px 0; 
        }}
        
        .summary-card {{ 
            background: #ffffff; 
            border: 1px solid #e2e8f0;
            border-radius: 16px; 
            padding: 24px 20px; 
            text-align: center; 
            box-shadow: 0 4px 6px -1px rgb(0 0 0 / 0.05), 0 2px 4px -2px rgb(0 0 0 / 0.05);
            position: relative;
        }}

        .summary-card::before {{
            content: '';
            position: absolute;
            top: 0; left: 0; right: 0;
            height: 4px;
            border-radius: 16px 16px 0 0;
        }}

        .card-income::before {{ background: #10b981; }}
        .card-expenses::before {{ background: #f43f5e; }}
        .card-balance::before {{ background: #3b82f6; }}
        
        .summary-card .value {{ 
            font-size: 26px; 
            font-weight: 800; 
            margin: 8px 0 0 0; 
            letter-spacing: -0.5px;
            font-family: 'JetBrains Mono', monospace;
        }}
        
        .summary-card .label {{ 
            font-size: 11px; 
            text-transform: uppercase; 
            color: #64748b; 
            letter-spacing: 0.5px; 
            font-weight: 600;
        }}
        
        .text-success {{ color: #10b981; }}
        .text-danger {{ color: #f43f5e; }}
        .text-primary {{ color: #3b82f6; }}
        .text-bold {{ font-weight: 600; }}
        .font-mono {{ font-family: monospace; font-size: 15px; }}
        
        .badge-category {{
            display: inline-flex;
            align-items: center;
            gap: 6px;
            padding: 4px 10px;
            border-radius: 20px;
            font-size: 12px;
            font-weight: 600;
            white-space: nowrap;
        }}
        
        .progress-wrapper {{
            display: flex;
            align-items: center;
            gap: 10px;
            width: 180px;
        }}

        .progress-track {{
            height: 6px;
            background: #e2e8f0;
            border-radius: 10px;
            flex-grow: 1;
            overflow: hidden;
        }}

        .progress-bar {{
            height: 100%;
            border-radius: 10px;
        }}

        .pct-label {{
            font-size: 12px;
            font-weight: 600;
            color: #475569;
            min-width: 38px;
            text-align: right;
        }}
        
        .footer {{ 
            margin-top: 60px; 
            text-align: center; 
            color: #94a3b8; 
            font-size: 11px; 
            border-top: 1px solid #e2e8f0; 
            padding-top: 20px; 
        }}
        
        @media print {{ 
            body {{ 
                padding: 0; 
                background-color: #ffffff;
            }}
            .summary-card {{
                box-shadow: none;
                background: #f8fafc !important;
                -webkit-print-color-adjust: exact;
                print-color-adjust: exact;
            }}
            .progress-track {{
                background: #e2e8f0 !important;
                -webkit-print-color-adjust: exact;
                print-color-adjust: exact;
            }}
            .badge-category {{
                -webkit-print-color-adjust: exact;
                print-color-adjust: exact;
            }}
        }}
    </style>
</head>
<body>
    <div class='header-container'>
        <div>
            <h1>📊 Resumen Mensual</h1>
            <div class='report-subtitle'>Periodo: {monthName}</div>
        </div>
        <div class='brand'>
            <i class='bi bi-wallet2'></i> Spendly
        </div>
    </div>

    <div class='summary-grid'>
        <div class='summary-card card-income'>
            <div class='label'>Ingresos Totales</div>
            <div class='value text-success'>${totalIncome:N2}</div>
        </div>
        <div class='summary-card card-expenses'>
            <div class='label'>Gastos Totales</div>
            <div class='value text-danger'>${totalExpenses:N2}</div>
        </div>
        <div class='summary-card card-balance'>
            <div class='label'>Saldo</div>
            <div class='value {(balance >= 0 ? "text-success" : "text-danger")}'>${balance:N2}</div>
        </div>
    </div>

    <h2>📂 Por Categoría</h2>
    <table>
        <thead><tr><th>Categoría</th><th>Monto</th><th style='text-align:center;'>Cantidad</th><th>% del Total</th></tr></thead>
        <tbody>";

            foreach (var cat in categoryBreakdown)
            {
                var pct = totalExpenses > 0 ? (cat.Total / totalExpenses * 100) : 0;
                var translatedCat = TranslateCategory(cat.Category);
                var style = GetCategoryStyle(cat.Category);
                var color = style.Color;
                var icon = style.Icon;

                html += $@"
            <tr>
                <td>
                    <span class='badge-category' style='background: {color}15; color: {color}; border: 1px solid {color}30;'>
                        <i class='bi {icon}'></i> {translatedCat}
                    </span>
                </td>
                <td class='font-mono text-bold'>${cat.Total:N2}</td>
                <td class='text-center'>{cat.Count}</td>
                <td>
                    <div class='progress-wrapper'>
                        <div class='progress-track'>
                            <div class='progress-bar' style='width: {pct.ToString("F1", CultureInfo.InvariantCulture)}%; background: {color};'></div>
                        </div>
                        <span class='pct-label'>{pct:N1}%</span>
                    </div>
                </td>
            </tr>";
            }

            html += @"
        </tbody>
    </table>

    <h2>🔝 Top 10 Gastos</h2>
    <table>
        <thead><tr><th>Fecha</th><th>Descripción</th><th>Categoría</th><th>Monto</th></tr></thead>
        <tbody>";

            foreach (var exp in topExpenses)
            {
                var translatedCat = TranslateCategory(exp.Category);
                var style = GetCategoryStyle(exp.Category);
                var color = style.Color;
                var icon = style.Icon;

                html += $@"
            <tr>
                <td class='text-muted'>{exp.Date.ToString("MMM dd", culture)}</td>
                <td class='text-bold'>{exp.Description}</td>
                <td>
                    <span class='badge-category' style='background: {color}15; color: {color}; border: 1px solid {color}30;'>
                        <i class='bi {icon}'></i> {translatedCat}
                    </span>
                </td>
                <td class='font-mono text-bold text-danger'>-${exp.Amount.Value:N2}</td>
            </tr>";
            }

            html += $@"
        </tbody>
    </table>

    <div class='footer'>
        Generado por Spendly el {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC
    </div>
</body>
</html>";

            return html;
        }
    }
}
