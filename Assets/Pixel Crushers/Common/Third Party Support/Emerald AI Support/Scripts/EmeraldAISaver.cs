// Copyright © Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEngine.AI;
using System;
using System.Collections;
using System.Collections.Generic;

namespace PixelCrushers.EmeraldAISupport
{

    /// <summary>
    /// Saves an Emerald AI's state.
    /// </summary>
    [AddComponentMenu("Pixel Crushers/Save System/Savers/Emerald AI/Emerald AI Saver")]
    [RequireComponent(typeof(EmeraldAI.EmeraldAISystem))]
    public class EmeraldAISaver : Saver
    {

        [Tooltip("This trigger parameter plays the dead state.")]
        public string deadAnimatorTrigger = "Dead";

        [Tooltip("This bool parameter needs to be cleared when in the dead state.")]
        public string disableIdleFromDeadAnimatorParameter = "Idle Active";

        public enum HideHealthBarMode { FadeOut, Deactivate }

        public HideHealthBarMode hideHealthBarMode = HideHealthBarMode.FadeOut;

        [Serializable]
        public class EmeraldAIData
        {
            public Vector3 position;
            public Quaternion rotation;
            public bool enabled;
            public int health;
            public int faction;
            public EmeraldAI.EmeraldAISystem.CurrentBehavior behavior;
            public EmeraldAI.EmeraldAISystem.ConfidenceType confidence;
            public EmeraldAI.EmeraldAISystem.WanderType wander;
            public string follow;
            public List<int> factionsList;
            public List<int> factionRelations;
        }

        private EmeraldAI.EmeraldAISystem m_ai = null;
        private EmeraldAI.EmeraldAISystem ai
        {
            get
            {
                if (m_ai == null) m_ai = GetComponent<EmeraldAI.EmeraldAISystem>();
                return m_ai;
            }
        }

        private EmeraldAIData m_data = new EmeraldAIData();

        public override string RecordData()
        {
            m_data.position = transform.position;
            m_data.rotation = transform.rotation;
            m_data.enabled = ai.enabled;
            m_data.health = ai.CurrentHealth;
            m_data.faction = ai.CurrentFaction;
            m_data.behavior = ai.BehaviorRef;
            m_data.confidence = ai.ConfidenceRef;
            m_data.wander = ai.WanderTypeRef;
            m_data.follow = (ai.CurrentFollowTarget != null) ? ai.CurrentFollowTarget.name : string.Empty;
            m_data.factionsList = new List<int>(ai.AIFactionsList);
            m_data.factionRelations = new List<int>(ai.FactionRelations);
            return SaveSystem.Serialize(m_data);
        }

        public override void ApplyData(string s)
        {
            if (string.IsNullOrEmpty(s)) return;
            var data = SaveSystem.Deserialize<EmeraldAIData>(s, m_data);
            if (data == null)
            {
                m_data = new EmeraldAIData();
                return;
            }
            transform.position = data.position;
            transform.rotation = data.rotation;
            ai.enabled = data.enabled;
            ai.CurrentHealth = data.health;
            ai.CurrentFaction = data.faction;
            ai.AIFactionsList = new List<int>(data.factionsList);
            ai.FactionRelations = new List<int>(data.factionRelations);
            ai.EmeraldEventsManagerComponent.ChangeBehavior(m_data.behavior);
            ai.EmeraldEventsManagerComponent.ChangeConfidence(m_data.confidence);
            ai.EmeraldEventsManagerComponent.ChangeWanderType(m_data.wander);
            if (!data.enabled || data.health <= 0) // dead
            {
                StartCoroutine(SetAIDead());
            }
            else
            {
                SetAIFollowTarget(data.behavior, data.follow);
            }
        }

        private void SetAIFollowTarget(EmeraldAI.EmeraldAISystem.CurrentBehavior behavior, string followTargetName)
        {
            var followTarget = !string.IsNullOrEmpty(followTargetName) ? GameObject.Find(followTargetName) : null;
            if (followTarget != null)
            {
                ai.EmeraldEventsManagerComponent.SetFollowerTarget(followTarget.transform);
            }
        }

        private IEnumerator SetAIDead()
        {
            var animator = ai.GetComponent<Animator>();
            ComponentUtility.SetComponentEnabled(ai.GetComponent<AudioSource>(), false);
            ComponentUtility.SetComponentEnabled(ai.GetComponent<NavMeshAgent>(), false);
            ComponentUtility.SetComponentEnabled(ai.GetComponent<Collider>(), false);
            ai.enabled = false;
            ai.tag = "Untagged";
            ai.OptimizedStateRef = EmeraldAI.EmeraldAISystem.OptimizedState.Inactive;
            ai.LineOfSightTargets.Clear();
            if (animator != null)
            {
                animator.SetBool(disableIdleFromDeadAnimatorParameter, false);
                animator.SetTrigger(deadAnimatorTrigger);
                yield return new WaitForSeconds(2);
            }
            if (ai.HealthBarCanvas != null)
            {
                if (hideHealthBarMode == HideHealthBarMode.FadeOut)
                {
                    var healthBar = ai.GetComponentInChildren<EmeraldAI.Utility.EmeraldAIHealthBar>();
                    if (healthBar != null)
                    {
                        healthBar.FadeOut();
                    }
                    else
                    {
                        ai.HealthBarCanvas.SetActive(false);
                    }
                }
                else
                {
                    ai.HealthBarCanvas.SetActive(false);
                }
            }
            ComponentUtility.SetComponentEnabled(animator, false);
        }

    }

}
