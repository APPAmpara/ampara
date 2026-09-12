using Jungle.Core;
using UnityEngine;

namespace Jungle.Player
{
    /// <summary>
    /// Movimento top-down (plano XZ, câmera ortográfica olhando de cima).
    /// Usa Input Manager legado (WASD/setas + Shift para correr + Ctrl/C para
    /// agachar) para não depender de pacotes extras. Delega stamina para
    /// StaminaSystem e ruído/camuflagem para StealthSystem.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(StaminaSystem))]
    [RequireComponent(typeof(StealthSystem))]
    public class PlayerController : MonoBehaviour
    {
        public enum MovementState { Idle, Walk, Sprint, Crouch }

        [SerializeField] private CharacterStats stats;

        private CharacterController controller;
        private StaminaSystem staminaSystem;
        private StealthSystem stealthSystem;

        public MovementState CurrentState { get; private set; } = MovementState.Idle;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            staminaSystem = GetComponent<StaminaSystem>();
            stealthSystem = GetComponent<StealthSystem>();

            if (stats == null)
                Debug.LogError($"{name}: CharacterStats não atribuído no PlayerController.", this);

            staminaSystem.Initialize(stats);
            stealthSystem.Initialize(stats);
        }

        private void Update()
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            Vector3 inputDir = new Vector3(h, 0f, v);
            if (inputDir.sqrMagnitude > 1f) inputDir.Normalize();

            bool isMoving = inputDir.sqrMagnitude > 0.0001f;
            bool wantsCrouch = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.C);
            bool wantsSprint = isMoving && !wantsCrouch && Input.GetKey(KeyCode.LeftShift) && staminaSystem.CanSprint;

            CurrentState = !isMoving ? MovementState.Idle
                : wantsSprint ? MovementState.Sprint
                : wantsCrouch ? MovementState.Crouch
                : MovementState.Walk;

            float speed = CurrentState switch
            {
                MovementState.Sprint => stats.sprintSpeed,
                MovementState.Crouch => stats.crouchSpeed,
                MovementState.Walk => stats.walkSpeed,
                _ => 0f,
            };

            controller.SimpleMove(inputDir * speed);

            if (isMoving)
            {
                Quaternion targetRot = Quaternion.LookRotation(inputDir, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, stats.rotationSpeedDegPerSec * Time.deltaTime);
            }

            staminaSystem.Tick(Time.deltaTime, CurrentState == MovementState.Sprint);
            stealthSystem.SetMovementState(CurrentState);
        }
    }
}
