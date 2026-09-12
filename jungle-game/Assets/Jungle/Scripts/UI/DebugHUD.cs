using Jungle.Core;
using Jungle.Player;
using UnityEngine;

namespace Jungle.UI
{
    /// <summary>
    /// HUD provisório via OnGUI (sem Canvas/arte) só para validar a Parte 1:
    /// estado de movimento, stamina, raio de ruído atual e cobertura/camuflagem.
    /// Será substituído por UI de verdade quando entrarmos na Parte de UI final.
    /// </summary>
    public class DebugHUD : MonoBehaviour
    {
        [SerializeField] private PlayerController player;
        [SerializeField] private StaminaSystem stamina;
        [SerializeField] private StealthSystem stealth;

        private void OnGUI()
        {
            if (player == null || stamina == null || stealth == null)
            {
                GUI.Label(new Rect(10, 10, 400, 20), "DebugHUD: arraste Player/Stamina/Stealth no Inspector.");
                return;
            }

            GUI.Box(new Rect(10, 10, 260, 130), "Jungle — Debug (Parte 1)");
            GUI.Label(new Rect(20, 35, 240, 20), $"Estado: {player.CurrentState}");

            GUI.Label(new Rect(20, 55, 240, 20), $"Stamina: {stamina.CurrentStamina:0}/{stamina.MaxStamina:0}");
            DrawBar(new Rect(20, 75, 220, 14), stamina.NormalizedStamina, Color.yellow);

            GUI.Label(new Rect(20, 95, 240, 20), $"Ruído (raio): {stealth.CurrentNoiseRadius:0.0} m");
            GUI.Label(new Rect(20, 115, 240, 20), $"Cobertura: {stealth.CurrentCoverType} (x{stealth.CamouflageMultiplier:0.00})");
        }

        private void DrawBar(Rect rect, float normalized, Color color)
        {
            GUI.Box(rect, GUIContent.none);
            var fill = new Rect(rect.x + 2, rect.y + 2, (rect.width - 4) * Mathf.Clamp01(normalized), rect.height - 4);
            var prevColor = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(fill, Texture2D.whiteTexture);
            GUI.color = prevColor;
        }
    }
}
