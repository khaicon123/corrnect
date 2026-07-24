using System.Collections.Generic;
using System.Linq;
using Corrnect.Swarm;
using Corrnect.Grid;

namespace Corrnect.Systems
{
    public static class WinSystem
    {
        public static bool IsLevelComplete(IReadOnlyList<SwarmGroup> groups)
        {
            if (groups == null)
                return false;

            return groups.Count(group => !HazardSystem.IsHazard(group)) == 1;
        }
    }
}
