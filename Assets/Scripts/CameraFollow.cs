using UnityEngine;

/// <summary>
/// Câmera isométrica suave que acompanha o jogador com ângulo e distância configuráveis.
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [Header("Alvo")]
    [Tooltip("Arraste o objeto do Jogador (Player) aqui")]
    public Transform target;

    [Header("Posicionamento Isométrico")]
    [Tooltip("Distância e altura da câmera em relação ao jogador")]
    public Vector3 offset = new Vector3(0f, 6.2f, -5.5f);

    [Tooltip("Suavidade da perseguição (quanto menor, mais rápida)")]
    public float smoothTime = 0.2f;

    private Vector3 currentVelocity = Vector3.zero;

    void LateUpdate()
    {
        if (target == null) return;

        // Posição desejada somando a posição do jogador ao offset
        Vector3 desiredPosition = target.position + offset;

        // Movimento suave usando SmoothDamp para evitar solavancos
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, smoothTime);

        // Aponta a câmera levemente acima dos pés do jogador (centro do corpo)
        transform.LookAt(target.position + Vector3.up * 1f);
    }
}
