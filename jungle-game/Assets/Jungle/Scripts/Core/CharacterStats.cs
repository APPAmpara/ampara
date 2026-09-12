using UnityEngine;

namespace Jungle.Core
{
    public enum CharacterType
    {
        Macaco,
        Cacador,
        Indio
    }

    /// <summary>
    /// Atributos de um personagem jogável. Crie um asset por personagem
    /// (menu Assets/Create/Jungle/Character Stats) e arraste no PlayerController.
    /// Valores iniciais abaixo são ESTIMATIVAS de design (não vieram do
    /// protótipo HTML, que não estava disponível nesta sessão) — ajuste
    /// comparando com o comportamento original antes de fechar o balanceamento.
    /// </summary>
    [CreateAssetMenu(fileName = "NewCharacterStats", menuName = "Jungle/Character Stats")]
    public class CharacterStats : ScriptableObject
    {
        [Header("Identidade")]
        public CharacterType characterType;
        public string displayName;

        [Header("Movimento (m/s)")]
        public float walkSpeed = 3.5f;
        public float sprintSpeed = 6.5f;
        public float crouchSpeed = 1.8f;
        public float rotationSpeedDegPerSec = 720f;

        [Header("Stamina")]
        public float maxStamina = 100f;
        public float sprintDrainPerSecond = 20f;
        public float staminaRegenPerSecond = 12f;
        [Tooltip("Segundos sem esprintar antes da stamina começar a regenerar.")]
        public float staminaRegenDelay = 0.75f;

        [Header("Ruído emitido (raio em metros que um inimigo com audição normal detecta)")]
        public float idleNoiseRadius = 0f;
        public float walkNoiseRadius = 4f;
        public float sprintNoiseRadius = 9f;
        public float crouchNoiseRadius = 1.2f;

        [Header("Percepção (usado pelo Índio — auditiva direcional, Parte 2)")]
        public bool hasDirectionalHearing = false;
        [Tooltip("Multiplica o alcance de audição do próprio personagem quando ele é o Índio.")]
        public float hearingRangeMultiplier = 1f;

        [Header("Combate (Parte 3)")]
        public bool isMelee = true;
        public float attackRange = 1.5f;
        public float attackCooldown = 0.6f;
    }
}
