// Copyright © Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEngine.Events;
using System;

namespace PixelCrushers.EmeraldAISupport
{

    public delegate void EmeraldAIDelegate(EmeraldAI.EmeraldAISystem emerald_AI);

    [Serializable]
    public class EmeraldAIEvent : UnityEvent<EmeraldAI.EmeraldAISystem> { }

    [Serializable]
    public class EmeraldPlayerHealthEvent : UnityEvent<EmeraldAI.Example.EmeraldAIPlayerHealth> { }

    /// <summary>
    /// Utility functions for working with Emerald AI.
    /// </summary>
    public static class EmeraldAIUtility
    {

        /// <summary>
        /// Applies a delegate function to all active Emerald AIs who belong to the specified faction.
        /// </summary>
        public static void ApplyToFaction(int faction, EmeraldAIDelegate delegateFunction)
        {
            var all = GameObject.FindObjectsOfType<EmeraldAI.EmeraldAISystem>();
            for (int i = 0;  i < all.Length; i++)
            {
                var ai = all[i];
                if (ai.enabled && ai.CurrentFaction == faction)
                {
                    delegateFunction(ai);
                }
            }
        }

        /// <summary>
        /// Updates or adds a faction relation to an Emerald AI.
        /// </summary>
        /// <param name="ai">The AI to change.</param>
        /// <param name="targetFactionID">The ID of the faction that this AI has a relation to.</param>
        /// <param name="relationID">The int value of an EmeraldAI.EmeraldAISystem.RelationType.</param>
        public static void SetFactionRelation(EmeraldAI.EmeraldAISystem ai, int targetFactionID, int relationID)
        {
            var index = -1;
            // Check existing factions list:
            for (int i = 0; i < ai.AIFactionsList.Count; i++)
            {
                if (ai.AIFactionsList[i] == targetFactionID)
                {
                    ai.FactionRelations[i] = relationID;
                    index = i;
                    break;
                }
            }
            if (index == -1)
            {
                // If not already in list, add it:
                index = ai.AIFactionsList.Count;
                ai.AIFactionsList.Add(targetFactionID);
                ai.FactionRelations.Add(relationID);
            }
            // Update inspector variables, too: (No longer necessary in Emerald AI 2.4.)
            //if (index == 0) { ai.OpposingFaction1 = targetFactionID; ai.FactionRelation1 = relationID; }
            //if (index == 1) { ai.OpposingFaction2 = targetFactionID; ai.FactionRelation2 = relationID; }
            //if (index == 2) { ai.OpposingFaction3 = targetFactionID; ai.FactionRelation3 = relationID; }
            //if (index == 3) { ai.OpposingFaction4 = targetFactionID; ai.FactionRelation4 = relationID; }
            //if (index == 4) { ai.OpposingFaction5 = targetFactionID; ai.FactionRelation5 = relationID; }
            //ai.OpposingFactionsEnumRef = (EmeraldAI.EmeraldAISystem.OpposingFactionsEnum)(Mathf.Clamp(ai.AIFactionsList.Count - 1, 0, 4));
        }

    }
}
