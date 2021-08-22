using UnityEngine;

namespace SpawningFramework
{
    public static class K2Maths
    {
        /// <summary>
        /// Returns the value that is the defined percentage between both values.
        /// </summary>
        public static float Lerp(float min, float max, float percentageBetween)
        {
            float differenceSign = (min - max > 0 ? -1 : 1);//if the difference is negative then subtract the value at the end!

            float value = min + (Mathf.Abs(min - max) * percentageBetween * differenceSign);

            //Debug.Log("Min: " + min + " Max: " + max + " value: " + value + " Percentage: " + percentageBetween);

            return value;
        }

        /// <summary>
        /// Deivates a given value by a random range. E.G if the base is 1 and the deviation is 0.5 then this returns a random number between 0.5 and 1.5
        /// </summary>
        public static float Deviate(float baseValue, float deviation)
        {
            return Random.Range(baseValue - deviation, baseValue + deviation);
        }

        /// <summary>
        /// returns the percentage of a given range
        /// </summary>
        /// <param name="value">Current value</param>
        /// <param name="maxValue">Max value</param>
        /// <returns></returns>
        public static float Percentage(float value, float maxValue)
        {
            return value / maxValue;
        }
    }
}