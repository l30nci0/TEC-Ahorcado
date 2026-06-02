# Estándar de Codificación: TEC-Ahorcado (LetterClash)

Este documento define el estándar de codificación y las buenas prácticas para el desarrollo del proyecto **TEC-Ahorcado (LetterClash)**. Todos los desarrolladores deben adherirse a estas directrices para asegurar la consistencia, legibilidad y mantenibilidad del código en las capas cliente y servidor.

---

## 1. Reglas Generales

1. **Idioma:** Todo el código fuente (nombres de variables, clases, métodos, comentarios) y la documentación técnica deben escribirse en **español** o siguiendo las convenciones de dominio del proyecto (como DTOs, entidades y bases de datos).
2. **Estilo de Llaves/Corchetes:** Se utilizará estrictamente el estilo **Same-Line** (la llave de apertura `{` en la misma línea que la declaración o la estructura de control, también conocido como estilo K&R/OTBS).

---

## 2. Convenciones de Nomenclatura

### 2.1 Vistas e Interfaces de Usuario (WPF)
* **Regla:** Todas las vistas deben llevar el prefijo `GUI` y terminar con el sufijo `View`.
* **Ejemplos:**
  - `GUILoginView.xaml` (y su code-behind `GUILoginView.xaml.cs`)
  - `GUIRegisterView.xaml`
  - `GUILobbyView.xaml`
  - `GUIPartidaView.xaml`
  - `GUIActualizarPerfilView.xaml`
  - `GUIPáginaInicioView.xaml`
  - `GUIModalRegistrarPartidaView.xaml`

### 2.2 Patrón MVVM (Model-View-ViewModel)
* **ViewModels:** Deben terminar con el sufijo `ViewModel`.
  - Ejemplo: `LoginViewModel.cs`, `LobbyViewModel.cs`, `PartidaViewModel.cs`.
* **Models:** Representan las estructuras de datos y estado local. Deben estar en PascalCase.
  - Ejemplo: `SessionContext.cs`, `LocalGameState.cs`.

### 2.3 WCF (Servicios y Contratos)
* **Interfaces de Contrato:** Deben comenzar con la letra `I` y terminar con el sufijo `Service`.
  - Ejemplo: `IJugadorService.cs`, `ILobbyService.cs`, `IGameService.cs`.
* **Callbacks:** Deben tener el sufijo `Callback` en su interfaz.
  - Ejemplo: `IGameServiceCallback.cs`.
* **Implementación de Servicios WCF:** Deben terminar con el sufijo `Service`.
  - Ejemplo: `JugadorService.cs`, `LobbyService.cs`, `GameService.cs`.

### 2.4 Acceso a Datos y Repositorios (ADO.NET)
* **Clases de Acceso a Datos:** Deben terminar con el sufijo `Repository`.
  - Ejemplo: `JugadorRepository.cs`, `PalabraRepository.cs`.

### 2.5 Variables, Parámetros y Miembros de Clase
* **Clases, Interfaces y Métodos Generales:** PascalCase.
  - Ejemplo: `CalcularPuntuacion()`, `IJugadorService`.
* **Propiedades de Clase:** PascalCase.
  - Ejemplo: `NombreUsuario`, `PuntuacionTotal`.
  - *Nota:* El modificador `readonly` no afecta esta regla; las propiedades de solo lectura se escriben en PascalCase.
* **Parámetros y Variables Locales:** camelCase.
  - Ejemplo: `correoElectronico`, `partidaID`.
* **Constantes:** SCREAMING_SNAKE_CASE (letras mayúsculas separadas por guion bajo).
  - Ejemplo: `MAX_PISTAS`, `TIEMPO_ESPERA_SEGUNDOS`.

### 2.6 Manejo de Acrónimos (DTO, ID, DB, UI, WCF, WPF, SQL)
* **Regla:** Los acrónimos deben escribirse completamente en mayúsculas, incluso cuando forman parte de nombres compuestos en PascalCase o camelCase.
* **Ejemplos:**
  - En clases/propiedades (PascalCase): `JugadorDTO`, `PartidaID`, `ConexionDB`, `ControladorUI`, `ServicioWCF`, `VistaWPF`, `ConsultaSQL`.
  - En variables/parámetros (camelCase): `jugadorDTO`, `partidaID`, `conexionDB`, `controladorUI`.

---

## 3. Formato y Estilo de Código (C#)

### 3.1 Estilo de Llaves (Same-Line Braces)
Las llaves de apertura `{` deben colocarse en la misma línea que la declaración de la clase, método, propiedad o estructura de control (`if`, `for`, `while`, `switch`). Las llaves de cierre `}` deben colocarse en su propia línea alineadas con el inicio de la declaración.

#### Ejemplo de Clase, Propiedades y Métodos:
```csharp
namespace LetterClashClient.Views {
  public class GUIEjemploView {
    private string nombreJugador;

    public string NombreJugador {
      get { return nombreJugador; }
      set { nombreJugador = value; }
    }

    public void ValidarJugador(string nombre) {
      if (string.IsNullOrEmpty(nombre)) {
        throw new ArgumentException("El nombre no puede estar vacío.");
      } else {
        NombreJugador = nombre;
      }
    }
  }
}
```

#### Ejemplo de Estructuras de Control:
```csharp
// Estructura IF-ELSE
if (intentosRestantes <= 0) {
  FinalizarPartida(ResultadoPartida.Derrota);
} else {
  ContinuarJuego();
}

// Estructura FOR / FOREACH
foreach (var letra in palabraRevelada) {
  ActualizarCasilla(letra);
}

// Estructura SWITCH
switch (estadoPartida) {
  case EstadoPartida.Pendiente: {
    EsperarJugadores();
    break;
  }
  case EstadoPartida.EnJuego: {
    IniciarTurno();
    break;
  }
  default: {
    throw new InvalidOperationException("Estado no soportado");
  }
}
```

### 3.2 Espaciado e Indentación
* Utilizar **2 espacios** para la indentación (no usar tabuladores tab/\t).
* Colocar un espacio después de palabras clave de control (`if`, `for`, `while`, `switch`).
* Colocar un espacio alrededor de operadores binarios (por ejemplo, `x + y`, `a == b`, `c = d`).

---

## 4. Arquitectura y Buenas Prácticas

### 4.1 UI y Code-Behind de Vistas (`GUI*View.xaml.cs`)
* **Regla:** El code-behind de la vista (controlador de la UI) debe usarse únicamente para la inicialización visual (`InitializeComponent()`) y, si es estrictamente necesario, para interacciones directas con la UI que no puedan resolverse fácilmente mediante enlace de datos (Binding) o comandos.
* **Nombramiento de Eventos (Controladores de la UI):** Si es necesario manejar eventos directamente en el code-behind, estos métodos de eventos deben nombrarse siguiendo el patrón `On{Evento}{NombreDescriptivoDelMetodo}`.
  - Ejemplo: `OnClicIniciarSesion`, `OnSelectionChangedFilas`, `OnLostFocusNombre`.
* Toda la lógica de negocio, validaciones complejas e interacción con los servicios de red debe residir en el **ViewModel** correspondiente.

### 4.2 Lógica de Red y Servicios (WCF)
* Todas las llamadas a servicios externos deben realizarse de forma segura utilizando bloques `try-catch` específicos para manejar fallos de comunicación (`CommunicationException`, `TimeoutException`).
* Los proxies de servicios WCF deben cerrarse correctamente (`Close()`) o abortarse (`Abort()`) en caso de error para evitar fugas de recursos en los canales de red.

### 4.3 Persistencia y Seguridad (Base de Datos)
* **Seguridad de Datos:** La contraseña de los usuarios nunca debe guardarse en texto plano. Debe aplicarse un hash seguro con sal (salt) antes de persistirse en la base de datos (según **RN INF05**).
* **Parámetros SQL:** Para prevenir ataques de Inyección SQL, se deben usar siempre consultas parametrizadas con `SqlParameter` en ADO.NET. Nunca concatenar strings directamente en las consultas.

### 4.4 Localización y Recursos
* Todo texto visible al usuario (etiquetas, botones, mensajes de error, diálogos) debe estar definido en los archivos de recursos (`.resx`) del cliente (`AppResources.resx` para español, `AppResources.en.resx` para inglés) para facilitar la internacionalización y cumplir con el requisito de cambio de idioma preferido.

---

## 5. Manejo de Excepciones y Logging

* **No silenciar excepciones:** Evitar bloques `catch` vacíos. Toda excepción capturada debe ser registrada o manejada adecuadamente.
* **Logging:** Utilizar el registrador de logs personalizado (`CustomLogger` en el servidor) para almacenar trazas detalladas de errores graves, intentos fallidos de conexión o accesos no autorizados.
* **Mensajes amigables:** En la UI, las excepciones técnicas deben ser interceptadas para mostrar mensajes claros y comprensibles al usuario final, en lugar de la traza de error completa del sistema.
