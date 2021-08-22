using UnityEngine;

namespace SpawningFramework
{
    [System.Serializable]
    public class Wave : WaveType
    {
        public float totalWaveTime, paddingTime;
        public int spawnChance = 1;//only applies for random waves

        [SerializeField]
        public SpawnInstruction[] spawnInstructions;

        public SpawnInstruction[] SpawnInstructions
        {
            get { return spawnInstructions; }
            set { spawnInstructions = value; }
        }

        public Wave(float totalWaveTime, int paddingTime, SpawnInstruction[] instructions)
        {
            this.totalWaveTime = totalWaveTime;
            this.paddingTime = paddingTime;

            this.spawnInstructions = instructions;
        }

        public Wave()
        {
        }
    }
}