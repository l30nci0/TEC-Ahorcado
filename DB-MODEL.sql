-- =========================================================================
-- SCRIPT DE CREACIÓN DE BASE DE DATOS Y SEGURIDAD: LETTERCLASH
-- =========================================================================

-- 1. CREACIÓN DE LA BASE DE DATOS
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'LetterClashDB')
BEGIN
    CREATE DATABASE LetterClashDB;
END;
GO

USE LetterClashDB;
GO

-- 2. CONFIGURACIÓN DE SEGURIDAD (AUTENTICACIÓN SQL SERVER)
-- Se crea el Login a nivel de servidor (debe ejecutarse desde la base de datos master)
USE master;
GO

IF NOT EXISTS (SELECT * FROM sys.server_principals WHERE name = 'LetterClashUser')
BEGIN
    -- Crea el inicio de sesión para usar autenticación de SQL Server
    CREATE LOGIN LetterClashUser WITH PASSWORD = 'TuContraseñaSegura123!', DEFAULT_DATABASE = LetterClashDB;
END;
GO

-- Se crea el usuario de base de datos en LetterClashDB y se le asocian los permisos correspondientes
USE LetterClashDB;
GO

IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = 'LetterClashUser')
BEGIN
    CREATE USER LetterClashUser FOR LOGIN LetterClashUser;
    
    -- Se le otorgan permisos de propietario únicamente sobre la base de datos del juego
    ALTER ROLE db_owner ADD MEMBER LetterClashUser;
END;
GO

-- 3. CREACIÓN DE LAS TABLAS
-- Se crea 'Palabra' primero porque 'Partida' depende de ella.
CREATE TABLE Palabra (
    IDPalabra INT IDENTITY(1,1) NOT NULL,
    Palabra NVARCHAR(16) NOT NULL,
    Descripcion NVARCHAR(128) NULL,
    Idioma NVARCHAR(10) NOT NULL,

    -- Restricciones de Clave y Unicidad
    CONSTRAINT PK_Palabra PRIMARY KEY (IDPalabra),
    CONSTRAINT UQ_Palabra_Palabra_Idioma UNIQUE (Palabra, Idioma),

    -- Validación del Enum para Idioma
    CONSTRAINT CK_Palabra_Idioma CHECK (Idioma IN ('ESPAÑOL', 'INGLÉS'))
);

-- 2. CREACIÓN DE LA TABLA: Jugador
-- Se crea antes de 'Partida' ya que aporta las llaves foráneas de usuarios.
CREATE TABLE Jugador (
    IDJugador INT IDENTITY(1,1) NOT NULL,
    Nombre NVARCHAR(64) NOT NULL,
    NombreDeUsuario NVARCHAR(16) NOT NULL, -- Regla readonly se controla a nivel aplicación/trigger
    Correo NVARCHAR(256) NOT NULL,
    Telefono NVARCHAR(20) NULL,
    Contrasena NVARCHAR(255) NOT NULL, -- Espacio suficiente para el hash de seguridad
    Puntuacion INT NOT NULL DEFAULT 0,
    Avatar VARBINARY(MAX) NULL,       -- Traducción de tipo blob
    IdiomaPreferido NVARCHAR(10) NOT NULL DEFAULT 'ESPAÑOL',
    FechaDeNacimiento DATETIME NOT NULL,

    -- Restricciones de Clave y Unicidad
    CONSTRAINT PK_Jugador PRIMARY KEY (IDJugador),
    CONSTRAINT UQ_Jugador_NombreDeUsuario UNIQUE (NombreDeUsuario),
    CONSTRAINT UQ_Jugador_Correo UNIQUE (Correo),

    -- Validación de rangos en NombreDeUsuario str[3:16]
    CONSTRAINT CK_Jugador_LongitudUsuario CHECK (LEN(NombreDeUsuario) >= 3),

    -- Validación del Enum para IdiomaPreferido
    CONSTRAINT CK_Jugador_IdiomaPreferido CHECK (IdiomaPreferido IN ('ESPAÑOL', 'INGLÉS'))
);

-- 3. CREACIÓN DE LA TABLA: Partida
-- Entidad central que unifica el flujo multijugador y las referencias relacionales.
CREATE TABLE Partida (
    IDPartida INT IDENTITY(1,1) NOT NULL,
    IDAnfitrion INT NOT NULL,
    IDAdivinador INT NULL, -- Permite NULL porque al crearse la sala 'PENDIENTE' podría no haber adivinador aún
    IDPalabra INT NOT NULL,
    Estado NVARCHAR(15) NOT NULL DEFAULT 'PENDIENTE',
    Resultado NVARCHAR(15) NOT NULL DEFAULT 'SIN_ADIVINAR',
    Privacidad NVARCHAR(10) NOT NULL DEFAULT 'PRIVADA',
    Turno INT NOT NULL DEFAULT 1,
    CodigoAcceso NVARCHAR(6) NOT NULL,
    FechaDeJuego DATETIME NOT NULL DEFAULT GETDATE(), -- Traducción de now()

    -- Restricciones de Clave Primaria y Unicidad
    CONSTRAINT PK_Partida PRIMARY KEY (IDPartida),
    CONSTRAINT UQ_Partida_CodigoAcceso UNIQUE (CodigoAcceso),

    -- Llaves Foráneas (Relaciones ref de PinoDB)
    CONSTRAINT FK_Partida_Anfitrion FOREIGN KEY (IDAnfitrion) REFERENCES Jugador(IDJugador),
    CONSTRAINT FK_Partida_Adivinador FOREIGN KEY (IDAdivinador) REFERENCES Jugador(IDJugador),
    CONSTRAINT FK_Partida_Palabra FOREIGN KEY (IDPalabra) REFERENCES Palabra(IDPalabra),

    -- Validación de Rango para el Turno int[1:6]
    CONSTRAINT CK_Partida_Turno CHECK (Turno BETWEEN 1 AND 6),

    -- Validaciones de Enums de estado y configuraciones de sala
    CONSTRAINT CK_Partida_Estado CHECK (Estado IN ('PENDIENTE', 'EN_JUEGO', 'CONCLUIDA')),
    CONSTRAINT CK_Partida_Resultado CHECK (Resultado IN ('ADIVINADA', 'SIN_ADIVINAR', 'ABANDONADA')),
    CONSTRAINT CK_Partida_Privacidad CHECK (Privacidad IN ('PRIVADA', 'PÚBLICA'))
);