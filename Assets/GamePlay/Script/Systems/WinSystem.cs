using System.Collections.Generic;
using Corrnect.Swarm;

namespace Corrnect.Systems
{
    public static class WinSystem
    {
        public static bool IsLevelComplete(IReadOnlyList<SwarmGroup> groups)
        {
            return groups.Count == 1;
        }
    }
}
