using UnityEngine;

/// <summary>
/// Gerencia o estado do jogo (pontuação, itens coletados) e desenha a interface na tela.
/// </summary>
public class IslandGameManager : MonoBehaviour
{
    public static IslandGameManager Instance { get; private set; }

    [Header("Status da Ilha")]
    public int totalItemsCollected = 0;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void AddScore(int amount, string itemName)
    {
        totalItemsCollected += amount;
    }

    // Interface limpa desenhada automaticamente no canto da tela (sem precisar configurar Canvas complexo)
    void OnGUI()
    {
        GUIStyle boxStyle = new GUIStyle(GUI.skin.box)
        {
            fontSize = 18,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        boxStyle.normal.textColor = Color.white;

        // Caixa bonita no canto superior esquerdo
        GUI.Box(new Rect(20, 20, 240, 50), $"🪙 Moedas Coletadas: {totalItemsCollected}", boxStyle);
    }
}
