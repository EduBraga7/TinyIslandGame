using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Controlador de movimentação do jogador para visão isométrica / top-down.
/// Utiliza o CharacterController para colisões suaves e lida com gravidade.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimentação")]
    [Tooltip("Define se o jogador pode se mover (bloqueado durante menus)")]
    public bool canMove = true;

    [Tooltip("Velocidade com que o personagem anda")]
    public float moveSpeed = 6f;

    [Tooltip("Velocidade de rotação para encarar a direção que está se movendo")]
    public float rotationSpeed = 12f;

    [Tooltip("Força da gravidade quando o jogador estiver no ar")]
    public float gravity = -20f;

    [Header("Partes do Corpo (Animação Procedural)")]
    public Transform torsoTransform;
    public Transform headTransform;
    public Transform armLTransform;
    public Transform armRTransform;
    public Transform legLTransform;
    public Transform legRTransform;

    [Header("Parâmetros da Caminhada")]
    public float walkAnimSpeed = 14f;
    public float legSwingAngle = 32f;
    public float armSwingAngle = 26f;
    public float bobAmount = 0.035f;

    private CharacterController controller;
    private Vector3 verticalVelocity;
    private Vector3 initialTorsoPos;
    private float animTimer = 0f;
    private float idleTimer = 0f;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    void Start()
    {
        FindBodyParts();
        if (torsoTransform != null)
        {
            initialTorsoPos = torsoTransform.localPosition;
        }
    }

    public void FindBodyParts()
    {
        if (torsoTransform == null) torsoTransform = transform.Find("Player_Torso");
        if (torsoTransform != null)
        {
            if (headTransform == null) headTransform = torsoTransform.Find("Player_Head");
            if (armLTransform == null) armLTransform = torsoTransform.Find("Player_Arm_L");
            if (armRTransform == null) armRTransform = torsoTransform.Find("Player_Arm_R");
            if (legLTransform == null) legLTransform = torsoTransform.Find("Player_Leg_L");
            if (legRTransform == null) legRTransform = torsoTransform.Find("Player_Leg_R");
        }
    }

    void Update()
    {
        bool movementAllowed = canMove && (IslandGameManager.Instance == null || IslandGameManager.Instance.CanPlayerMove());
        Vector2 input = movementAllowed ? GetMovementInput() : Vector2.zero;

        // Converte o input 2D (teclas W,A,S,D ou Setas) para direção 3D no plano X/Z
        Vector3 moveDirection = new Vector3(input.x, 0f, input.y).normalized;

        // Se houver movimentação, rotaciona o personagem suavemente para a direção
        if (moveDirection.magnitude >= 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // Aplica gravidade para manter os pés no chão da ilha
        if (controller.isGrounded && verticalVelocity.y < 0)
        {
            verticalVelocity.y = -2f;
        }
        else
        {
            verticalVelocity.y += gravity * Time.deltaTime;
        }

        // Move o personagem considerando velocidade e gravidade
        Vector3 finalMove = (moveDirection * moveSpeed) + verticalVelocity;
        controller.Move(finalMove * Time.deltaTime);

        // Animação procedural de caminhada e respiração ociosa
        AnimateCharacter(moveDirection.magnitude);

        // Se o jogador cair da ilha no mar (abaixo de Y = -1), respawna suavemente no centro!
        if (transform.position.y < -1f)
        {
            controller.enabled = false;
            transform.position = new Vector3(0f, 1.5f, 0f);
            verticalVelocity = Vector3.zero;
            controller.enabled = true;
        }
    }

    private void AnimateCharacter(float inputMagnitude)
    {
        if (torsoTransform == null && legLTransform == null) return;

        bool isMoving = inputMagnitude > 0.1f && controller.isGrounded;

        if (isMoving)
        {
            animTimer += Time.deltaTime * walkAnimSpeed;
            float legAngle = Mathf.Sin(animTimer) * legSwingAngle;
            float armAngle = Mathf.Sin(animTimer) * armSwingAngle;

            if (legLTransform != null) legLTransform.localRotation = Quaternion.Euler(legAngle, 0, 0);
            if (legRTransform != null) legRTransform.localRotation = Quaternion.Euler(-legAngle, 0, 0);
            if (armLTransform != null) armLTransform.localRotation = Quaternion.Euler(-armAngle, 0, 0);
            if (armRTransform != null) armRTransform.localRotation = Quaternion.Euler(armAngle, 0, 0);

            // Bobbing vertical do corpo ao correr
            if (torsoTransform != null)
            {
                float bob = Mathf.Abs(Mathf.Sin(animTimer)) * bobAmount;
                torsoTransform.localPosition = initialTorsoPos + new Vector3(0, bob, 0);
            }
        }
        else
        {
            // Respiração suave quando parado (idle)
            idleTimer += Time.deltaTime * 3f;
            float breath = Mathf.Sin(idleTimer) * 0.008f;
            if (torsoTransform != null)
            {
                torsoTransform.localPosition = Vector3.Lerp(torsoTransform.localPosition, initialTorsoPos + new Vector3(0, breath, 0), Time.deltaTime * 6f);
            }
            if (legLTransform != null) legLTransform.localRotation = Quaternion.Slerp(legLTransform.localRotation, Quaternion.identity, Time.deltaTime * 10f);
            if (legRTransform != null) legRTransform.localRotation = Quaternion.Slerp(legRTransform.localRotation, Quaternion.identity, Time.deltaTime * 10f);
            if (armLTransform != null) armLTransform.localRotation = Quaternion.Slerp(armLTransform.localRotation, Quaternion.identity, Time.deltaTime * 10f);
            if (armRTransform != null) armRTransform.localRotation = Quaternion.Slerp(armRTransform.localRotation, Quaternion.identity, Time.deltaTime * 10f);
        }
    }

    /// <summary>
    /// Captura as teclas do teclado (WASD / Setas) ou analógico do controle.
    /// Funciona nativamente com o novo Input System do Unity 6.
    /// </summary>
    private Vector2 GetMovementInput()
    {
        Vector2 input = Vector2.zero;

#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) input.y += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) input.y -= 1f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) input.x -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) input.x += 1f;
        }

        if (Gamepad.current != null)
        {
            Vector2 stick = Gamepad.current.leftStick.ReadValue();
            if (stick.magnitude > 0.1f)
            {
                input = stick;
            }
        }
#else
        input.x = Input.GetAxisRaw("Horizontal");
        input.y = Input.GetAxisRaw("Vertical");
#endif

        return input.normalized;
    }
}
