using Corrnect.Core;
using UnityEngine;

namespace Corrnect.Units
{
    public abstract class Unit : MonoBehaviour
    {
        public abstract UnitType UnitType { get; }
    }
}
