using UnityEngine;

namespace SpawningFramework
{
    public class WaveGroup : RestrictedType
    {
        public Wave[] waves;

        public int wavesToSpawn;//this is how many instructions to run when spawning randomly

        /// <summary>
        /// Used by the editor to give a rough estimate for this waves duration
        /// </summary>
        /// <returns></returns>
        public string RoughDetails()
        {
            #region Times
            float minTotal = 0, maxTotal = 0;

            for(int i = 0; i < waves.Length; i++)
                switch(waves[i].type)
                {
                    case WaveType.Type.SpawnSimultaneously:
                        minTotal += waves[i].totalWaveTime;//this one is dead easy
                        maxTotal += waves[i].totalWaveTime;
                        break;

                    case WaveType.Type.SpawnRandomly:
                    case WaveType.Type.SpawnSequentially:
                        for(int ii = 0; ii < waves[i].spawnInstructions.Length; ii++)
                        {
                            minTotal += waves[i].spawnInstructions[ii].minInstructionDuration - waves[i].spawnInstructions[ii].minDurationDeviation;
                            maxTotal += waves[i].spawnInstructions[ii].maxInstructionDuration - waves[i].spawnInstructions[ii].maxDurationDeviation;
                        }
                        break;
                }
            #endregion


            if(minTotal == maxTotal || maxTotal == 0)
                return "<b>Runs</b>: " + RunTimes() + "     <b>Duration:</b> " + minTotal.ToString("N1");
            else
                return "<b>Runs</b>: " + RunTimes() + "     <b>Duration:</b> " + minTotal.ToString("N1") + " to " + maxTotal.ToString("N1");
        }

        string RunTimes()
        {
            if(wavesToSpawn == 1 || wavesToSpawn == 0)
                return "Once";
            else if(wavesToSpawn == WaveType.InfiniteWaves)
                return "Infinite";
            else
                return wavesToSpawn.ToString();
        }
    }
}
