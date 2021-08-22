using UnityEngine;
using System.Collections;
using System;

namespace SpawningFramework
{
    public class SimplifiedInstruction : IComparable
    {
        public float time;//how long to wait before spawning the next instruction
        public EnemyTypes.Type type;//which enemy to spawn
        public int count;//how many times to run this instruction

        public SimplifiedInstruction(EnemyTypes.Type type, float time, int count)
        {
            this.type = type;
            this.time = time;
            this.count = count;

            //Debug.Log(Log"Spawning: " + count + " " + type + " every " + time);
        }

        public int CompareTo(object obj)
        {
            if(obj == null) return 1;

            SimplifiedInstruction otherInstruction = obj as SimplifiedInstruction;
            if(otherInstruction != null)
                return this.time.CompareTo(otherInstruction.time);
            else
                throw new ArgumentException("Object is not a SimplifiedInstruction");
        }
    }
}
