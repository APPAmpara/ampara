using Jungle.Core;
using UnityEngine;

namespace Jungle.Player
{
    /// <summary>
    /// Drena stamina ao esprintar, regenera após um pequeno atraso quando o
    /// jogador para de esprintar. Agachar não consome stamina (só reduz ruído,
    /// ver CharacterStats/StealthSystem).
    /// </summary>
    public class StaminaSystem : MonoBehaviour
    {
        private CharacterStats stats;
        private float timeSinceSprintStopped;

        public float CurrentStamina { get; private set; }
        public float MaxStamina => stats != null ? stats.maxStamina : 100f;
        public float NormalizedStamina => MaxStamina <= 0f ? 0f : CurrentStamina / MaxStamina;
        public bool CanSprint => CurrentStamina > 0f;

        public void Initialize(CharacterStats characterStats)
        {
            stats = characterStats;
            CurrentStamina = stats.maxStamina;
            timeSinceSprintStopped = stats.staminaRegenDelay;
        }

        public void Tick(float deltaTime, bool isSprinting)
        {
            if (stats == null) return;

            if (isSprinting && CurrentStamina > 0f)
            {
                CurrentStamina = Mathf.Max(0f, CurrentStamina - stats.sprintDrainPerSecond * deltaTime);
                timeSinceSprintStopped = 0f;
            }
            else
            {
                timeSinceSprintStopped += deltaTime;
                if (timeSinceSprintStopped >= stats.staminaRegenDelay)
                    CurrentStamina = Mathf.Min(stats.maxStamina, CurrentStamina + stats.staminaRegenPerSecond * deltaTime);
            }
        }
    }
}
