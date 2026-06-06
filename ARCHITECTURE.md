# Arquitectura y Componentes: TEC-Ahorcado (LetterClash)

Este documento detalla la arquitectura de software propuesta y los componentes requeridos para el cliente y el servidor de **TEC-Ahorcado (LetterClash)**, vinculándola con la estrategia de **Integración Sandwich**.

---

## 1. Organización del Proyecto (Unificado vs. Separado)

**Recomendación:** Mantener la arquitectura de Cliente y Servidor en **un solo archivo (`ARCHITECTURE.md`)**. 
Como es un sistema cliente-servidor basado en WCF, el cliente y el servidor están íntimamente acoplados a través de los contratos de servicios (`Contracts`) y las estructuras de datos compartidas (DTOs/Enums). Tener una única referencia centralizada evita el desfase de documentación y facilita a los desarrolladores comprender el flujo dúplex de extremo a extremo.

---

## 2. Estrategia de Desarrollo: Integración Sandwich (Híbrida)

La **Integración Sandwich** combina los enfoques *Top-Down* (de arriba hacia abajo) y *Bottom-Up* (de abajo hacia arriba), encontrándose en una capa intermedia (sistema central/servicios). 

En este proyecto de WPF + WCF + SQL Server, el desarrollo y pruebas se dividen en tres niveles:

```
  ┌────────────────────────────────────────────────────────┐
  │                    NIVEL SUPERIOR                      │  (Enfoque Top-Down)
  │  Cliente: WPF Views & ViewModels                       │  Pruebas de UI, navegación
  │  Servidor: Control de Sesiones de Juego (GameEngine)   │  y lógica con Mocks/Stubs.
  └──────────────────────────┬─────────────────────────────┘
                             │
                             ▼
  ┌────────────────────────────────────────────────────────┐
  │                    NIVEL INTERMEDIO                    │  (Punto de Encuentro)
  │  Contratos WCF (IJugadorService, IGameService)         │  Integración real del canal
  │  Implementaciones de Servicios WCF (Services)          │  de red y flujo de mensajes.
  └──────────────────────────▲─────────────────────────────┘
                             │
                             │
  ┌────────────────────────────────────────────────────────┐
  │                    NIVEL INFERIOR                      │  (Enfoque Bottom-Up)
  │  Cliente: Helper de Red / Proxies WCF                  │  Pruebas unitarias de SQL,
  │  Servidor: DataAccess (SQL Repositories) & Seguridad   │  hashing y conectividad.
  └────────────────────────────────────────────────────────┘
```

1.  **Capa Inferior (Bottom-Up):**
    *   **Servidor:** Base de datos SQL Server, Repositorios (`DataAccess/Repositories`) y componente de encriptación (`Security`). Se prueban de forma aislada para asegurar la persistencia.
    *   **Cliente:** Configuración del cliente WCF y archivos de recursos de traducción.
2.  **Capa Superior (Top-Down):**
    *   **Cliente:** Diseños en XAML (`Views`) y lógica de presentación (`ViewModels`). Inicialmente se prueban usando datos fijos (hardcodeados) o *mocks* del backend para validar animaciones, diseño adaptativo e internacionalización.
    *   **Servidor:** El motor del juego en memoria (`GameSessionManager`) que gestiona las salas activas de manera abstracta.
3.  **Capa Intermedia (Capa de Integración / Sandwich):**
    *   Los contratos WCF (`Contracts/`) y la lógica de los servicios (`Services/`). Es aquí donde se conectan las vistas/viewmodels (superior) con la base de datos y la red (inferior).

---

## 2.3 Diagrama de Componentes del Sistema

A continuación se presenta el diagrama de componentes que ilustra la relación entre el cliente WPF, los contratos WCF compartidos lógicamente, los servicios del servidor, el motor de juego en memoria y la capa de acceso a datos:

```mermaid
graph TD
    %% Styling
    classDef client fill:#eef2f7,stroke:#3182ce,stroke-width:2px,color:#2d3748;
    classDef contract fill:#fffaf0,stroke:#dd6b20,stroke-width:2px,color:#2d3748;
    classDef server fill:#f0fff4,stroke:#38a169,stroke-width:2px,color:#2d3748;
    classDef db fill:#faf5ff,stroke:#805ad5,stroke-width:2px,color:#2d3748;

    subgraph Cliente ["Cliente (LetterClashClient)"]
        direction TB
        V["Views<br/>(Interfaz XAML)"]:::client --> VM["ViewModels<br/>(Lógica de Presentación)"]:::client
        VM --> M_Client["Models<br/>(Estado de Sesión / Juego)"]:::client
        VM --> P_Manager["ServiceProxyManager<br/>(Proxies de WCF)"]:::client
        P_Manager --> C_Callback["GameCallbackHandler<br/>(Manejo de Callback Dúplex)"]:::client
    end

    subgraph Contratos ["Contratos WCF (Compartido)"]
        direction TB
        CI["IJugadorService<br/>ILobbyService<br/>IPalabraService<br/>IAutenticacionService"]:::contract
        CG["IGameService<br/>(Duplex)"]:::contract
        CC["IGameServiceCallback<br/>(Interface Callback)"]:::contract
    end

    subgraph Servidor ["Servidor (LetterClashServer)"]
        direction TB
        subgraph WCF_Services ["Servicios WCF"]
            JS["JugadorService<br/>LobbyService<br/>PalabraService<br/>AutenticacionService<br/>(PerCall)"]:::server
            GS["GameService<br/>(Single / Singleton)"]:::server
        end
        
        subgraph Motor ["Motor del Juego"]
            GE["GameSessionManager<br/>(Singleton)"]:::server
            AG["ActiveGame<br/>(Partida en Memoria)"]:::server
        end
        
        subgraph AccesoDatos ["Acceso a Datos (EF 6)"]
            Rep["Repositories<br/>(Jugador, Partida, Palabra)"]:::server
            Ctx["LetterClashContext<br/>(DbContext)"]:::server
        end
        
        GS --> GE
        GE --> AG
        JS --> Rep
        GS --> Rep
        Rep --> Ctx
    end

    subgraph DB_Layer ["Base de Datos"]
        DB[("SQL Server<br/>(Tablas: Jugador, Partida, Palabra)")]:::db
    end

    %% Relaciones inter-capa
    VM -.-> CI
    VM -.-> CG
    CC -.-> C_Callback
    
    P_Manager ==>|Llamadas SOAP / HTTP| CI
    P_Manager ==>|Conexión Dúplex SOAP / HTTP| CG
    GS ==>|Callbacks de Servidor| CC
    
    Ctx ==>|ADO.NET / TDS| DB

    %% Apply Classes
    class V,VM,M_Client,P_Manager,C_Callback client;
    class CI,CG,CC contract;
    class JS,GS,GE,AG,Rep,Ctx server;
    class DB db;
```

---

## 3. Arquitectura del Servidor (`LetterClashServer`)

El servidor gestiona la lógica de negocio central, la persistencia y la coordinación de las partidas multijugador.

```
LetterClashServer/
│
├── App_Data/                          # Datos de base de datos locales (opcional)
├── Properties/
│   └── AssemblyInfo.cs                # Metadata del ensamblado
│
├── Contracts/                         # Interfaces de WCF (Contratos de Servicio)
│   ├── IJugadorService.cs             # Operaciones síncronas de perfil e historial
│   ├── ILobbyService.cs               # Gestión de salas públicas y códigos de acceso
│   ├── IGameService.cs                # Operaciones de juego interactivo en tiempo real
│   └── IGameServiceCallback.cs        # Métodos que el servidor invoca en el cliente (dúplex)
│
├── Services/                          # Implementaciones de los servicios WCF
│   ├── JugadorService.cs              # Implementa IJugadorService
│   ├── LobbyService.cs                # Implementa ILobbyService
│   └── GameService.cs                 # Implementa IGameService (DuplexService)
│
├── Domain/                            # Lógica de Negocio y Motor del Juego
│   ├── Models/                        # DTOs, Enums y modelos de servicio
│   │   ├── JugadorDTO.cs              # Información del jugador a transferir
│   │   ├── JugadorPublicoDTO.cs       # Información pública reducida del jugador
│   │   ├── PartidaDTO.cs              # Detalles de la partida
│   │   ├── PalabraDTO.cs              # Palabra seleccionada con descripción
│   │   ├── Idiomas.cs                 # Constantes de idiomas del sistema
│   │   ├── CodigoError.cs             # Enum de códigos de error de WCF
│   │   ├── ServiceFault.cs            # Estructura de falla WCF para errores
│   │   └── ServiceResult.cs           # Wrapper genérico para retornos de servicio WCF
│   │
│   ├── GameEngine/                    # Motor de emparejamiento y juego en memoria
│   │   ├── ActiveGame.cs              # Estado de una partida activa en el servidor
│   │   └── GameSessionManager.cs      # Singleton para coordinar todas las salas en ejecución
│   │
│   └── Security/                      # Seguridad del lado del servidor
│       └── CryptographyHelper.cs      # Hashing de contraseñas y sanitización
│
├── DataAccess/                        # Capa de Persistencia (SQL Server)
│   ├── Context/                       # Contexto de Entity Framework 6 y entidades auto-generadas
│   │   ├── LetterClashContext.cs      # DbContext para gestionar las entidades en la BD
│   │   ├── Jugador.cs                 # Entidad mapeada de la tabla Jugador
│   │   ├── Palabra.cs                 # Entidad mapeada de la tabla Palabra
│   │   └── Partida.cs                 # Entidad mapeada de la tabla Partida
│   │
│   └── Repositories/                  # Acceso a datos y consultas utilizando EF 6
│       ├── JugadorRepository.cs
│       ├── PalabraRepository.cs
│       └── PartidaRepository.cs
│
├── Shared/                            # Utilidades compartidas en el proyecto
│   ├── Enums/                         # Enumeraciones de base de datos y flujo
│   │   └── GameEnums.cs               # EstadoPartida, ResultadoPartida, Idioma
│   └── Utilities/
│       └── CustomLogger.cs            # Registro de logs del servidor
│
├── Web.config                         # Configuración de WCF, Endpoints (HTTP/Dúplex) e IIS Express
├── JugadorService.svc                 # Archivo de host del servicio de Jugador
├── LobbyService.svc                   # Archivo de host del servicio de Lobby
├── GameService.svc                    # Archivo de host del servicio de Game (dúplex)
└── PalabraService.svc                 # Archivo de host del servicio de Palabra
```

### 3.1 Ciclo de Vida y Hospedaje de Servicios (WCF)

A diferencia de los entornos de un solo proceso en ejecución continua (como Bun o Node.js), los servicios WCF en este servidor se ejecutan bajo un esquema de **Hospedaje Administrado** por IIS/IIS Express:

1. **Hospedador del Proceso (Process Host):** IIS Express se inicia de forma pasiva escuchando en el puerto asignado. Los servicios no ocupan memoria hasta recibir la primera petición del cliente.
2. **Activación Bajo Demanda (On-Demand):** WCF intercepta la llamada dirigida a un archivo `.svc` y en ese instante instancia la clase de servicio correspondiente para resolver la petición.
3. **Modos de Instanciación (`InstanceContextMode`):**
   * **`PerCall` (Por llamada - Predeterminado):** WCF crea una nueva instancia del servicio para cada petición entrante y la destruye inmediatamente tras finalizar la ejecución. Esto asegura que los servicios sean libres de estado (*stateless*). Se aplica a: `JugadorService`, `LobbyService` y `PalabraService`.
   * **`Single` (Singleton):** Se crea una única instancia del servicio para todo el tiempo de vida de la aplicación. Todas las peticiones de todos los clientes comparten este mismo objeto en memoria. Se aplica de forma obligatoria a: `GameService`, permitiendo que el motor de juego en memoria (`GameSessionManager`) centralice el flujo de partidas multijugador activas en tiempo real.

---

## 4. Arquitectura del Cliente (`LetterClashClient`)

El cliente WPF se estructura bajo el patrón **MVVM (Model-View-ViewModel)** para separar de forma limpia la interfaz gráfica de la lógica de presentación y comunicaciones.

```
LetterClashClient/
│
├── Properties/                        # Configuración del ensamblado
│   ├── AssemblyInfo.cs
│   ├── Resources.Designer.cs
│   ├── Settings.Designer.cs
│   └── Resources.resx                 # Recursos de localización por defecto (Español)
│
├── App.config                         # Configuración de Endpoints WCF (direcciones del servidor)
├── App.xaml                           # Archivo de inicio y recursos globales del cliente
├── App.xaml.cs                        # Código subyacente de App.xaml (inicializaciones)
│
├── Views/                             # Vistas de WPF (XAML + Code-Behind mínimo)
│   ├── GUILoginView.xaml              # Pantalla de inicio de sesión
│   ├── GUIRegisterView.xaml           # Pantalla de registro de nuevos usuarios
│   ├── GUILobbyView.xaml              # Lista de partidas públicas y código de acceso a privada
│   ├── GUIGameView.xaml               # Pantalla de juego (ahorcado, chat y puntuación)
│   ├── GUIProfileView.xaml            # Visualización y edición de perfil de usuario
│   ├── GUIHistoryView.xaml            # Historial de partidas jugadas
│   └── GUILeaderboardView.xaml        # Marcador global (Top 100)
│
├── ViewModels/                        # Lógica de presentación y enlace de datos (MVVM)
│   ├── ViewModelBase.cs               # Base con implementación de INotifyPropertyChanged
│   ├── LoginViewModel.cs
│   ├── RegisterViewModel.cs
│   ├── LobbyViewModel.cs
│   ├── GameViewModel.cs
│   ├── ProfileViewModel.cs
│   ├── HistoryViewModel.cs
│   └── LeaderboardViewModel.cs
│
├── Models/                            # Clases de dominio locales y estado del cliente
│   ├── SessionContext.cs              # Almacena el usuario logueado en la sesión local
│   └── LocalGameState.cs              # Mantiene el estado visual del juego (guiones, fallos locales)
│
├── Services/                          # Lógica de comunicación con el Servidor WCF
│   ├── ServiceProxyManager.cs         # Administra la apertura, cierre y reintento de conexión WCF
│   └── GameCallbackHandler.cs         # Implementa IGameServiceCallback (escucha al servidor dúplex)
│
├── Resources/                         # Estilos y Diccionarios de Recursos de WPF
│   ├── AppResources.resx              # Localización (Español)
│   ├── AppResources.en.resx           # Localización (Inglés)
│   ├── Styles/                        # Archivos XAML de estilos de diseño
│   │   ├── Colors.xaml                # Paleta de colores unificada (Temas)
│   │   ├── Buttons.xaml               # Estilos personalizados para botones
│   │   └── TextStyles.xaml            # Tipografías y tamaños de fuentes
│   └── Images/                        # Assets estáticos (iconos, avatares, partes del ahorcado)
│
└── Utilities/                         # Helpers y comandos generales
    ├── RelayCommand.cs                # Implementación de ICommand para enlazar eventos del XAML a VMs
    └── InputValidator.cs              # Validación de contraseñas, correos y campos de texto
```

---

## 5. Flujo de Control en Tiempo Real (Ejemplo)

A continuación se muestra el ciclo de vida de un juego a nivel componentes:

```mermaid
sequenceDiagram
    autonumber
    actor C1 as Cliente 1 (Anfitrión)
    participant S as GameService (Server)
    participant M as GameSessionManager
    actor C2 as Cliente 2 (Adivinador)
    participant DB as SQL Server (DB)

    C1->>S: CrearPartida(idioma, palabraId)
    S->>DB: INSERT INTO Partida (Anfitrion, Palabra, Estado='PENDIENTE')
    S->>M: RegistrarPartidaEnMemoria(código, partidaId)
    S-->>C1: Retornar CódigoAcceso (e.g. "AB12CD")

    Note over C1,C2: Cliente 1 espera en la pantalla GUIPartidaView

    C2->>S: UnirseAPartidaPrivada("AB12CD")
    S->>M: ObtenerPartidaPorCodigo("AB12CD")
    S->>DB: UPDATE Partida SET Adivinador=Id, Estado='EN_JUEGO'
    S->>M: ConectarAdivinador(canalCallback)
    S-->>C1: Callback.OnJugadorSeUnio(C2_Info)
    S-->>C2: Confirmar Unión & Cargar Pantalla Juego

    Note over C1,C2: El juego inicia. Turno del Adivinador

    C2->>S: EscribirLetra('A')
    S->>M: EvaluarLetraEnPartida('A')
    alt Letra es correcta
        S-->>C1: Callback.OnLetraPropuesta('A', esCorrecta=true, palabraRevelada="A _ _ A _ _")
        S-->>C2: Callback.OnLetraPropuesta('A', esCorrecta=true, palabraRevelada="A _ _ A _ _")
    else Letra es incorrecta (Turno Fallido)
        S->>M: DecrementarTurno()
        S-->>C1: Callback.OnLetraPropuesta('A', esCorrecta=false, vidaRestante=80)
        S-->>C2: Callback.OnLetraPropuesta('A', esCorrecta=false, vidaRestante=80)
        Note over C1: C1 dibuja parte del ahorcado correspondiente
    end

    Note over C1,C2: Fin del Juego (Palabra Adivinada o Ahorcado Completo)
    S->>DB: UPDATE Partida SET Estado='CONCLUIDA', Resultado='ADIVINADA'/'SIN_ADIVINAR'
    S->>DB: UPDATE Jugador SET Puntuacion = Puntuacion + 10 / -5
    S-->>C1: Callback.OnPartidaFinalizada(ganador, puntuacion)
    S-->>C2: Callback.OnPartidaFinalizada(ganador, puntuacion)
```

---

## 6. Diagrama de Despliegue

El siguiente diagrama detalla la arquitectura física y de red de la aplicación, mostrando cómo se distribuyen los componentes en los nodos cliente, servidor de aplicaciones y servidor de bases de datos, así como los puertos y protocolos utilizados para la intercomunicación:

```mermaid
graph TB
    %% Styling
    classDef client fill:#eef2f7,stroke:#3182ce,stroke-width:2px,color:#2d3748;
    classDef server fill:#f0fff4,stroke:#38a169,stroke-width:2px,color:#2d3748;
    classDef db fill:#faf5ff,stroke:#805ad5,stroke-width:2px,color:#2d3748;

    subgraph ClientNode ["Nodo Cliente: PC de Usuario"]
        direction TB
        ClientOS["Sistema Operativo Windows"]:::client
        NETClient[".NET Framework 4.7.2 Runtime"]:::client
        ClientApp["LetterClashClient.exe<br/>(Aplicación de Escritorio WPF)"]:::client
        
        ClientOS --- NETClient
        NETClient --- ClientApp
    end

    subgraph AppServerNode ["Nodo Servidor: Servidor de Aplicaciones Web"]
        direction TB
        IISServer["Internet Information Services (IIS / IIS Express)"]:::server
        WCFRuntime["Entorno de Ejecución WCF (.NET 4.7.2)"]:::server
        ServerApp["LetterClashServer (WCF Services)<br/>- Autenticacion, Jugador, Lobby, Palabra, Game"]:::server
        
        IISServer --- WCFRuntime
        WCFRuntime --- ServerApp
    end

    subgraph DBNode ["Nodo Base de Datos"]
        direction TB
        SQLServer["Motor de Base de Datos SQL Server"]:::db
        Database["Base de Datos Relacional: LetterClashDB"]:::db
        
        SQLServer --- Database
    end

    %% Communication paths with ports and protocols
    ClientApp ==>|Puerto 80/443: basicHttpBinding<br/>(Servicios síncronos / SOAP over HTTP)| IISServer
    ClientApp <=>|Puerto 80/443: wsDualHttpBinding<br/>(Canal de comunicación dúplex / SOAP over HTTP)| IISServer
    ServerApp ==>|Puerto 1433: Protocolo TDS<br/>(Entity Framework 6 / ADO.NET)| SQLServer

    %% Apply Classes
    class ClientOS,NETClient,ClientApp client;
    class IISServer,WCFRuntime,ServerApp server;
    class SQLServer,Database db;
```
