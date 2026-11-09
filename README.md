# 🍽️ RestoApp - Sistema de Gestión Gastronómica & Punto de Venta (POS)

[![.NET 10.0](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Avalonia UI](https://img.shields.io/badge/Avalonia-11.x-8A2BE2?logo=avalonia)](https://avaloniaui.net/)
[![SQL Server 2022](https://img.shields.io/badge/SQL%20Server-2022-CC292B?logo=microsoftsqlserver)](https://www.microsoft.com/sql-server)
[![Architecture](https://img.shields.io/badge/Architecture-MVVM%20%7C%20N--Tier-blue)](#-arquitectura-del-sistema)

Sistema de escritorio integral para la administración operativa, monitoreo de salón en tiempo real, comandeo, facturación, reservas, personal, analítica de negocio y recuperación ante desastres en restaurantes, bares y locales gastronómicos.

Desarrollado como proyecto para la cátedra **Taller de Programación II** de la **Universidad Nacional del Nordeste (UNNE)**.

---

## 📌 Tabla de Contenidos
- [Características Principales](#-características-principales)
- [Matriz de Roles y Permisos (RBAC)](#-matriz-de-roles-y-permisos-rbac)
- [Credenciales de Demostración](#-credenciales-de-demostración)
- [Arquitectura del Sistema](#-arquitectura-del-sistema)
- [Modelo de Base de Datos y Persistencia](#-modelo-de-base-de-datos-y-persistencia)
- [Requisitos Previos](#-requisitos-previos)
- [Instalación y Configuración](#-instalación-y-configuración)
- [Ejecución](#-ejecución)
- [Estructura del Proyecto](#-estructura-del-proyecto)
- [Documentación Adicional](#-documentación-adicional)

---

## 🚀 Características Principales

### 1. 🔐 Autenticación y Control de Acceso (RBAC)
- Inicio de sesión unificado ingresando **Usuario** (DNI, Alias o Email) y **Contraseña**.
- Validación estricta con alertas visuales contextuales (banner de error informativo).
- Adaptación dinámica de la interfaz y barra de navegación según el rol del usuario autenticado.
- Modo de contingencia y resiliencia con soporte de cuentas de demostración offline.

### 2. 🪑 Monitoreo de Salón y Mesas en Tiempo Real
- Cuadrícula visual del salón con codificación por colores según el estado operativo:
  - 🟢 **LIBRE**: Disponible para asignación inmediata.
  - 🔴 **OCUPADA**: Con comanda abierta en atención activa.
  - 🔵 **RESERVADA**: Asignada a reserva del turno actual o próximo.
  - 🟠 **EN LIMPIEZA**: En reacondicionamiento higiénico tras el cobro.
- Filtrado por sectores físicos: *Planta Baja*, *2do Piso*, *Terraza*, *Patio*.
- ABM de mesas con número, capacidad y sector asignado, con soporte de **baja lógica** y reactivación.

### 3. 📋 Menú y Carta Gastronómica
- Clasificación de productos por categorías (*Entradas*, *Parrilla y Carnes*, *Pastas*, *Minutas*, *Bebidas*, *Vinos*, *Postres*, *Cafetería*).
- Gestión de artículos, actualización de precios y descripciones.
- Filtro tri-estado para administración y auditoría: *Activos*, *Desactivados* o *Todos*.
- Soporte de baja lógica para preservar históricos de comanda sin inconsistencias.

### 4. 🧾 Comandas y Cuentas de Mesa
- Apertura de comanda (`cuenta_head`) vinculada al número de mesa y mozo a cargo.
- Carga de artículos consumidos (`cuenta_body`) con captura de precio unitario histórico.
- **Cálculo automático del total** mediante Trigger nativo en base de datos (`trg_CuentaBody_RecalcularTotal`).
- Monitoreo en vivo de cuentas pendientes listas para facturación en caja.

### 5. 📅 Reservas y Gestión de Clientes
- Padrón centralizado de clientes (DNI, teléfono y correo electrónico únicos sin duplicados).
- Agendamiento de reservas individuales o grupales con asignación de múltiples mesas.
- Validación de aforo físico para garantizar espacio suficiente a los comensales.
- **Regla de 48 horas:** Columna calculada `fecha_max_cancelacion` para control de penalizaciones de cancelación.
- Historial de reservas activas y panel de auditoría de bajas/cancelaciones.

### 6. 💵 Caja, Facturación y Arqueo de Turno
- Cobro multimedio: *Efectivo*, *Tarjeta de Débito*, *Tarjeta de Crédito*, *Transferencia Bancaria / QR*.
- Liquidación definitiva de cuentas de salón con cambio automático de estado de mesa.
- **Arqueo y balance continuo:**
  - Registro de fondo de caja inicial.
  - Desglose de ingresos discriminados por método de cobro.
  - Cálculo de efectivo esperado vs. efectivo recontado.
  - Detección automática de discrepancias (sobrante/faltante) y resumen de cierre.
- Previsualización y emisión de tickets/comprobantes de pago.

### 7. 🎪 Eventos Especiales
- Programación de fechas conmemorativas, cenas show, eventos privados o degustaciones.
- Vinculación directa con reservas de salón.

### 8. 👥 Gestión de Personal y Turnos
- Directorio de colaboradores con asignación de turnos laborales (`inicio_turno`, `fin_turno`).
- Restricción de base de datos (`UQ_Empleado_RolActivo`) para garantizar que un colaborador tenga **un único rol activo** a la vez.

### 9. 📊 Reportes Analíticos y Business Intelligence (BI)
- Tablero ejecutivo con indicadores clave (KPIs):
  - **Total de Ventas ($)**
  - **Cuentas Cerradas**
  - **Ticket Promedio ($)**
  - **Total de Reservas Concertadas**
  - **Unidades de Menú Vendidas**
- **Ranking Top 10 Platos Más Vendidos** (unidades recaudadas e importe).
- **Desglose de Ingresos por Medio de Pago** (porcentajes y totales monetarios).
- **Métrica de Rendimiento por Mozo** (cuentas atendidas y facturación producida).
- Filtros por fechas personalizadas con presets inmediatos (*Hoy*, *Última Semana*, *Mes*).

### 10. 🛡️ Respaldo y Restauración (Backup & Restore)
- **Copias de Seguridad Nativas SQL Server:** Generación de archivos `.bak` completos con compresión y formato seguro.
- **Diagnóstico Preventivo:** Lectura previa de metadatos vía `RESTORE HEADERONLY` para verificar integridad, base origen (`resto_DB`) y tipo de copia completa (`BackupType = 1`).
- **Protocolo de Doble Autorización Directiva:** La restauración requiere obligatoriamente que **dos (2) directivos distintos** (Dueño y/o Gerente) certifiquen sus credenciales en la ventana de seguridad (`AutorizacionRestoreWindow`).
- **Aislamiento Seguro:** Desconexión forzada de sesiones activas (`SINGLE_USER WITH ROLLBACK IMMEDIATE`), restauración (`WITH REPLACE`) y reconexión inmediata (`MULTI_USER`).

---

## 👥 Matriz de Roles y Permisos (RBAC)

El sistema provee control de acceso granular según la función del colaborador:

| Módulo / Funcionalidad | Dueño | Gerente | Cajero | Recepción | Mozo |
| :--- | :---: | :---: | :---: | :---: | :---: |
| **Inicio (Salón / Estado de Mesas)** | ✅ | ✅ | ✅ | ✅ | ✅ |
| **Gestión de Mesas y Sectores** | ✅ | ✅ | ❌ | 👁️ Lectura | 👁️ Lectura |
| **Consulta de Menú y Precios** | ✅ | ✅ | ❌ | ❌ | ✅ |
| **Administración de Menú (Precios/ABM)** | ✅ | ✅ | ❌ | ❌ | ❌ |
| **Reservas y Clientes** | ✅ | ✅ | ❌ | ✅ | ❌ |
| **Eventos Especiales** | ✅ | ✅ | ❌ | ✅ | ❌ |
| **Personal y Turnos** | ✅ | ✅ Excepto Admins | ❌ | ❌ | ❌ |
| **Caja, Cobranzas y Arqueo** | ✅ | ✅ | ✅ | ❌ | ❌ |
| **Reportes Analíticos (BI)** | ✅ | ✅ | ❌ | ❌ | ❌ |
| **Crear Copia de Seguridad** | ✅ | ❌ | ❌ | ❌ | ❌ |
| **Restaurar Base de Datos** | ✅ Requiere 2 Admins | ✅ Como 2do Autorizante | ❌ | ❌ | ❌ |

---

## 🔑 Credenciales de Demostración

Para agilizar pruebas y evaluación funcional, la base de datos incluye los siguientes usuarios preconfigurados:

| Rol | Nombre y Apellido | Usuario / Alias / DNI | Contraseña | Permisos Destacados |
| :--- | :--- | :--- | :---: | :--- |
| **Dueño** | Giovanni Centurion | `giovanni` / `dueño` / `96079673` | `123123` | Acceso irrestricto, Backups, Restauración, Personal, Reportes. |
| **Gerente** | Hernan Cespedes | `hernan` / `gerente` / `42739764` | `123123` | Administración de salón, Menú, Personal, Caja, 2do autorizante de Restore. |
| **Cajero** | Carlos Cajero | `cajero` / `11111111` | `123123` | Cobro de cuentas, Arqueo de caja, Emisión de comprobantes. |
| **Mozo** | Marcos Mozo | `mozo` / `22222222` | `123123` | Consulta de carta, asignación y estado operativo de mesas. |
| **Recepción** | Rocio Recepcion | `recepcion` / `33333333` | `123123` | Agenda de reservas, eventos, padrón de clientes y disponibilidad. |

> **Nota:** La contraseña por defecto para todos los usuarios del lote de prueba es `123123` (también se admite `123` en el login rápido).

---

## 🏗️ Arquitectura del Sistema

El proyecto sigue una arquitectura multicapa desacoplada y orientada al dominio:

```
resto_app/
├── RestoApp.Desktop/       # Capa de Presentación (Avalonia UI XAML + ViewModels)
│   ├── Assets/             # Íconos, recursos y estilos
│   ├── ViewModels/         # Lógica de presentación (MVVM con CommunityToolkit)
│   ├── Views/              # Vistas XAML (Ventanas modales, controles y navegación)
│   └── Services/           # Servicios de UI (Diálogos de alerta, selectores de archivo)
│
├── RestoApp.Business/      # Capa de Lógica de Negocio
│   └── Services/           # Servicios de dominio (MesaService, MenuService, PagoService, ReporteService, BackupService, etc.)
│
├── RestoApp.Data/          # Capa de Acceso a Datos
│   ├── Repositories/       # Repositorios basados en EF Core y Stored Procedures ADO.NET
│   └── RestoAppDbContext.cs# Contexto principal de Entity Framework Core
│
├── RestoApp.Entities/      # Entidades de Dominio y DTOs
│   ├── Mesa.cs, Reserva.cs, Pago.cs, MenuItem.cs, etc.
│   └── ReporteDtos.cs, BackupHeaderInfo.cs, UsuarioAutorizanteItem.cs
│
└── DB_resto/               # Scripts SQL del Motor de Base de Datos
    ├── RestoApp_DB.sql     # Definición de tablas, relaciones, triggers e índices
    ├── stored_procedures.sql # Procedimientos almacenados para CRUDs, comandeo y reportes
    └── lote_datos_Proyect.sql# Lote de datos iniciales, histórico de cuentas y credenciales
```

---

## 🗄️ Modelo de Base de Datos y Persistencia

El motor relacional **Microsoft SQL Server** implementa un esquema robusto con las siguientes características:
- **Triggers de Integridad Aritmética:** `trg_CuentaBody_RecalcularTotal` actualiza en tiempo real el monto total en `cuenta_head` ante cambios en sus renglones.
- **Índices Filtrados (Conditional Unique Index):** `UQ_Empleado_RolActivo` en la tabla `empleado` sobre la columna `dni_empleado` donde `activo_en_rol = 1`.
- **Columnas Calculadas:** `fecha_max_cancelacion` en la tabla `reserva` calculada automáticamente como `DATEADD(hour, -48, fecha_reserva)`.
- **Procedimientos Almacenados Optimizados:**
  - `sp_Cuenta_Crear`, `sp_Cuenta_AgregarItem`, `sp_Cuenta_Cerrar`
  - `sp_Pago_Crear`, `sp_Pago_ObtenerArqueoTurno`
  - `sp_Reportes_KpisGenerales`, `sp_Reportes_TopPlatos`, `sp_Reportes_VentasPorMetodoPago`, `sp_Reportes_RendimientoMozos`
  - `sp_Mesa_*`, `sp_Menu_*`, `sp_Reserva_*`, `sp_Empleado_*`, `sp_Evento_*`

---

## 💻 Requisitos Previos

- **SDK de .NET:** [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) instalado en el sistema.
- **Servidor de Base de Datos:** [Microsoft SQL Server 2022](https://www.microsoft.com/sql-server) o instancia local (`SQLEXPRESS`) o contenedor Docker.
- **Sistema Operativo:** Linux (Ubuntu 22.04+, Debian, Fedora, Arch) o Microsoft Windows 10/11.

---

## ⚙️ Instalación y Configuración

### 1. Clonar el Repositorio
```bash
git clone https://github.com/tu-usuario/resto_app.git
cd resto_app
```

### 2. Inicializar la Base de Datos
Ejecutar secuencialmente los siguientes scripts en SQL Server Management Studio (SSMS), Azure Data Studio o mediante la herramienta de línea de comandos `sqlcmd`:

1. `DB_resto/RestoApp_DB.sql` — Creación de la base de datos `resto_DB`, tablas, claves foráneas, restricciones y triggers.
2. `DB_resto/stored_procedures.sql` — Creación de los Procedimientos Almacenados transaccionales y de reporte.
3. `DB_resto/lote_datos_Proyect.sql` — Carga del lote de datos iniciales con colaboradores, mesas, carta, cuentas cerradas para reportes y credenciales.

### 3. Configurar la Cadena de Conexión
Verificar y ajustar la cadena de conexión en el archivo `RestoApp.Data/RestoAppDbContext.cs`:

```csharp
optionsBuilder.UseSqlServer("Server=localhost;Database=resto_DB;User Id=SA;Password=TuPassword;TrustServerCertificate=True;");
```
*(O utilizar autenticación integrada para instancias locales de Windows: `Server=localhost\\SQLEXPRESS;Database=resto_DB;Integrated Security=True;TrustServerCertificate=True;`)*

---

## ▶️ Ejecución

Para compilar y ejecutar la aplicación de escritorio:

```bash
# Compilar la solución
dotnet build

# Ejecutar el proyecto de escritorio
dotnet run --project RestoApp.Desktop
```

> **Nota para entornos Linux:** Si se ejecuta en terminales con restricciones de variables o telemetría:
> ```bash
> env DOTNET_CLI_TELEMETRY_OPTOUT=1 AVALONIA_TELEMETRY_OPTOUT=1 dotnet run --project RestoApp.Desktop
> ```

---

## 📚 Documentación Adicional

- [ERS_RestoApp.md](file:///home/shiox/git_Projects/resto_app/ERS_RestoApp.md): **Especificación de Requisitos de Software (ERS)** completa en estándar formal con Requisitos Funcionales, No Funcionales y Reglas de Negocio.
- [DER_resto_App.png](file:///home/shiox/git_Projects/resto_app/DER_resto_App.png): Diagrama Entidad-Relación visual de la base de datos.
- [stored_procedures.sql](file:///home/shiox/git_Projects/resto_app/DB_resto/stored_procedures.sql): Procedimientos almacenados para comandeo, pagos y BI.
