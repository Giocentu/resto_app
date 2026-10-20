USE resto_DB;

CREATE TABLE menu (
	id_item INT IDENTITY(1,1) NOT NULL,
	CONSTRAINT pk_menu PRIMARY KEY (id_item),
	descripcion VARCHAR(30) NOT NULL,
	precio FLOAT NOT NULL,
	activo BIT
)
ALTER TABLE menu 
ADD CONSTRAINT chk_precio CHECK (precio > 0);

CREATE TABLE cuenta_head (
	cuenta_id INT IDENTITY(1,1) NOT NULL,
	total FLOAT NOT NULL,
	id_mesa INT NOT NULL,
	CONSTRAINT pk_cuenta PRIMARY KEY (cuenta_id),
	CONSTRAINT fk_mesa FOREIGN KEY (id_mesa) REFERENCES mesa(id_mesa)
)
CREATE TABLE cuenta_body (
	cuenta_id INT NOT NULL,
	id_item INT NOT NULL,
	CONSTRAINT fk_cuenta FOREIGN KEY (cuenta_id) REFERENCES cuenta_head(cuenta_id),
	CONSTRAINT fk_item FOREIGN KEY (id_item) REFERENCES menu(id_item)
)
