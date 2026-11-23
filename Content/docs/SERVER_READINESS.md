# EXILION: estado para comenzar el servidor

Fecha de revision: 2026-11-22

## Veredicto

El repositorio todavia no esta listo para implementar un servidor dedicado
autoritativo. Ya existe una separacion compilable de `Shared`, cliente y un
entry point headless, pero quedan dependencias estructurales que impedirian
compilar y ejecutar toda la simulacion sin MonoGame.

Se puede comenzar el desarrollo del servidor cuando se complete el bloque
minimo de preparacion indicado mas abajo. No hace falta terminar primero el
transporte TCP/UDP para empezar: el primer objetivo debe ser un servidor local
headless con un tick determinista y una API de comandos.

## Evidencia del estado actual

- `EXILION.sln` separa `EXILION.Shared`, el cliente existente (`EXILION.csproj`)
  y `EXILION.Server`.
- `EXILION.Shared` compila sin paquetes MonoGame y contiene contratos, catalogos
  de items, datos de mundo, guardado y logica ya independiente.
- `EXILION.Server` es un entry point headless sin dependencias de graficos,
  audio ni red. Aun no tiene simulacion ni transporte.
- `classes/main/Program.cs` siempre crea `MainGame` y llama a `Run()`.
- `MainGame` crea `GraphicsDeviceManager`, `SpriteBatch`, camara, audio,
  escenas y recursos de cliente.
- `Entity`, `LivingThing`, `Player`, `Creature`, `Hand`, `World` y las
  inteligencias usan tipos de `Microsoft.Xna.Framework` para posiciones,
  hitboxes y geometria.
- `GameScene` sigue coordinando entrada, simulacion, IA, spawns, drops,
  persistencia, HUD y presentacion.
- `Creature.Update` recibe una unica posicion de jugador y el mundo actualiza
  chunks/regrowth alrededor de una unica posicion.
- `WorldSaveData` tiene un unico `Player`; no hay identidad de jugador,
  reconexion ni coleccion de jugadores.
- No existen comandos de red, DTOs versionados, sesiones, transporte,
  snapshots, secuencias ni confirmaciones de acciones.
- La aleatoriedad de spawns y regrowth no esta centralizada en un reloj/servicio
  de simulacion autoritativo.

## Bloqueos que deben resolverse antes del servidor

### 1. Completar la separacion de proyectos y dependencias

Crear como minimo:

```text
EXILION.Shared  estado, reglas, identificadores, comandos y mensajes
EXILION.Server  proceso headless, sesiones, tick y persistencia
EXILION.Client  MonoGame, entrada, UI, audio y renderizado
```

La estructura de proyectos ya fue creada. Falta migrar el resto de la
simulacion a `Shared`; el servidor no debe referenciar `MonoGame.Framework` ni
`MonoGame.Content.Builder.Task`.

### 2. Migrar la geometria del nucleo

Extraer a `Shared` tipos propios o una libreria sin framework grafico para:

- vector 2D;
- punto, rectangulo y hitbox;
- angulos y operaciones de distancia/interseccion;
- reloj/tick y delta de simulacion.

Luego reemplazar los usos de `Vector2`, `Point`, `Rectangle` y
`MathHelper` dentro de la simulacion. MonoGame puede tener adaptadores en el
cliente para convertir esos valores al renderizado.

### 3. Extraer la simulacion de `GameScene`

Crear un coordinador de servidor, por ejemplo `GameSimulation`, responsable
de aplicar un tick a:

- jugadores conectados;
- movimiento y colisiones;
- ataques, dano y consumo;
- crafting, interaccion y construccion;
- IA, drops, spawns y regrowth;
- reloj de dia/noche;
- eventos de gameplay y cambios de estado.

`GameScene` debe quedar como adaptador de entrada/renderizado del cliente.
Toda accion recibida debe llegar como intencion y ser validada por la
simulacion, sin aceptar posiciones, dano o inventarios calculados por el
cliente.

### 4. Hacer multi-jugador el estado y el guardado

Cambiar el modelo de guardado para incluir:

- lista de jugadores por `PlayerId`;
- nombre/identidad y estado de reconexion;
- posicion, inventario, estadisticas y equipamiento por jugador;
- version de esquema y migraciones;
- seed, reloj, objetos mutados, criaturas y drops del mundo compartido.

Tambien hay que eliminar las APIs que reciben una unica posicion de jugador
cuando la decision depende de todos los jugadores activos. El servidor debe
determinar que chunks simula y que entidades replica para cada cliente.

### 5. Definir contrato de red minimo

Antes de conectar el cliente hay que documentar y probar:

- handshake, version de protocolo y capacidades;
- crear/unirse/salir/reconectar una sesion;
- `PlayerId`, tick, secuencia y orden de mensajes;
- comandos de intencion del cliente;
- snapshot inicial, deltas y eventos confiables;
- limites, validacion y respuesta de errores;
- politica de perdida, duplicacion y reordenamiento.

La eleccion TCP/UDP puede quedar encapsulada detras de interfaces. No debe
mezclarse el socket con las reglas del juego.

## Orden recomendado de implementacion

1. Crear `Shared` y `Server` y hacer que ambos compilen sin MonoGame.
2. Extraer geometria y convertir `Entity`/`World`/entidades a tipos compartidos.
3. Implementar `GameSimulation` con un tick fijo y RNG inyectado/sembrado.
4. Agregar comandos locales y validacion autoritativa sin sockets.
5. Convertir el guardado a multiples jugadores y agregar pruebas de round-trip.
6. Implementar sesiones y snapshots en memoria.
7. Incorporar transporte y conectar un cliente local.
8. Recién despues agregar prediccion/interpolacion, compresion y optimizaciones.

## Criterio de entrada al desarrollo del servidor

El equipo puede cambiar al modelo de servidor cuando se cumpla este checklist:

- [ ] `dotnet build` compila `Shared` y `Server` sin procesar contenido de MonoGame.
- [ ] Existe un proceso headless que inicia un mundo desde una seed.
- [ ] Un tick puede avanzar sin ventana, `GraphicsDevice`, audio ni input local.
- [ ] Dos jugadores pueden existir en el mismo estado de mundo en memoria.
- [ ] Movimiento, ataque, consumo y construccion se validan en el servidor.
- [ ] El estado puede serializarse y restaurarse sin perder jugadores ni mutaciones.
- [ ] Hay al menos una prueba de simulacion y una prueba de persistencia.
- [ ] El contrato inicial de comandos/snapshots esta versionado.

## Verificacion realizada

Se inspeccionaron `EXILION.csproj`, `Program.cs`, `MainGame`, `GameScene`,
`World`, entidades, generadores y modelos de guardado. Tambien se ejecuto:

```text
dotnet build --no-restore
```

La compilacion no llega a validar todo el codigo porque el pipeline de
contenido falla en `FontDescriptionProcessor` para las tres fuentes
`.spritefont` (`PixelArt`, `PixelArtBig` y `PixelArtSmall`) con una
`NullReferenceException`. Es un bloqueo del build actual del cliente, no una
implementacion faltante del servidor, y debe corregirse o aislarse al separar
los proyectos.

## Relacion con la documentacion previa

`NETWORK_CONTEXT.md` y `NETWORK_CHANGES.md` siguen siendo validos como historial
de la refactorizacion. Este documento agrega el criterio de salida: las etapas
de renderizado, audio, drops y entidades ya realizadas no alcanzan por si solas
para iniciar un servidor dedicado.
