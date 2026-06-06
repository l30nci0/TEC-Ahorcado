# Especificación del Proyecto: TEC-Ahorcado (LetterClash)

Este documento contiene las reglas de negocio y los casos de uso para el sistema **TEC-Ahorcado (LetterClash)**.

---

## Planteamiento del Problema

En el ámbito de la ingeniería de software y el desarrollo de sistemas distribuidos, la construcción de aplicaciones interactivas en tiempo real representa un reto técnico significativo. El juego clásico de "El Ahorcado" es ampliamente conocido, pero sus implementaciones convencionales suelen ser locales, de un solo jugador o limitadas a arquitecturas de petición-respuesta síncronas tradicionales. 

El desarrollo de una plataforma interactiva, distribuida y multijugador como **TEC-Ahorcado (LetterClash)** plantea diversos problemas y desafíos de ingeniería:

1. **Sincronización Bidireccional en Tiempo Real:** Establecer canales de comunicación dúplex estables donde el servidor pueda notificar de forma asíncrona a los clientes conectados sobre eventos clave (entrada de jugadores, envío de mensajes por chat, propuesta de letras y decremento de turnos) sin depender de esquemas ineficientes de consulta continua (*polling*).
2. **Desacoplamiento Arquitectónico:** Diseñar y enlazar una interfaz gráfica responsiva en el cliente mediante el patrón MVVM (Model-View-ViewModel) en WPF, comunicándose con un servidor basado en servicios WCF (.NET Framework 4.7.2), protegiendo la lógica de negocio y encapsulando las consultas de datos.
3. **Gestión de Concurrencia y Estado en Memoria:** Mantener de forma consistente el estado de las salas de juego y las partidas en curso mediante un administrador de sesiones en memoria en el servidor (`GameSessionManager` de tipo *Singleton*), coordinando la concurrencia de múltiples hilos de ejecución de clientes y actualizando la base de datos de manera atómica con Entity Framework 6.
4. **Resiliencia y Manejo Estandarizado de Fallos:** Implementar un esquema uniforme de propagación de excepciones en red que traduzca fallos internos o de validación de negocio del backend a códigos de error comprensibles (`CodigoError`) estructurados en objetos genéricos (`ServiceResult`), permitiendo al cliente WPF reaccionar de forma amigable ante fallos de conectividad, base de datos o lógica de juego.

Este proyecto universitario de ingeniería de software aborda de forma integral estos desafíos, integrando prácticas y patrones recomendados para el desarrollo de software seguro, escalable y con una experiencia de usuario fluida.

---

## Decisiones de Diseño y Justificación

Para dar solución a los problemas planteados, se tomaron decisiones tecnológicas y de diseño fundamentales, justificadas bajo criterios de modularidad, mantenibilidad, rendimiento y seguridad:

### 1. Arquitectura de Comunicación: WCF con Binding Dúplex (`wsDualHttpBinding`)
*   **Decisión:** Utilizar Windows Communication Foundation (WCF) configurando `wsDualHttpBinding` para la comunicación del servicio de partidas (`GameService`).
*   **Justificación:** El juego requiere sincronización en tiempo real y flujo bidireccional de datos (por ejemplo, notificar al Anfitrión del daño recibido o al Adivinador del cambio de turno). Un binding convencional (como `basicHttpBinding`) forzaría al cliente a realizar solicitudes continuas (*polling*), saturando el canal de red y aumentando la latencia. `wsDualHttpBinding` establece un canal de retorno (*callback*) que permite al servidor empujar eventos al cliente instantáneamente bajo demanda.

### 2. Estructuración del Cliente: Patrón MVVM (Model-View-ViewModel) en WPF
*   **Decisión:** Desarrollar la aplicación de escritorio en C#/WPF separando estrictamente la presentación usando el patrón de diseño arquitectónico MVVM.
*   **Justificación:** Al desacoplar la interfaz gráfica (XAML Views) de la lógica de presentación (ViewModels) y del estado local/red (Models), facilitamos el mantenimiento independiente de la UI. Esto permite realizar pruebas unitarias sobre los ViewModels utilizando *mocks* o datos de prueba fijos, sin necesidad de estar conectados a la red durante las primeras fases de desarrollo.

### 3. Gestión de Ciclo de Vida: Servicios Stateless (`PerCall`) vs. Stateful (`Single`)
*   **Decisión:** Asignar `InstanceContextMode.PerCall` a los servicios de perfil e historial (`JugadorService`, `LobbyService`, `PalabraService`), y `InstanceContextMode.Single` al servicio de juego (`GameService`).
*   **Justificación:** 
    *   Los servicios estándar son sin estado (`PerCall`), lo que libera memoria inmediatamente tras resolver la llamada SOAP, optimizando los recursos del servidor IIS y garantizando la escalabilidad de la base de usuarios.
    *   El servicio de juego requiere un estado persistente en memoria (`Single`) porque debe coordinar las conexiones concurrentes de jugadores en las mismas salas de juego y enviar callbacks de red a los clientes correctos en tiempo real mediante el objeto singleton `GameSessionManager`.

### 4. Capa de Acceso a Datos: ORM Entity Framework 6 (EF6)
*   **Decisión:** Utilizar Entity Framework 6 para la gestión y mapeo de tablas relacionales (Jugador, Partida, Palabra) a objetos C#.
*   **Justificación:** El uso de un mapeador objeto-relacional reduce drásticamente el código boilerplate de consultas SQL tradicionales (evitando sentencias propensas a inyección SQL). Asimismo, asegura la validación en tiempo de compilación de los tipos de datos y simplifica la lógica de transacciones complejas en la base de datos relacional SQL Server.

### 5. Control de Fallos en Sistemas Distribuidos: Serializable `ServiceResult` y `CodigoError` Enum
*   **Decisión:** Retornar un objeto envoltorio genérico `ServiceResult<T>` que contiene un enum tipado `CodigoError` (en lugar de lanzar SOAP Faults directos).
*   **Justificación:** 
    *   **Seguridad:** Impedir que excepciones crudas del servidor (como fallos de base de datos o referencias nulas) se expongan en el protocolo SOAP, mitigando la divulgación de información sensible sobre la infraestructura del backend.
    *   **Resiliencia:** Al estandarizar los códigos de error mediante un enum serializable de WCF (`PARAMETRO_INVALIDO`, `RECURSO_NO_ENCONTRADO`, etc.), el cliente WPF puede capturar y procesar de manera segura y controlada las fallos de red o de negocio, permitiendo mostrar avisos amigables localizados al usuario sin interrumpir el flujo o provocar el cierre inesperado de la aplicación.

### 6. Persistencia Selectiva y Estado Efímero en Memoria (Chat y Progreso de Partida)
*   **Decisión:** Manejar el chat en tiempo real y el progreso intermedio de la partida (letras propuestas, fallos y vida) estrictamente en la memoria RAM del servidor (a través de `GameSessionManager`), persistiendo en la base de datos únicamente el resultado final consolidado de la partida y penalizaciones.
*   **Justificación:** 
    *   **Rendimiento y Reducción de E/S (I/O):** El envío de mensajes de chat y el tecleo de letras ocurren con una frecuencia y velocidad altas durante una partida activa. Escribir cada mensaje o letra propuesta en la base de datos relacional SQL Server generaría una sobrecarga masiva de operaciones de entrada/salida y bloqueos concurrentes innecesarios, degradando el rendimiento general del sistema.
    *   **Naturaleza Efímera:** El historial del chat y el estado visual del ahorcado son datos transitorios relevantes únicamente mientras la partida está en curso. Una vez que la partida concluye, esta información pierde valor operativo.
    *   **Optimización del Almacenamiento:** Al persistir solo los datos críticos finales (ganador, perdedor, variaciones de puntuación, fecha de juego y estado final), optimizamos significativamente el espacio en disco del servidor SQL Server y simplificamos el modelo relacional del sistema.

---

## Reglas de Negocio (RN)

| ID | Descripción |
| :--- | :--- |
| **INF01** | Un Jugador solo puede actualizar o eliminar los datos de su registro Jugador en la base de datos. |
| **INF02** | El Correo Electrónico no puede ser actualizado. |
| **INF03** | El NombreDeUsuario no puede ser actualizado. |
| **INF04** | La Contraseña nunca debe ser visible en la UI. |
| **INF05** | La Contraseña debe ser guardada con encriptación. |
| **INF06** | El Lobby solo contiene un listado de Partidas Públicas que aún no están en curso. |
| **INF07** | Una Partida solo puede ser asignada a un único Adivinador. |
| **INF08** | Un Jugador solo puede unirse a una Partida que no tenga un Adivinador. |
| **INF09** | El progreso de una Partida se guarda en la memoria del servidor. |
| **INF10** | Si un jugador abandona una partida que ya está en curso, se le aplicará una penalización de 3 puntos en su puntuación. En caso contrario (si la partida aún no ha comenzado), no se penalizará a ningún jugador. |
| **INF11** | Una Partida termina cuando el Cuerpo Humano ha sido dibujado completamente. |
| **INF12** | Al terminar una Partida el Jugador ganador debe recibir un incremento de 10 puntos en su Puntuación, caso contrario, el perdedor debe perder 5 puntos. |
| **INF13** | Una Partida tiene un máximo de 2 pistas. |

---

## Casos de Uso (CU)

### CU01: Actualizar Perfil

| Atributo | Detalle |
| :--- | :--- |
| **ID** | CU01 |
| **Nombre** | Actualizar Perfil |
| **Actor** | AC01 Jugador |
| **Descripción** | El Jugador actualiza los datos de su registro Perfil en la base de datos al introducir los datos que pueden ser actualizados (Nombre, Teléfono, Contraseña y Avatar). |
| **Disparador** | El Jugador presiona “Actualizar Perfil” desde la pantalla `GUIPáginaInicio`. |
| **Precondiciones** | **PRE-01**: El Jugador ha iniciado sesión.<br>**PRE-02**: El Perfil existe en la base de datos. |
| **Postcondiciones** | **POST01**: El perfil de un Jugador ha sido actualizado en la base de datos. |
| **Reglas de Negocio** | [INF01](#reglas-de-negocio-rn), [INF02](#reglas-de-negocio-rn), [INF03](#reglas-de-negocio-rn), [INF04](#reglas-de-negocio-rn), [INF05](#reglas-de-negocio-rn) |
| **Relaciones** | N/A |

#### Flujo Normal
1. El Sistema recupera los datos del Jugador (IDJugador, Nombre, NombreDeUsuario, Correo, Teléfono, Contraseña y Avatar). *(Ver EX1)*
2. El Sistema muestra la pantalla `GUIActualizarPerfil` con un formulario con un campo para cada propiedad del Perfil rellenos con los datos previos y los botones “Cancelar” y “Actualizar Perfil”.
3. El Jugador introduce los datos correspondientes y presiona el botón “Actualizar Perfil”.
4. El Sistema valida los datos introducidos.
5. El Sistema actualiza el Jugador en la base de datos. *(Ver FA1)* *(Ver EX2)*
6. El Sistema muestra un modal con el mensaje “Su perfil ha sido actualizado correctamente.” y el botón “Aceptar”.
7. El Jugador presiona el botón “Aceptar”.
8. El Sistema cierra el modal.
9. Termina el Caso de Uso.

#### Flujo Alterno
*   **FA1: Datos Inválidos**
    1. El Sistema muestra una etiqueta con un mensaje de error de validación debajo de cada campo incorrecto.
    2. Regresa al paso 3.

#### Flujos de Excepción
*   **EX1: Error al Consultar Perfil**
    1. El Sistema muestra un modal de error de servidor con el mensaje “No ha sido posible recuperar la información de su perfil debido a un error en el sistema. Intente de nuevo más tarde.” y el botón “Aceptar”.
    2. El Jugador presiona el botón “Aceptar”.
    3. El Sistema cierra el modal.
*   **EX2: Error al Actualizar Perfil**
    1. El Sistema muestra un modal de error de servidor con el mensaje “No ha sido posible actualizar su perfil debido a un error en el sistema. Intente de nuevo más tarde.” y el botón “Aceptar”.
    2. El Jugador presiona el botón “Aceptar”.
    3. El Sistema cierra el modal.

---

### CU02: Entrar a Lobby

| Atributo | Detalle |
| :--- | :--- |
| **ID** | CU02 |
| **Nombre** | Entrar a Lobby |
| **Actor** | AC01 Jugador |
| **Descripción** | El Jugador consulta el Lobby con las Partidas públicas para poder unirse y jugar en una de estas. |
| **Disparador** | El Jugador presiona “Entrar a Lobby” desde la pantalla `GUIPáginaInicio`. |
| **Precondiciones** | **PRE-01**: El Jugador ha iniciado sesión. |
| **Postcondiciones** | N/A |
| **Reglas de Negocio** | [INF06](#reglas-de-negocio-rn) |
| **Relaciones** | N/A |

#### Flujo Normal
1. El Sistema recupera un listado de Partidas disponibles que no tengan un Adivinador asignado. *(Ver EX1)*
2. El Sistema muestra la pantalla `GUILobby` con el botón “Unirse a Partida Privada” y una tabla con cada una de las Partidas con sus datos (NombreDeUsuario del Anfitrión, Idioma y FechaDeCreación) con el botón “Unirse” para cada fila.
3. Termina el Caso de Uso.

#### Flujo Alterno
*   N/A

#### Flujos de Excepción
*   **EX1: Error al Consultar Lobby**
    1. El Sistema muestra un modal de error de servidor con el mensaje “No ha sido posible recuperar la información del lobby debido a un error en el sistema. Intente de nuevo más tarde.” y el botón “Aceptar”.
    2. El Jugador presiona el botón “Aceptar”.
    3. El Sistema cierra el modal.

---

### CU03: Crear Partida

| Atributo | Detalle |
| :--- | :--- |
| **ID** | CU03 |
| **Nombre** | Crear Partida |
| **Actor** | AC01 Jugador |
| **Descripción** | El Jugador crea una partida nueva y configura su palabra para esperar a otro jugador. |
| **Disparador** | El Jugador presiona “Crear Partida” desde la pantalla `GUIPáginaInicio`. |
| **Precondiciones** | **PRE-01**: El Jugador ha iniciado sesión. |
| **Postcondiciones** | **POST01**: Una Partida queda registrada en el sistema.<br>**POST02**: Una Partida queda visible en el Lobby (si es pública). |
| **Reglas de Negocio** | [INF07](#reglas-de-negocio-rn), [INF08](#reglas-de-negocio-rn) |
| **Relaciones** | N/A |

#### Flujo Normal
1. El Sistema muestra el modal `GUIModalRegistrarPartida` con un formulario para crear una Partida con campos para seleccionar Idioma, Palabra, así como el botón “Crear Partida”.
2. El Jugador presiona “Seleccionar Palabra”.
3. El Sistema recupera un listado de Palabras de la base de datos correspondiente al Idioma Preferido del Usuario. *(Ver EX1)*
4. El Sistema muestra el `GUIModalSeleccionarPalabra` con el campo Idioma de Palabra y una tabla de Palabras con sus datos (Palabra y Descripción) y el botón “Seleccionar Palabra”.
5. El Jugador selecciona una Palabra y presiona “Seleccionar Palabra”. *(Ver FA1)*
6. El Sistema cierra el `GUIModalSeleccionarPalabra`.
7. El Jugador presiona “Crear Partida”.
8. El Sistema registra una Partida Privada en la base de datos. *(Ver EX2)*
9. El Sistema muestra la Partida en el Lobby.
10. El Sistema muestra un modal con el mensaje “Su partida ha sido creada con éxito. Tendrá que compartir su código de acceso para que alguien pueda unirse.” y el botón “Aceptar”.
11. El Jugador presiona el botón “Aceptar”.
12. El Sistema cierra el modal.
13. El Sistema muestra la pantalla `GUIPartida`.
14. Termina el Caso de Uso.

#### Flujo Alterno
*   **FA1: Cambiar Idioma de Palabra**
    1. El Jugador selecciona un Idioma.
    2. El Sistema recupera un listado de Palabras de la base de datos correspondiente al Idioma seleccionado. *(Ver EX1)*
    3. El Sistema actualiza la tabla de Palabras.
    4. Continúa en el paso 5.

#### Flujos de Excepción
*   **EX1: Error al Recuperar Palabras**
    1. El Sistema muestra un modal de error de servidor con el mensaje “No ha sido posible registrar la partida porque no se han podido recuperar las palabras para su selección. Intente de nuevo más tarde.” y el botón “Aceptar”.
    2. El Jugador presiona el botón “Aceptar”.
    3. El Sistema cierra el modal.
*   **EX2: Error al registrar Partida**
    1. El Sistema muestra un modal de error de servidor con el mensaje “No ha sido posible registrar la partida debido a un error en el sistema. Intente de nuevo más tarde.” y el botón “Aceptar”.
    2. El Jugador presiona el botón “Aceptar”.
    3. El Sistema cierra el modal.

---

### CU04: Unirse a Partida de Lobby

| Atributo | Detalle |
| :--- | :--- |
| **ID** | CU04 |
| **Nombre** | Unirse a Partida de Lobby |
| **Actor** | AC01 Jugador |
| **Descripción** | El Jugador se une a una Partida pública disponible en el Lobby. |
| **Disparador** | El Jugador presiona “Unirse” en alguna partida pública dentro de la pantalla `GUILobby`. |
| **Precondiciones** | **PRE-01**: El Jugador ha iniciado sesión.<br>**PRE-02**: La Partida existe en la base de datos.<br>**PRE-03**: La Partida tiene su Estado en `PENDIENTE`.<br>**PRE-04**: La Partida aún no tiene un Adivinador asignado. |
| **Postcondiciones** | **POST01**: Una Partida queda con un Adivinador asignado en la base de datos.<br>**POST02**: Una Partida queda con su Estado actualizado a `EN_JUEGO`. |
| **Reglas de Negocio** | [INF07](#reglas-de-negocio-rn), [INF08](#reglas-de-negocio-rn) |
| **Relaciones** | **Incluye**: [CU02 Entrar a Lobby](#cu02-entrar-a-lobby) |

#### Flujo Normal
1. El Sistema asigna al Jugador como Adivinador de la Partida en la base de datos. *(Ver EX1)*
2. El Sistema remueve la Partida del Lobby.
3. El Sistema muestra la pantalla `GUIPartida`.
4. Termina el Caso de Uso.

#### Flujo Alterno
*   N/A

#### Flujos de Excepción
*   **EX1: Error al Asignar Adivinador**
    1. El Sistema muestra un modal de error de servidor con el mensaje “No ha sido posible comenzar la partida debido a un error en el sistema. Intente de nuevo más tarde.” y el botón “Aceptar”.
    2. El Jugador presiona el botón “Aceptar”.
    3. El Sistema cierra el modal.

---

### CU05: Unirse a Partida Privada

| Atributo | Detalle |
| :--- | :--- |
| **ID** | CU05 |
| **Nombre** | Unirse a Partida Privada |
| **Actor** | AC01 Jugador |
| **Descripción** | El Jugador se une a una Partida privada al introducir el `CódigoDeAcceso` correspondiente. |
| **Disparador** | El Jugador presiona “Unirse a Partida Privada” desde la pantalla `GUILobby`. |
| **Precondiciones** | **PRE-01**: El Jugador ha iniciado sesión.<br>**PRE-02**: La Partida existe en la base de datos.<br>**PRE-03**: La Partida aún no tiene un Adivinador asignado. |
| **Postcondiciones** | **POST01**: Una Partida queda con un Adivinador asignado en la base de datos.<br>**POST02**: Una Partida queda con su Estado actualizado a `EN_JUEGO`. |
| **Reglas de Negocio** | [INF07](#reglas-de-negocio-rn), [INF08](#reglas-de-negocio-rn) |
| **Relaciones** | **Incluye**: [CU02 Entrar a Lobby](#cu02-entrar-a-lobby) |

#### Flujo Normal
1. El Sistema muestra el modal `GUIModalCódigoDeAcceso` con un campo para introducir un Código de Acceso y el botón “Unirse a Partida”.
2. El Jugador introduce el Código de Acceso y presiona “Unirse a Partida”.
3. El Sistema valida el formato del Código de Acceso.
4. El Sistema recupera una Partida con el Código de Acceso. *(Ver FA1)* *(Ver FA2)*
5. El Sistema asigna al Jugador como Adivinador de la Partida en la base de datos. *(Ver EX1)*
6. El Sistema remueve la Partida del Lobby.
7. El Sistema muestra la pantalla `GUIPartida`.
8. Termina el Caso de Uso.

#### Flujos Alternos
*   **FA1: Código de Acceso con Mal Formato**
    1. El Sistema muestra una etiqueta del campo de texto con el mensaje “El Código de Acceso no cumple el formato requerido de 6 dígitos.”.
    2. Regresa al paso 2.
*   **FA2: Partida no encontrada**
    1. El Sistema muestra un modal con el mensaje “Partida no encontrada” y el botón “Aceptar”. 
    2. El Jugador da clic en “Aceptar”.
    3. Regresa al paso 2.

#### Flujos de Excepción
*   **EX1: Error al Asignar Adivinador**
    1. El Sistema muestra un modal de error de servidor con el mensaje “No ha sido posible comenzar la partida debido a un error en el sistema. Intente de nuevo más tarde.” y el botón “Aceptar”.
    2. El Jugador presiona el botón “Aceptar”.
    3. El Sistema cierra el modal.

---

### CU06: Escribir Letra

| Atributo | Detalle |
| :--- | :--- |
| **ID** | CU06 |
| **Nombre** | Escribir Letra |
| **Actor** | AC02 Adivinador |
| **Descripción** | El Adivinador introduce una letra para intentar adivinar la Palabra. |
| **Disparador** | El Jugador introduce una letra en la pantalla `GUIPartida` en alguno de los campos de texto dedicados a las letras de la Palabra asignada. |
| **Precondiciones** | **PRE-01**: El Jugador ha iniciado sesión.<br>**PRE-02**: La Partida existe en la base de datos. |
| **Postcondiciones** | **POST01**: El contador de Turnos se incrementa si la letra es incorrecta. |
| **Reglas de Negocio** | [INF09](#reglas-de-negocio-rn) |
| **Relaciones** | N/A |

#### Flujo Normal
1. El Sistema revela la letra en todos los campos de texto que correspondan a la misma letra de la Palabra. *(Ver FA1)*
2. El Sistema reproduce un sonido de acierto y la barra de vida del Anfitrión se reduce en un 20%.
3. El Sistema incrementa el contador de Turnos en el sistema.
4. Termina el Caso de Uso.

#### Flujo Alterno
*   **FA1: Letra Equivocada**
    1. El Sistema reproduce un sonido de error.
    2. El Sistema dibuja la siguiente parte del cuerpo humano (Ver [CU07](#cu07-dibujar-cuerpo-humano)).
    3. Continúa en el paso 3.

#### Flujos de Excepción
*   N/A

---

### CU07: Dibujar Cuerpo Humano

| Atributo | Detalle |
| :--- | :--- |
| **ID** | CU07 |
| **Nombre** | Dibujar Cuerpo Humano |
| **Actor** | AC03 Anfitrión |
| **Descripción** | El Anfitrión dibuja la parte del cuerpo humano correspondiente al fallo del adivinador. |
| **Disparador** | El Adivinador introduce una letra errónea en la pantalla `GUIPartida`. |
| **Precondiciones** | **PRE-01**: El Anfitrión ha iniciado sesión. |
| **Postcondiciones** | **POST01**: La barra de vida del Adivinador se actualiza y la parte del cuerpo se dibuja. |
| **Reglas de Negocio** | [INF11](#reglas-de-negocio-rn) |
| **Relaciones** | N/A |

#### Flujo Normal
1. El Sistema dibuja la parte del cuerpo humano lentamente. 
2. El Sistema reproduce un sonido de daño y la barra de vida del Adivinador se reduce en un 20%.
3. El Sistema espera la entrada del Adivinador.
4. Termina el Caso de Uso.

#### Flujo Alterno
*   **FA1: Fin del Juego (Cuerpo Completo)**
    1. El Sistema reproduce un sonido de daño fatal y la barra de vida del Adivinador se vacía (se completan los 6 turnos fallidos).
    2. El Sistema marca la Partida como `CONCLUIDA` con Resultado `SIN_ADIVINAR`.
    3. Aplica los cambios de puntuación correspondientes.
    4. Continúa en el paso 3.

#### Flujos de Excepción
*   N/A

---

### CU08: Abandonar Partida

| Atributo | Detalle |
| :--- | :--- |
| **ID** | CU08 |
| **Nombre** | Abandonar Partida |
| **Actor** | AC01 Jugador |
| **Descripción** | Uno de los jugadores abandona la Partida activa. |
| **Disparador** | El Jugador utiliza cualquier método para salir de la Partida (cerrar la ventana, finalizar el proceso, etc.). |
| **Precondiciones** | **PRE-01**: El Jugador ha iniciado sesión.<br>**PRE-02**: La Partida existe en la base de datos y está en curso. |
| **Postcondiciones** | **POST01**: La puntuación del Jugador que abandonó la partida se reduce 3 puntos (si la partida ya estaba en curso). |
| **Reglas de Negocio** | [INF10](#reglas-de-negocio-rn) |
| **Relaciones** | **Incluye**: [CU04](#cu04-unirse-a-partida-de-lobby), [CU05](#cu05-unirse-a-partida-privada) |

#### Flujo Normal
1. El Sistema detecta la desconexión del Jugador.
2. El Sistema actualiza la Partida al marcarla como concluida y penaliza al Jugador que haya abandonado (-3 puntos).
3. El Sistema muestra un modal al jugador restante con el mensaje “El otro jugador ha abandonado la partida.” y el botón “Aceptar”.
4. El Jugador restante da clic en “Aceptar”.
5. El Sistema envía al Jugador restante a la pantalla `GUILobby`.
6. Termina el Caso de Uso.

#### Flujo Alterno
*   N/A

#### Flujos de Excepción
*   N/A

---

### CU09: Consultar Historial de Partidas

| Atributo | Detalle |
| :--- | :--- |
| **ID** | CU09 |
| **Nombre** | Consultar Historial de Partidas |
| **Actor** | AC01 Jugador |
| **Descripción** | El Jugador consulta un listado de sus Partidas concluidas. |
| **Disparador** | El Jugador presiona “Historial de Partidas” desde la página `GUIPáginaInicio`. |
| **Precondiciones** | **PRE-01**: El Jugador ha iniciado sesión. |
| **Postcondiciones** | N/A |
| **Reglas de Negocio** | N/A |
| **Relaciones** | N/A |

#### Flujo Normal
1. El Sistema recupera un listado de Partidas pertenecientes al Jugador desde la base de datos. *(Ver EX1)*
2. El Sistema muestra la pantalla `GUIPáginaHistorial` con una tabla con los datos de cada Partida (Nombre de Usuario del Oponente, Resultado, Puntuación obtenida/perdida, Fecha de Juego).
3. Termina el Caso de Uso.

#### Flujo Alterno
*   N/A

#### Flujos de Excepción
*   **EX1: Error al Consultar Historial de Partidas**
    1. El Sistema muestra un modal de error de servidor con el mensaje “No ha sido posible recuperar la información del historial de partidas debido a un error en el sistema. Intente de nuevo más tarde.” y el botón “Aceptar”.
    2. El Jugador presiona el botón “Aceptar”.
    3. El Sistema cierra el modal.

---

### CU10: Consultar Marcadores

| Atributo | Detalle |
| :--- | :--- |
| **ID** | CU10 |
| **Nombre** | Consultar Marcadores |
| **Actor** | AC01 Jugador |
| **Descripción** | El Jugador consulta un listado de los Jugadores con mayor Puntuación. |
| **Disparador** | El Jugador presiona “Marcadores” desde la página `GUIPáginaInicio`. |
| **Precondiciones** | **PRE-01**: El Jugador ha iniciado sesión. |
| **Postcondiciones** | N/A |
| **Reglas de Negocio** | N/A |
| **Relaciones** | N/A |

#### Flujo Normal
1. El Sistema recupera un listado de Jugadores ordenados descendentemente por Puntuación desde la base de datos. *(Ver EX1)*
2. El Sistema muestra la pantalla `GUIPáginaMarcadores` con una tabla con los datos de cada Jugador (Nombre, Nombre de Usuario y Puntuación).
3. Termina el Caso de Uso.

#### Flujo Alterno
*   N/A

#### Flujos de Excepción
*   **EX1: Error al Consultar Marcadores**
    1. El Sistema muestra un modal de error de servidor con el mensaje “No ha sido posible recuperar la información de los marcadores debido a un error en el sistema. Intente de nuevo más tarde.” y el botón “Aceptar”.
    2. El Jugador presiona el botón “Aceptar”.
    3. El Sistema cierra el modal.

---

### CU11: Publicar Partida

| Atributo | Detalle |
| :--- | :--- |
| **ID** | CU11 |
| **Nombre** | Publicar Partida |
| **Actor** | AC02 Anfitrión |
| **Descripción** | El Anfitrión cambia la Privacidad de la Partida activa a PÚBLICA. |
| **Disparador** | El Jugador presiona “Hacer Pública” desde la página `GUIPaginaPartida`. |
| **Precondiciones** | **PRE-01**: El Anfitrión ha iniciado sesión.<br>**PRE-02**: La Partida existe en la base de datos.<br>**PRE-03**: La Partida está registrada como “Privada”. |
| **Postcondiciones** | **POST01**: La Privacidad de la Partida se actualiza a “Pública” y se hace visible en el Lobby. |
| **Reglas de Negocio** | N/A |
| **Relaciones** | N/A |

#### Flujo Normal
1. El Sistema cambia la Privacidad de la Partida a pública en la base de datos. *(Ver EX1)*
2. El Sistema publica la Partida en el Lobby general.
3. Termina el Caso de Uso.

#### Flujo Alterno
*   N/A

#### Flujos de Excepción
*   **EX1: Error al Publicar Partida**
    1. El Sistema muestra un modal de error de servidor con el mensaje “No ha sido posible cambiar la privacidad de la partida debido a un error en el sistema. Intente de nuevo más tarde.” y el botón “Aceptar”.
    2. El Jugador presiona el botón “Aceptar”.
    3. El Sistema cierra el modal.

---

### CU12: Ver Pista

| Atributo | Detalle |
| :--- | :--- |
| **ID** | CU12 |
| **Nombre** | Ver Pista |
| **Actor** | AC02 Adivinador |
| **Descripción** | El Adivinador solicita revelar letras aleatorias de la Palabra como ayuda. |
| **Disparador** | El Adivinador presiona “Ver Pista” en la pantalla de juego. |
| **Precondiciones** | **PRE-01**: El Adivinador ha iniciado sesión.<br>**PRE-02**: La Partida existe en la base de datos.<br>**PRE-03**: La Palabra completa no ha sido revelada.<br>**PRE-04**: El número de pistas usadas es menor a 2. |
| **Postcondiciones** | **POST01**: Se consume una pista disponible y se revela una letra en pantalla. |
| **Reglas de Negocio** | [INF13](#reglas-de-negocio-rn) |
| **Relaciones** | N/A |

#### Flujo Normal
1. El Sistema selecciona una de las letras ocultas de la Palabra y la revela en todos los campos correspondientes.
2. El Sistema actualiza el contador de pistas de la partida en el servidor.
3. Termina el Caso de Uso.

#### Flujo Alterno
*   N/A

#### Flujos de Excepción
*   N/A

---

### CU13: Enviar Mensaje

| Atributo | Detalle |
| :--- | :--- |
| **ID** | CU13 |
| **Nombre** | Enviar Mensaje |
| **Actor** | AC01 Jugador |
| **Descripción** | El Jugador envía un mensaje en el chat en tiempo real de la Partida. |
| **Disparador** | El Jugador introduce un mensaje en el chat y presiona “Enviar” o la tecla “Enter”. |
| **Precondiciones** | **PRE-01**: El Jugador ha iniciado sesión.<br>**PRE-02**: La Partida existe en la base de datos y está activa. |
| **Postcondiciones** | **POST01**: Un mensaje nuevo es registrado y mostrado en el chat de la partida. |
| **Reglas de Negocio** | N/A |
| **Relaciones** | N/A |

#### Flujo Normal
1. El Sistema valida el Mensaje introducido (formato, palabras ofensivas o longitud).
2. El Sistema muestra el Mensaje en el chat de ambos Jugadores. *(Ver FA1)*
3. Termina el Caso de Uso.

#### Flujo Alterno
*   **FA1: Mensaje Inválido**
    1. El Sistema ignora el mensaje y no lo transmite.
    2. Termina el Caso de Uso.

#### Flujos de Excepción
*   N/A

---

### CU14: Cambiar Idioma Preferido

| Atributo | Detalle |
| :--- | :--- |
| **ID** | CU14 |
| **Nombre** | Cambiar Idioma Preferido |
| **Actor** | AC01 Jugador |
| **Descripción** | El Jugador cambia su idioma preferido actualizando la interfaz y su registro. |
| **Disparador** | El Jugador selecciona su Idioma Preferido desde la pantalla `GUIPáginaInicio`. |
| **Precondiciones** | **PRE-01**: El Jugador ha iniciado sesión. |
| **Postcondiciones** | **POST01**: Los elementos textuales del sistema cambian al idioma seleccionado por el Jugador. |
| **Reglas de Negocio** | N/A |
| **Relaciones** | N/A |

#### Flujo Normal
1. El Sistema refresca la pantalla activa modificando el idioma de todos los elementos visuales.
2. El Sistema actualiza el campo `IdiomaPreferido` del Jugador en la base de datos. *(Ver EX1)*
3. Termina el Caso de Uso.

#### Flujo Alterno
*   N/A

#### Flujos de Excepción
*   **EX1: Error al Actualizar Idioma**
    1. El Sistema muestra un modal de error de servidor con el mensaje “No ha sido posible cambiar el idioma debido a un error en el sistema. Intente de nuevo más tarde.” y el botón “Aceptar”.
    2. El Jugador presiona el botón “Aceptar”.
    3. El Sistema cierra el modal.
