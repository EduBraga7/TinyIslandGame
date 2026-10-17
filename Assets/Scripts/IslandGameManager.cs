using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Gerencia o estado completo do jogo (objetivo, contagem de moedas, tempo, vitória)
/// e provê feedback sonoro e visual procedural diretamente no jogo.
/// </summary>
public class IslandGameManager : MonoBehaviour
{
    public static IslandGameManager Instance { get; private set; }

    [Header("Regras do Jogo")]
    public int targetCoins = 6;
    public int totalItemsCollected = 0;
    public bool isVictory = false;

    [Header("Cronômetro")]
    public float gameTimer = 0f;

    // Componente de Áudio para efeitos sonoros procedurais
    private static AudioSource audioSource;
    private static AudioClip coinSound;
    private static AudioClip victorySound;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Cria o emissor de áudio se não existir
        audioSource = gameObject.GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        // Gera sons procedurais sintetizados matematicamente (sem precisar de arquivos externos .wav)
        GenerateProceduralAudio();
    }

    void Update()
    {
        if (!isVictory)
        {
            gameTimer += Time.deltaTime;
        }
    }

    public void AddScore(int amount, string itemName)
    {
        if (isVictory) return;

        totalItemsCollected += amount;
        PlayCoinSound();

        // Checa condição de vitória
        if (totalItemsCollected >= targetCoins)
        {
            TriggerVictory();
        }
    }

    private void TriggerVictory()
    {
        isVictory = true;
        Debug.Log("<color=yellow><b>[TinyIsland] VITÓRIA!</b> Todas as 6 moedas foram coletadas!</color>");

        // Abre o baú do tesouro
        if (TreasureChest.Instance != null)
        {
            TreasureChest.Instance.OpenChest();
        }
    }

    public static void PlayCoinSound()
    {
        if (audioSource != null && coinSound != null)
        {
            audioSource.PlayOneShot(coinSound, 0.7f);
        }
    }

    public static void PlayVictorySound()
    {
        if (audioSource != null && victorySound != null)
        {
            audioSource.PlayOneShot(victorySound, 0.9f);
        }
    }

    // Sintetizador procedural de áudio para som retrô agradável
    private void GenerateProceduralAudio()
    {
        // 1. Som de Moeda: arpeggio agudo suave de 2 notas (B5 -> E6)
        int sampleRate = 44100;
        int coinSamples = (int)(sampleRate * 0.22f);
        float[] coinData = new float[coinSamples];
        for (int i = 0; i < coinSamples; i++)
        {
            float t = (float)i / sampleRate;
            float freq = t < 0.09f ? 987.77f : 1318.51f; // B5 depois E6
            float envelope = 1f - (float)i / coinSamples;
            coinData[i] = Mathf.Sin(2 * Mathf.PI * freq * t) * envelope * 0.4f;
        }
        coinSound = AudioClip.Create("CoinChime", coinSamples, 1, sampleRate, false);
        coinSound.SetData(coinData, 0);

        // 2. Som de Vitória: acorde triunfante ascendente (C5 -> E5 -> G5 -> C6)
        int victorySamples = (int)(sampleRate * 1.6f);
        float[] victoryData = new float[victorySamples];
        float[] chordFreqs = new float[] { 523.25f, 659.25f, 783.99f, 1046.50f };
        for (int i = 0; i < victorySamples; i++)
        {
            float t = (float)i / sampleRate;
            int noteIndex = Mathf.Clamp((int)(t / 0.22f), 0, chordFreqs.Length - 1);
            float freq = chordFreqs[noteIndex];
            float envelope = Mathf.Exp(-t * 1.8f);
            victoryData[i] = Mathf.Sin(2 * Mathf.PI * freq * t) * envelope * 0.5f;
        }
        victorySound = AudioClip.Create("VictoryFanfare", victorySamples, 1, sampleRate, false);
        victorySound.SetData(victoryData, 0);
    }

    // Interface limpa na tela usando OnGUI estilizado
    void OnGUI()
    {
        // Estilo do Placar Superior
        GUIStyle boxStyle = new GUIStyle(GUI.skin.box)
        {
            fontSize = 16,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft
        };
        boxStyle.normal.textColor = Color.white;

        // Barra de progresso no topo
        int boxWidth = 320;
        int boxHeight = 65;
        GUI.Box(new Rect(20, 20, boxWidth, boxHeight), "", boxStyle);

        GUIStyle textStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 16,
            fontStyle = FontStyle.Bold
        };
        textStyle.normal.textColor = new Color(1.0f, 0.85f, 0.2f); // Dourado
        GUI.Label(new Rect(32, 26, 300, 26), $"🪙 Moedas: {totalItemsCollected} / {targetCoins}", textStyle);

        GUIStyle subStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 12,
            fontStyle = FontStyle.Normal
        };
        subStyle.normal.textColor = new Color(0.9f, 0.9f, 0.9f);
        if (!isVictory)
        {
            GUI.Label(new Rect(32, 52, 300, 26), "Colete todas para abrir o Baú do Tesouro!", subStyle);
        }
        else
        {
            subStyle.normal.textColor = new Color(0.3f, 1f, 0.4f); // Verde
            GUI.Label(new Rect(32, 52, 300, 26), "✨ O Baú do Tesouro foi aberto!", subStyle);
        }

        // TELA DE VITÓRIA (quando atinge 6 moedas)
        if (isVictory)
        {
            int panelW = 460;
            int panelH = 220;
            int screenX = (Screen.width - panelW) / 2;
            int screenY = (Screen.height - panelH) / 2 - 40;

            GUIStyle victoryBox = new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.UpperCenter
            };
            GUI.Box(new Rect(screenX, screenY, panelW, panelH), "", victoryBox);

            GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 22,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            titleStyle.normal.textColor = new Color(1f, 0.84f, 0f);
            GUI.Label(new Rect(screenX, screenY + 20, panelW, 35), "🏆 PARABÉNS! 🏆", titleStyle);

            GUIStyle msgStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                alignment = TextAnchor.MiddleCenter
            };
            msgStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(screenX, screenY + 60, panelW, 30), "Você descobriu o Segredo de Tiny Island!", msgStyle);

            GUIStyle timeStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                fontStyle = FontStyle.Italic,
                alignment = TextAnchor.MiddleCenter
            };
            timeStyle.normal.textColor = new Color(0.85f, 0.85f, 0.85f);
            GUI.Label(new Rect(screenX, screenY + 95, panelW, 25), $"Tempo de Expedição: {gameTimer:F1} segundos", timeStyle);

            // Botão de Jogar Novamente
            GUIStyle btnStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold
            };
            if (GUI.Button(new Rect(screenX + (panelW - 200) / 2, screenY + 140, 200, 48), "🔄 Jogar Novamente", btnStyle))
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }
    }
}
