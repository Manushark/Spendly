# Changelog

Todos los cambios notables de este proyecto serán documentados en este archivo.

El formato está basado en [Keep a Changelog](https://keepachangelog.com/es-ES/1.0.0/),
y este proyecto se adhiere a [Semantic Versioning](https://semver.org/lang/es/).

## [Unreleased]

## [1.22.1] - 2026-09-07
*(Optimizaciones de diseño responsivo móvil, fijación de sidebar móvil con soporte de viewport dinámico, adaptación de menús dropdown para pantallas angostas, soporte de flex-wrap en encabezados y refinamiento de tarjetas de autenticación y landing page)*

### Arreglado
- **Fijación y dimensionamiento del Sidebar Móvil**:
  - Coordenadas explícitas `top: 0; left: 0; bottom: 0;` y altura dinámica `100dvh` con soporte de `safe-area-inset-bottom` en dispositivos móviles.
  - Cierre automático del overlay y menú lateral al redimensionar la ventana a pantallas de escritorio (`resize` event listener).
  - Overlay de navegación móvil con `inset: 0` y `backdrop-filter: blur(3px)` para cubrir de forma consistente todo el viewport.
- **Desbordamiento de Dropdowns en pantallas angostas (< 380px)**:
  - Ancho responsivo `max-width: calc(100vw - 1.5rem)` en el menú de notificaciones y selector de temas en `_Layout.cshtml`.
  - Regla `@media (max-width: 575.98px)` para `.dropdown-menu-end` garantizando que los menús desplegables nunca desborden hacia la izquierda.
- **Ajuste de Cabeceras en Vistas Principales (Flex-Wrap)**:
  - Soporte de `flex-wrap gap-3` en encabezados de páginas para evitar solapamiento entre títulos y botones de acción en Dashboard, Incomes, Budgets, Categories, SavingsGoals, Insights y Tags.
- **Landing Page Responsiva**:
  - Tipografía escalable en el título Hero (`hero-title` de 4.25rem a 1.95rem en móviles).
  - Botones de llamada a la acción (*Call to Action*) adaptados a 100% de ancho con `flex-direction: column` en pantallas pequeñas.
  - Espaciados y espaciado de secciones fluidos en la vista de presentación.
- **Tarjetas de Autenticación en Dispositivos Móviles**:
  - Relleno ergonómico (`padding: 32px 20px` y `border-radius: 20px`) para `.auth-card` en las vistas de Login, Registro, Recuperación de Contraseña y Reseteo de Contraseña.

## [1.22.0] - 2026-09-01
*(Flujo de recuperación de contraseña, sistema de notificaciones por email SMTP, suite completa de pruebas unitarias al 100%, personalización de temas cristalinos, modernización analítica de Dashboard, Reports, Insights y Savings Goals, y documentación técnica completa)*

### Añadido
- **Flujo Completo de Recuperación de Contraseña**:
  - Entidad `PasswordResetToken` con expiración segura de 1 hora.
  - Casos de uso `ForgotPasswordUseCase` y `ResetPasswordUseCase`.
  - Vistas dedicadas `ForgotPassword.cshtml` y `ResetPassword.cshtml` con validación de seguridad y diseño glassmorphic.
  - Endpoints en `AuthController` (API y Web) con protección anti-enumeración de usuarios.
- **Sistema de Notificaciones por Correo Electrónico (SMTP)**:
  - Servicio `SmtpEmailService` con plantillas HTML responsivas y fallback automático a `ConsoleEmailService`.
  - Alertas automáticas de presupuesto por correo electrónico al alcanzar el 80% y 100% de uso mensual.
  - Servicio en segundo plano `WeeklySummaryBackgroundService` para envío del resumen semanal de gastos cada lunes a las 08:00 UTC.
  - Pestaña de Notificaciones en la vista de Ajustes (Settings) con toggles configurables por usuario para alertas de presupuesto y digest semanal.
- **Personalización de Temas y Paletas Cristalinas (Theme & Accent Customizer)**:
  - Selector interactivo en el navbar superior con 8 temas (*Verde Esmeralda, Azul Zafiro, Púrpura Real, Rosa Rubí, Cristal Cian, Oro Ámbar, Índigo Clásico y Obsidiana*).
  - Cambio en caliente instantáneo y persistencia en `localStorage`.
  - Gradientes dinámicos para el sidebar, enlaces activos y halos de luz ambiental (*ambient glow blobs*) adaptados a Modo Claro y Modo Oscuro.
- **Modernización Analítica y Micro-animaciones en Vistas**:
  - **Dashboard**: Barras comparativas Ingresos vs Gastos por mes, lista vertical de categorías con barras de progreso gradiente y shimmer animado, y contadores animados (*count-up*) en tarjetas KPI.
  - **Reportes**: Selector interactivo de Columnas Agrupadas vs Apiladas (*Grouped / Stacked*), líneas de separación visual de períodos con `crosshairs` de selección, y tooltip enriquecido con cálculo de balance neto mensual.
  - **Insights**: Gráfico de área acumulativo con proyección punteada a fin de mes y barra de comparación vs Techo Presupuestario con badges de estado (*On Track / Watch Out / Over Budget*).
  - **Metas de Ahorro**: Efectos de resplandor de neón reactivos al color de cada meta, barras de progreso con gradiente y shimmer, animación de icono y badge dinámico *"¡Casi listo!"*.
- **Suite de Pruebas Unitarias Completa (117 Tests)**:
  - 117 pruebas unitarias con 100% de éxito cubriendo todos los casos de uso (*Auth, User, Budgets, Expenses, Incomes, Categories, SavingsGoals, Tags, RecurringExpenses, Notifications, Dashboard, BudgetAlertService*).
- **Documentación Técnica del Proyecto**:
  - `docs/API_REFERENCE.md` con especificación completa de los 14 controladores y sus endpoints.
  - `docs/ARCHITECTURE.md` con arquitectura limpia por capas y decisiones de diseño.
  - `docs/SETUP.md` con guía paso a paso para despliegue y desarrollo local.
  - `docs/CONTRIBUTING.md` con guía de GitFlow y estándares de código.

### Arreglado
- Bug donde traductores de navegador (como Google Translate) causaban errores al intentar traducir campos de entrada de correo electrónico y contraseña en las vistas de autenticación, agregando el atributo `translate="no"` a los inputs correspondientes.
- Bug en las pantallas de inicio de sesión y registro donde el overlay de carga se vinculaba incorrectamente al formulario de cambio de idioma en el navbar público en lugar del formulario de autenticación correspondiente.
- Inconsistencia visual en los formularios de autenticación, actualizados con cajas de texto de alto contraste y fondo grid interactivo.

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
