using System.Collections.Generic;
using System.Linq;

namespace EXILION.World;

/// <summary>Recuerda los cambios del jugador sin tener que guardar todos los objetos del mundo.</summary>
public sealed class WorldObjectStateStore
{
    private readonly Dictionary<WorldObjectId, WorldObjectState> _states = new();

    public bool TryGet(WorldObjectId id, out WorldObjectState state) => _states.TryGetValue(id, out state);

    internal void Record(WorldObjectState state)
    {
        _states[state.Id] = state;
    }

    /// <summary>Prepara una copia de los cambios para que más adelante se pueda guardar la partida.</summary>
    public IReadOnlyList<WorldObjectState> CaptureSnapshot() => _states.Values.ToList();

    /// <summary>Carga los cambios guardados, incluso los de zonas que todavía no están abiertas.</summary>
    public void RestoreSnapshot(IEnumerable<WorldObjectState> states)
    {
        _states.Clear();
        foreach (var state in states)
            _states[state.Id] = state;
    }
}
