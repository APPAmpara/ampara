using System.Collections.Generic;
using Jungle.Player;
using UnityEngine;

namespace Jungle.Core
{
    /// <summary>
    /// Responsável por DUAS coisas independentes:
    ///
    /// 1) RUÍDO: qual o raio de detecção sonora atual, dado o estado de
    ///    movimento (idle/andando/correndo/agachado). Quem vai reagir a isso
    ///    é a percepção auditiva do Índio (Parte 2) e o EnemyAI em geral.
    ///
    /// 2) VISÃO (LOS): um método reutilizável IsVisibleFrom(...) que qualquer
    ///    observador (o EnemyAI na Parte 2, ou o fog-of-war do próprio jogador)
    ///    pode chamar para saber "eu consigo ver este alvo agora?". Faz
    ///    checagem de distância, ângulo de cone e raycast contra obstáculos.
    ///
    /// Este componente fica no Player. O mesmo método de LOS será chamado
    /// pelo EnemyAI (Parte 2) passando a posição/direção do bot como
    /// observador — por isso a lógica de raycast já nasce genérica.
    /// </summary>
    public class StealthSystem : MonoBehaviour
    {
        [Header("Referências")]
        [SerializeField] private CharacterStats stats;
        [Tooltip("Altura aproximada dos olhos, usada como origem/alvo dos raycasts de LOS.")]
        [SerializeField] private Transform eyePoint;

        [Header("Camada de obstáculos (bloqueio TOTAL de visão — árvores, paredes, cenário sólido)")]
        [SerializeField] private LayerMask obstacleMask;

        [Header("Debug — opcional, para testar LOS sem precisar do EnemyAI ainda")]
        [Tooltip("Arraste qualquer Transform na cena (ex: um cubo) para simular um observador e ver no Gizmo se ele te enxergaria.")]
        [SerializeField] private Transform debugWatcher;
        [SerializeField] private float debugWatcherVisionRadius = 8f;
        [SerializeField] private float debugWatcherVisionAngle = 110f;

        private readonly List<CoverZone> activeCovers = new List<CoverZone>();
        private PlayerController.MovementState movementState = PlayerController.MovementState.Idle;

        public float CurrentNoiseRadius { get; private set; }
        public CoverType CurrentCoverType { get; private set; } = CoverType.None;

        /// <summary>1 = sem camuflagem. Quanto menor, mais escondido (ver CoverZone).</summary>
        public float CamouflageMultiplier { get; private set; } = 1f;

        public Vector3 EyePosition => eyePoint != null ? eyePoint.position : transform.position + Vector3.up * 1.5f;

        private bool debugLastVisible;

        public void Initialize(CharacterStats characterStats)
        {
            stats = characterStats;
            RecalculateNoise();
        }

        public void SetMovementState(PlayerController.MovementState state)
        {
            movementState = state;
            RecalculateNoise();
        }

        private void RecalculateNoise()
        {
            if (stats == null) return;
            CurrentNoiseRadius = movementState switch
            {
                PlayerController.MovementState.Sprint => stats.sprintNoiseRadius,
                PlayerController.MovementState.Crouch => stats.crouchNoiseRadius,
                PlayerController.MovementState.Walk => stats.walkNoiseRadius,
                _ => stats.idleNoiseRadius,
            };
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out CoverZone cover) && cover.coverType == CoverType.Bush)
            {
                activeCovers.Add(cover);
                RecalculateCover();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent(out CoverZone cover))
            {
                activeCovers.Remove(cover);
                RecalculateCover();
            }
        }

        private void RecalculateCover()
        {
            activeCovers.RemoveAll(c => c == null);
            if (activeCovers.Count == 0)
            {
                CurrentCoverType = CoverType.None;
                CamouflageMultiplier = 1f;
                return;
            }

            float strongest = 1f;
            foreach (var cover in activeCovers)
                strongest = Mathf.Min(strongest, cover.camouflageMultiplier);

            CurrentCoverType = CoverType.Bush;
            CamouflageMultiplier = strongest;
        }

        /// <summary>
        /// Checa se ESTE stealth system (o jogador) é visível a partir de um
        /// observador externo. Usado hoje só pelo debugWatcher; na Parte 2 o
        /// EnemyAI chamará este mesmo método passando os próprios dados de visão.
        /// </summary>
        /// <param name="effectiveMultiplier">
        /// Repassa o CamouflageMultiplier atual — quem chama usa isso para
        /// reduzir a taxa de subida da suspeita (DetectionMeter, Parte 2).
        /// </param>
        public bool IsVisibleFrom(Vector3 observerEye, float visionRadius, float visionAngleDeg, Vector3 observerForward, out float effectiveMultiplier)
        {
            effectiveMultiplier = CamouflageMultiplier;

            Vector3 toPlayer = EyePosition - observerEye;
            float distance = toPlayer.magnitude;
            if (distance > visionRadius) return false;

            float angle = Vector3.Angle(observerForward, toPlayer);
            if (angle > visionAngleDeg * 0.5f) return false;

            // Bloqueio TOTAL: se algo na obstacleMask (árvore, parede) estiver
            // no caminho, o observador não vê o jogador — camuflagem de arbusto
            // não importa aqui, cobertura de árvore já resolve sozinha.
            if (Physics.Raycast(observerEye, toPlayer.normalized, distance, obstacleMask))
                return false;

            return true;
        }

        private void Update()
        {
            if (debugWatcher == null) return;
            debugLastVisible = IsVisibleFrom(
                debugWatcher.position,
                debugWatcherVisionRadius,
                debugWatcherVisionAngle,
                debugWatcher.forward,
                out _);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = CurrentCoverType == CoverType.None ? Color.white : Color.green;
            Gizmos.DrawWireSphere(EyePosition, 0.2f);

            Gizmos.color = new Color(1f, 0.5f, 0f, 0.25f);
            Gizmos.DrawWireSphere(transform.position, CurrentNoiseRadius);

            if (debugWatcher != null)
            {
                Gizmos.color = debugLastVisible ? Color.red : Color.gray;
                Gizmos.DrawLine(debugWatcher.position, EyePosition);
            }
        }
    }
}
