using UnityEngine;
using System.Collections;

namespace SpawningFramework
{
    public class EnemyTypes
    {
        public enum Type
        {
            None = 0,//Dont' delete this!

            BlueAsteroid,//replace these with your enemy types! If you are remving this in favour of an existing enum don't forget to update SpawnInstructionEditor.cs
            BlueAsteroid2,
            BigBlueAsteroid,
            BrownAsteroid,
            ToEditMe,
            FindTheFileNamed,
            EnemyTypes,

            BigBrownAsteroid,

            Wait,//These are needed for the framework
            Pause,
            Last
        }
    }
}