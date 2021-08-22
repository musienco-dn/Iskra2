using UnityEngine;
using SpawningFramework;

namespace SpawningFramework
{
    /// <summary>
    /// A simple example to control if a player is performing better or worse.
    /// 
    /// Take a twin stick shooter as an example. If the player is killing enemies very quickly then you want to spawn more enemies then usual to keep them challenged.
    /// Call this class when you have detected if the player is performing well or poorly and the waves (if dynamic) will compensate 
    /// </summary>
    public class PlayerSkill : MonoBehaviour
    {
        #region Variables
        [HideInInspector] 
        public int skillValue;

        [HideInInspector]
        public float percentageSkill;

        /// <summary>
        /// This is basically a scale to see how many levels of skill we track. 
        /// If this is a low number then you will see wild jumps. 
        /// Higher numbers will give a smoother increase but will require a more complex system to detect if a player is doing well.
        /// </summary>
        public int maxSkillValue;

        bool hadChance;
        #endregion

        #region Methods
        /// <summary>
        /// Called when the player is doing well. E.G not taken damage for X seconds
        /// </summary>
        public void IncreaseSkill()
        {
            if(skillValue < maxSkillValue)
                skillValue++;

            percentageSkill = K2Maths.Percentage((float)skillValue, maxSkillValue);
        }

        /// <summary>
        /// Called when the player is struggling. E.G taking damage
        /// </summary>
        public void DecreaseSkill()
        {
            if(skillValue > 0)
            {
                if(!hadChance)//give players room to make one mistake before reducing their skill
                    hadChance = true;
                else
                    skillValue--;
            }
        }
        #endregion
    }
}