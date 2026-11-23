# Registro de cambios para la refactorización de red

## Etapa 1: extracción inicial de lógica

- Añadido `PlayerInput` para que la escena convierta controles locales en intenciones del jugador; `Player` ya no consulta `InputManager` ni `Keys`.
- Extraídas a `GameWorldLogic` reglas de colocación de estructuras, recolección, ataques y daño de contacto.
- La escena conserva el orden local de actualización y presentación.
- Sin sockets, protocolo ni cambios de guardado.

## Etapa 2: datos de ítems sin recursos gráficos

- `Item`, `Tool`, `Consumable` y `Structure` ya no almacenan ni reciben `Texture2D` o rectángulos de atlas.
- `ItemRegistry` queda con datos del juego y las IDs estables.
- Añadido `ItemVisuals` del lado cliente para resolver texturas por ID.
- UI, mano, drops y renderizado de estructuras usan el adaptador visual.
- Sin cambios de IDs, atributos, recetas ni red.

## Etapa 3: separación inicial de audio de objetos

- `ObjectDefinition` y `ObjectProperties` ya no cargan ni guardan `SoundEffect`.
- `World` emite `ObjectDamaged(ObjectType)`; `ObjectAudio` del cliente convierte el ID del objeto en el audio correspondiente.
- `CreatureSpawnGenerator` recibe el radio de colisión escalado y deja de depender directamente de `GameContext`.
- El merge de `origin/develop` quedó integrado en `b2a413e`. Se conservaron estadísticas/modificadores de armadura y su persistencia por ID; el renderizado de armadura también usa `ItemVisuals`.
- No se agregó transporte de red.

## Etapa 4: avance inicial en el límite de criaturas

- `CreatureSpawnGenerator` recibe una fábrica por delegate y el radio de colisión ya resuelto; deja de construir criaturas directamente y de depender de `GameContext`.
- El cliente sigue proporcionando una fábrica que construye `Creature` con sprite y `GameContext`. La separación de entidad y renderer queda pendiente; living things todavía no está listo para servidor.

## Etapa 5: reloj de partida

- Extraído el estado y avance del ciclo día/noche a `GameClock`, sin UI, música, graphics device ni `GameContext`.
- `GameScene` mantiene la sincronización de HUD y guarda los valores actuales del reloj.

## Etapa 6: audio semántico y catálogo visual de objetos

- Las entidades de vida y `Hand` emiten `GameAudioCue`; ya no llaman directamente a `SFX` ni acceden al catálogo de sonidos.
- `GameAudio` resuelve esos cues a assets y los reproduce en el cliente; la escena conecta los eventos de jugador y criaturas.
- `ObjectDefinition`/`ObjectProperties` ahora contienen reglas de gameplay. Atlas, orden de dibujo, tamaño y metadatos del rango de oxígeno viven en `ObjectVisuals`, indexado por `ObjectType`.
- `MapRenderer` convierte tipos de objeto a datos visuales sin contaminar el catálogo de reglas.
- La animación de impacto de objetos fue retirada de `WorldObject` y ahora vive en `MapRenderer`, keyed por `WorldObjectId`; `World` emite el ID y tipo como evento de gameplay para que el cliente resuelva sonido y feedback visual.
- Corregido el acceso a `SoundRequested`: las subclases ahora llaman al helper protegido `RequestSound`, respetando las reglas de eventos de C#; `Player` usa el helper para pickup/consumo y reenvía los cues de `Hand`.
- El jugador también reenvía los golpes de la mano izquierda cuando no tiene una herramienta equipada; los ataques de apoyo de herramientas no generan un segundo sonido.

## Estado de pendientes tras la etapa 6

- **Living things no queda completamente desacoplado:** `Entity`, `LivingThing`, `Player`, `Creature` y `Hand` todavía usan `Sprite`, `GameContext`, gráficos de MonoGame y/o lógica de presentación. Su audio sí fue extraído.
- `CreatureFactory` todavía construye entidades con sprites; aunque `CreatureSpawnGenerator` usa una fábrica inyectada, el cliente sigue siendo el único factory disponible.
- El timer/escala de impacto de `WorldObject` ya está en el cliente.
- `CatchableItem` continúa combinando stack, hitbox y vida útil con pulso visual y `Entity` renderizable.
- `GameScene` continúa siendo el coordinador local de simulación, persistencia e interacciones; no hay aún una sesión servidor ni varios jugadores.
- El núcleo usa tipos matemáticos de `Microsoft.Xna.Framework`; para un servidor verdaderamente sin MonoGame habría que migrar a tipos compartidos propios o a un assembly común sin framework gráfico.

## Dependencias pendientes de extraer

- `Entity`, `LivingThing`, `Player`, `Creature` y `Hand` aún combinan estado/reglas con sprites, `GameContext`, escalado y gráficos; la resolución de audio ya es cliente.
- `CatchableItem` aún hereda de `Entity` renderizable y su pulso visual comparte clase con vida útil/recogida.
- El temporizador/escala de impacto de `WorldObject` está en `MapRenderer` del cliente.
- `GameScene` todavía coordina IA, spawns, drops, persistencia e interacciones; el reloj ahora es `GameClock`, pero la escena sigue adaptando el estado al HUD.
- `World` y lógica de entidades aún usan tipos de `Microsoft.Xna.Framework`; el servidor dedicado sin MonoGame no está habilitado.

## Verificación

- Revisión estática del diff, conflictos de merge y referencias a assets. No se ejecutaron compilación ni pruebas por pedido del usuario.

## Etapa 7: estado de drops y aleatoriedad inyectable

- Extraido CatchableItemState con stack, recogida y vida util; CatchableItem conserva la adaptacion a hitbox/renderizado.
- Extraido el pulso visual del drop a CatchableItemPulse en cliente.
- CreatureSpawnGenerator acepta una fuente Random inyectada para controlar su aleatoriedad desde el coordinador.
- Sigue pendiente desacoplar entidades y mundo de Microsoft.Xna.Framework; la inyeccion de RNG no hace determinista por si sola la simulacion.

## Etapa 8: entidades sin presentacion

- `Entity`, `LivingThing`, `Player`, `Creature`, `Hand` y `CatchableItem` ya no guardan sprites, texturas, `SpriteBatch`, `GameContext` ni `GameTime`.
- Sus actualizaciones reciben segundos transcurridos y exponen estado de simulacion para el cliente: posicion, orientacion, manos, item equipado, hitboxes y feedback de dano.
- `EntityRenderer` es el adaptador MonoGame del cliente: resuelve sprites desde `Assets` e `ItemVisuals`, aplica pulso de drops y dibuja feedback e hitboxes.
- `GameScene` adapta `GameTime` a segundos y construye entidades con hitboxes escaladas. `CreatureFactory` recibe esas dimensiones sin usar `GameContext` ni assets.
- La simulacion aun usa `Microsoft.Xna.Framework` para geometria. Esa migracion sigue siendo el pendiente separado para un servidor sin MonoGame.

## Etapa 9: auditoria de preparacion del servidor

- Se verifico el estado actual contra el codigo: sigue existiendo un unico
  proyecto `WinExe` de MonoGame, sin entry point headless ni transporte.
- Se confirmo que `GameScene` continua siendo el coordinador de simulacion,
  persistencia, IA, spawns e interacciones.
- Se confirmo que `WorldSaveData` representa un unico jugador y que el nucleo
  aun depende de tipos geometricos de `Microsoft.Xna.Framework`.
- Se agrego `SERVER_READINESS.md` con el veredicto, los bloqueos, el orden de
  trabajo y el checklist minimo para comenzar el servidor.
- La compilacion actual tambien tiene un problema independiente en
  `FontDescriptionProcessor` al procesar las tres fuentes `.spritefont`.

Conclusión: todavia queda refactorizacion obligatoria antes del servidor
dedicado. El siguiente cambio de modelo recomendado es extraer `Shared` y
`Server`, comenzando por la geometria y `GameSimulation`; el transporte de red
puede venir despues.

## Etapa 10: limite de dependencias compilable

- Agregada la solucion `EXILION.sln` con tres proyectos: cliente MonoGame,
  `EXILION.Shared` y `EXILION.Server`.
- `EXILION.Shared` no referencia MonoGame e incluye los contratos y datos que
  ya eran independientes: catalogos de items, datos/estados de mundo, guardado,
  `GameClock`, `PlayerInput`, `GameAudioCue` y datos de estadisticas.
- El cliente referencia `EXILION.Shared` y deja de compilar esas fuentes de
  forma local, evitando tipos duplicados.
- `EXILION.Server` es un ejecutable headless que solo referencia `Shared`; no
  incluye red, simulacion ni assets. Es el limite de dependencias para la
  proxima etapa, no una implementacion de servidor.
- Verificado con `dotnet build EXILION.Shared.csproj --no-restore` y
  `dotnet build EXILION.Server/EXILION.Server.csproj --no-restore`.
- La ejecucion del binario de servidor no pudo verificarse en este entorno
  porque falta el runtime `Microsoft.NETCore.App 9.0`; las compilaciones usan
  el SDK instalado.

