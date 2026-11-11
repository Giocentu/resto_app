# Especificación de Requisitos de Software (ERS) - RestoApp
---

## 1. Introducción

### 1.1 Propósito
El propósito de este documento es formalizar de forma exhaustiva, estandarizada y precisa la **Especificación de Requisitos de Software (ERS)** para el sistema **RestoApp**. Este documento establece el acuerdo técnico y funcional entre el equipo de ingeniería de software y los responsables del establecimiento gastronómico, detallando las capacidades funcionales, las restricciones operativas, los atributos de calidad y las directrices de diseño que rigen la versión productiva del sistema.

### 1.2 Alcance del Sistema
**RestoApp** es un sistema de escritorio integral de punto de venta (POS) y gestión operativa para restaurantes, bares y establecimientos gastronómicos. La solución centraliza los flujos de trabajo del salón, la administración y la caja, erradicando la dispersión informativa y las fallas de los registros manuales. Sus principales módulos incluyen:
- **Autenticación y Control de Acceso (RBAC):** Identificación segura de usuarios con asignación estricta de privilegios por rol operativo (Dueño, Gerente, Cajero, Recepción, Mozo).
- **Gestión de Salón y Mesas en Tiempo Real:** Monitor interactivo del estado de ocupación (Libre, Ocupada, Reservada, En Limpieza) distribuido por sectores (Planta Baja, 2do Piso, Terraza, Patio).
- **Administración de Reservas y Clientes:** Registro de reservas individuales y grupales, verificación de aforo, control de duplicados de clientes y política de cancelación automática (48 horas).
- **Catálogo de Menú y Carta Gastronómica:** Clasificación por categorías, gestión de precios, control de disponibilidad con soporte de baja lógica y reactivación inmediata.
- **Gestión de Cuentas y Comandas:** Registro de comandas vinculadas a mesas con cálculo dinámico del total mediante triggers transaccionales en base de datos.
- **Facturación, Medios de Pago y Cobranzas:** Soporte multimedio (Efectivo con cálculo automático de cambio/vuelto, Débito, Crédito, Transferencia/QR), liquidación directa de cuentas de mesa e historial de pagos.
- **Gestión de Personal y Turnos:** Ficha del empleado, vinculación de roles activos únicos e historiales de franjas horarias.
- **Gestión de Eventos Especiales:** Planificación de eventos (cumpleaños, shows, cenas corporativas) vinculables a las reservas.
- **Reportes Analíticos y Business Intelligence (BI):** Tablero con KPIs de ventas, ticket promedio, comensales, ranking Top 10 de platos más vendidos, facturación por método de pago y productividad de mozos.
- **Seguridad y Recuperación ante Desastres (Backup & Restore):** Generación de respaldos nativos SQL Server (.bak) y restauración segura validada con protocolo de **Doble Autorización** (dos directivos obligatorios).

### 1.3 Definiciones, Acrónimos y Abreviaturas
- **ERS:** Especificación de Requisitos de Software.
- **RBAC:** *Role-Based Access Control* (Control de Acceso Basado en Roles).
- **POS:** *Point of Sale* (Punto de Venta gastronómico).
- **MVVM:** *Model-View-ViewModel* (Patrón arquitectónico de interfaz de usuario).
- **EF Core:** Entity Framework Core (ORM principal de persistencia de datos).
- **ADO.NET / SqlClient:** Proveedor nativo de acceso a SQL Server para Stored Procedures y comandos de infraestructura.
- **SP:** *Stored Procedure* (Procedimiento Almacenado de base de datos).
- **KPI:** *Key Performance Indicator* (Indicador Clave de Rendimiento).
- **DNI:** Documento Nacional de Identidad.
- **MSSQL:** Microsoft SQL Server (Motor de base de datos relacional).
- **RESTORE HEADERONLY:** Comando de diagnóstico de SQL Server para validar la cabecera de un archivo de backup sin aplicarlo.

### 1.4 Referencias
- Código fuente y scripts de inicialización:
  - Base de Datos: `DB_resto/RestoApp_DB.sql`
  - Procedimientos Almacenados: `DB_resto/stored_procedures.sql`
  - Lote de Datos de Prueba: `DB_resto/lote_datos_Proyect.sql`
  - Diagrama Entidad-Relación: `DER_resto_App.png`
- Documentación de Arquitectura de Microsoft: *Modern Desktop Applications with .NET and Avalonia UI*.

---

## 2. Descripción General

### 2.1 Perspectiva del Producto
**RestoApp** es un sistema autónomo de escritorio estructurado en una arquitectura multicapa en **.NET 10 (C#)**, ejecutado sobre **Avalonia UI** (multiplataforma Linux y Windows) y respaldado por una base de datos relacional **Microsoft SQL Server 2022** (desplegable de forma local o conteinerizada mediante Docker).

```
+--------------------------------------------------------------------------+
|                       RestoApp.Desktop (Avalonia UI)                     |
|           Vistas XAML + ViewModels MVVM (CommunityToolkit.Mvvm)           |
+--------------------------------------------------------------------------+
                                     |
                                     v
+--------------------------------------------------------------------------+
|                     RestoApp.Business (Lógica de Negocio)                |
|           Servicios de Aplicación, DTOs, Validaciones y Reportes         |
+--------------------------------------------------------------------------+
                                     |
                                     v
+--------------------------------------------------------------------------+
|                  RestoApp.Data (Capa de Acceso a Datos)                  |
|     EF Core (DbContext) + Repositorios ADO.NET (SPs) + Backup Engine     |
+--------------------------------------------------------------------------+
                                     |
                                     v
+--------------------------------------------------------------------------+
|                    Base de Datos Microsoft SQL Server                    |
|          `resto_DB` (Tablas, Vistas, Índices Filtrados)        |
+--------------------------------------------------------------------------+
```

### 2.2 Funcionalidades Principales del Producto
1. **Acceso Seguro y Validación:** Login centralizado por Usuario/DNI y contraseña con avisos informativos contextuales y tolerancia a fallos.
2. **Visualización Gráfica del Salón:** Monitor dinámico de mesas con filtrado por sector y codificación por color según estado operativo (*Libre*, *Ocupada*, *Reservada*, *En Limpieza*).
3. **Gestión de Carta y Precios:** Catálogo categorizado de artículos gastronómicos con soporte de alta, baja lógica, edición de precios y restauración.
4. **Comandas y Cuentas de Mesa:** Registro de consumos asociados a mesas con cálculo automatizado del importe total mediante trigger de integridad.
5. **Recepción y Reservas:** Registro de reservas por fecha, hora y comensales, con validación de capacidad física de mesas e historial de cancelaciones.
6. **Facturación y Caja:** Registro de pagos en múltiples medios (Efectivo, Débito, Crédito, Transferencia/QR), auditoría de transacciones y emisión de comprobantes fiscales/comerciales.
7. **Gestión de Personal:** Padrón de colaboradores con asignación de turnos y restricción estricta de un solo rol activo simultáneo.
8. **Planificación de Eventos:** Vinculación de reservas con celebraciones, espectáculos o fechas especiales.
9. **Inteligencia de Negocio y Reportes:** Métricas ejecutivas (Facturación total, Ticket promedio, Platos más vendidos, Rendimiento por mozo) con selector de rangos de fecha.
10. **Respaldo y Seguridad de Datos:** Copias de seguridad completas `.bak` y restauración protegida por protocolo de doble autorización directiva.

### 2.3 Matriz de Control de Acceso Basada en Roles (RBAC)

La interfaz se adapta dinámicamente según el rol del usuario autenticado:

| Módulo / Funcionalidad | Dueño | Gerente | Cajero | Recepción | Mozo |
| :--- | :---: | :---: | :---: | :---: | :---: |
| **Inicio (Salón interactivo / Estado de Mesas)** | ✅ Acceso | ✅ Acceso | ✅ Acceso | ✅ Acceso | ✅ Acceso |
| **Gestión de Mesas y Ubicaciones (ABM)** | ✅ Total | ✅ Total | ❌ | 👁️ Lectura | 👁️ Lectura |
| **Carta y Menú (Consulta)** | ✅ Total | ✅ Total | ❌ | ❌ | ✅ Consulta |
| **Carta y Menú (Creación / Edición / Precios)** | ✅ Total | ✅ Total | ❌ | ❌ | ❌ |
| **Reservas y Clientes (Crear / Modificar / Bajas)** | ✅ Total | ✅ Total | ❌ | ✅ Total | ❌ |
| **Eventos Especiales** | ✅ Total | ✅ Total | ❌ | ✅ Total | ❌ |
| **Personal y Turnos (Alta / Asignación de Roles)** | ✅ Total | ✅ Excepto Admins | ❌ | ❌ | ❌ |
| **Caja, Facturación e Historial de Cobros** | ✅ Total | ✅ Total | ✅ Total | ❌ | ❌ |
| **Reportes y Estadísticas (BI)** | ✅ Total | ✅ Total | ❌ | ❌ | ❌ |
| **Crear Copia de Seguridad (.bak)** | ✅ Total | ❌ | ❌ | ❌ | ❌ |
| **Restaurar Base de Datos (Doble Autorización)** | ✅ Requiere 2 Admins | ✅ Como 2do Autorizante | ❌ | ❌ | ❌ |

### 2.4 Entorno Operativo y Requisitos de Plataforma
- **Sistema Operativo:** Linux (Ubuntu 22.04 LTS o superior, Debian, Arch Linux) / Microsoft Windows 10 u 11 (64 bits).
- **Entorno de Ejecución:** .NET 10.0 SDK o .NET 10.0 Desktop Runtime.
- **Servidor de Base de Datos:** Microsoft SQL Server 2022 o superior / Azure SQL Edge / Contenedor Docker oficial (`mcr.microsoft.com/mssql/server:2022-latest`).
- **Resolución Mínima:** 1024 x 768 píxeles (diseño optimizado con paneles deslizables y soporte táctil para puestos de salón y caja).

### 2.5 Restricciones de Diseño e Implementación
- **Framework de Presentación:** Avalonia UI versión 11+ implementando arquitectura MVVM pura con `CommunityToolkit.Mvvm`.
- **Integridad de Datos:** Triggers nativos para actualización de totales en comanda (`trg_CuentaBody_RecalcularTotal`) y restricciones *UNIQUE* condicionales en asignación de personal (`UQ_Empleado_RolActivo`).
- **Persistencia Mixta:** EF Core para consultas reactivas y mapeos entidad-relación; Procedimientos Almacenados (SPs) transaccionales para operaciones críticas de creación, arqueo y liquidación.
- **Concurrencia en Restauración:** El proceso de Restore exige conmutar la base a `SINGLE_USER WITH ROLLBACK IMMEDIATE` y volver a `MULTI_USER` para evitar bloqueos por conexiones huérfanas.

---

## 3. Requisitos Funcionales (RF)

### Módulo 1: Autenticación, Seguridad y Perfiles

#### RF-01.1: Inicio de Sesión Unificado
El sistema debe permitir a cualquier empleado registrar su ingreso utilizando su identificador de usuario (DNI numérico, dirección de correo electrónico o nombre de usuario/rol registrado) junto con su contraseña.

#### RF-01.2: Validación de Credenciales y Mensajes Contextuales
El sistema debe comprobar la exactitud de las credenciales ingresadas. Ante campos vacíos, credenciales inválidas o usuarios inexistentes, la interfaz debe desplegar de inmediato un mensaje de alerta visible (banner de error) sin abortar la ejecución.

#### RF-01.3: Verificación de Estado Operativo Activo
El acceso debe ser denegado automáticamente a empleados cuyo estado en la tabla `empleado` tenga `activo_en_rol = 0`, indicando que la cuenta ha sido dada de baja temporal o definitivamente.

#### RF-01.4: Modo de Resiliencia / Sesión Fuera de Línea
En caso de interrupción del enlace de red con el servidor de base de datos durante el arranque, el sistema debe proveer acceso en modo de contingencia con credenciales autorizadas de demostración para permitir operaciones esenciales de emergencia.

#### RF-01.5: Cierre de Sesión y Restablecimiento de Contexto
El usuario puede cerrar su sesión en cualquier momento mediante un botón claramente identificado en la cabecera, limpiando la memoria del contexto (`SesionGlobal`) y retornando a la ventana de acceso principal.

---

### Módulo 2: Salón y Mesas

#### RF-02.1: Catálogo de Sectores y Ubicaciones
El sistema debe gestionar los sectores físicos del establecimiento (`ubicacion_mesa`: Terraza, 2do Piso, Planta Baja, Patio, etc.) permitiendo altas y asignaciones de mesas.

#### RF-02.2: Catálogo y Atributos de Mesas
Cada mesa debe contar con número identificador único activo (`nro_mesa`), capacidad máxima de personas (`capacidad`), sector físico asignado (`id_ubicacion`) y estado operativo (`estado`).

#### RF-02.3: Monitoreo Visual en Tiempo Real
La pantalla de inicio debe exhibir una cuadrícula interactiva con la totalidad de las mesas activas, distinguiendo visualmente mediante colores distintivos su estado:
- **Verde / Libre:** Disponible para comensales inmediatos o asignación.
- **Rojo / Ocupada:** Con clientes y comanda en curso.
- **Azul / Reservada:** Bloqueada por reserva anticipada para el turno.
- **Naranja / En Limpieza:** En proceso de acondicionamiento higiénico tras el cierre de cuenta.

#### RF-02.4: Bajas Lógicas y Reactivación de Mesas
El Dueño y el Gerente podrán inhabilitar mesas mediante baja lógica (`es_activo = 0`), impidiendo que figuren en la cuadrícula de comensales pero preservando la integridad histórica de reservas y pagos pasados, con capacidad de reactivación posterior.

---

### Módulo 3: Menú, Categorías y Carta Gastronómica

#### RF-03.1: Clasificación por Categorías
El sistema debe permitir organizar la carta mediante categorías de productos (`categoria_menu`: Entradas, Carnes y Parrilla, Pastas, Minutas, Bebidas, Vinos, Postres, etc.).

#### RF-03.2: Gestión de Artículos del Menú
Los administradores (Dueño y Gerente) podrán dar de alta, editar la descripción y actualizar los precios de venta al público (`precio > 0`) de cada ítem de la carta.

#### RF-03.3: Filtros Dinámicos por Estado
La interfaz del menú debe permitir alternar la visualización entre ítems:
- **Activos:** Disponibles para comanda y venta en salón.
- **Desactivados:** Artículos fuera de temporada o sin stock.
- **Todos:** Vista global para administración y auditoría de precios.

#### RF-03.4: Bajas Lógicas y Restauración de Ítems
Los ítems descontinuados deben poder darse de baja lógica (`activo = 0`) para no perturbar registros históricos de ventas anteriores, pudiendo ser restaurados en cualquier instante.

---

### Módulo 4: Cuentas y Comandas de Salón

#### RF-04.1: Apertura de Cuenta por Mesa
Al ocuparse una mesa, el sistema debe permitir la apertura de una comanda encabezado (`cuenta_head`) asociada al número de mesa y al empleado (Mozo o Cajero) que atiende.

#### RF-04.2: Incorporación de Ítems Consumidos
El personal de salón debe poder agregar artículos del catálogo de menú a la comanda (`cuenta_body`), especificando la cantidad consumida y capturando el precio unitario vigente al momento de la orden.

#### RF-04.3: Recálculo Transaccional del Total
La base de datos debe actualizar automáticamente el campo `total` de la comanda mediante el trigger `trg_CuentaBody_RecalcularTotal`, garantizando que cualquier inserción, modificación o supresión de renglones refleje inmediatamente la suma exacta `∑(cantidad * precio_unitario)`.

#### RF-04.4: Visualización de Cuentas Pendientes
El módulo de caja debe presentar en todo momento la lista de mesas ocupadas con comandas abiertas, exhibiendo número de mesa, mozo a cargo, hora de apertura e importe acumulado.

---

### Módulo 5: Reservas y Gestión de Clientes

#### RF-05.1: Padrón Unificado de Clientes
El sistema debe mantener un registro centralizado de clientes con DNI, nombre, apellido, teléfono y correo electrónico, validando unicidad de datos para prevenir duplicados.

#### RF-05.2: Creación de Reservas Simples y Grupales
El personal autorizado (Dueño, Gerente, Recepción) podrá agendar reservas ingresando cliente, fecha y hora futura, número de comensales, evento especial opcional y la asignación de una o más mesas físicas.

#### RF-05.3: Validación de Aforo y Capacidad de Mesas
El sistema debe comprobar que la suma de capacidades de las mesas seleccionadas en `reserva_mesa` sea suficiente para alojar la cantidad de comensales estipulada.

#### RF-05.4: Política de Cancelación Anticipada (Regla de 48 Horas)
La base de datos calculará en tiempo real la columna calculada `fecha_max_cancelacion` como `DATEADD(hour, -48, fecha_reserva)`. Las solicitudes de cancelación registradas con posterioridad a dicho límite quedarán tipificadas para su posterior penalización.

#### RF-05.5: Auditoría de Bajas y Cancelaciones
El módulo de reservas debe incorporar una pestaña o vista dedicada para inspeccionar las reservas canceladas o no efectivizadas, permitiendo análisis de mermas de público.

---

### Módulo 6: Caja, Facturación y Medios de Pago

#### RF-06.1: Procesamiento y Liquidación de Cuentas
El Cajero o Administrador puede seleccionar una comanda abierta, corroborar el detalle de ítems y procesar el cobro total.

#### RF-06.2: Registro Multimedio de Pago y Cálculo de Vuelto
El cobro admite los métodos oficiales homologados en `metodo_pago` (Efectivo, Tarjeta de Débito, Tarjeta de Crédito, Transferencia Bancaria / QR), vinculando el monto liquidado con la cuenta (`id_cuenta`) o reserva (`id_reserva`). En cobros en efectivo, el sistema calcula y exhibe en tiempo real el vuelto a entregar según el importe abonado por el cliente.

#### RF-06.3: Historial y Auditoría de Pagos Realizados
El módulo dispone de una vista dedicada para consultar todos los cobros procesados, con filtros rápidos (*Solo Hoy* / *Todos*), buscador por comanda o mozo, y soporte de anulación/rollback de cobros erróneos.

#### RF-06.4: Emisión y Consulta de Comprobantes Digitales (Tickets)
El sistema genera el comprobante fiscal/comercial detallado de cada operación con desglose de productos consumidos, subtotales, método de pago e importe total facturado.

---

### Módulo 7: Gestión de Personal, Turnos y Eventos

#### RF-07.1: Registro de Empleados y Padrón Laboral
Permitir el alta de colaboradores vinculándolos a su registro de persona, con datos de contacto, rol asignado y franja horaria laboral.

#### RF-07.2: Regla de Unicidad de Rol Activo
El sistema debe aplicar una restricción de índice único condicional (`UQ_Empleado_RolActivo`) para impedir que un mismo empleado tenga dos registros con `activo_en_rol = 1`.

#### RF-07.3: Gestión de Franjas Horarias
Administración de turnos de trabajo (`inicio_turno`, `fin_turno`) para asignación a mozos, cocineros, cajeros y personal de salón.

#### RF-07.4: Catálogo de Eventos Especiales
Registro de celebraciones especiales (cumpleaños, shows, cenas de fin de año, catas de vino) asociables a reservas para reserva prioritaria de salones o menús especiales.

---

### Módulo 8: Reportes Analíticos y Business Intelligence (BI)

#### RF-08.1: Panel de KPIs Ejecutivos
El sistema debe calcular y mostrar indicadores de desempeño en tiempo real para el rango de fechas seleccionado:
- **Facturación Total:** Suma total de pagos registrados en el período.
- **Cuentas Cerradas:** Cantidad total de comandas liquidadas con éxito.
- **Ticket Promedio:** Importe promedio por comanda cerrada (`Total Ventas / Cuentas Cerradas`).
- **Total de Reservas:** Volumen de reservas concertadas en el intervalo.
- **Artículos Vendidos:** Sumatoria de ítems de menú consumidos.

#### RF-08.2: Ranking Top 10 Platos Más Vendidos
Generación de un ranking ordenado por volumen de unidades y recaudación total de los artículos de la carta más demandados (`sp_Reportes_TopPlatos`).

#### RF-08.3: Desglose de Ventas por Método de Pago
Estadística porcentual y monetaria de ingresos discriminados por Efectivo, Tarjetas de Débito/Crédito y Transferencias/QR (`sp_Reportes_VentasPorMetodoPago`).

#### RF-08.4: Rendimiento y Productividad de Mozos
Métrica de productividad individual por mozo/empleado de atención: total de cuentas atendidas y volumen total de facturación generado (`sp_Reportes_RendimientoMozos`).

#### RF-08.5: Filtros Temporales y Presets
El módulo debe incluir selector de fecha inicial/final y accesos directos (*Hoy*, *Últimos 7 Días*, *Mes en Curso*).

---

### Módulo 9: Respaldo, Integridad y Recuperación ante Desastres

#### RF-09.1: Generación de Copias de Seguridad Nativas (.bak)
El Dueño puede solicitar la creación de un backup completo de la base de datos `resto_DB` mediante el motor SQL Server (`BACKUP DATABASE ... WITH FORMAT`), seleccionando la ruta de almacenamiento local o compartida.

#### RF-09.2: Inspección y Validación de Archivo (.bak)
Antes de ejecutar cualquier restauración, el sistema debe invocar `RESTORE HEADERONLY` para verificar:
1. Que el archivo exista y sea legible por el motor de base de datos.
2. Que corresponda a una copia de seguridad completa (`BackupType = 1`).
3. Que el nombre de base de datos contenido en la cabecera sea `resto_DB`, impidiendo la sobreescritura con copias de otros sistemas.

#### RF-09.3: Protocolo de Doble Autorización de Restauración
Dado el carácter destructivo de restaurar una base de datos sobre los datos operativos en curso, el sistema debe exigir obligatoriamente la confirmación de **dos (2) usuarios directivos independientes** (Dueño y/o Gerente) mediante una ventana modal de seguridad (`AutorizacionRestoreWindow`), validando ambas contraseñas antes de liberar la orden técnica de restauración.

#### RF-09.4: Aislamiento de Conexiones Activas y Restauración Transaccional
La restauración debe poner la base de datos en modo exclusivo forzado (`SET SINGLE_USER WITH ROLLBACK IMMEDIATE`), aplicar el comando `RESTORE DATABASE ... WITH REPLACE` desde la base auxiliar `master`, y restablecer inmediatamente el modo concurrente `SET MULTI_USER`.

---

## 4. Requisitos No Funcionales (RNF)

### RNF-01: Seguridad, Integridad y Control de Acceso
- **RNF-01.1 Protección de Credenciales:** Las contraseñas se almacenan en campos dedicados con validación segura de accesos.
- **RNF-01.2 Interfaz Adaptable al Privilegio:** La barra lateral de navegación y las acciones de edición ocultan o desactivan los comandos según la matriz RBAC.
- **RNF-01.3 Restricción Directiva de Operaciones Críticas:** Solo el rol Dueño tiene acceso a la creación de copias de seguridad, y la restauración exige doble firma digital gerencial.

### RNF-02: Rendimiento y Tiempos de Respuesta
- **RNF-02.1 Respuesta de Consultas Habituales:** Las pantallas de monitoreo de salón, reservas y catálogo de menú deben cargar en **menos de 800 milisegundos**.
- **RNF-02.2 Ejecución de Reportes Analíticos:** El cálculo de los KPIs y tablas de BI para períodos de hasta 1 año no debe superar los **2 segundos**.
- **RNF-02.3 Operaciones de Backup:** La generación de la copia de seguridad no debe exceder los **15 segundos** en bases de hasta 500 MB.

### RNF-03: Usabilidad, Ergonomía y Accesibilidad
- **RNF-03.1 Diseño Táctil y Teclado:** Botones y cuadrículas con dimensiones mínimas de 44x44 px aptas para pantallas táctiles de terminales POS.
- **RNF-03.2 Mensajería Clara y Alertas Visuales:** Todo error de conexión, fallo de validación o confirmación de cobro se notifica con tarjetas visuales de alto contraste.
- **RNF-03.3 Barra Lateral con Scroll Adaptativo:** Menú de navegación lateral con `ScrollViewer` integrado para garantizar visibilidad completa en cualquier resolución sin solapamiento de opciones.

### RNF-04: Concurrencia e Integridad de Persistencia
- **RNF-04.1 Triggers de Consistencia:** Imposibilidad de inconsistencias aritméticas entre los detalles de consumo y el encabezado de la cuenta gracias a triggers en base de datos.
- **RNF-04.2 Prevención de Colisiones de Mesas:** Transacciones con nivel de aislamiento adecuado para impedir asignar una misma mesa a dos comandas activas en simultáneo.

### RNF-05: Mantenibilidad y Arquitectura de Código
- **RNF-05.1 Desacoplamiento en Capas:** Estricta separación de responsabilidades:
  - `RestoApp.Desktop`: Capa XAML y ViewModels.
  - `RestoApp.Business`: Capa de reglas de negocio y servicios.
  - `RestoApp.Data`: Capa de persistencia, EF Core y Stored Procedures.
  - `RestoApp.Entities`: Entidades de dominio y DTOs transversales.
- **RNF-05.2 Cumplimiento de Estándares C#:** Uso de tipos no anulables (*nullable reference types*), comandos RelayCommand tipados y nombres convencionales de Microsoft .NET.

---

## 5. Reglas de Negocio (RN)

- **RN-01 (Cancelación de Reserva):** Una reserva solo puede cancelarse sin penalidad comercial si se solicita antes de la `fecha_max_cancelacion` (48 horas previas al turno agendado).
- **RN-02 (Unicidad de Contacto de Persona):** No se permite duplicar DNI, correo electrónico ni teléfono entre personas registradas (`uq_persona_email`, `UQ_Persona_Tel`).
- **RN-03 (Jerarquía de Creación de Usuarios):**
  - El **Dueño** puede dar de alta o modificar usuarios con cualquier rol (`Dueño`, `Gerente`, `Cajero`, `Recepción`, `Mozo`).
  - El **Gerente** puede gestionar usuarios de atención de salón y cocina, pero no puede crear otros Dueños o Gerentes.
- **RN-04 (Compatibilidad de Aforo):** La capacidad acumulada de las mesas asignadas a una reserva debe ser igual o superior a la cantidad de personas registradas.
- **RN-05 (Cálculo Automático de Comandas):** Ningún usuario puede alterar manualmente el importe total de una cuenta; dicho valor es computado exclusivamente por la base de datos en función de las cantidades y precios unitarios.
- **RN-06 (Un Solo Rol Activo por Empleado):** Un colaborador no puede tener asignados dos roles activos simultáneamente (`UQ_Empleado_RolActivo`).
- **RN-07 (Doble Autorización para Restauración):** La base de datos solo puede ser restaurada si dos administradores distintos ingresan sus contraseñas válidas en la misma solicitud.
- **RN-08 (Validación de Cabecera de Backup):** No se ejecutará ninguna restauración si el archivo `.bak` no pertenece a `resto_DB` o si no corresponde a una copia completa (`BackupType = 1`).
- **RN-09 (Liquidación para Cierre de Mesa):** Una mesa no puede pasar al estado `LIBRE` o `EN LIMPIEZA` hasta tanto la cuenta comanda asociada tenga estado `CERRADA` y su pago registrado en la tabla `pagos`.
- **RN-10 (Bajas Lógicas Preservativas):** Las bajas de mesas, ítems de menú y personal deben ser lógicas (`es_activo = 0`), manteniendo la integridad de las relaciones históricas y estadísticas de reportes.

---

## 6. Delimitación y Alcance Futuro

### 6.1 Exclusiones del Alcance Actual
- **Facturación Electrónica Fiscal Externa:** No incluye la conexión directa por webservice con servidores de la agencia tributaria (AFIP/ARCA) para la obtención electrónica de CAE.
- **Liquidación Previsional de Haberes:** No abarca el cálculo de aportes patronales, liquidación de sueldos o emisión de recibos de haberes de empleados.
- **Aplicación Móvil para Clientes:** El sistema opera en puestos de trabajo de salón y caja del personal del establecimiento.

### 6.2 Líneas de Trabajo Futuras
1. **Comandera Móvil para Mozos:** Interfaz ligera táctil optimizada para tabletas o dispositivos móviles de salón que permita cargar órdenes directo al pie de la mesa.
2. **KDS de Cocina (Kitchen Display System):** Pantalla interactiva en el sector cocina/elaboración para despachar comandas en orden cronológico según tiempo de espera.
3. **Módulo de Gestión de Inventario e Insumos:** Registro de recetas por plato con descuento automático de stock de ingredientes (kilos de carne, verduras, bebidas) ante cada venta.
4. **Módulo de Arqueo y Control de Sesiones de Caja:** Incorporación de apertura obligatoria de caja con fondo fijo inicial, declaración de efectivo físico recontado por turno, conciliación automática de diferencias (sobrantes/faltantes) y auditoría histórica de cierres de caja por cajero.
