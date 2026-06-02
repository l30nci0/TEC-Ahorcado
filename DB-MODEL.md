# Modelo de Base de Datos: LetterClash (TEC-Ahorcado)

Este documento define la estructura de persistencia para el sistema **LetterClash**, incluyendo la definición conceptual de los tipos y el script SQL (T-SQL) correspondiente para SQL Server.

---

## 1. Modelo Conceptual (Tipos)

A continuación se definen los tipos conceptuales del modelo de datos:

### Jugador
Representa a los usuarios registrados en el sistema, almacenando sus credenciales, puntuación acumulada e idioma preferido.

```typescript
type Jugador {
  IDJugador: int [auto_increment, identity]
  Nombre: str[:64]
  readonly NombreDeUsuario: str[3:16] [unique]
  readonly Correo: str [unique]
  Teléfono: str
  Contraseña: str
  Puntuación: int [default: 0]
  Avatar?: blob
  IdiomaPreferido: enum('ESPAÑOL', 'INGLÉS') [default: 'ESPAÑOL']
  FechaDeNacimiento: datetime
}
```

### Palabra
Contiene el catálogo de palabras disponibles para el juego, asociadas a su respectiva pista (descripción) e idioma.

```typescript
type Palabra {
  IDPalabra: int [auto_increment, identity]
  Palabra: str[3:16]
  Descripción: str[:128]
  Idioma: enum('ESPAÑOL', 'INGLÉS')

  [unique (Palabra, Idioma)]
}
```

### Partida
Registra las salas de juego creadas en el sistema, vinculando al Anfitrión, al Adivinador y la palabra seleccionada.

```typescript
type Partida {
  IDPartida: int [auto_increment, identity]
  IDAnfitrión: ref Jugador::IDJugador
  IDAdivinador?: ref Jugador::IDJugador
  IDPalabra: ref Palabra::IDPalabra
  Estado: enum('PENDIENTE', 'EN_JUEGO', 'CONCLUIDA') [default: 'PENDIENTE']
  Resultado: enum('ADIVINADA', 'SIN_ADIVINAR', 'ABANDONADA') [default: 'SIN_ADIVINAR']
  Privacidad: enum('PRIVADA', 'PÚBLICA') [default: 'PRIVADA']
  Turno: int[1:6] [default: 1]
  CódigoAcceso: str[6] [unique]
  readonly FechaDeJuego: datetime [default: now()]
}
```

---

## 2. Script de Creación de Base de Datos (T-SQL)

El siguiente script en SQL Server (T-SQL) crea las tablas necesarias respetando la integridad referencial y aplicando restricciones de validación correspondientes al modelo conceptual.

```sql
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
    CONSTRAINT UQ_Palabra_Palabra UNIQUE (Palabra),

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
```