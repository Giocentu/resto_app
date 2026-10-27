USE resto_DB;
GO
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

-- =================================================================================
-- PROCEDIMIENTOS ALMACENADOS (SPs)
-- =================================================================================

-- =================================================================================
-- 1. CRUD MESAS
-- =================================================================================

-- 1.1 Crear Mesa
CREATE OR ALTER PROCEDURE sp_Mesa_Crear
    @NroMesa INT,
    @Capacidad INT,
    @IdUbicacion INT,
    @Estado VARCHAR(20) = 'LIBRE',
    @NuevoId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        IF EXISTS (SELECT 1 FROM mesa WHERE nro_mesa = @NroMesa AND es_activo = 1)
        BEGIN
            RAISERROR('El número de mesa ya se encuentra registrado y activo.', 16, 1);
            RETURN;
        END

        INSERT INTO mesa (nro_mesa, capacidad, id_ubicacion, estado, es_activo)
        VALUES (@NroMesa, @Capacidad, @IdUbicacion, @Estado, 1);

        SET @NuevoId = SCOPE_IDENTITY();
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END;
GO

-- 1.2 Obtener Todas las Mesas (Activas o con filtro)
CREATE OR ALTER PROCEDURE sp_Mesa_ObtenerTodas
    @SoloActivas BIT = 1
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        m.id_mesa AS IdMesa,
        m.nro_mesa AS NroMesa,
        m.capacidad AS Capacidad,
        m.id_ubicacion AS IdUbicacion,
        u.ubicacion AS UbicacionDescripcion,
        m.estado AS Estado,
        m.es_activo AS EsActivo
    FROM mesa m
    INNER JOIN ubicacion_mesa u ON m.id_ubicacion = u.id_ubicacion
    WHERE (@SoloActivas = 0 OR m.es_activo = 1)
    ORDER BY m.nro_mesa ASC;
END;
GO

-- 1.3 Editar Mesa
CREATE OR ALTER PROCEDURE sp_Mesa_Editar
    @IdMesa INT,
    @NroMesa INT,
    @Capacidad INT,
    @IdUbicacion INT,
    @Estado VARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        UPDATE mesa
        SET nro_mesa = @NroMesa,
            capacidad = @Capacidad,
            id_ubicacion = @IdUbicacion,
            estado = @Estado
        WHERE id_mesa = @IdMesa;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END;
GO

-- 1.4 Baja Lógica de Mesa
CREATE OR ALTER PROCEDURE sp_Mesa_BajaLogica
    @IdMesa INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        UPDATE mesa
        SET es_activo = 0
        WHERE id_mesa = @IdMesa;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END;
GO

-- 1.5 Restaurar Mesa
CREATE OR ALTER PROCEDURE sp_Mesa_Restaurar
    @IdMesa INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        UPDATE mesa
        SET es_activo = 1
        WHERE id_mesa = @IdMesa;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END;
GO


-- =================================================================================
-- 2. CRUD RESERVAS
-- =================================================================================

-- 2.1 Crear Reserva (con transacción e inserción en tabla intermedia)
CREATE OR ALTER PROCEDURE sp_Reserva_Crear
    @FechaReserva DATETIME,
    @CantPersonas INT,
    @IdEstado INT,
    @DniCliente BIGINT,
    @IdEvento INT = NULL,
    @DniEmpleado BIGINT = NULL,
    @IdRol INT = NULL,
    @IdMesa INT = NULL, -- Mesa principal asignada opcional
    @NuevaReservaId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    BEGIN TRY
        -- Insertar la Reserva principal
        INSERT INTO reserva (fecha_reserva, cant_personas, id_estado, id_evento, dni_cliente, dni_empleado, id_rol)
        VALUES (@FechaReserva, @CantPersonas, @IdEstado, @IdEvento, @DniCliente, @DniEmpleado, @IdRol);

        SET @NuevaReservaId = SCOPE_IDENTITY();

        -- Si se especificó una mesa, vincularla en reserva_mesa
        IF @IdMesa IS NOT NULL
        BEGIN
            INSERT INTO reserva_mesa (id_reserva, id_mesa)
            VALUES (@NuevaReservaId, @IdMesa);
        END

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

-- 2.2 Obtener Reservas
CREATE OR ALTER PROCEDURE sp_Reserva_ObtenerTodas
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        r.id_reserva AS IdReserva,
        r.fecha_reserva AS FechaReserva,
        r.cant_personas AS CantPersonas,
        r.fecha_max_cancelacion AS FechaMaxCancelacion,
        r.id_estado AS IdEstado,
        er.estado AS EstadoDescripcion,
        r.id_evento AS IdEvento,
        e.nombre_evento AS NombreEvento,
        r.dni_cliente AS DniCliente,
        p.nombre + ' ' + p.apellido AS ClienteNombre,
        p.telefono AS ClienteTelefono
    FROM reserva r
    INNER JOIN estado_reserva er ON r.id_estado = er.id_estado
    LEFT JOIN evento e ON r.id_evento = e.id_evento
    INNER JOIN cliente c ON r.dni_cliente = c.dni_cliente
    INNER JOIN persona p ON c.dni_cliente = p.dni
    ORDER BY r.fecha_reserva DESC;
END;
GO

-- 2.3 Cancelar / Modificar Estado de Reserva
CREATE OR ALTER PROCEDURE sp_Reserva_CambiarEstado
    @IdReserva INT,
    @NuevoEstadoId INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE reserva
    SET id_estado = @NuevoEstadoId
    WHERE id_reserva = @IdReserva;
END;
GO


-- =================================================================================
-- 3. CRUD EMPLEADOS Y PERSONAL
-- =================================================================================

-- 3.1 Crear Empleado (Atómico: Persona + Empleado)
CREATE OR ALTER PROCEDURE sp_Empleado_Crear
    @Dni BIGINT,
    @Nombre VARCHAR(50),
    @Apellido VARCHAR(50),
    @Email VARCHAR(50),
    @Telefono BIGINT,
    @Password NVARCHAR(50),
    @IdRol INT,
    @IdTurno INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    BEGIN TRY
        -- Insertar Persona si no existe
        IF NOT EXISTS (SELECT 1 FROM persona WHERE dni = @Dni)
        BEGIN
            INSERT INTO persona (dni, nombre, apellido, email, telefono, password)
            VALUES (@Dni, @Nombre, @Apellido, @Email, @Telefono, @Password);
        END

        -- Insertar Empleado
        INSERT INTO empleado (dni_empleado, id_rol, id_turno, activo_en_rol)
        VALUES (@Dni, @IdRol, @IdTurno, 1);

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

-- 3.2 Obtener Empleados
CREATE OR ALTER PROCEDURE sp_Empleado_ObtenerTodos
    @SoloActivos BIT = 1
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        e.dni_empleado AS DniEmpleado,
        p.nombre AS Nombre,
        p.apellido AS Apellido,
        p.email AS Email,
        p.telefono AS Telefono,
        e.id_rol AS IdRol,
        r.descripcion AS RolDescripcion,
        e.id_turno AS IdTurno,
        t.inicio_turno AS InicioTurno,
        t.fin_turno AS FinTurno,
        e.activo_en_rol AS ActivoEnRol
    FROM empleado e
    INNER JOIN persona p ON e.dni_empleado = p.dni
    INNER JOIN rol_empleado r ON e.id_rol = r.id_rol
    INNER JOIN turno_empleado t ON e.id_turno = t.id_turno
    WHERE (@SoloActivos = 0 OR e.activo_en_rol = 1)
    ORDER BY p.apellido, p.nombre;
END;
GO

-- 3.3 Baja Lógica Empleado
CREATE OR ALTER PROCEDURE sp_Empleado_BajaLogica
    @DniEmpleado BIGINT,
    @IdRol INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE empleado
    SET activo_en_rol = 0
    WHERE dni_empleado = @DniEmpleado AND id_rol = @IdRol;
END;
GO


-- =================================================================================
-- 4. CRUD EVENTOS
-- =================================================================================

-- 4.1 Crear Evento
CREATE OR ALTER PROCEDURE sp_Evento_Crear
    @NombreEvento VARCHAR(50),
    @FechaEvento DATETIME = NULL,
    @Descripcion VARCHAR(255) = NULL,
    @NuevoId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO evento (nombre_evento, fecha_evento, descripcion, es_activo)
    VALUES (@NombreEvento, @FechaEvento, @Descripcion, 1);

    SET @NuevoId = SCOPE_IDENTITY();
END;
GO

-- 4.2 Obtener Eventos
CREATE OR ALTER PROCEDURE sp_Evento_ObtenerTodos
    @SoloActivos BIT = 1
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        e.id_evento AS IdEvento,
        e.nombre_evento AS NombreEvento,
        e.fecha_evento AS FechaEvento,
        e.descripcion AS Descripcion,
        e.es_activo AS EsActivo,
        (SELECT COUNT(*) FROM reserva r WHERE r.id_evento = e.id_evento) AS CantReservasVinculadas
    FROM evento e
    WHERE (@SoloActivos = 0 OR e.es_activo = 1)
    ORDER BY e.fecha_evento DESC;
END;
GO

-- 4.3 Baja Lógica Evento
CREATE OR ALTER PROCEDURE sp_Evento_BajaLogica
    @IdEvento INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE evento
    SET es_activo = 0
    WHERE id_evento = @IdEvento;
END;
GO


-- =================================================================================
-- 5. CRUD PAGOS Y CAJA
-- =================================================================================

-- 5.1 Registrar Pago (Con actualización de estado de mesa opcional y soporte para cuenta_head)
CREATE OR ALTER PROCEDURE sp_Pago_Crear
    @Monto FLOAT,
    @FechaPago DATE,
    @IdMetodo INT,
    @IdReserva INT = NULL,
    @IdMesa INT = NULL,
    @CuentaId INT = NULL,
    @NuevoId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    BEGIN TRY
        -- Si se especificó una cuenta pero el monto no se brindó o es cero, se toma el total de la cuenta
        IF @CuentaId IS NOT NULL AND (@Monto IS NULL OR @Monto <= 0)
        BEGIN
            SELECT @Monto = total, @IdMesa = ISNULL(@IdMesa, id_mesa)
            FROM cuenta_head
            WHERE cuenta_id = @CuentaId;
        END

        INSERT INTO pagos (monto, fecha_pago, id_metodo, id_reserva, id_cuenta)
        VALUES (@Monto, @FechaPago, @IdMetodo, @IdReserva, @CuentaId);

        SET @NuevoId = SCOPE_IDENTITY();

        -- Si se asoció a una cuenta, marcarla como CERRADA
        IF @CuentaId IS NOT NULL
        BEGIN
            UPDATE cuenta_head
            SET estado = 'CERRADA',
                fecha_cierre = GETDATE()
            WHERE cuenta_id = @CuentaId;
        END

        -- Liberar mesa a EN LIMPIEZA si fue especificada
        IF @IdMesa IS NOT NULL
        BEGIN
            UPDATE mesa 
            SET estado = 'EN LIMPIEZA' 
            WHERE id_mesa = @IdMesa;
        END

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

-- 5.2 Obtener Arqueo de Caja del Turno / Día
CREATE OR ALTER PROCEDURE sp_Pago_ObtenerArqueoTurno
    @Fecha DATE
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        mp.forma_pago AS MedioPago,
        ISNULL(SUM(p.monto), 0) AS TotalCobrado,
        COUNT(p.id_pago) AS CantidadTransacciones
    FROM metodo_pago mp
    LEFT JOIN pagos p ON mp.id_metodo = p.id_metodo AND p.fecha_pago = @Fecha
    GROUP BY mp.id_metodo, mp.forma_pago
    ORDER BY mp.id_metodo;
END;
GO


-- =================================================================================
-- 6. GESTIÓN DE CARTA (CATEGORÍAS Y MENÚ)
-- =================================================================================

-- 6.1 Crear Categoría de Menú
CREATE OR ALTER PROCEDURE sp_CategoriaMenu_Crear
    @Nombre VARCHAR(30),
    @NuevoId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        IF @Nombre IS NULL OR LTRIM(RTRIM(@Nombre)) = ''
        BEGIN
            RAISERROR('El nombre de la categoría no puede estar vacío.', 16, 1);
            RETURN;
        END

        INSERT INTO categoria_menu (nombre)
        VALUES (LTRIM(RTRIM(@Nombre)));

        SET @NuevoId = SCOPE_IDENTITY();
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END;
GO

-- 6.2 Obtener Todas las Categorías de Menú
CREATE OR ALTER PROCEDURE sp_CategoriaMenu_ObtenerTodas
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        id_categoria AS IdCategoria,
        nombre AS Nombre
    FROM categoria_menu
    ORDER BY nombre ASC;
END;
GO

-- 6.3 Crear Item de Menú (Con Categoría)
CREATE OR ALTER PROCEDURE sp_Menu_Crear
    @Descripcion VARCHAR(30),
    @Precio FLOAT,
    @IdCategoria INT,
    @Activo BIT = 1,
    @NuevoId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        IF @Precio <= 0
        BEGIN
            RAISERROR('El precio del ítem debe ser mayor a cero.', 16, 1);
            RETURN;
        END

        IF @Descripcion IS NULL OR LTRIM(RTRIM(@Descripcion)) = ''
        BEGIN
            RAISERROR('La descripción del ítem no puede estar vacía.', 16, 1);
            RETURN;
        END

        IF NOT EXISTS (SELECT 1 FROM categoria_menu WHERE id_categoria = @IdCategoria)
        BEGIN
            RAISERROR('La categoría especificada no existe.', 16, 1);
            RETURN;
        END

        INSERT INTO menu (descripcion, precio, id_categoria, activo)
        VALUES (LTRIM(RTRIM(@Descripcion)), @Precio, @IdCategoria, @Activo);

        SET @NuevoId = SCOPE_IDENTITY();
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END;
GO

-- 6.4 Obtener Items de Menú (Con Filtro por Categoría y Estado)
CREATE OR ALTER PROCEDURE sp_Menu_ObtenerTodos
    @SoloActivos BIT = 1,
    @IdCategoria INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        m.id_item AS IdItem,
        m.descripcion AS Descripcion,
        m.precio AS Precio,
        m.id_categoria AS IdCategoria,
        cm.nombre AS CategoriaNombre,
        m.activo AS Activo
    FROM menu m
    INNER JOIN categoria_menu cm ON m.id_categoria = cm.id_categoria
    WHERE (@SoloActivos = 0 OR m.activo = 1)
      AND (@IdCategoria IS NULL OR m.id_categoria = @IdCategoria)
    ORDER BY cm.nombre ASC, m.descripcion ASC;
END;
GO

-- 6.5 Obtener Item de Menú por ID
CREATE OR ALTER PROCEDURE sp_Menu_ObtenerPorId
    @IdItem INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        m.id_item AS IdItem,
        m.descripcion AS Descripcion,
        m.precio AS Precio,
        m.id_categoria AS IdCategoria,
        cm.nombre AS CategoriaNombre,
        m.activo AS Activo
    FROM menu m
    INNER JOIN categoria_menu cm ON m.id_categoria = cm.id_categoria
    WHERE m.id_item = @IdItem;
END;
GO

-- 6.6 Editar Item de Menú
CREATE OR ALTER PROCEDURE sp_Menu_Editar
    @IdItem INT,
    @Descripcion VARCHAR(30),
    @Precio FLOAT,
    @IdCategoria INT,
    @Activo BIT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        IF @Precio <= 0
        BEGIN
            RAISERROR('El precio del ítem debe ser mayor a cero.', 16, 1);
            RETURN;
        END

        IF NOT EXISTS (SELECT 1 FROM categoria_menu WHERE id_categoria = @IdCategoria)
        BEGIN
            RAISERROR('La categoría especificada no existe.', 16, 1);
            RETURN;
        END

        UPDATE menu
        SET descripcion = LTRIM(RTRIM(@Descripcion)),
            precio = @Precio,
            id_categoria = @IdCategoria,
            activo = @Activo
        WHERE id_item = @IdItem;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END;
GO

-- 6.7 Baja Lógica de Item de Menú
CREATE OR ALTER PROCEDURE sp_Menu_BajaLogica
    @IdItem INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        UPDATE menu
        SET activo = 0
        WHERE id_item = @IdItem;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END;
GO

-- 6.8 Restaurar Item de Menú
CREATE OR ALTER PROCEDURE sp_Menu_Restaurar
    @IdItem INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        UPDATE menu
        SET activo = 1
        WHERE id_item = @IdItem;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END;
GO


-- =================================================================================
-- 7. GESTIÓN DE CUENTAS Y COMANDAS (cuenta_head / cuenta_body)
-- =================================================================================

-- 7.1 Abrir / Crear Cuenta para una Mesa (Con Mozo / Empleado asignado)
CREATE OR ALTER PROCEDURE sp_Cuenta_Crear
    @IdMesa INT,
    @DniEmpleado BIGINT = NULL,
    @IdRol INT = NULL,
    @NuevaCuentaId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM mesa WHERE id_mesa = @IdMesa AND es_activo = 1)
        BEGIN
            RAISERROR('La mesa especificada no existe o no se encuentra activa.', 16, 1);
            RETURN;
        END

        -- Verificar si la mesa ya tiene una cuenta ABIERTA
        DECLARE @CuentaExistenteId INT;
        SELECT TOP 1 @CuentaExistenteId = cuenta_id 
        FROM cuenta_head 
        WHERE id_mesa = @IdMesa AND estado = 'ABIERTA';

        IF @CuentaExistenteId IS NOT NULL
        BEGIN
            -- Si ya tiene cuenta abierta, retornamos el ID de la misma
            SET @NuevaCuentaId = @CuentaExistenteId;
            COMMIT TRANSACTION;
            RETURN;
        END

        -- Validar empleado si se envió
        IF @DniEmpleado IS NOT NULL AND @IdRol IS NOT NULL
        BEGIN
            IF NOT EXISTS (SELECT 1 FROM empleado WHERE dni_empleado = @DniEmpleado AND id_rol = @IdRol AND activo_en_rol = 1)
            BEGIN
                RAISERROR('El empleado asignado no existe o no está activo en ese rol.', 16, 1);
                RETURN;
            END
        END

        -- Crear cabecera de la cuenta
        INSERT INTO cuenta_head (id_mesa, total, estado, fecha_apertura, dni_empleado, id_rol)
        VALUES (@IdMesa, 0, 'ABIERTA', GETDATE(), @DniEmpleado, @IdRol);

        SET @NuevaCuentaId = SCOPE_IDENTITY();

        -- Marcar la mesa como ocupada al abrir cuenta
        UPDATE mesa
        SET estado = 'OCUPADA'
        WHERE id_mesa = @IdMesa;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

-- 7.2 Agregar Item al Consumo de la Cuenta (Con Cantidad y Snapshot de Precio)
CREATE OR ALTER PROCEDURE sp_Cuenta_AgregarItem
    @CuentaId INT,
    @IdItem INT,
    @Cantidad INT = 1,
    @PrecioUnitario FLOAT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM cuenta_head WHERE cuenta_id = @CuentaId AND estado = 'ABIERTA')
        BEGIN
            RAISERROR('La cuenta especificada no existe o ya no se encuentra abierta.', 16, 1);
            RETURN;
        END

        IF NOT EXISTS (SELECT 1 FROM menu WHERE id_item = @IdItem AND activo = 1)
        BEGIN
            RAISERROR('El ítem del menú no existe o no se encuentra activo.', 16, 1);
            RETURN;
        END

        IF @Cantidad <= 0
        BEGIN
            SET @Cantidad = 1;
        END

        -- Si no se suministra precio unitario, tomar el precio vigente del menú
        IF @PrecioUnitario IS NULL OR @PrecioUnitario <= 0
        BEGIN
            SELECT @PrecioUnitario = precio FROM menu WHERE id_item = @IdItem;
        END

        -- Si el ítem ya existe en la comanda con el mismo precio unitario, sumar la cantidad
        IF EXISTS (SELECT 1 FROM cuenta_body WHERE cuenta_id = @CuentaId AND id_item = @IdItem AND precio_unitario = @PrecioUnitario)
        BEGIN
            UPDATE cuenta_body
            SET cantidad = cantidad + @Cantidad
            WHERE cuenta_id = @CuentaId AND id_item = @IdItem AND precio_unitario = @PrecioUnitario;
        END
        ELSE
        BEGIN
            INSERT INTO cuenta_body (cuenta_id, id_item, cantidad, precio_unitario)
            VALUES (@CuentaId, @IdItem, @Cantidad, @PrecioUnitario);
        END

        -- El trigger trg_CuentaBody_RecalcularTotal actualiza automáticamente cuenta_head.total

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

-- 7.3 Quitar / Reducir Item del Consumo de la Cuenta
CREATE OR ALTER PROCEDURE sp_Cuenta_QuitarItem
    @CuentaId INT,
    @IdItem INT,
    @Cantidad INT = 1
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM cuenta_head WHERE cuenta_id = @CuentaId AND estado = 'ABIERTA')
        BEGIN
            RAISERROR('La cuenta especificada no existe o ya no se encuentra abierta.', 16, 1);
            RETURN;
        END

        IF @Cantidad <= 0
        BEGIN
            SET @Cantidad = 1;
        END

        DECLARE @CantActual INT, @IdCuentaBody INT;

        SELECT TOP 1 @IdCuentaBody = id_cuenta_body, @CantActual = cantidad
        FROM cuenta_body
        WHERE cuenta_id = @CuentaId AND id_item = @IdItem
        ORDER BY id_cuenta_body DESC;

        IF @IdCuentaBody IS NULL
        BEGIN
            RAISERROR('El ítem no se encuentra registrado en el consumo de la cuenta.', 16, 1);
            RETURN;
        END

        IF @CantActual > @Cantidad
        BEGIN
            UPDATE cuenta_body
            SET cantidad = cantidad - @Cantidad
            WHERE id_cuenta_body = @IdCuentaBody;
        END
        ELSE
        BEGIN
            DELETE FROM cuenta_body
            WHERE id_cuenta_body = @IdCuentaBody;
        END

        -- El trigger trg_CuentaBody_RecalcularTotal actualiza automáticamente cuenta_head.total

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

-- 7.4 Obtener Detalle de Consumos de una Cuenta
CREATE OR ALTER PROCEDURE sp_Cuenta_ObtenerDetalle
    @CuentaId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        cb.id_cuenta_body AS IdCuentaBody,
        cb.id_item AS IdItem,
        m.descripcion AS Descripcion,
        cm.nombre AS Categoria,
        cb.precio_unitario AS PrecioUnitario,
        cb.cantidad AS Cantidad,
        (cb.cantidad * cb.precio_unitario) AS Subtotal
    FROM cuenta_body cb
    INNER JOIN menu m ON cb.id_item = m.id_item
    INNER JOIN categoria_menu cm ON m.id_categoria = cm.id_categoria
    WHERE cb.cuenta_id = @CuentaId
    ORDER BY cm.nombre ASC, m.descripcion ASC;
END;
GO

-- 7.5 Obtener Cuenta Actual de una Mesa
CREATE OR ALTER PROCEDURE sp_Cuenta_ObtenerPorMesa
    @IdMesa INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP 1
        ch.cuenta_id AS CuentaId,
        ch.id_mesa AS IdMesa,
        m.nro_mesa AS NroMesa,
        u.ubicacion AS Ubicacion,
        m.estado AS EstadoMesa,
        ch.estado AS EstadoCuenta,
        ch.fecha_apertura AS FechaApertura,
        ch.fecha_cierre AS FechaCierre,
        ch.dni_empleado AS DniMozo,
        p.nombre + ' ' + p.apellido AS NombreMozo,
        ch.total AS Total,
        ISNULL((SELECT SUM(cb.cantidad) FROM cuenta_body cb WHERE cb.cuenta_id = ch.cuenta_id), 0) AS TotalItems
    FROM cuenta_head ch
    INNER JOIN mesa m ON ch.id_mesa = m.id_mesa
    INNER JOIN ubicacion_mesa u ON m.id_ubicacion = u.id_ubicacion
    LEFT JOIN empleado e ON ch.dni_empleado = e.dni_empleado AND ch.id_rol = e.id_rol
    LEFT JOIN persona p ON e.dni_empleado = p.dni
    WHERE ch.id_mesa = @IdMesa
    ORDER BY CASE WHEN ch.estado = 'ABIERTA' THEN 0 ELSE 1 END, ch.cuenta_id DESC;
END;
GO

-- 7.6 Obtener Todas las Cuentas Pendientes (para Módulo de Caja)
CREATE OR ALTER PROCEDURE sp_Cuenta_ObtenerTodasPendientes
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        ch.cuenta_id AS CuentaId,
        m.id_mesa AS IdMesa,
        m.nro_mesa AS NroMesa,
        u.ubicacion AS Sector,
        ch.total AS MontoConsumo,
        ISNULL((SELECT SUM(cb.cantidad) FROM cuenta_body cb WHERE cb.cuenta_id = ch.cuenta_id), 0) AS CantidadItems,
        ch.fecha_apertura AS FechaApertura,
        pMozo.nombre + ' ' + pMozo.apellido AS AtendidoPor,
        ISNULL(
            (SELECT TOP 1 pCli.nombre + ' ' + pCli.apellido 
             FROM reserva_mesa rm
             INNER JOIN reserva r ON rm.id_reserva = r.id_reserva
             INNER JOIN cliente c ON r.dni_cliente = c.dni_cliente
             INNER JOIN persona pCli ON c.dni_cliente = pCli.dni
             WHERE rm.id_mesa = m.id_mesa AND r.id_estado = 1
             ORDER BY r.fecha_reserva DESC),
            'Cliente Particular'
        ) AS ClienteNombre
    FROM cuenta_head ch
    INNER JOIN mesa m ON ch.id_mesa = m.id_mesa
    INNER JOIN ubicacion_mesa u ON m.id_ubicacion = u.id_ubicacion
    LEFT JOIN empleado e ON ch.dni_empleado = e.dni_empleado AND ch.id_rol = e.id_rol
    LEFT JOIN persona pMozo ON e.dni_empleado = pMozo.dni
    WHERE ch.estado = 'ABIERTA'
    ORDER BY m.nro_mesa ASC;
END;
GO

-- 7.7 Cerrar y Liquidar Cuenta con Pago
CREATE OR ALTER PROCEDURE sp_Cuenta_Cerrar
    @CuentaId INT,
    @IdMetodo INT = 3, -- 3 = Efectivo por defecto
    @IdReserva INT = NULL,
    @NuevoPagoId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    BEGIN TRY
        DECLARE @Total FLOAT, @IdMesa INT, @Estado VARCHAR(20);

        SELECT @Total = total, @IdMesa = id_mesa, @Estado = estado
        FROM cuenta_head
        WHERE cuenta_id = @CuentaId;

        IF @Total IS NULL
        BEGIN
            RAISERROR('La cuenta especificada no existe.', 16, 1);
            RETURN;
        END

        IF @Estado <> 'ABIERTA'
        BEGIN
            RAISERROR('La cuenta especificada no se encuentra abierta.', 16, 1);
            RETURN;
        END

        -- Si no se pasó reserva explícita, buscar si la mesa tiene reserva confirmada activa
        IF @IdReserva IS NULL
        BEGIN
            SELECT TOP 1 @IdReserva = rm.id_reserva
            FROM reserva_mesa rm
            INNER JOIN reserva r ON rm.id_reserva = r.id_reserva
            WHERE rm.id_mesa = @IdMesa AND r.id_estado = 1
            ORDER BY r.fecha_reserva DESC;
        END

        -- Cerrar la cuenta
        UPDATE cuenta_head
        SET estado = 'CERRADA',
            fecha_cierre = GETDATE()
        WHERE cuenta_id = @CuentaId;

        -- Registrar el pago vinculando la cuenta
        INSERT INTO pagos (monto, fecha_pago, id_metodo, id_reserva, id_cuenta)
        VALUES (@Total, CAST(GETDATE() AS DATE), @IdMetodo, @IdReserva, @CuentaId);

        SET @NuevoPagoId = SCOPE_IDENTITY();

        -- Si la reserva existía, pasar su estado a Pagado (id_estado = 3)
        IF @IdReserva IS NOT NULL
        BEGIN
            UPDATE reserva
            SET id_estado = 3
            WHERE id_reserva = @IdReserva;
        END

        -- Liberar la mesa y ponerla EN LIMPIEZA
        UPDATE mesa
        SET estado = 'EN LIMPIEZA'
        WHERE id_mesa = @IdMesa;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

-- 7.8 Cancelar Cuenta Abierta (por cancelación o error de mesa)
CREATE OR ALTER PROCEDURE sp_Cuenta_Cancelar
    @CuentaId INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    BEGIN TRY
        DECLARE @IdMesa INT, @Estado VARCHAR(20);

        SELECT @IdMesa = id_mesa, @Estado = estado
        FROM cuenta_head
        WHERE cuenta_id = @CuentaId;

        IF @IdMesa IS NULL
        BEGIN
            RAISERROR('La cuenta especificada no existe.', 16, 1);
            RETURN;
        END

        IF @Estado <> 'ABIERTA'
        BEGIN
            RAISERROR('Solo se pueden cancelar cuentas en estado ABIERTA.', 16, 1);
            RETURN;
        END

        UPDATE cuenta_head
        SET estado = 'CANCELADA',
            fecha_cierre = GETDATE()
        WHERE cuenta_id = @CuentaId;

        -- Liberar mesa a LIBRE
        UPDATE mesa
        SET estado = 'LIBRE'
        WHERE id_mesa = @IdMesa;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

