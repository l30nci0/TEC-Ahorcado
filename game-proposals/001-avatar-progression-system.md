# Propuesta de Diseño: Sistema de Progresión y Crossover de Avatares

Este documento detalla la propuesta para reemplazar la funcionalidad de carga de imágenes personalizadas de los usuarios por un sistema gamificado de avatares predefinidos, desbloqueables mediante el progreso en el juego y con colaboraciones (crossovers) con otros proyectos (como Pino de Pino Lang).

---

## 1. Justificación y Objetivos

### Consistencia Visual y Estética
Permitir que los usuarios suban imágenes sin restricciones rompe el estilo artístico monocromático y retro-gamer de terminal OLED establecido para *Letter Clash*. Un sistema de avatares pixelados curados mantiene la coherencia visual.

### Motivación y Retención del Jugador
El desbloqueo de avatares mediante logros introduce un ciclo de progresión (gamification). Los avatares actúan como medallas o "badges" de prestigio que los usuarios pueden lucir en los lobbies.

### Seguridad y Optimización
Evita la necesidad de validar contenido inapropiado en imágenes subidas y reduce drásticamente el peso de las transferencias WCF y del almacenamiento en la base de datos (pasando de binarios BLOB a identificadores tipo cadena).

---

## 2. Mecánicas de Desbloqueo y Crossovers

El juego contará con diversas categorías de avatares:

| ID de Avatar | Nombre | Requisito de Desbloqueo | Estilo / Origen |
| :--- | :--- | :--- | :--- |
| `avatar_classic` | Ahorcado Clásico | Inicial (Disponible por defecto) | Silueta clásica pixel-art |
| `avatar_pino` | Pino (Crossover) | Ganar 10 partidas o alcanzar 500 pts | Personaje de *Pino Lang* |
| `avatar_perfect` | Mente Brillante | Ganar una partida sin fallar ninguna letra | Gráfico con destellos OLED |
| `avatar_veteran` | Veterano del Lazo | Jugar 50 partidas en total | Aspecto con soga deshilachada |
| `avatar_speed` | Rayo de Letras | Adivinar una palabra en menos de 15 segundos | Pixel art con efecto de velocidad |

### Colaboración: Crossover con *Pino Lang* 🌲
Para rendir homenaje al ecosistema de proyectos integrados, el personaje de **Pino** (el personaje Pino de Ergo Proxy, mascota del lenguaje de programación *Pino Lang*) estará disponible como un avatar de prestigio. Esto añade humor y cohesión entre los desarrolladores y la comunidad.

---

## 3. Arquitectura Técnica Propuesta

### Cambios en Base de Datos (DB-MODEL)
En lugar de almacenar la imagen completa en la base de datos como un arreglo de bytes (`VARBINARY(MAX)` o similar), se guardará un identificador de texto.

```diff
-- Modelo Actual en SQL
CREATE TABLE Jugador (
    ID INT PRIMARY KEY,
    NombreDeUsuario VARCHAR(50),
-   Avatar VARBINARY(MAX), -- Imagen cargada por el usuario
+   AvatarKey VARCHAR(50) DEFAULT 'avatar_classic', -- Clave del avatar seleccionado
    Puntuacion INT
);
```

### Gestión de Activos en el Cliente
Los archivos de imagen (`.png` en escala de grises de 128x128 píxeles) se almacenarán localmente en el cliente en `Assets/Images/Avatars/`:
- `avatar_classic.png`
- `avatar_pino.png`
- `avatar_perfect.png`

El cliente cargará la imagen dinámicamente haciendo un mapeo sencillo:
```csharp
string path = $"/Assets/Images/Avatars/{usuario.AvatarKey}.png";
ImageUserAvatar.Source = new BitmapImage(new Uri(path, UriKind.Relative));
```

### Registro de Desbloqueos
Se puede manejar una tabla relacional simple `JugadorAvatar` para guardar qué avatares ha desbloqueado cada jugador:
```sql
CREATE TABLE JugadorAvatar (
    JugadorID INT,
    AvatarKey VARCHAR(50),
    FechaDesbloqueo DATETIME,
    PRIMARY KEY (JugadorID, AvatarKey)
);
```

---

## 4. Diseño de Interfaz de Usuario (UI) en el Perfil

En la sección de edición del perfil (`GUIProfileView.xaml`), el botón clásico de subir imagen (`+`) se sustituirá por un botón de **"Cambiar Avatar"** que abrirá una ventana emergente o modal con una cuadrícula (WrapPanel):

1. **Avatares Desbloqueados**: Se muestran con su color y contraste completo, listos para ser seleccionados.
2. **Avatares Bloqueados**: Se muestran en una silueta más oscura o con un icono de candado (`🔒`) superpuesto. Al pasar el cursor, se muestra una ventana emergente (Tooltip) con el requisito para conseguirlo (ej. *"Bloqueado: Consigue 500 puntos para desbloquear a Pino"*).

---

## 5. Próximos Pasos (Roadmap)

1. **Fase 1 (Actual)**: Completar la unificación de la UI en escala de grises.
2. **Fase 2**: Modificar las entidades y contratos de servicio WCF para migrar de `byte[] Avatar` a `string AvatarKey` en el DTO del jugador.
3. **Fase 3**: Agregar la tabla `JugadorAvatar` y la lógica de asignación en la base de datos de SQL Server.
4. **Fase 4**: Diseñar la galería de selección de avatares en `GUIProfileView.xaml` y añadir los activos de pixel art correspondientes a la carpeta `/Assets`.
