using UnityEngine;
using System.Collections;

namespace SpawningFramework
{
    public class WaveType : DelayedInstruction
    {
        public const int InfiniteWaves = -1;

        public enum Type { SpawnSequentially = 0, SpawnRandomly, SpawnSimultaneously }
        public Type type;
    }

    public class RestrictedType : DelayedInstruction
    {
        public enum Type { SpawnSequentially = 0, SpawnRandomly }
        public Type type;
    }

    /// <summary>
    /// This is a super type for waves or insstructions that can only be run every so often. 
    /// E.G when you spawn a tough enemy it can be a good idea to not spawn that enemy again for the next few seconds or minutes
    /// </summary>
    public class DelayedInstruction : MonoBehaviour
    {
        public float noRepeatInterval;//how much time has to pass before this wave can be run again

        [HideInInspector]
        public float nextRunTime;//used with re-use to stop certain instructions from appearing too often

    }
}
