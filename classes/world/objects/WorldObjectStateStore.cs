using System.Collections.Generic;
using System.Linq;

namespace EXILION.World;

public sealed class WorldObjectStateStore
{
    private readonly Dictionary<WorldObjectId, WorldObjectState> _states = new();

    public bool TryGet(WorldObjectId id, out WorldObjectState state) => _states.TryGetValue(id, out state);

    internal void Record(WorldObjectState state)
    {
        _states[state.Id] = state;
    }

    public IReadOnlyList<WorldObjectState> CaptureSnapshot() => _states.Values.ToList();

    public void RestoreSnapshot(IEnumerable<WorldObjectState> states)
    {
        _states.Clear();
        foreach (var state in states)
            _states[state.Id] = state;
    }
}
