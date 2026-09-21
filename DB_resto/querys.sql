--USE resto_DB
-- =============================================
-- VERIFICACION DE CANTIDAD DE REGISTROS POR TABLA
-- =============================================
SELECT 'Persona' AS Tabla, COUNT(*) AS Cantidad FROM persona
UNION ALL
SELECT 'Ubicacion Mesa', COUNT(*) FROM ubicacion_mesa
UNION ALL
SELECT 'Mesa', COUNT(*) FROM mesa
UNION ALL
SELECT 'Estado Reserva', COUNT(*) FROM estado_reserva
UNION ALL
SELECT 'Evento', COUNT(*) FROM evento
UNION ALL
SELECT 'Cliente', COUNT(*) FROM cliente
UNION ALL
SELECT 'Rol Empleado', COUNT(*) FROM rol_empleado
UNION ALL
SELECT 'Turno Empleado', COUNT(*) FROM turno_empleado
UNION ALL
SELECT 'Empleado', COUNT(*) FROM empleado
UNION ALL
SELECT 'Reserva', COUNT(*) FROM reserva
UNION ALL
SELECT 'Reserva Mesa', COUNT(*) FROM reserva_mesa
UNION ALL
SELECT 'Metodo Pago', COUNT(*) FROM metodo_pago
UNION ALL
SELECT 'Pagos', COUNT(*) FROM pagos
ORDER BY Cantidad DESC; -- Ordena de mayor a menor cantidad


-- =============================================
-- TABLAS MAESTRAS / CONFIGURACIÓN (Pocos datos)
-- =============================================
SELECT '--- UBICACION MESA ---' AS Tabla;
SELECT * FROM ubicacion_mesa;

SELECT '--- ESTADO RESERVA ---' AS Tabla;
SELECT * FROM estado_reserva;

SELECT '--- EVENTO ---' AS Tabla;
SELECT * FROM evento;

SELECT '--- ROL EMPLEADO ---' AS Tabla;
SELECT * FROM rol_empleado;

SELECT '--- TURNO EMPLEADO ---' AS Tabla;
SELECT * FROM turno_empleado;

SELECT '--- METODO PAGO ---' AS Tabla;
SELECT * FROM metodo_pago;

-- =============================================
-- TABLAS DE ENTIDADES E INFRAESTRUCTURA
-- =============================================
SELECT '--- MESA ---' AS Tabla;
SELECT * FROM mesa;

SELECT '--- CLIENTE ---' AS Tabla;
-- Nota: Muestra solo el DNI porque el nombre está en Persona
SELECT * FROM cliente; 

SELECT '--- EMPLEADO ---' AS Tabla;
-- Nota: Muestra ID_ROL, ID_TURNO y el booleano activo
SELECT * FROM empleado;

SELECT '--- PERSONA ---' AS Tabla;
-- Nota: Muestra ID_ROL, ID_TURNO y el booleano activo
SELECT * FROM persona;
-- =============================================
-- TABLAS TRANSACCIONALES (Pueden tener muchos datos)
-- =============================================

SELECT '--- RESERVA ---' AS Tabla;
SELECT * FROM reserva; 

SELECT '--- RESERVA_MESA ---' AS Tabla;
-- Muestra la relación entre la reserva y la mesa asignada
SELECT * FROM reserva_mesa;

SELECT '--- PAGOS ---' AS Tabla;
SELECT * FROM pagos;