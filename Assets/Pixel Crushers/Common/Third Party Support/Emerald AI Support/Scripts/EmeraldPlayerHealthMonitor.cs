// Copyright © Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.EmeraldAISupport
{

    /// <summary>
    /// Adds an onDamage event that watches Emerald AI's EmeraldAI.Example.EmeraldAIPlayerHealth for damage.
    /// </summary>
    [AddComponentMenu("Pixel Crushers/Third Party/Emerald AI/Emerald Player Health Monitor")]
    [RequireComponent(typeof(EmeraldAI.Example.EmeraldAIPlayerHealth))]
    public class EmeraldPlayerHealthMonitor : MonoBehaviour
    {

        [Tooltip("Optional message to send to Message System, such as Damaged:Player.")]
        public string damageMessage = string.Empty;

        public EmeraldPlayerHealthEvent onDamage = new EmeraldPlayerHealthEvent();

        private EmeraldAI.Example.EmeraldAIPlayerHealth m_playerHealth;
        private float m_previousHealth;

        private void Start()
        {
            m_playerHealth = GetComponent<EmeraldAI.Example.EmeraldAIPlayerHealth>();
            m_previousHealth = m_playerHealth.CurrentHealth;
        }

        private void Update()
        {
            if (m_playerHealth == null) return;
            var currentHealth = m_playerHealth.CurrentHealth;
            if (m_previousHealth > currentHealth)
            {
                // If health is lower than last frame, it was damaged:
                MessageSystem.SendCompositeMessage(this, damageMessage);
                onDamage.Invoke(m_playerHealth);
            }
            m_previousHealth = currentHealth;
        }

    }
}