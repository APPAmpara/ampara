using UnityEngine;

namespace Jungle.Core
{
    public enum CoverType
    {
        None,
        Bush,
        Tree
    }

    /// <summary>
    /// Marca um volume de arbusto: bloqueio PARCIAL de visão (o raycast de LOS
    /// ainda enxerga através, mas a taxa de suspeita do inimigo é multiplicada
    /// por camouflageMultiplier enquanto o jogador estiver dentro).
    ///
    /// Árvores NÃO usam este componente: para bloqueio TOTAL de LOS, basta um
    /// collider sólido (não-trigger) na layer "Obstacle" — o raycast do
    /// StealthSystem já é bloqueado fisicamente por ele, sem precisar de lógica
    /// extra. Ver instruções de setup no final do arquivo StealthSystem.cs.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class CoverZone : MonoBehaviour
    {
        public CoverType coverType = CoverType.Bush;

        [Range(0f, 1f)]
        [Tooltip("Multiplica a taxa de subida da suspeita enquanto o jogador está aqui dentro. 0.35 = 65% mais difícil de ser notado.")]
        public float camouflageMultiplier = 0.35f;

        private void Reset()
        {
            var col = GetComponent<Collider>();
            col.isTrigger = true;
        }
    }
}
