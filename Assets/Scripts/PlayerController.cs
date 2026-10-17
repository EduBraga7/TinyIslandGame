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
    [Tooltip("Velocidade com que o personagem anda")]
    public float moveSpeed = 6f;

    [Tooltip("Velocidade de rotação para encarar a direção que está se movendo")]
    public float rotationSpeed = 12f;

    [Tooltip("Força da gravidade quando o jogador estiver no ar")]
    public float gravity = -20f;

    private CharacterController controller;
    private Vector3 verticalVelocity;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        Vector2 input = GetMovementInput();

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

        // Se o jogador cair da ilha no mar (abaixo de Y = -1), respawna suavemente no centro!
        if (transform.position.y < -1f)
        {
            controller.enabled = false;
            transform.position = new Vector3(0f, 1.5f, 0f);
            verticalVelocity = Vector3.zero;
            controller.enabled = true;
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
