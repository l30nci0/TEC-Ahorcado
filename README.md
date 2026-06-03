# 🎮 LetterClash (TEC-Ahorcado)

¡Bienvenido a **LetterClash**! Un emocionante juego del ahorcado multijugador en tiempo real desarrollado para el proyecto final de la materia de Tecnologías para la Construcción de Software.

El sistema se basa en una arquitectura **Cliente-Servidor** utilizando **WPF (Windows Presentation Foundation)** en el cliente y **WCF (Windows Communication Foundation)** en el servidor con comunicación dúplex en tiempo real (NetTcpBinding), respaldado por una persistencia en **SQL Server** mediante repositorios ADO.NET.

---

## 🚀 Características Clave

* **👥 Multijugador en Tiempo Real:** Crea partidas públicas/privadas o únete a las existentes a través del Lobby.
* **💬 Chat Integrado:** Chatea con tu oponente en vivo durante la partida activa.
* **🧠 Sistema de Pistas:** Cada partida cuenta con hasta 2 pistas (revelación de letra aleatoria) para ayudarte a adivinar.
* **🏆 Marcadores y Historial:** Consulta el Top 100 de jugadores globales y tu propio historial detallado de partidas.
* **🌐 Multilingüe (Localización):** Soporte completo e instantáneo para **Español** e **Inglés**.
* **🛡️ Seguridad Robusta:** Contraseñas encriptadas con hashing seguro (Salt) en el servidor y consultas parametrizadas para evitar inyección SQL.

---

## 🛠️ Tecnologías Utilizadas

### Cliente (`LetterClashClient`)
* **Framework:** .NET / WPF (C# / XAML)
* **Arquitectura:** Patrón MVVM (Model-View-ViewModel)
* **Comunicación:** Cliente de WCF (Dúplex con callback handler)

### Servidor (`LetterClashServer`)
* **Framework:** .NET / WCF (Servicios hospedados con IIS Express)
* **Persistencia:** SQL Server (consultas nativas parametrizadas con ADO.NET)
* **Lógica del Juego:** GameEngine en memoria (diseñado para concurrencia)

---

## 📂 Estructura del Repositorio

El proyecto está organizado en las siguientes carpetas principales:

* `LetterClashClient/`: Contiene la aplicación de escritorio en WPF.
  * `Views/`: Interfaces de usuario (`GUI*View.xaml`).
  * `ViewModels/`: Lógica de presentación y enlaces de datos.
  * `Models/`: Contextos de sesión y estado local del juego.
  * `Services/`: Proxies y CallbackHandlers para la comunicación con el servidor.
* `LetterClashServer/`: Contiene el backend de WCF.
  * `Contracts/`: Interfaces de servicio WCF (`IGameService`, etc.).
  * `Services/`: Implementación de la lógica de servicio de red.
  * `Domain/`: Modelos de negocio y lógica del motor de juego (`GameEngine`).
  * `DataAccess/`: Repositorios SQL y gestor de conexiones.

Para más detalles, consulta el documento completo de [Arquitectura y Componentes](./ARCHITECTURE.md).

---

## ⚙️ Guía de Configuración e Instalación

### 1. Requisitos Previos
* **Visual Studio 2022** con las cargas de trabajo de:
  * Desarrollo de escritorio de .NET.
  * Desarrollo web y de ASP.NET (para soporte de WCF).
* **SQL Server** LocalDB o una instancia Express ejecutable.

### 2. Base de Datos
1. Ejecuta los scripts de creación de la base de datos (puedes usar el script [DB-MODEL.sql](./DB-MODEL.sql) para la estructura del esquema y [DB-SEED.sql](./DB-SEED.sql) para poblar las palabras de juego).
2. Copia el archivo `LetterClashServer/connections.config.template`, renómbralo como `connections.config` en la misma carpeta y configura allí las credenciales y nombre de tu servidor SQL Server local.

### 3. Ejecución del Servidor
1. Abre la solución `TEC-Ahorcado.slnx` en Visual Studio.
2. Establece `LetterClashServer` como proyecto de inicio y ejecútalo (correrá en IIS Express u hospedado localmente).

### 4. Ejecución del Cliente
1. Asegúrate de que la dirección del endpoint en `App.config` del cliente coincida con la URL en la que está corriendo el servidor.
2. Ejecuta una o más instancias de `LetterClashClient` para simular partidas multijugador.

---

## ✏️ Estándar de Codificación del Proyecto

Para mantener el código limpio y estructurado, nos regimos por el estándar de codificación oficial del proyecto en español:
* **Estilo de Llaves:** Se usa estrictamente **Same-Line** (estilo K&R/OTBS).
* **Espaciado:** Indentación de **2 espacios** (sin tabuladores).
* **Nomenclatura de Vistas:** Las vistas de WPF llevan el prefijo `GUI` y el sufijo `View` (ej. `GUILoginView.xaml`).
* **Acrónimos:** Siempre en mayúsculas (`DTO`, `ID`, `DB`, `UI`, `WCF`, `WPF`, `SQL`).
* **Eventos:** Los manejadores de eventos del Code-Behind se nombran como `On{Evento}{NombreDescriptivo}` (ej. `OnClicIniciarSesion`).

Para una guía detallada, lee el [Estándar de Codificación](./CODING-STANDARD.md).

---

## 👥 Desarrolladores
* Desarrollado por el equipo de **TEC-Ahorcado**.

---
*¡Que empiece el juego y que no te ahorquen!* 🪓💀
