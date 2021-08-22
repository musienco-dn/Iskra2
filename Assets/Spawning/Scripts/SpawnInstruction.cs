using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpawningFramework
{
    public class SpawnInstruction : DelayedInstruction
    {
        #region Variables
        public EnemyTypes.Type type;//what to spawn

        /// <summary>
        /// These variables define the range of time an instruction can run for in relation to the players skill. E.G if the player is doing well spend more time spawning the harder enemies
        /// </summary>
        public float minInstructionDuration, maxInstructionDuration;

        /// <summary>
        /// These determine the devitation at the given players skill level. The min also works for sequential instructions.
        /// E.G if the min duration is 5 and the min deviation is 1 then this instruction will run for anywhere between 4 and 6 seconds
        /// </summary>
        public float minDurationDeviation, maxDurationDeviation;

        /// <summary>
        /// This is the chance of this instruction being selected to run. Only applicable when spawning randomly. Useful to determine which waves to spawn as the player progresses.
        /// E.G if you have a rare enemy with a 1% min chance and a 50% max chance then as the player reaches max skill there is a 50% chance it will appear
        /// </summary>
        public LinkedPercentage minSpawnPercentage, maxSpawnPercentage;

        /// <summary>
        /// How often to spawn an enemy in seconds.
        /// E.G if the min interval is 1 and the max is 0.25 then at max skill 4X as many enemies will spawn 
        /// </summary>
        public float minSpawnInterval, maxSpawnInterval;

        public float minSpawnIntervalDeviation, maxSpawnIntervalDeviation;

        /// <summary>
        /// This custom value can be read by individual enemies and used externally to tweak for individual use.
        /// E.G an enemy has a 50% chance to fire a stun attack instead of a normal one. That chance can then be multipled by the value below. For one wave the value will be 1 (100%). Meaning the enemy still has a 50% chance to stun.
        /// However on a wave with twice as many enemies the chance of getting stunned could be too frustrating. So for that particular wave it could be a good idea to reduce the difficulty percentage to 0.5 (50%).
        /// Thus the chance of being able to stun on that wave becomes the original 50% multiplied by the 50% from this value which equals a 25% chance to stun, thus balancing for the extra enemies.
        /// 
        /// This has far simpler implementations as well. For example enemies on one wave can use this value to have twice as much armour as the same enemies on another wave.
        /// See Enemy.cs for an example implementation
        /// </summary>
        public float minCustomValue = 1, maxCustomValue = 1;

        public float minCustomValueDeviation, maxCustomValueDeviation;
        #endregion
    }
}