// Copyright © Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.EmeraldAISupport
{

    /// <summary>
    /// Sends Message System messages on Emerald_AI for damage and death.
    /// </summary>
    [AddComponentMenu("Pixel Crushers/Third Party/Emerald AI/Emerald AI Health Monitor")]
    [RequireComponent(typeof(EmeraldAI.EmeraldAISystem))]
    public class EmeraldAIHealthMonitor : MonoBehaviour
    {

        [Tooltip("Optional message to send to Message System, such as Damaged:Skeleton.")]
        public string damageMessage = string.Empty;

        [Tooltip("Optional message to send to Message System, such as Killed:Skeleton.")]
        public string deathMessage = string.Empty;

        private EmeraldAI.EmeraldAISystem m_ai = null;

        private void Start()
        {
            m_ai = GetComponent<EmeraldAI.EmeraldAISystem>();
            m_ai.DamageEvent.AddListener(() => { OnDamage(); });
            m_ai.DeathEvent.AddListener(() => { OnDeath(); });
        }

        private void OnDamage()
        {
            MessageSystem.SendCompositeMessage(this, damageMessage);
        }

        private void OnDeath()
        {
            MessageSystem.SendCompositeMessage(this, deathMessage);

        }

    }
}