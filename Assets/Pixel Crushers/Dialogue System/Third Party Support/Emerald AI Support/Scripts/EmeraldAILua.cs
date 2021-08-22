// Copyright © Pixel Crushers. All rights reserved.

using System.Collections.Generic;
using UnityEngine;

namespace PixelCrushers.DialogueSystem.EmeraldAISupport
{

    /// <summary>
    /// Adds Lua functions to interface with Emerald AI.
    /// </summary>
    [AddComponentMenu("Pixel Crushers/Dialogue System/Third Party/Emerald AI/Emerald AI Lua")]
    public class EmeraldAILua : MonoBehaviour
    {

        private delegate void EmeraldAIDelegate(EmeraldAI.EmeraldAISystem emerald_AI, object[] args);

        private static List<string> m_factionNameList = null;
        public static List<string> factionNameList
        {
            get
            {
                if (m_factionNameList == null)
                {
                    var factionData = Resources.Load("EmeraldAIFactions") as TextAsset;
                    if (factionData != null)
                    {
                        m_factionNameList = new List<string>(factionData.text.Split(','));
                    }
                    else
                    {
                        m_factionNameList = new List<string>();
                    }
                }
                return m_factionNameList;
            }
        }

        protected static bool areFunctionsRegistered = false;

        private bool didIRegisterFunctions = false;

        void OnEnable()
        {
            if (areFunctionsRegistered)
            {
                didIRegisterFunctions = false;
            }
            else
            {
                // Make the functions available to Lua:
                didIRegisterFunctions = true;
                areFunctionsRegistered = true;
                Lua.RegisterFunction("GetEmeraldAIHealth", this, SymbolExtensions.GetMethodInfo(() => GetEmeraldAIHealth(string.Empty)));
                Lua.RegisterFunction("DamageEmeraldAI", this, SymbolExtensions.GetMethodInfo(() => DamageEmeraldAI(string.Empty, (double)0, string.Empty)));
                Lua.RegisterFunction("EmeraldAIEmote", this, SymbolExtensions.GetMethodInfo(() => EmeraldAIEmote(string.Empty, (double)0, (double)0)));
                Lua.RegisterFunction("EmeraldAIFollow", this, SymbolExtensions.GetMethodInfo(() => EmeraldAIFollow(string.Empty, string.Empty, string.Empty)));
                Lua.RegisterFunction("EmeraldAIStay", this, SymbolExtensions.GetMethodInfo(() => EmeraldAIStay(string.Empty)));
                Lua.RegisterFunction("EmeraldAIAttacksPlayer", this, SymbolExtensions.GetMethodInfo(() => EmeraldAIAttacksPlayer(string.Empty, false)));
                Lua.RegisterFunction("SetEmeraldPlayerRelation", this, SymbolExtensions.GetMethodInfo(() => SetEmeraldPlayerRelation(string.Empty, string.Empty)));
                Lua.RegisterFunction("SetEmeraldFactionRelation", this, SymbolExtensions.GetMethodInfo(() => SetEmeraldFactionRelation(string.Empty, string.Empty, string.Empty)));
                Lua.RegisterFunction("SetEmeraldBehavior", this, SymbolExtensions.GetMethodInfo(() => SetEmeraldBehavior(string.Empty, string.Empty)));
                Lua.RegisterFunction("SetEmeraldConfidence", this, SymbolExtensions.GetMethodInfo(() => SetEmeraldConfidence(string.Empty, string.Empty)));
                Lua.RegisterFunction("SetEmeraldDestination", this, SymbolExtensions.GetMethodInfo(() => SetEmeraldDestination(string.Empty, string.Empty)));
                Lua.RegisterFunction("SetEmeraldItem", this, SymbolExtensions.GetMethodInfo(() => SetEmeraldItem(string.Empty, (double)0, false)));
                Lua.RegisterFunction("SetEmeraldWander", this, SymbolExtensions.GetMethodInfo(() => SetEmeraldWander(string.Empty, string.Empty)));
                Lua.RegisterFunction("SetEmeraldWeapon", this, SymbolExtensions.GetMethodInfo(() => SetEmeraldWeapon(string.Empty, string.Empty, false)));
                //Lua.RegisterFunction("SetEmeraldTargetTags", this, SymbolExtensions.GetMethodInfo(() => SetEmeraldTargetTags(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty)));
                //--- Will add SetEmeraldTargetTags only if requested.
            }
        }

        void OnDisable()
        {
            if (didIRegisterFunctions)
            {
                // Remove the functions from Lua:
                didIRegisterFunctions = false;
                areFunctionsRegistered = false;
                Lua.UnregisterFunction("GetEmeraldAIHealth");
                Lua.UnregisterFunction("DamageEmeraldAI");
                Lua.UnregisterFunction("EmeraldAIEmote");
                Lua.UnregisterFunction("EmeraldAIFollow");
                Lua.UnregisterFunction("EmeraldAIStay");
                Lua.UnregisterFunction("EmeraldAIAttacksPlayer");
                Lua.UnregisterFunction("SetEmeraldPlayerRelation");
                Lua.UnregisterFunction("SetEmeraldFactionRelation");
                Lua.UnregisterFunction("SetEmeraldBehavior");
                Lua.UnregisterFunction("SetEmeraldConfidence");
                Lua.UnregisterFunction("SetEmeraldDestination");
                Lua.UnregisterFunction("SetEmeraldItem");
                Lua.UnregisterFunction("SetEmeraldWander");
                Lua.UnregisterFunction("SetEmeraldWeapon");
                //Lua.UnregisterFunction("SetEmeraldTargetTags");
            }
        }

        public double GetEmeraldAIHealth(string subjectOrFaction)
        {
            var ais = FindObjectsOfType<EmeraldAI.EmeraldAISystem>();
            foreach (var ai in ais)
            {
                if (ai.name == subjectOrFaction) return ai.CurrentHealth;
            }
            var factionID = GetFactionID(subjectOrFaction);
            foreach (var ai in ais)
            {
                if (ai.CurrentFaction == factionID) return ai.CurrentHealth;
            }
            return 0;
        }

        public void DamageEmeraldAI(string faction, double amount, string targetType)
        {
            ApplyToFaction(faction, Damage, new object[] { (int)amount, targetType });
        }

        private void Damage(EmeraldAI.EmeraldAISystem ai, params object[] args)
        {
            if (ai == null) return;
            var damageAmount = (int)args[0];
            var targetType = (string)args[1];
            if (DialogueDebug.LogInfo) Debug.Log("Dialogue System: Emerald AI " + ai.name + " taking " + damageAmount + " damage from " + targetType + ".", ai);
            ai.Damage(damageAmount, GetTargetType(targetType));
        }

        public void EmeraldAIEmote(string faction, double audioID, double animationID)
        {
            ApplyToFaction(faction, Emote, new object[] { (int)audioID, (int)animationID });
        }

        private void Emote(EmeraldAI.EmeraldAISystem ai, params object[] args)
        {
            if (ai == null) return;
            var audioID = (int)args[0];
            var animationID = (int)args[1];
            var playSound = audioID >= 0;
            var playAnimation = animationID >= 0;
            if (DialogueDebug.LogInfo)
            {
                if (playSound && playAnimation) Debug.Log("Dialogue System: Emerald AI " + ai.name + " playing sound " + audioID + " and animation " + animationID, ai);
                else if (playSound) Debug.Log("Dialogue System: Emerald AI " + ai.name + " playing sound " + audioID, ai);
                else if (playAnimation) Debug.Log("Dialogue System: Emerald AI " + ai.name + " playing animation " + animationID, ai);
            }            
            if (playSound) ai.EmeraldEventsManagerComponent.PlaySoundEffect(audioID);
            if (playAnimation) ai.EmeraldEventsManagerComponent.PlayEmoteAnimation(animationID);
        }

        public void EmeraldAIFollow(string faction, string target, string followAs)
        {
            ApplyToFaction(faction, Follow, new object[] { target, followAs });
        }

        private void Follow(EmeraldAI.EmeraldAISystem ai, object[] args)
        {
            if (ai == null) return;
            var target = (string)args[0];
            var followAs = (string)args[1];
            var go = GameObject.Find(target);
            if (go == null)
            {
                if (DialogueDebug.LogWarnings) Debug.Log("Dialogue System: Emerald AI " + ai.name + " can't find " + target + " to follow.", ai);
                return;
            }
            if (DialogueDebug.LogInfo) Debug.Log("Dialogue System: Emerald AI " + ai.name + " following " + go + " as " + followAs + ".", ai);
            ai.BehaviorRef = (string.Equals(followAs, "companion", System.StringComparison.OrdinalIgnoreCase)) ? EmeraldAI.EmeraldAISystem.CurrentBehavior.Companion : EmeraldAI.EmeraldAISystem.CurrentBehavior.Pet;
            ai.EmeraldEventsManagerComponent.SetFollowerTarget(go.transform);
            ai.EmeraldEventsManagerComponent.ResumeFollowing();
        }

        public void EmeraldAIStay(string faction)
        {
            ApplyToFaction(faction, Stay, new object[] { });
        }

        private void Stay(EmeraldAI.EmeraldAISystem ai, object[] args)
        {
            if (ai == null) return;
            if (DialogueDebug.LogInfo) Debug.Log("Dialogue System: Emerald AI " + ai.name + " staying.", ai);
            ai.EmeraldEventsManagerComponent.StopFollowing();
        }

        public void EmeraldAIAttacksPlayer(string faction, bool value)
        {
            Debug.LogWarning("Dialogue System: Lua function AIAttacksPlayer() was replaced by SetEmeraldPlayerRelation() for Emerald AI 2.4.");
            //ApplyToFaction(faction, AIAttacksPlayer, new object[] { value });
        }

        //private void AIAttacksPlayer(EmeraldAI.EmeraldAISystem ai, object[] args)
        //{
        //    if (ai == null) return;
        //    var value = (bool)args[0];
        //    var enumValue = (value == true) ? EmeraldAI.EmeraldAISystem.AIAttacksPlayer.Always : EmeraldAI.EmeraldAISystem.AIAttacksPlayer.Never;
        //    if (DialogueDebug.LogInfo) Debug.Log("Dialogue System: Emerald AI " + ai.name + " setting AI Attacks Player? to " + enumValue + ".", ai);
        //    ai.AIAttacksPlayerRef = enumValue;
        //}

        public void SetEmeraldPlayerRelation(string faction, string relation)
        {
            ApplyToFaction(faction, SetPlayerRelation, new object[] { relation });
        }

        public void SetEmeraldFactionRelation(string faction, string targetFaction, string relation)
        {
            ApplyToFaction(faction, SetFactionRelation, new object[] { targetFaction, relation });
        }

        private void SetPlayerRelation(EmeraldAI.EmeraldAISystem ai, object[] args)
        {
            if (ai == null) return;
            var relationName = (string)args[0];
            var relationID = GetRelationID(relationName);
            if (relationID == -1)
            {
                if (DialogueDebug.LogWarnings) Debug.Log("Dialogue System: Emerald AI " + ai.name + " can't set player relation to invalid relation name  " + relationName + ".", ai);
            }
            else
            {
                if (DialogueDebug.LogInfo) Debug.Log("Dialogue System: Emerald AI " + ai.name + " setting player relation to " + relationName + ".", ai);
                ai.PlayerFaction[0].RelationTypeRef = (EmeraldAI.EmeraldAISystem.PlayerFactionClass.RelationType)relationID;
            }
        }

        private void SetFactionRelation(EmeraldAI.EmeraldAISystem ai, object[] args)
        {
            if (ai == null) return;
            var targetFactionName = (string)args[0];
            var relationName = (string)args[1];
            var targetFactionID = GetFactionID(targetFactionName);
            var relationID = GetRelationID(relationName);
            if (targetFactionID == -1)
            {
                if (DialogueDebug.LogWarnings) Debug.Log("Dialogue System: Emerald AI " + ai.name + " can't find faction named " + targetFactionName + " to set relation to " + relationName + ".", ai);
            }
            else if (relationID == -1)
            {
                if (DialogueDebug.LogWarnings) Debug.Log("Dialogue System: Emerald AI " + ai.name + " can't set faction relation " + targetFactionName + " to invalid relation name  " + relationName + ".", ai);
            }
            else
            {
                if (DialogueDebug.LogInfo) Debug.Log("Dialogue System: Emerald AI " + ai.name + " setting faction " + targetFactionName + " relation to " + relationName + ".", ai);
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
                // Update inspector variables, too:
                //if (index == 0) { ai.OpposingFaction1 = targetFactionID; ai.FactionRelation1 = relationID; }
                //if (index == 1) { ai.OpposingFaction2 = targetFactionID; ai.FactionRelation2 = relationID; }
                //if (index == 2) { ai.OpposingFaction3 = targetFactionID; ai.FactionRelation3 = relationID; }
                //if (index == 3) { ai.OpposingFaction4 = targetFactionID; ai.FactionRelation4 = relationID; }
                //if (index == 4) { ai.OpposingFaction5 = targetFactionID; ai.FactionRelation5 = relationID; }
                //ai.OpposingFactionsEnumRef = (EmeraldAI.EmeraldAISystem.OpposingFactionsEnum)(Mathf.Clamp(ai.AIFactionsList.Count - 1, 0, 4));
            }
        }

        private int GetRelationID(string relationName)
        {
            if (string.Equals(relationName, "Enemy", System.StringComparison.OrdinalIgnoreCase)) return (int)EmeraldAI.EmeraldAISystem.RelationType.Enemy;
            if (string.Equals(relationName, "Neutral", System.StringComparison.OrdinalIgnoreCase)) return (int)EmeraldAI.EmeraldAISystem.RelationType.Neutral;
            if (string.Equals(relationName, "Friendly", System.StringComparison.OrdinalIgnoreCase)) return (int)EmeraldAI.EmeraldAISystem.RelationType.Friendly;
            if (string.Equals(relationName, "Friend", System.StringComparison.OrdinalIgnoreCase)) return (int)EmeraldAI.EmeraldAISystem.RelationType.Friendly;
            return -1;
        }

        public void SetEmeraldBehavior(string faction, string behavior)
        {
            ApplyToFaction(faction, SetBehavior, new object[] { behavior });
        }

        private void SetBehavior(EmeraldAI.EmeraldAISystem ai, object[] args)
        {
            if (ai == null) return;
            var behavior = GetBehaviorType((string)args[0]);
            if (DialogueDebug.LogInfo) Debug.Log("Dialogue System: Emerald AI " + ai.name + " setting behavior to " + behavior + ".", ai);
            ai.EmeraldEventsManagerComponent.ChangeBehavior(behavior);
        }

        public void SetEmeraldConfidence(string faction, string confidenceType)
        {
            ApplyToFaction(faction, SetConfidence, new object[] { confidenceType });
        }

        private void SetConfidence(EmeraldAI.EmeraldAISystem ai, object[] args)
        {
            if (ai == null) return;
            var confidenceType = GetConfidenceType((string)args[0]);
            if (DialogueDebug.LogInfo) Debug.Log("Dialogue System: Emerald AI " + ai.name + " setting confidence to " + confidenceType + ".", ai);
            ai.EmeraldEventsManagerComponent.ChangeConfidence(confidenceType);
        }

        public void SetEmeraldDestination(string faction, string destination)
        {
            ApplyToFaction(faction, SetDestination, new object[] { destination });
        }

        private void SetDestination(EmeraldAI.EmeraldAISystem ai, object[] args)
        {
            if (ai == null) return;
            var destination = (string)args[0];
            var go = GameObject.Find(destination);
            if (go == null)
            {
                if (DialogueDebug.LogWarnings) Debug.Log("Dialogue System: Emerald AI " + ai.name + " can't find destination " + destination + ".", ai);
            }
            else
            {
                if (DialogueDebug.LogInfo) Debug.Log("Dialogue System: Emerald AI " + ai.name + " going to destination " + go + ".", ai);
                ai.EmeraldEventsManagerComponent.SetDestination(go.transform);
            }
        }

        public void SetEmeraldItem(string faction, double itemID, bool enable)
        {
            ApplyToFaction(faction, SetItem, new object[] { (int)itemID, enable});
        }

        private void SetItem(EmeraldAI.EmeraldAISystem ai, object[] args)
        {
            if (ai == null) return;
            var itemID = (int)args[0];
            var enable = (bool)args[1];
            var disableAll = itemID == -1 && !enable;
            if (disableAll)
            {
                if (DialogueDebug.logInfo) Debug.Log("Dialogue System: Emerald AI " + ai.name + " disabling all items.", ai);
                ai.EmeraldEventsManagerComponent.DisableAllItems();
            }
            else if (enable)
            {
                if (DialogueDebug.logInfo) Debug.Log("Dialogue System: Emerald AI " + ai.name + " enabling item " + itemID, ai);
                ai.EmeraldEventsManagerComponent.EnableItem(itemID);
            }
            else
            {
                if (DialogueDebug.logInfo) Debug.Log("Dialogue System: Emerald AI " + ai.name + " disabling item " + itemID, ai);
                ai.EmeraldEventsManagerComponent.DisableItem(itemID);
            }
        }

        public void SetEmeraldWander(string faction, string wanderType)
        {
            ApplyToFaction(faction, SetWander, new object[] { wanderType });
        }

        private void SetWander(EmeraldAI.EmeraldAISystem ai, object[] args)
        {
            if (ai == null) return;
            var wanderType = GetWanderType((string)args[0]);
            if (DialogueDebug.LogInfo) Debug.Log("Dialogue System: Emerald AI " + ai.name + " setting wander type to " + wanderType + ".", ai);
            ai.EmeraldEventsManagerComponent.ChangeWanderType(wanderType);
        }

        public void SetEmeraldWeapon(string faction, string weaponType, bool enable)
        {
            weaponType = string.Equals(weaponType, "Ranged", System.StringComparison.OrdinalIgnoreCase)
                ? "Ranged" : "Melee";
            ApplyToFaction(faction, SetWeapon, new object[] { weaponType, enable });
        }

        private void SetWeapon(EmeraldAI.EmeraldAISystem ai, object[] args)
        {
            if (ai == null) return;
            var weaponType = (string)args[0];
            var enable = (bool)args[1];
            if (enable)
            {
                if (DialogueDebug.logInfo) Debug.Log("Dialogue System: Emerald AI " + ai.name + " enabling " + weaponType + " weapon.", ai);
                ai.EmeraldEventsManagerComponent.EnableWeapon(weaponType);
            }
            else
            {
                if (DialogueDebug.logInfo) Debug.Log("Dialogue System: Emerald AI " + ai.name + " disabling " + weaponType + " weapon.", ai);
                ai.EmeraldEventsManagerComponent.DisableWeapon(weaponType);
            }
        }

        //public void SetEmeraldTargetTags(string faction, string newFaction, string tag1, string tag2, string tag3, string tag4, string tag5)
        //{
        //    ApplyToFaction(faction, SetTargetTags, new object[] { tag1, tag2, tag3, tag4, tag5 });
        //}

        //private void SetTargetTags(EmeraldAI.EmeraldAISystem ai, object[] args)
        //{
        //    if (ai == null) return;
        //    if (DialogueDebug.logInfo) Debug.Log("Dialogue System: Emerald AI " + ai.name + " setting target tags.", ai);
        //    ai.CurrentFaction = newFaction;
        //    ai.OpposingFaction1 = opposingFaction1;
        //    ai.OpposingFaction2 = opposingFaction2;
        //    ai.OpposingFaction3 = opposingFaction3;
        //    ai.OpposingFaction4 = opposingFaction4;
        //    ai.OpposingFaction5 = opposingFaction5;
        //}

        #region Utility

        private EmeraldAI.EmeraldAISystem.CurrentBehavior GetBehaviorType(string behavior)
        {
            switch (behavior.ToLower())
            {
                case "aggressive":
                    return EmeraldAI.EmeraldAISystem.CurrentBehavior.Aggressive;
                case "cautious":
                    return EmeraldAI.EmeraldAISystem.CurrentBehavior.Cautious;
                case "companion":
                    return EmeraldAI.EmeraldAISystem.CurrentBehavior.Companion;
                default:
                case "passive":
                    return EmeraldAI.EmeraldAISystem.CurrentBehavior.Passive;
                case "pet":
                    return EmeraldAI.EmeraldAISystem.CurrentBehavior.Pet;
            }
        }

        private EmeraldAI.EmeraldAISystem.ConfidenceType GetConfidenceType(string confidence)
        {
            switch (confidence.ToLower())
            {
                case "coward":
                    return EmeraldAI.EmeraldAISystem.ConfidenceType.Coward;
                default:
                case "brave":
                    return EmeraldAI.EmeraldAISystem.ConfidenceType.Brave;
                case "foolhardy":
                    return EmeraldAI.EmeraldAISystem.ConfidenceType.Foolhardy;
            }
        }

        private EmeraldAI.EmeraldAISystem.WanderType GetWanderType(string wanderType)
        {
            switch (wanderType.ToLower())
            {
                case "destination":
                    return EmeraldAI.EmeraldAISystem.WanderType.Destination;
                case "dynamic":
                    return EmeraldAI.EmeraldAISystem.WanderType.Dynamic;
                default:
                case "stationary":
                    return EmeraldAI.EmeraldAISystem.WanderType.Stationary;
                case "waypoints":
                    return EmeraldAI.EmeraldAISystem.WanderType.Waypoints;
            }
        }

        private EmeraldAI.EmeraldAISystem.TargetType GetTargetType(string targetType)
        {
            switch (targetType.ToLower())
            {
                case "ai":
                    return EmeraldAI.EmeraldAISystem.TargetType.AI;
                case "player":
                    return EmeraldAI.EmeraldAISystem.TargetType.Player;
                default:
                    return EmeraldAI.EmeraldAISystem.TargetType.NonAITarget;
            }
        }

        private int GetFactionID(string factionName)
        {
            for (int i = 0; i < factionNameList.Count; i++)
            {
                if (string.Equals(factionNameList[i], factionName)) return i;
            }
            return -1;
        }

        /// <summary>
        /// Applies a delegate function to all active Emerald AIs who belong to the specified faction.
        /// </summary>
        private void ApplyToFaction(string factionName, EmeraldAIDelegate delegateFunction, object[] args)
        {
            if (string.IsNullOrEmpty(factionName)) return;
            if (factionName.StartsWith("go=", System.StringComparison.OrdinalIgnoreCase))
            {
                // If faction name string starts with "go", find a single GameObject by name:
                var go = GameObject.Find(factionName.Substring("go=".Length).Trim());
                if (go != null)
                {
                    var ai = go.GetComponent<EmeraldAI.EmeraldAISystem>();
                    if (ai.enabled)
                    {
                        delegateFunction(ai, args);
                    }
                }
            }
            else
            {
                // Otherwise apply to all AI of the specified faction:
                for (int i = 0; i < factionNameList.Count; i++)
                {
                    if (string.Equals(factionNameList[i], factionName))
                    {
                        ApplyToFaction(i, delegateFunction, args);
                    }
                }
            }
        }

        /// <summary>
        /// Applies a delegate function to all active Emerald AIs who belong to the specified faction.
        /// </summary>
        private void ApplyToFaction(int faction, EmeraldAIDelegate delegateFunction, object[] args)
        {
            var all = GameObject.FindObjectsOfType<EmeraldAI.EmeraldAISystem>();
            for (int i = 0; i < all.Length; i++)
            {
                var ai = all[i];
                if (ai.enabled && ai.CurrentFaction == faction)
                {
                    delegateFunction(ai, args);
                }
            }
        }

        #endregion

    }
}