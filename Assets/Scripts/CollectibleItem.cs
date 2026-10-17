using UnityEngine;

/// <summary>
/// Item coletável da ilha (ex: frutas, conchas, gemas).
/// Fica girando e flutuando no ar até o jogador encostar.
/// </summary>
public class CollectibleItem : MonoBehaviour
{
    [Header("Identificação do Item")]
    public string itemName = "Concha Dourada";
    public int points = 1;

    [Header("Efeito de Flutuação e Rotação")]
    public float rotationSpeed = 90f; // Graus por segundo
    public float floatAmplitude = 0.25f; // Altura do sobe e desce
    public float floatFrequency = 2.5f; // Velocidade da flutuação

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;

        // Garante que o Collider seja um Gatilho (Trigger) para não bloquear a passagem física
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    void Update()
    {
        // Gira continuamente no eixo vertical (Y)
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);

        // Flutua suavemente para cima e para baixo
        float newY = startPos.y + Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Se quem encostou foi o jogador (tem o script PlayerController)
        if (other.GetComponent<PlayerController>() != null || other.CompareTag("Player"))
        {
            Collect();
        }
    }

    private void Collect()
    {
        if (IslandGameManager.Instance != null)
        {
            IslandGameManager.Instance.AddScore(points, itemName);
        }

        Debug.Log($"<color=cyan>[TinyIsland]</color> Coletado: {itemName}! (+{points})");

        // Remove o item da cena
        Destroy(gameObject);
    }
}
