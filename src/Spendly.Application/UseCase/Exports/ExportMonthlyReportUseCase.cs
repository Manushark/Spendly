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

            // Weekly calculations
            var week1 = expenses.Where(e => e.Date.Day >= 1 && e.Date.Day <= 7).Sum(e => e.Amount.Value);
            var week2 = expenses.Where(e => e.Date.Day >= 8 && e.Date.Day <= 14).Sum(e => e.Amount.Value);
            var week3 = expenses.Where(e => e.Date.Day >= 15 && e.Date.Day <= 21).Sum(e => e.Amount.Value);
            var week4 = expenses.Where(e => e.Date.Day >= 22).Sum(e => e.Amount.Value);

            var weeklyData = new[]
            {
                new { Name = "Semana 1 (Días 1-7)", Total = week1 },
                new { Name = "Semana 2 (Días 8-14)", Total = week2 },
                new { Name = "Semana 3 (Días 15-21)", Total = week3 },
                new { Name = "Semana 4+ (Días 22+)", Total = week4 }
            };
            var maxWeeklyTotal = weeklyData.Max(w => w.Total);

            // Savings Rate & Classification
            var savingsRate = totalIncome > 0 ? (balance / totalIncome * 100) : 0;
            var savingsRateString = totalIncome > 0 ? $"{savingsRate:N1}%" : "0.0%";
            string savingsClass;
            string savingsBadgeColor;
            string savingsIcon;

            if (totalIncome == 0)
            {
                savingsClass = "Sin Ingresos";
                savingsBadgeColor = "#64748b";
                savingsIcon = "bi-question-circle-fill";
            }
            else if (savingsRate >= 25)
            {
                savingsClass = "Excelente (≥25%)";
                savingsBadgeColor = "#10b981";
                savingsIcon = "bi-emoji-laughing-fill";
            }
            else if (savingsRate >= 10)
            {
                savingsClass = "Saludable (10-25%)";
                savingsBadgeColor = "#3b82f6";
                savingsIcon = "bi-emoji-smile-fill";
            }
            else if (savingsRate >= 0)
            {
                savingsClass = "Ajustado (0-10%)";
                savingsBadgeColor = "#f59e0b";
                savingsIcon = "bi-emoji-neutral-fill";
            }
            else
            {
                savingsClass = "Déficit (<0%)";
                savingsBadgeColor = "#f43f5e";
                savingsIcon = "bi-exclamation-triangle-fill";
            }

            // Smart Insights
            var largestExpense = expenses.OrderByDescending(e => e.Amount.Value).FirstOrDefault();
            var largestExpenseText = largestExpense != null
                ? $"{largestExpense.Description} (-${largestExpense.Amount.Value:N2})"
                : "No hay registros";

            var peakDayGroup = expenses
                .GroupBy(e => e.Date.Date)
                .Select(g => new { Date = g.Key, Total = g.Sum(e => e.Amount.Value) })
                .OrderByDescending(g => g.Total)
                .FirstOrDefault();
            var peakDayText = peakDayGroup != null
                ? $"{peakDayGroup.Date.ToString("dd MMM", culture)} (${peakDayGroup.Total:N2})"
                : "No hay registros";

            var primaryCategoryGroup = categoryBreakdown.FirstOrDefault();
            var primaryCategoryText = primaryCategoryGroup != null
                ? $"{TranslateCategory(primaryCategoryGroup.Category)} ({primaryCategoryGroup.Total / (totalExpenses > 0 ? totalExpenses : 1) * 100:N1}%)"
                : "No hay registros";

            var html = $@"<!DOCTYPE html>
<html lang='es'>
<head>
    <meta charset='utf-8'>
    <title>Reporte de Spendly - {monthName}</title>
    <style>
        @import url('https://fonts.googleapis.com/css2?family=Outfit:wght@300;400;500;600;700;800&family=JetBrains+Mono:wght@400;500;600;700&display=swap');
        @import url('https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css');

        body {{ 
            font-family: 'Outfit', -apple-system, sans-serif; 
            max-width: 900px; 
            margin: 0 auto; 
            padding: 40px 24px; 
            color: #0f172a; 
            background-color: #f8fafc;
            line-height: 1.5;
        }}
        
        /* Banner Gradient */
        .hero-banner {{
            background: linear-gradient(135deg, #4f46e5 0%, #1e1b4b 100%);
            border-radius: 24px;
            padding: 36px;
            color: #ffffff;
            margin-bottom: 30px;
            box-shadow: 0 10px 25px -5px rgba(79, 70, 229, 0.3), 0 8px 10px -6px rgba(79, 70, 229, 0.3);
            position: relative;
            overflow: hidden;
        }}
        
        .hero-banner::before {{
            content: '';
            position: absolute;
            top: -20%;
            right: -10%;
            width: 300px;
            height: 300px;
            background: radial-gradient(circle, rgba(99, 102, 241, 0.45) 0%, rgba(99, 102, 241, 0) 70%);
            pointer-events: none;
        }}

        .hero-content {{
            display: flex;
            justify-content: space-between;
            align-items: center;
            position: relative;
            z-index: 1;
        }}
        
        .report-badge {{
            background: rgba(255, 255, 255, 0.15);
            backdrop-filter: blur(4px);
            padding: 4px 12px;
            border-radius: 12px;
            font-size: 11px;
            font-weight: 700;
            letter-spacing: 1.5px;
            text-transform: uppercase;
            border: 1px solid rgba(255, 255, 255, 0.25);
            display: inline-block;
            margin-bottom: 12px;
            color: #e0e7ff;
        }}

        .hero-banner h1 {{ 
            font-size: 32px;
            font-weight: 800;
            color: #ffffff; 
            margin: 0;
            letter-spacing: -0.5px;
            line-height: 1.2;
        }}
        
        .hero-period {{
            font-size: 15px;
            color: #c7d2fe;
            margin: 6px 0 0 0;
            font-weight: 500;
        }}
        
        .hero-brand {{
            display: flex;
            align-items: center;
            gap: 12px;
        }}
        
        .brand-logo {{
            width: 48px;
            height: 48px;
            background: rgba(255, 255, 255, 0.2);
            backdrop-filter: blur(8px);
            border: 1px solid rgba(255, 255, 255, 0.3);
            border-radius: 14px;
            display: flex;
            align-items: center;
            justify-content: center;
            font-size: 24px;
            color: #ffffff;
        }}
        
        .brand-name {{
            font-size: 24px;
            font-weight: 800;
            letter-spacing: -0.5px;
            color: #ffffff;
        }}
        
        h2 {{ 
            font-size: 20px;
            font-weight: 700;
            color: #1e293b; 
            margin-top: 40px; 
            margin-bottom: 20px;
            display: flex;
            align-items: center;
            gap: 10px;
            letter-spacing: -0.5px;
        }}

        h2 i {{
            color: #4f46e5;
            font-size: 22px;
        }}
        
        /* Grid Layout */
        .summary-grid {{ 
            display: grid; 
            grid-template-columns: repeat(4, 1fr); 
            gap: 16px; 
            margin: 24px 0; 
        }}
        
        .summary-card {{ 
            background: #ffffff; 
            border: 1px solid #e2e8f0;
            border-radius: 20px; 
            padding: 20px 16px; 
            text-align: left; 
            box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.05), 0 2px 4px -2px rgba(0, 0, 0, 0.05);
            position: relative;
            overflow: hidden;
            display: flex;
            flex-direction: column;
            justify-content: space-between;
        }}

        .summary-card::before {{
            content: '';
            position: absolute;
            top: 0; left: 0; right: 0;
            height: 4px;
        }}

        .card-income::before {{ background: #10b981; }}
        .card-expenses::before {{ background: #f43f5e; }}
        .card-balance::before {{ background: #3b82f6; }}
        .card-savings::before {{ background: #8b5cf6; }}

        .card-icon-wrapper {{
            position: absolute;
            right: 16px;
            top: 16px;
            font-size: 20px;
            opacity: 0.15;
        }}

        .card-income .card-icon-wrapper {{ color: #10b981; }}
        .card-expenses .card-icon-wrapper {{ color: #f43f5e; }}
        .card-balance .card-icon-wrapper {{ color: #3b82f6; }}
        .card-savings .card-icon-wrapper {{ color: #8b5cf6; }}
        
        .summary-card .value {{ 
            font-size: 22px; 
            font-weight: 800; 
            margin: 8px 0 0 0; 
            letter-spacing: -0.5px;
            font-family: 'JetBrains Mono', monospace;
            line-height: 1.2;
        }}
        
        .summary-card .label {{ 
            font-size: 11px; 
            text-transform: uppercase; 
            color: #64748b; 
            letter-spacing: 0.75px; 
            font-weight: 700;
        }}
        
        /* Two Column Section */
        .dashboard-row {{
            display: grid;
            grid-template-columns: 1.2fr 1fr;
            gap: 20px;
            margin-top: 30px;
        }}
        
        .dashboard-col {{
            display: flex;
            flex-direction: column;
        }}
        
        .card-wrapper {{
            background: #ffffff;
            border: 1px solid #e2e8f0;
            border-radius: 20px;
            padding: 24px;
            box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.05), 0 2px 4px -2px rgba(0, 0, 0, 0.05);
            flex-grow: 1;
        }}

        .card-wrapper h3 {{
            margin-top: 0;
            margin-bottom: 18px;
            font-size: 16px;
            font-weight: 700;
            color: #1e293b;
            display: flex;
            align-items: center;
            gap: 8px;
            border-bottom: 1px solid #f1f5f9;
            padding-bottom: 12px;
        }}

        .card-wrapper h3 i {{
            color: #6366f1;
        }}
        
        /* Metric Blocks */
        .insight-grid {{
            display: grid;
            grid-template-columns: repeat(2, 1fr);
            gap: 16px;
        }}
        
        .insight-block {{
            background: #f8fafc;
            border: 1px solid #f1f5f9;
            border-radius: 14px;
            padding: 14px 16px;
        }}
        
        .insight-title {{
            font-size: 11px;
            font-weight: 700;
            text-transform: uppercase;
            color: #64748b;
            letter-spacing: 0.5px;
            margin-bottom: 4px;
        }}
        
        .insight-value {{
            font-size: 13px;
            font-weight: 600;
            color: #1e293b;
            line-height: 1.4;
        }}

        .insight-value .badge {{
            display: inline-flex;
            align-items: center;
            gap: 4px;
            padding: 4px 10px;
            border-radius: 20px;
            font-size: 11px;
            font-weight: 700;
            color: #ffffff;
        }}
        
        /* Weekly Progress Styling */
        .weekly-item {{
            margin-bottom: 16px;
        }}

        .weekly-item:last-child {{
            margin-bottom: 0;
        }}

        .weekly-header {{
            display: flex;
            justify-content: space-between;
            align-items: center;
            font-size: 12px;
            font-weight: 600;
            color: #475569;
            margin-bottom: 6px;
        }}

        .weekly-value {{
            font-family: 'JetBrains Mono', monospace;
            font-weight: 700;
            color: #0f172a;
        }}

        .weekly-track {{
            height: 8px;
            background: #f1f5f9;
            border-radius: 8px;
            overflow: hidden;
            position: relative;
        }}

        .weekly-bar {{
            height: 100%;
            border-radius: 8px;
            background: linear-gradient(90deg, #6366f1 0%, #4f46e5 100%);
        }}
        
        /* Tables */
        table {{ 
            width: 100%; 
            border-collapse: separate; 
            border-spacing: 0;
            margin: 15px 0; 
            background: #ffffff;
            border: 1px solid #e2e8f0;
            border-radius: 16px;
            overflow: hidden;
            box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.05), 0 2px 4px -2px rgba(0, 0, 0, 0.05);
        }}
        
        th {{ 
            background: #f8fafc; 
            text-align: left; 
            padding: 14px 18px; 
            font-size: 11px;
            text-transform: uppercase;
            font-weight: 700;
            color: #475569;
            letter-spacing: 0.75px;
            border-bottom: 1px solid #e2e8f0;
        }}
        
        td {{ 
            padding: 14px 18px; 
            border-bottom: 1px solid #f1f5f9; 
            font-size: 13px;
            color: #334155;
        }}
        
        tr:last-child td {{
            border-bottom: none;
        }}

        tr:hover td {{
            background-color: #f8fafc;
        }}
        
        /* Utilities */
        .text-success {{ color: #10b981; }}
        .text-danger {{ color: #f43f5e; }}
        .text-primary {{ color: #3b82f6; }}
        .text-bold {{ font-weight: 600; }}
        .font-mono {{ font-family: 'JetBrains Mono', monospace; font-size: 13.5px; }}
        .text-center {{ text-align: center; }}
        .text-muted {{ color: #64748b; font-size: 12px; }}
        
        .badge-category {{
            display: inline-flex;
            align-items: center;
            gap: 6px;
            padding: 4px 10px;
            border-radius: 20px;
            font-size: 11.5px;
            font-weight: 700;
            white-space: nowrap;
        }}
        
        .progress-wrapper {{
            display: flex;
            align-items: center;
            gap: 10px;
            width: 100%;
            max-width: 200px;
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
            font-size: 11.5px;
            font-weight: 700;
            color: #475569;
            min-width: 38px;
            text-align: right;
            font-family: 'JetBrains Mono', monospace;
        }}
        
        .footer {{ 
            margin-top: 50px; 
            text-align: center; 
            color: #94a3b8; 
            font-size: 11px; 
            border-top: 1px solid #e2e8f0; 
            padding-top: 20px; 
        }}
        
        /* Print Styles */
        @media print {{ 
            body {{ 
                padding: 0; 
                background-color: #ffffff;
                color: #000000;
            }}
            .hero-banner {{
                background: linear-gradient(135deg, #4f46e5 0%, #1e1b4b 100%) !important;
                -webkit-print-color-adjust: exact !important;
                print-color-adjust: exact !important;
                box-shadow: none !important;
            }}
            .summary-card {{
                box-shadow: none !important;
                border: 1px solid #cbd5e1 !important;
                background: #ffffff !important;
                -webkit-print-color-adjust: exact !important;
                print-color-adjust: exact !important;
            }}
            .card-income::before {{ background: #10b981 !important; }}
            .card-expenses::before {{ background: #f43f5e !important; }}
            .card-balance::before {{ background: #3b82f6 !important; }}
            .card-savings::before {{ background: #8b5cf6 !important; }}

            .insight-block {{
                background: #f8fafc !important;
                border: 1px solid #e2e8f0 !important;
                -webkit-print-color-adjust: exact !important;
                print-color-adjust: exact !important;
            }}
            .weekly-bar {{
                background: #6366f1 !important;
                -webkit-print-color-adjust: exact !important;
                print-color-adjust: exact !important;
            }}
            .weekly-track {{
                background: #e2e8f0 !important;
                -webkit-print-color-adjust: exact !important;
                print-color-adjust: exact !important;
            }}
            .progress-track {{
                background: #e2e8f0 !important;
                -webkit-print-color-adjust: exact !important;
                print-color-adjust: exact !important;
            }}
            .badge-category {{
                -webkit-print-color-adjust: exact !important;
                print-color-adjust: exact !important;
            }}
            
            table, tr, td, th, .card-wrapper {{
                page-break-inside: avoid;
            }}
        }}
    </style>
</head>
<body>
    <div class='hero-banner'>
        <div class='hero-content'>
            <div class='hero-main'>
                <span class='report-badge'>Reporte Mensual</span>
                <h1>Resumen Financiero</h1>
                <p class='hero-period'>{monthName}</p>
            </div>
            <div class='hero-brand'>
                <div class='brand-logo'><i class='bi bi-wallet2'></i></div>
                <span class='brand-name'>Spendly</span>
            </div>
        </div>
    </div>

    <div class='summary-grid'>
        <div class='summary-card card-income'>
            <span class='card-icon-wrapper'><i class='bi bi-arrow-down-left-circle-fill'></i></span>
            <div class='label'>Ingresos Totales</div>
            <div class='value text-success'>${totalIncome:N2}</div>
        </div>
        <div class='summary-card card-expenses'>
            <span class='card-icon-wrapper'><i class='bi bi-arrow-up-right-circle-fill'></i></span>
            <div class='label'>Gastos Totales</div>
            <div class='value text-danger'>${totalExpenses:N2}</div>
        </div>
        <div class='summary-card card-balance'>
            <span class='card-icon-wrapper'><i class='bi bi-bank'></i></span>
            <div class='label'>Saldo Neto</div>
            <div class='value {(balance >= 0 ? "text-success" : "text-danger")}'>${balance:N2}</div>
        </div>
        <div class='summary-card card-savings'>
            <span class='card-icon-wrapper'><i class='bi bi-piggy-bank-fill'></i></span>
            <div class='label'>Tasa de Ahorro</div>
            <div class='value text-primary'>{savingsRateString}</div>
        </div>
    </div>

    <div class='dashboard-row'>
        <div class='dashboard-col'>
            <div class='card-wrapper'>
                <h3><i class='bi bi-lightning-charge-fill'></i> Diagnóstico & Insights</h3>
                <div class='insight-grid'>
                    <div class='insight-block'>
                        <div class='insight-title'>Salud del Ahorro</div>
                        <div class='insight-value' style='margin-top: 4px;'>
                            <span class='badge' style='background-color: {savingsBadgeColor};'>
                                <i class='bi {savingsIcon}'></i> {savingsClass}
                            </span>
                        </div>
                    </div>
                    <div class='insight-block'>
                        <div class='insight-title'>Categoría Principal</div>
                        <div class='insight-value'>{primaryCategoryText}</div>
                    </div>
                    <div class='insight-block'>
                        <div class='insight-title'>Gasto Más Alto</div>
                        <div class='insight-value'>{largestExpenseText}</div>
                    </div>
                    <div class='insight-block'>
                        <div class='insight-title'>Día de Mayor Gasto</div>
                        <div class='insight-value'>{peakDayText}</div>
                    </div>
                </div>
            </div>
        </div>
        
        <div class='dashboard-col'>
            <div class='card-wrapper'>
                <h3><i class='bi bi-calendar3'></i> Distribución Semanal</h3>
                <div style='display: flex; flex-direction: column; justify-content: center; height: calc(100% - 40px);'>
                    ";

            foreach (var week in weeklyData)
            {
                var weekPct = maxWeeklyTotal > 0 ? (week.Total / maxWeeklyTotal * 100) : 0;
                html += $@"
                    <div class='weekly-item'>
                        <div class='weekly-header'>
                            <span>{week.Name}</span>
                            <span class='weekly-value'>${week.Total:N2}</span>
                        </div>
                        <div class='weekly-track'>
                            <div class='weekly-bar' style='width: {weekPct.ToString("F1", CultureInfo.InvariantCulture)}%;'></div>
                        </div>
                    </div>";
            }

            html += @"
                </div>
            </div>
        </div>
    </div>

    <h2><i class='bi bi-grid-fill'></i> Análisis por Categoría</h2>
    <table>
        <thead>
            <tr>
                <th>Categoría</th>
                <th>Monto</th>
                <th style='text-align:center;'>Transacciones</th>
                <th>% del Total Gastado</th>
            </tr>
        </thead>
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

    <h2><i class='bi bi-list-ol'></i> Transacciones Destacadas (Top 10)</h2>
    <table>
        <thead>
            <tr>
                <th>Fecha</th>
                <th>Descripción</th>
                <th>Categoría</th>
                <th>Monto</th>
            </tr>
        </thead>
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
        Reporte generado de forma segura por Spendly el {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC
    </div>
</body>
</html>";

            return html;
        }
    }
}
