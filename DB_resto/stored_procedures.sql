USE resto_DB;
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

-- 5.1 Registrar Pago (Con actualización de estado de mesa opcional)
CREATE OR ALTER PROCEDURE sp_Pago_Crear
    @Monto FLOAT,
    @FechaPago DATE,
    @IdMetodo INT,
    @IdReserva INT = NULL,
    @IdMesa INT = NULL,
    @NuevoId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    BEGIN TRY
        INSERT INTO pagos (monto, fecha_pago, id_metodo, id_reserva)
        VALUES (@Monto, @FechaPago, @IdMetodo, @IdReserva);

        SET @NuevoId = SCOPE_IDENTITY();

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
