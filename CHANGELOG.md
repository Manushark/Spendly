# Changelog

Todos los cambios notables de este proyecto serán documentados en este archivo.

El formato está basado en [Keep a Changelog](https://keepachangelog.com/es-ES/1.0.0/),
y este proyecto se adhiere a [Semantic Versioning](https://semver.org/lang/es/).

## [Unreleased]

## [1.21.0] - 2026-06-08
*(Rediseño visual del módulo de etiquetas, análisis de participación con ApexCharts, CRUD de edición y filtrado avanzado multi-etiqueta)*

### Añadido
- Dashboard premium para el módulo de etiquetas (Tags) con vista de dos columnas.
- Gráfico circular (Donut chart) interactivo de ApexCharts que visualiza la participación del gasto por cada etiqueta.
- Modal interactivo para edición de nombre y color de etiquetas existentes con paleta curada de colores premium.
- Filtro avanzado multi-check por etiquetas en el listado de transacciones de gastos.
- Filtro rápido al hacer clic sobre el badge de una etiqueta en la tabla de gastos.
- Localización y traducción completa al español y al inglés de todo el módulo de etiquetas (`SharedResource`).
- Pruebas unitarias de casos de uso y repositorios actualizadas para soportar filtrado de gastos por múltiples etiquetas de forma opcional.

## [1.20.0] - 2026-05-23
*(Implementación del sistema de reportes avanzados, exportación, heatmap de gastos y sugerencias financieras)*

### Añadido
- Sistema de reportes financieros avanzados con gráficos interactivos de categorías y tendencias (Chart.js).
- Exportación de reportes a PDF y CSV integrando el servicio `ReportExportService` con QuestPDF.
- Heatmap de gastos diarios estilo GitHub con escala de colores de 5 niveles y tooltip flotante interactivo.
- Tarjeta de sugerencias financieras avanzadas ("Financial Insights") basada en un motor de reglas en la capa de casos de uso.
- Columna "Presupuesto vs Real" en la tabla de desglose de categorías en los reportes.
- Selector interactivo de rango de fechas con presets dinámicos y Flatpickr.
- Localización completa de la sección de reportes al español y al inglés.
- Rediseño visual de la sección de reportes para alinearse al tema premium de Spendly.
- Estrategia de ramas GitFlow simplificada (main, develop, feature/, fix/).
- Documentación inicial del proyecto (CHANGELOG).

## [1.0.0] - 2026-05-13
*(Finalización del sistema de notificaciones y consolidación de la versión estable)*

### Añadido
- Centro de Notificaciones en la UI con marcado interactivo de leídas.
- Alertas de notificaciones emergentes (Toasts) en tiempo real con UI animada (Campana).
- Sistema de `Page Visibility API` para optimizar el polling (reducción de consumo de batería/servidor).
- Notificaciones automáticas para: Gastos recurrentes generados, límites de presupuestos superados y metas de ahorro.

### Arreglado
- Bug en `BudgetAlertService` que impedía la escalación de notificaciones de advertencia (80%) a excedido (100%) al editar un gasto.
- Inconsistencia de fecha y zona horaria (UTC) al generar los gastos recurrentes; ahora respetan la hora local del usuario.

## [0.9.0] - 2026-05-09
### Añadido
- Soporte Multi-idioma (i18n) completo implementado en Español e Inglés (`@L` system).
- Archivos `.resx` para vistas y controladores incluyendo reportes, presupuestos, configuraciones y autenticación.
- Funcionalidad de eliminación lógica (Soft-delete) en entidades clave.

### Arreglado
- Remanentes del antiguo dashboard multi-moneda (cleanup total).
- Bug de modales que colisionaban al intentar eliminar registros en la UI.

## [0.8.0] - 2026-05-08
### Añadido
- Hardening de la infraestructura API (Mejoras de Seguridad).
- Implementación de Rate Limiting y Security Middleware para headers.
- Validación de datos externos mediante validadores más robustos en `ImportCsvUseCase`.
- Centralización de variables de entorno sensibles en configuración segura (`appsettings.Production.json`).

## [0.7.0] - 2026-04-15
### Añadido
- Funcionalidad de Metas de Ahorro (Savings Goals).
- Gestión avanzada de Gastos Recurrentes (generación en background).
- Funcionalidad para importar gastos masivos mediante CSV.

### Arreglado
- Bug de error HTTP 500 al filtrar gastos por montos en el repositorio (movido a operación en memoria controlada).

## [0.6.0] - 2026-04-09
### Modificado
- Refactorización total de la aplicación a **Clean Architecture**.
- Separación de capas estricta: `Domain`, `Application`, `Infrastructure`, `Api` y `Web`.
- Introducción de Patrón Repositorio y Casos de Uso (Use Cases) para toda la lógica de negocio.

## [0.5.0] - 2026-04-06
### Añadido
- Autenticación segura mediante JWT Token.

### Modificado
- Optimización de rendimiento: Migración de queries pesadas a SQL asíncrono y eliminación de scaffoldings redundantes.
- Estandarización del manejo de fechas a UTC de manera global en el backend.

## [0.4.0] - 2026-04-01
### Modificado
- Modernización completa de la Interfaz de Usuario (UI).
- Rediseño de las vistas de Login y Registro para igualar el aspecto premium tipo SaaS del dashboard.
- Unificación del layout global eliminando pie de página redundante.

## [0.1.0] - Versiones Iniciales
### Añadido
- Creación del proyecto base en ASP.NET Core 8 MVC.
- Sistema básico de CRUD para Gastos (Expenses) e Ingresos (Incomes).
- Dashboard inicial y creación de la base de datos SQL Server mediante Entity Framework Core.
- Despliegue automatizado CI/CD en Azure (Github Actions).
