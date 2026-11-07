CREATE DATABASE resto_DB;


GO
USE resto_DB;


GO
SET ANSI_NULLS ON;

SET QUOTED_IDENTIFIER ON;


GO
/* Cancela todas las conexiones activas a la base de datos y la elimina
ALTER DATABASE [resto_DB] 
SET SINGLE_USER 
WITH ROLLBACK IMMEDIATE;
GO
drop database resto_DB*/
-- =================================================================================
-- CREACION DE TABLAS
-- =================================================================================
CREATE TABLE persona (
  dni      BIGINT        NOT NULL,
  nombre   VARCHAR (50)  NOT NULL,
  apellido VARCHAR (50)  NOT NULL,
  email    VARCHAR (50)  NOT NULL,
  telefono BIGINT        NOT NULL,
  password NVARCHAR (50) NOT NULL,
  CONSTRAINT pk_persona PRIMARY KEY (dni)
);


GO
ALTER TABLE persona
  ADD CONSTRAINT chk_persona_dni_pos CHECK (dni > 0),
      CONSTRAINT chk_persona_telefono_pos CHECK (telefono > 0),
      CONSTRAINT chk_persona_email_formato CHECK (email LIKE '%@%.%');

ALTER TABLE persona
  ADD CONSTRAINT uq_persona_email UNIQUE (email);

ALTER TABLE persona
  ADD CONSTRAINT UQ_Persona_Tel UNIQUE (telefono);


GO
CREATE TABLE ubicacion_mesa (
  id_ubicacion INT          IDENTITY (1, 1) NOT NULL,
  ubicacion    VARCHAR (30) NOT NULL,
  CONSTRAINT pk_ubicacion PRIMARY KEY (id_ubicacion)
);

ALTER TABLE ubicacion_mesa
  ADD CONSTRAINT uq_ubicacion_name UNIQUE (ubicacion);


GO
CREATE TABLE mesa (
  id_mesa      INT          IDENTITY (1, 1) NOT NULL,
  nro_mesa     INT          NOT NULL,
  capacidad    INT          NOT NULL,
  id_ubicacion INT          NOT NULL,
  estado       VARCHAR (20) DEFAULT 'LIBRE' NOT NULL,
  es_activo    BIT          DEFAULT 1 NOT NULL,
  CONSTRAINT pk_mesa PRIMARY KEY (id_mesa),
  CONSTRAINT fk_mesa_ubicacion FOREIGN KEY (id_ubicacion) REFERENCES ubicacion_mesa (id_ubicacion)
);

ALTER TABLE mesa
  ADD CONSTRAINT chk_mesa_capacidad_pos CHECK (capacidad > 0);

ALTER TABLE mesa
  ADD CONSTRAINT chk_nro_mesa CHECK (nro_mesa > 0);


GO
CREATE TABLE estado_reserva (
  id_estado INT          NOT NULL,
  estado    VARCHAR (30) NOT NULL,
  CONSTRAINT pk_estado PRIMARY KEY (id_estado)
);

ALTER TABLE estado_reserva
  ADD CONSTRAINT chk_estado_id_pos CHECK (id_estado > 0);

ALTER TABLE estado_reserva
  ADD CONSTRAINT uq_estado_name UNIQUE (estado);


GO
CREATE TABLE evento (
  id_evento     INT           IDENTITY (1, 1) NOT NULL,
  nombre_evento VARCHAR (50)  NOT NULL,
  fecha_evento  DATETIME      NULL,
  descripcion   VARCHAR (255) NULL,
  es_activo     BIT           DEFAULT 1 NOT NULL,
  CONSTRAINT pk_evento PRIMARY KEY (id_evento)
);

ALTER TABLE evento
  ADD CONSTRAINT uq_evento_name UNIQUE (nombre_evento);


GO
CREATE TABLE cliente (
  dni_cliente BIGINT NOT NULL,
  CONSTRAINT pk_cliente PRIMARY KEY (dni_cliente),
  CONSTRAINT fk_cliente_persona FOREIGN KEY (dni_cliente) REFERENCES persona (dni)
);


GO
CREATE TABLE rol_empleado (
  id_rol        INT          NOT NULL,
  descripcion   VARCHAR (30) NOT NULL,
  permiso_admin BIT          DEFAULT 0 NOT NULL,
  CONSTRAINT pk_rolempleado PRIMARY KEY (id_rol)
);

ALTER TABLE rol_empleado
  ADD CONSTRAINT chk_rol_id_pos CHECK (id_rol > 0);

ALTER TABLE rol_empleado
  ADD CONSTRAINT uq_rol UNIQUE (descripcion);


GO
CREATE TABLE turno_empleado (
  id_turno     INT  IDENTITY (1, 1) NOT NULL,
  inicio_turno TIME NOT NULL,
  fin_turno    TIME NOT NULL,
  CONSTRAINT pk_turno PRIMARY KEY (id_turno)
);

ALTER TABLE turno_empleado
  ADD CONSTRAINT chk_turno_valido CHECK (fin_turno > inicio_turno
                                         OR inicio_turno > fin_turno);


GO
CREATE TABLE empleado (
  dni_empleado BIGINT NOT NULL,
  id_rol       INT    NOT NULL,
  id_turno     INT    NOT NULL,
  CONSTRAINT pk_empleado PRIMARY KEY (dni_empleado, id_rol),
  CONSTRAINT fk_persona_empleado FOREIGN KEY (dni_empleado) REFERENCES persona (dni),
  CONSTRAINT fk_rol_empleado FOREIGN KEY (id_rol) REFERENCES rol_empleado (id_rol),
  CONSTRAINT fk_turno_empleado FOREIGN KEY (id_turno) REFERENCES turno_empleado (id_turno)
);


GO
-- Asigna a cada empleado 1 si esta usando ese rol y 0 si ya no lo usa
ALTER TABLE empleado
  ADD activo_en_rol BIT DEFAULT 1 NOT NULL;


GO
-- Restriccion unica condicional
-- Esto asegura que no pueda haber dos filas con el mismo DNI y activo_en_rol = 1
CREATE UNIQUE NONCLUSTERED INDEX UQ_Empleado_RolActivo
  ON empleado(dni_empleado) WHERE (activo_en_rol = 1);


GO
CREATE TABLE reserva (
  id_reserva            INT      IDENTITY (1, 1) NOT NULL,
  fecha_reserva         DATETIME NOT NULL,
  cant_personas         INT      NOT NULL,
  fecha_max_cancelacion AS       DATEADD(hour, -48, fecha_reserva), --Columna calculada en base a la fecha ingresada para la reserva
  id_estado             INT      NOT NULL,
  id_evento             INT      NULL,
  dni_cliente           BIGINT   NOT NULL,
  dni_empleado          BIGINT   NULL,
  id_rol                INT      NULL,
  CONSTRAINT pk_reserva PRIMARY KEY (id_reserva),
  CONSTRAINT fk_reserva_estado FOREIGN KEY (id_estado) REFERENCES estado_reserva (id_estado),
  CONSTRAINT fk_reserva_evento FOREIGN KEY (id_evento) REFERENCES evento (id_evento),
  CONSTRAINT fk_reserva_cliente FOREIGN KEY (dni_cliente) REFERENCES cliente (dni_cliente),
  CONSTRAINT fk_reserva_empleado FOREIGN KEY (dni_empleado, id_rol) REFERENCES empleado (dni_empleado, id_rol)
);

ALTER TABLE reserva
  ADD CONSTRAINT chk_rol_permitido CHECK (id_rol IS NULL
                                          OR id_rol IN (1, 2, 3, 4, 5)); -- Solo roles permitidos pueden gestionar reservas

ALTER TABLE reserva
  ADD CONSTRAINT chk_reserva_cant CHECK (cant_personas > 0),
      CONSTRAINT chk_reserva_fecha_futura CHECK (fecha_reserva >= GETDATE());


GO
CREATE TABLE reserva_mesa (
  id_reserva INT NOT NULL,
  id_mesa    INT NOT NULL,
  CONSTRAINT pk_reserva_mesa PRIMARY KEY (id_reserva, id_mesa),
  CONSTRAINT fk_reserva FOREIGN KEY (id_reserva) REFERENCES reserva (id_reserva) ON DELETE CASCADE,
  CONSTRAINT fk_mesa FOREIGN KEY (id_mesa) REFERENCES mesa (id_mesa)
);


GO
-- =================================================================================
-- CARTA Y MENÚ
-- =================================================================================
CREATE TABLE categoria_menu (
  id_categoria INT          IDENTITY (1, 1) NOT NULL,
  nombre       VARCHAR (30) NOT NULL,
  CONSTRAINT pk_categoria_menu PRIMARY KEY (id_categoria),
  CONSTRAINT uq_categoria_menu_nombre UNIQUE (nombre)
);


GO
CREATE TABLE menu (
  id_item      INT          IDENTITY (1, 1) NOT NULL,
  descripcion  VARCHAR (30) NOT NULL,
  precio       FLOAT        NOT NULL,
  id_categoria INT          NOT NULL,
  activo       BIT          DEFAULT 1 NOT NULL,
  CONSTRAINT pk_menu PRIMARY KEY (id_item),
  CONSTRAINT fk_menu_categoria FOREIGN KEY (id_categoria) REFERENCES categoria_menu (id_categoria),
  CONSTRAINT chk_precio CHECK (precio > 0)
);


GO
-- =================================================================================
-- CUENTAS Y COMANDAS
-- =================================================================================
CREATE TABLE cuenta_head (
  cuenta_id      INT          IDENTITY (1, 1) NOT NULL,
  id_mesa        INT          NOT NULL,
  total          FLOAT        DEFAULT 0 NOT NULL,
  estado         VARCHAR (20) DEFAULT 'ABIERTA' NOT NULL,
  fecha_apertura DATETIME     DEFAULT GETDATE() NOT NULL,
  fecha_cierre   DATETIME     NULL,
  dni_empleado   BIGINT       NULL,
  id_rol         INT          NULL,
  CONSTRAINT pk_cuenta PRIMARY KEY (cuenta_id),
  CONSTRAINT fk_cuenta_mesa FOREIGN KEY (id_mesa) REFERENCES mesa (id_mesa),
  CONSTRAINT fk_cuenta_empleado FOREIGN KEY (dni_empleado, id_rol) REFERENCES empleado (dni_empleado, id_rol),
  CONSTRAINT chk_cuenta_estado CHECK (estado IN ('ABIERTA', 'CERRADA', 'CANCELADA')),
  CONSTRAINT chk_cuenta_total CHECK (total >= 0)
);


GO
CREATE TABLE cuenta_body (
  id_cuenta_body  INT   IDENTITY (1, 1) NOT NULL,
  cuenta_id       INT   NOT NULL,
  id_item         INT   NOT NULL,
  cantidad        INT   DEFAULT 1 NOT NULL,
  precio_unitario FLOAT NOT NULL,
  CONSTRAINT pk_cuenta_body PRIMARY KEY (id_cuenta_body),
  CONSTRAINT fk_cuenta FOREIGN KEY (cuenta_id) REFERENCES cuenta_head (cuenta_id) ON DELETE CASCADE,
  CONSTRAINT fk_item FOREIGN KEY (id_item) REFERENCES menu (id_item),
  CONSTRAINT chk_cuenta_body_cant CHECK (cantidad > 0),
  CONSTRAINT chk_cuenta_body_precio CHECK (precio_unitario > 0)
);


GO
-- =================================================================================
-- PAGOS Y CAJA
-- =================================================================================
CREATE TABLE metodo_pago (
  id_metodo  INT          NOT NULL,
  forma_pago VARCHAR (30) NOT NULL,
  CONSTRAINT pk_metodo_pago PRIMARY KEY (id_metodo)
);

ALTER TABLE metodo_pago
  ADD CONSTRAINT chk_metodo_id_pos CHECK (id_metodo > 0);

ALTER TABLE metodo_pago
  ADD CONSTRAINT uq_metodo_name UNIQUE (forma_pago);


GO
CREATE TABLE pagos (
  id_pago    INT   IDENTITY (1, 1) NOT NULL,
  monto      FLOAT NOT NULL,
  fecha_pago DATE  NOT NULL,
  id_metodo  INT   NOT NULL,
  id_reserva INT   NULL,
  id_cuenta  INT   NULL,
  CONSTRAINT pk_pagos PRIMARY KEY (id_pago),
  CONSTRAINT fk_pago_metodo FOREIGN KEY (id_metodo) REFERENCES metodo_pago (id_metodo),
  CONSTRAINT fk_pago_reserva FOREIGN KEY (id_reserva) REFERENCES reserva (id_reserva) ON DELETE CASCADE,
  CONSTRAINT fk_pago_cuenta FOREIGN KEY (id_cuenta) REFERENCES cuenta_head (cuenta_id)
);

ALTER TABLE pagos
  ADD CONSTRAINT chk_pagos_monto CHECK (monto > 0),
      CONSTRAINT chk_pagos_fecha_actual CHECK (fecha_pago <= GETDATE());


GO
-- =================================================================================
-- TRIGGERS DE INTEGRIDAD
-- =================================================================================
CREATE OR ALTER TRIGGER trg_CuentaBody_RecalcularTotal
  ON cuenta_body
  AFTER INSERT, UPDATE, DELETE
  AS BEGIN
       SET NOCOUNT ON;
       WITH CuentasModificadas
       AS   (SELECT DISTINCT cuenta_id
             FROM   inserted
             UNION
             SELECT DISTINCT cuenta_id
             FROM   deleted)
       UPDATE ch
       SET    ch.total = ISNULL((SELECT SUM(cb.cantidad * cb.precio_unitario)
                                 FROM   cuenta_body AS cb
                                 WHERE  cb.cuenta_id = ch.cuenta_id), 0)
       FROM   cuenta_head AS ch
              INNER JOIN
              CuentasModificadas AS cm
              ON ch.cuenta_id = cm.cuenta_id;
     END