# EXILION: contexto para la arquitectura cliente-servidor

## Estado relevado

EXILION es actualmente una aplicación MonoGame de escritorio para .NET 9. `Program.cs` siempre crea `MainGame`, y `EXILION.csproj` genera un ejecutable gráfico único. No hay proyectos de servidor/cliente separados, transporte de red, protocolo, gestión de sesiones ni replicación. El README menciona TCP y UDP como tecnologías objetivo, pero aún no están implementadas.

El funcionamiento es monojugador y local. `GameScene` recibe la entrada, ejecuta las reglas, avanza el mundo, actualiza entidades, dibuja y guarda la partida. El estado del mundo se guarda en JSON con un jugador.

## Responsabilidades que deben separarse

### Arranque y ciclo de vida

- `classes/main/Program.cs` inicia siempre el cliente MonoGame.
- `classes/main/MainGame.cs` crea ventana, GraphicsDevice, cámara, entrada, escenas y audio.
- `classes/scenes/SceneManager.cs` gobierna escenas de presentación.
- Para un servidor dedicado habrá que introducir un punto de entrada sin ventana, GraphicsDevice, SpriteBatch, ContentManager ni audio. Cliente y servidor deberán compartir el modelo de reglas y estado.

### Simulación y acciones de partida

- `classes/scenes/GameScene.cs` todavía coordina actualización de jugador/IA, interacciones, aparición de criaturas, drops, cámara, UI y guardado. El reloj de día/noche se extrajo a `GameClock`.
- `classes/entities/livingThings/player/Player.cs` ya recibe `PlayerInput` en vez de leer `InputManager`, pero aún mezcla estadísticas y reglas con manos, sprites, sonido e inventario.
- `classes/entities/livingThings/Hand.cs` mezcla animación/manipulación visual con temporizadores de ataque, hitboxes, cooldown y consumo.
- `classes/entities/livingThings/LivingThing.cs` une movimiento, colisiones, vida/daño/muerte con efectos visuales y sonido.
- `classes/entities/livingThings/creatures/Creature.cs` y `ICreatureIntelligence` contienen IA y movimiento, pero dependen de tipos MonoGame y de entidades con sprites.
- `GameClock` contiene tiempo restante, día y fase noche como estado lógico, y devuelve si cambió la fase para que el cliente actualice su HUD.
- El servidor deberá recibir intenciones del cliente y validar/aplicar movimiento, ataques, consumo, fabricación, interacción y construcción. No debe confiar en resultados autorreportados por el cliente.

### Mundo procedural

- `classes/world/World.cs` contiene carga de chunks alrededor de una posición, consultas de colisión, objetos, colocación, daño, regeneración y persistencia de estados.
- `ObjectProperties` contiene reglas de mundo; `ObjectVisuals` del cliente resuelve atlas, escala y orden de dibujo por `ObjectType`. Los sonidos de impacto también son adaptados en cliente.
- `ObjectDefinition` y `WorldObject` ya no contienen datos gráficos ni timers de animación; `MapRenderer` guarda feedback temporal por `WorldObjectId`.
- `MapGenerator` y `WorldObjectGenerator` derivan contenido de una semilla; esto permite generar terreno localmente si versión y parámetros coinciden.
- `CreatureSpawnGenerator` usa aleatoriedad local no determinista. Spawns, regrowth y cambios mutables deben tener autoridad única en el servidor.
- El generador recibe ahora una fábrica y un radio físico, aunque el cliente todavía proporciona una fábrica visual acoplada a sprites.
- Con varios jugadores, chunks activos y simulación deberán considerar las posiciones de todos los jugadores conectados.

### Presentación y recursos de cliente

- `GameContext` (`classes/utilities/GameContext.cs`) depende de `MainGame.GraphicsDevice` para escalado.
- `ItemRegistry` contiene definiciones sin texturas; `classes/client/ItemVisuals.cs` resuelve los íconos a partir de IDs estables.
- `Entity`, `LivingThing`, `Player`, `Creature`, `Hand` y `CatchableItem` aún refieren `Sprite`, `Texture2D` o `GameContext`. Audio de living things ahora usa cues semánticos adaptados por el cliente.
- `GameAudioCue` son solicitudes semánticas sin referencias a assets; `GameAudio` del cliente las convierte en efectos concretos.
- Los golpes de mano emiten audio desde ambas manos sin herramienta; al usar herramienta, solo su mano principal produce el cue.
- HUD, crafting UI, paneles, cámara, música, cursor y escenas pertenecen al cliente. Una pausa local no debe detener una sesión compartida.
- Para poder alojar servidor sin gráficos, las reglas/estado deben dejar de necesitar estos recursos y el cliente debe representar ese estado por separado.

### Persistencia

- `classes/saving/WorldSaveData.cs` representa mundo, tiempo, objetos, criaturas e inventario/estadísticas de un solo jugador.
- `WorldSaveService` escribe archivos JSON; `GameScene` decide cuándo cargar/guardar.
- En cooperativo, el servidor debe ser propietario del guardado compartido. El esquema deberá guardar múltiples jugadores, identidad/reconexión y la versión de datos.

## Arquitectura objetivo

Separar gradualmente el repositorio en responsabilidades equivalentes a:

```text
EXILION.Shared  estado, reglas puras, identificadores, comandos y DTO de red
EXILION.Server  sesión, autoridad, simulación, validación, spawns y persistencia
EXILION.Client  MonoGame, entrada local, interfaz, audio y representación
```

El cliente traduce teclado/UI en comandos de intención. El servidor valida comandos y modifica el estado autoritativo. El servidor emite estado inicial, actualizaciones y eventos; el cliente interpola/predice visualmente y corrige con la autoridad. TCP puede servir para negociación/sesión y mensajes confiables; UDP es adecuado para estados frecuentes, con secuencias y tratamiento de pérdida/reordenamiento. Esta elección es futura: no se incorporó red en esta etapa.

## Secuencia sugerida

1. Extraer las mutaciones y reglas de `GameScene` a componentes de lógica explícitos, conservando el modo local.
2. Convertir entrada local en comandos/intenciones independientes de las reglas.
3. Mantener definiciones de ítems con IDs/datos; resolver íconos y demás recursos mediante adaptadores del cliente.
4. Desacoplar entidades y simulación de `GameContext`, sprites, sonidos y gráficos MonoGame.
5. Definir el estado compartido y la sesión multijugador (jugadores, objetivos de IA, desconexión y pausa).
6. Crear servidor dedicado autoritativo y protocolo; luego conectar el cliente y migrar el guardado.

## Viabilidad y principales riesgos

La generación procedural, las reglas existentes de inventario, objetos, criaturas y el guardado JSON son una base reutilizable. El principal costo es el acoplamiento transversal de simulación a entrada y gráficos, y que el formato de guardado modela un jugador. Además, aleatoriedad, temporizadores, ticks, validación de acciones y sincronización de versiones deberán tener autoridad/control explícitos para evitar divergencia o resultados manipulables.

Este documento es una evaluación estática del código, no una implementación de red ni un protocolo definitivo.
