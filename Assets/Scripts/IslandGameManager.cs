using UnityEngine;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Gerencia o estado completo do jogo (Menu Inicial, Transição, Gameplay, Pausa, Vitória)
/// e provê interface estilizada minimalista, feedback sonoro procedural e controle de áudio/saída.
/// </summary>
public class IslandGameManager : MonoBehaviour
{
    public static IslandGameManager Instance { get; private set; }

    public enum GameState
    {
        MainMenu,
        Transitioning,
        Playing,
        Paused,
        Victory
    }

    [Header("Estado do Jogo")]
    public GameState currentState = GameState.MainMenu;

    [Header("Regras da Expedição")]
    public int targetCoins = 12;
    public int totalItemsCollected = 0;
    public bool isVictory = false;
    public string lastCollectedItemName = "";
    private float lastCollectDisplayTimer = 0f;
    private static bool restartDirectlyToGame = false;

    [Header("Cronômetro")]
    public float gameTimer = 0f;

    [Header("Menu e Animações")]
    public float menuAlpha = 1f;
    public bool showControlsModal = false;
    public bool showExitConfirmModal = false;
    public bool isAudioMuted = false;

    // Componente de Áudio para efeitos sonoros procedurais e trilha musical
    private static AudioSource audioSource;
    private static AudioSource ambientAudioSource;
    private static AudioSource musicAudioSource;
    private static AudioClip coinSound;
    private static AudioClip victorySound;
    private static AudioClip menuClickSound;
    private static AudioClip startSound;
    private static AudioClip oceanAmbienceClip;
    private static AudioClip cozyMusicClip;
    private static AudioClip jumpSound;
    private static AudioClip landSound;

    // Texturas procedurais da UI (geradas em runtime para acabamento moderno arredondado)
    private static Texture2D texPanelNavy;
    private static Texture2D texPanelModal;
    private static Texture2D texBtnGoldNormal;
    private static Texture2D texBtnGoldHover;
    private static Texture2D texBtnGoldActive;
    private static Texture2D texBtnSlateNormal;
    private static Texture2D texBtnSlateHover;
    private static Texture2D texBtnSlateActive;
    private static Texture2D texBtnRedNormal;
    private static Texture2D texBtnRedHover;
    private static Texture2D texBtnRedActive;

    // Estilos de GUI em cache
    private GUIStyle stylePanelNavy;
    private GUIStyle stylePanelModal;
    private GUIStyle styleBtnGold;
    private GUIStyle styleBtnSlate;
    private GUIStyle styleBtnRed;
    private GUIStyle styleTitle;
    private GUIStyle styleTitleShadow;
    private GUIStyle styleSubtitle;
    private GUIStyle styleHint;
    private GUIStyle styleCredits;
    private GUIStyle styleHudBox;
    private GUIStyle styleHudCoins;
    private GUIStyle styleHudSub;
    private bool stylesInitialized = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Cria o emissor de áudio principal para efeitos se não existir
        audioSource = gameObject.GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        // Emissores dedicados para ambiente contínuo do mar e música aconchegante
        AudioSource[] allSources = gameObject.GetComponents<AudioSource>();
        if (allSources.Length > 2)
        {
            ambientAudioSource = allSources[1];
            musicAudioSource = allSources[2];
        }
        else if (allSources.Length == 2)
        {
            ambientAudioSource = allSources[1];
            musicAudioSource = gameObject.AddComponent<AudioSource>();
        }
        else
        {
            ambientAudioSource = gameObject.AddComponent<AudioSource>();
            musicAudioSource = gameObject.AddComponent<AudioSource>();
        }

        ambientAudioSource.playOnAwake = false;
        ambientAudioSource.loop = true;
        ambientAudioSource.volume = 0.22f;

        musicAudioSource.playOnAwake = false;
        musicAudioSource.loop = true;
        musicAudioSource.volume = 0.24f;

        // Gera sons procedurais sintetizados matematicamente
        GenerateProceduralAudio();

        // Gera texturas suaves para interface minimalista
        GenerateProceduralTextures();

        // Garante o gerenciador de atmosfera (Pôr do Sol Dourado / Golden Hour)
        if (gameObject.GetComponent<IslandAtmosphereManager>() == null)
        {
            gameObject.AddComponent<IslandAtmosphereManager>();
        }
    }

    void Start()
    {
        // Inicia o som de mar contínuo relaxante e a música tropical aconchegante
        if (ambientAudioSource != null && oceanAmbienceClip != null && !ambientAudioSource.isPlaying)
        {
            ambientAudioSource.clip = oceanAmbienceClip;
            ambientAudioSource.Play();
        }

        if (musicAudioSource != null && cozyMusicClip != null && !musicAudioSource.isPlaying)
        {
            musicAudioSource.clip = cozyMusicClip;
            musicAudioSource.Play();
        }

        // Se foi clicado em "Jogar Novamente", inicia direto no gameplay!
        if (restartDirectlyToGame)
        {
            restartDirectlyToGame = false;
            currentState = GameState.Playing;
            menuAlpha = 0f;
            if (CameraFollow.Instance != null)
            {
                CameraFollow.Instance.mode = CameraFollow.CameraMode.GameplayFollow;
            }
        }
        else
        {
            currentState = GameState.MainMenu;
            menuAlpha = 1f;
        }

        CollectibleItem[] allCoins = FindObjectsByType<CollectibleItem>();
        if (allCoins != null && allCoins.Length > 0)
        {
            targetCoins = allCoins.Length;
        }
    }

    void Update()
    {
        // Tratamento global de modais (fechar com ESC, Espaço ou Enter)
        if (showControlsModal)
        {
            if (WasPauseHotkeyPressed() || WasStartHotkeyPressed())
            {
                showControlsModal = false;
                PlayMenuClickSound();
                return;
            }
        }
        else if (showExitConfirmModal)
        {
            if (WasPauseHotkeyPressed())
            {
                showExitConfirmModal = false;
                PlayMenuClickSound();
                return;
            }
        }

        // Fading suave da interface do menu
        if (currentState == GameState.MainMenu)
        {
            menuAlpha = Mathf.MoveTowards(menuAlpha, 1f, Time.deltaTime * 3.5f);

            // Atalho para iniciar (Espaço / Enter / Gamepad A) quando nenhum modal estiver ativo
            if (WasStartHotkeyPressed())
            {
                StartExpedition();
            }
            else if (WasPauseHotkeyPressed())
            {
                showExitConfirmModal = true;
                PlayMenuClickSound();
            }
        }
        else if (currentState == GameState.Transitioning)
        {
            menuAlpha = Mathf.MoveTowards(menuAlpha, 0f, Time.deltaTime * 2.2f);
        }
        // Cronômetro apenas durante o gameplay ativo
        else if (currentState == GameState.Playing)
        {
            gameTimer += Time.deltaTime;

            // Tecla ESC ou botão Start para pausar
            if (WasPauseHotkeyPressed())
            {
                currentState = GameState.Paused;
                PlayMenuClickSound();
            }
        }
        else if (currentState == GameState.Paused)
        {
            if (WasPauseHotkeyPressed())
            {
                currentState = GameState.Playing;
                PlayMenuClickSound();
            }
        }
    }

    private bool WasStartHotkeyPressed()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            if (Keyboard.current.spaceKey.wasPressedThisFrame ||
                Keyboard.current.enterKey.wasPressedThisFrame ||
                Keyboard.current.numpadEnterKey.wasPressedThisFrame)
            {
                return true;
            }
        }
        if (Gamepad.current != null)
        {
            if (Gamepad.current.buttonSouth.wasPressedThisFrame ||
                Gamepad.current.startButton.wasPressedThisFrame)
            {
                return true;
            }
        }
        return false;
#else
        return Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return);
#endif
    }

    private bool WasPauseHotkeyPressed()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            return true;
        }
        if (Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame)
        {
            return true;
        }
        return false;
#else
        return Input.GetKeyDown(KeyCode.Escape);
#endif
    }

    public bool CanPlayerMove()
    {
        return currentState == GameState.Playing;
    }

    public void StartExpedition()
    {
        if (currentState != GameState.MainMenu) return;

        PlayStartSound();
        currentState = GameState.Transitioning;

        // Inicia a transição cinematográfica da câmera de volta para o jogador
        if (CameraFollow.Instance != null)
        {
            CameraFollow.Instance.StartTransitionToGameplay(() =>
            {
                currentState = GameState.Playing;
            });
        }
        else
        {
            currentState = GameState.Playing;
        }
    }

    public void ReturnToMainMenu()
    {
        showControlsModal = false;
        showExitConfirmModal = false;
        restartDirectlyToGame = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void RestartGame()
    {
        showControlsModal = false;
        showExitConfirmModal = false;
        restartDirectlyToGame = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ToggleAudio()
    {
        isAudioMuted = !isAudioMuted;
        AudioListener.volume = isAudioMuted ? 0f : 1f;
        PlayMenuClickSound();
    }

    public void QuitGame()
    {
        PlayMenuClickSound();
        Debug.Log("<color=cyan>[TinyIsland]</color> Fechando o jogo com sucesso!");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void AddScore(int amount, string itemName)
    {
        if (isVictory) return;

        totalItemsCollected += amount;
        lastCollectedItemName = itemName;
        lastCollectDisplayTimer = 3.5f;
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
        currentState = GameState.Victory;
        Debug.Log($"<color=yellow><b>[TinyIsland] VITÓRIA!</b> Todas as {targetCoins} moedas foram coletadas!</color>");

        // Abre o baú do tesouro
        if (TreasureChest.Instance != null)
        {
            TreasureChest.Instance.OpenChest();
        }
    }

    // ==========================================
    // SISTEMA DE ÁUDIO PROCEDURAL SINTETIZADO
    // ==========================================

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

    public static void PlayMenuClickSound()
    {
        if (audioSource != null && menuClickSound != null)
        {
            audioSource.PlayOneShot(menuClickSound, 0.6f);
        }
    }

    public static void PlayStartSound()
    {
        if (audioSource != null && startSound != null)
        {
            audioSource.PlayOneShot(startSound, 0.85f);
        }
    }

    public static void PlayJumpSound()
    {
        if (audioSource != null && jumpSound != null)
        {
            audioSource.PlayOneShot(jumpSound, 0.45f);
        }
    }

    public static void PlayLandSound()
    {
        if (audioSource != null && landSound != null)
        {
            audioSource.PlayOneShot(landSound, 0.35f);
        }
    }

    private void GenerateProceduralAudio()
    {
        int sampleRate = 44100;

        // 1. Som de Moeda: arpeggio agudo suave de 2 notas (B5 -> E6)
        int coinSamples = (int)(sampleRate * 0.22f);
        float[] coinData = new float[coinSamples];
        for (int i = 0; i < coinSamples; i++)
        {
            float t = (float)i / sampleRate;
            float freq = t < 0.09f ? 987.77f : 1318.51f;
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

        // 3. Som de Clique Minimalista: pop suave e amadeirado
        int clickSamples = (int)(sampleRate * 0.08f);
        float[] clickData = new float[clickSamples];
        for (int i = 0; i < clickSamples; i++)
        {
            float t = (float)i / sampleRate;
            float freq = Mathf.Lerp(450f, 220f, t / 0.08f);
            float envelope = Mathf.Exp(-t * 40f);
            clickData[i] = Mathf.Sin(2 * Mathf.PI * freq * t) * envelope * 0.35f;
        }
        menuClickSound = AudioClip.Create("MenuClick", clickSamples, 1, sampleRate, false);
        menuClickSound.SetData(clickData, 0);

        // 4. Som de Início de Expedição: arpeggio ascendente acolhedor (D5, F#5, A5, D6)
        int startSamples = (int)(sampleRate * 0.65f);
        float[] startData = new float[startSamples];
        float[] startNotes = new float[] { 587.33f, 739.99f, 880.00f, 1174.66f };
        for (int i = 0; i < startSamples; i++)
        {
            float t = (float)i / sampleRate;
            int nIdx = Mathf.Clamp((int)(t / 0.13f), 0, startNotes.Length - 1);
            float noteTime = t - (nIdx * 0.13f);
            float freq = startNotes[nIdx];
            float envelope = Mathf.Exp(-noteTime * 7.5f);
            startData[i] = Mathf.Sin(2 * Mathf.PI * freq * t) * envelope * 0.45f;
        }
        startSound = AudioClip.Create("StartExpedition", startSamples, 1, sampleRate, false);
        startSound.SetData(startData, 0);

        // 5. Áudio Ambiente Contínuo de Ondas do Mar & Brisa (Loop perfeito de 8 segundos)
        int oceanDurationSec = 8;
        int oceanSamples = sampleRate * oceanDurationSec;
        float[] oceanData = new float[oceanSamples];
        System.Random rng = new System.Random(42);

        float filterState = 0f;
        for (int i = 0; i < oceanSamples; i++)
        {
            float t = (float)i / sampleRate;

            // Dois ciclos de marés harmônicos com período de 8s e 4s (garante continuidade matemática no loop)
            float swell1 = (Mathf.Sin(2f * Mathf.PI * (t / 4.0f) - Mathf.PI * 0.5f) + 1f) * 0.5f;
            float swell2 = (Mathf.Sin(2f * Mathf.PI * (t / 8.0f) + Mathf.PI * 0.25f) + 1f) * 0.5f;
            float swell = swell1 * 0.7f + swell2 * 0.3f;

            // Filtro IIR passa-baixas modulando frequência de corte suave (espuma e quebra das ondas)
            float cutoff = Mathf.Lerp(160f, 680f, swell);
            float rc = 1f / (2f * Mathf.PI * cutoff);
            float dt = 1f / sampleRate;
            float alpha = dt / (rc + dt);

            float whiteNoise = (float)(rng.NextDouble() * 2.0 - 1.0);
            filterState += alpha * (whiteNoise - filterState);

            // Brisa marinha suave de baixa frequência
            float breeze = Mathf.Sin(2f * Mathf.PI * 110f * t) * 0.035f * swell;

            oceanData[i] = (filterState * 0.30f * (0.35f + 0.65f * swell)) + breeze;
        }

        // Cross-fade suave de 0.5s nas extremidades para eliminar estalos no looping
        int crossfadeSamples = (int)(sampleRate * 0.5f);
        for (int i = 0; i < crossfadeSamples; i++)
        {
            float blend = (float)i / crossfadeSamples;
            int tailIdx = oceanSamples - crossfadeSamples + i;
            float mixed = Mathf.Lerp(oceanData[tailIdx], oceanData[i], blend);
            oceanData[i] = mixed;
            oceanData[tailIdx] = mixed;
        }

        oceanAmbienceClip = AudioClip.Create("OceanWavesAmbience", oceanSamples, 1, sampleRate, false);
        oceanAmbienceClip.SetData(oceanData, 0);

        // 6. Som de Pulo Fofo (Whoosh ascendente de ~0.14s)
        int jumpSamples = (int)(sampleRate * 0.14f);
        float[] jumpData = new float[jumpSamples];
        for (int i = 0; i < jumpSamples; i++)
        {
            float t = (float)i / sampleRate;
            float progress = (float)i / jumpSamples;
            float freq = Mathf.Lerp(280f, 540f, progress);
            float envelope = Mathf.Sin(progress * Mathf.PI) * (1f - progress * 0.4f);
            jumpData[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * envelope * 0.32f;
        }
        jumpSound = AudioClip.Create("PlayerJump", jumpSamples, 1, sampleRate, false);
        jumpSound.SetData(jumpData, 0);

        // 7. Som de Aterrissagem Suave na Areia (~0.09s)
        int landSamples = (int)(sampleRate * 0.09f);
        float[] landData = new float[landSamples];
        for (int i = 0; i < landSamples; i++)
        {
            float t = (float)i / sampleRate;
            float progress = (float)i / landSamples;
            float freq = Mathf.Lerp(130f, 65f, progress);
            float envelope = Mathf.Exp(-progress * 5.5f);
            float sandCrunch = ((float)(rng.NextDouble() * 2.0 - 1.0)) * 0.12f * envelope;
            landData[i] = (Mathf.Sin(2f * Mathf.PI * freq * t) * 0.22f + sandCrunch) * envelope;
        }
        landSound = AudioClip.Create("PlayerLand", landSamples, 1, sampleRate, false);
        landSound.SetData(landData, 0);

        // 8. Trilha Sonora Aconchegante Tropical (Melodia Acústica Kalimba + Ukulele + Baixo)
        GenerateCozyAcousticMusic(sampleRate);
    }

    /// <summary>
    /// Gera a melodia acústica relaxante de Tiny Island sintetizada proceduralmente.
    /// Loop de 16 compassos a 84 BPM (~45.7 segundos) com timbres de Kalimba, dedilhado de Ukulele e contrabaixo suave.
    /// </summary>
    private static void GenerateCozyAcousticMusic(int sampleRate)
    {
        float bpm = 84f;
        float beatDuration = 60f / bpm; // ~0.7142857s
        float barDuration = beatDuration * 4f; // ~2.857143s
        int totalBars = 16;
        float totalDuration = totalBars * barDuration; // ~45.714s
        int totalSamples = (int)(sampleRate * totalDuration);
        float[] musicData = new float[totalSamples];

        // Frequências das notas fundamentais (Hz)
        float C2 = 65.41f, D2 = 73.42f, E2 = 82.41f, F2 = 87.31f, G2 = 98.00f, A2 = 110.00f, B2 = 123.47f;
        float C3 = 130.81f, D3 = 146.83f, E3 = 164.81f, F3 = 174.61f, G3 = 196.00f, A3 = 220.00f, B3 = 246.94f;
        float C4 = 261.63f, D4 = 293.66f, E4 = 329.63f, F4 = 349.23f, G4 = 392.00f, A4 = 440.00f, B4 = 493.88f;
        float C5 = 523.25f, D5 = 587.33f, E5 = 659.25f, F5 = 698.46f, G5 = 783.99f, A5 = 880.00f, B5 = 987.77f;
        float C6 = 1046.50f, D6 = 1174.66f;

        // Progressão harmônica dos 16 compassos:
        // C - G - Am - F | C - Em - F - G7 | Am - Em - F - C | Dm7 - Em7 - F - G
        float[] chordRootsBass = new float[16] { C2, G2, A2, F2, C2, E2, F2, G2, A2, E2, F2, C2, D2, E2, F2, G2 };

        // Arpeggios de dedilhado (Ukulele / Violão Nylon): 4 notas por compasso repetidas
        float[][] chordArpNotes = new float[16][]
        {
            new float[] { C3, G3, E4, C4 }, // C
            new float[] { G2 * 2f, D3, B3, G3 }, // G
            new float[] { A2 * 2f, E3, C4, A3 }, // Am
            new float[] { F2 * 2f, C3, A3, F3 }, // F
            new float[] { C3, G3, E4, C4 }, // C
            new float[] { E2 * 2f, B2 * 2f, G3, E3 }, // Em
            new float[] { F2 * 2f, C3, A3, F3 }, // F
            new float[] { G2 * 2f, D3, B3, F3 }, // G7
            new float[] { A2 * 2f, E3, C4, A3 }, // Am
            new float[] { E2 * 2f, B2 * 2f, G3, E3 }, // Em
            new float[] { F2 * 2f, C3, A3, F3 }, // F
            new float[] { C3, G3, E4, C4 }, // C
            new float[] { D3, A3, F4, C4 }, // Dm7
            new float[] { E3, B3, G4, D4 }, // Em7
            new float[] { F2 * 2f, C3, A3, F4 }, // F
            new float[] { G2 * 2f, D3, B3, G4 }  // G
        };

        // Melodia cantante de Kalimba (Thumb Piano com chime de sino):
        // (compasso, tempo no compasso [0..4 em batidas], nota)
        (int bar, float beat, float freq)[] melodyNotes = new (int, float, float)[]
        {
            // Parte A (Luz solar acolhedora)
            (0, 0.0f, E5), (0, 1.5f, G5), (0, 2.5f, A5), (0, 3.5f, G5),
            (1, 0.0f, D5), (1, 1.5f, E5), (1, 2.5f, D5),
            (2, 0.0f, C5), (2, 1.0f, E5), (2, 2.0f, D5), (2, 3.0f, C5),
            (3, 0.0f, D5), (3, 1.5f, C5), (3, 2.5f, A4), (3, 3.25f, C5),

            // Parte B (Curiosidade & Exploração tropical)
            (4, 0.0f, E5), (4, 1.0f, G5), (4, 2.0f, C6), (4, 3.0f, D6),
            (5, 0.0f, B5), (5, 1.5f, G5), (5, 2.5f, E5),
            (6, 0.0f, A5), (6, 1.0f, C6), (6, 2.0f, A5), (6, 3.0f, G5),
            (7, 0.0f, F5), (7, 1.0f, E5), (7, 2.0f, D5), (7, 3.0f, G5),

            // Parte C (Aconchego contemplativo da ilha)
            (8, 0.0f, C5), (8, 1.0f, D5), (8, 2.0f, E5), (8, 3.0f, G5),
            (9, 0.0f, G5), (9, 1.5f, E5), (9, 2.5f, B4), (9, 3.0f, C5),
            (10, 0.0f, A4), (10, 1.0f, C5), (10, 2.0f, E5), (10, 3.0f, D5),
            (11, 0.0f, C5), (11, 1.5f, G4), (11, 2.5f, C5),

            // Parte D (Resolução e volta fluida ao início)
            (12, 0.0f, D5), (12, 1.0f, F5), (12, 2.0f, A5), (12, 3.0f, G5),
            (13, 0.0f, E5), (13, 1.0f, G5), (13, 2.0f, B5), (13, 3.0f, C6),
            (14, 0.0f, D6), (14, 1.0f, C6), (14, 2.0f, A5), (14, 3.0f, F5),
            (15, 0.0f, G5), (15, 1.0f, D5), (15, 2.0f, E5), (15, 3.0f, D5)
        };

        // Renderiza as camadas sonoras combinadas
        for (int i = 0; i < totalSamples; i++)
        {
            float t = (float)i / sampleRate;
            int currentBar = Mathf.Clamp((int)(t / barDuration), 0, totalBars - 1);
            float barTime = t - (currentBar * barDuration);

            float sampleMix = 0f;

            // 1. Camada de Contrabaixo Acústico Suave (bate no tempo 0 e tempo 2.0)
            float bassTime1 = barTime;
            float bassTime2 = barTime - (beatDuration * 2f);
            float bassEnv1 = Mathf.Exp(-bassTime1 * 2.2f) * (1f - Mathf.Exp(-bassTime1 * 80f));
            float bassFreq = chordRootsBass[currentBar];
            float bass1 = (Mathf.Sin(2f * Mathf.PI * bassFreq * t) + 0.18f * Mathf.Sin(4f * Mathf.PI * bassFreq * t)) * bassEnv1;
            float bass2 = 0f;
            if (bassTime2 >= 0f)
            {
                float bassEnv2 = Mathf.Exp(-bassTime2 * 2.5f) * (1f - Mathf.Exp(-bassTime2 * 80f));
                bass2 = (Mathf.Sin(2f * Mathf.PI * bassFreq * 1.5f * t) + 0.15f * Mathf.Sin(4f * Mathf.PI * bassFreq * t)) * bassEnv2 * 0.7f;
            }
            sampleMix += (bass1 + bass2) * 0.28f;

            // 2. Camada de Dedilhado de Ukulele / Violão (8 notas por compasso, colcheias)
            float eighthDuration = beatDuration * 0.5f;
            int eighthIdx = Mathf.Clamp((int)(barTime / eighthDuration), 0, 7);
            float noteTimeArp = barTime - (eighthIdx * eighthDuration);
            float[] arpNotes = chordArpNotes[currentBar];
            float arpFreq = arpNotes[eighthIdx % arpNotes.Length];
            float arpEnv = Mathf.Exp(-noteTimeArp * 7.2f) * (1f - Mathf.Exp(-noteTimeArp * 180f));
            float arpWave = (Mathf.Sin(2f * Mathf.PI * arpFreq * t) + 0.25f * Mathf.Sin(4f * Mathf.PI * arpFreq * t) + 0.08f * Mathf.Sin(6f * Mathf.PI * arpFreq * t)) * arpEnv;
            sampleMix += arpWave * 0.18f;

            musicData[i] = sampleMix;
        }

        // 3. Aplica a melodia da Kalimba por cima com timbres de sino ressonante
        for (int m = 0; m < melodyNotes.Length; m++)
        {
            var note = melodyNotes[m];
            float noteStartTime = (note.bar * barDuration) + (note.beat * beatDuration);
            int startSample = (int)(noteStartTime * sampleRate);
            int noteDurationSamples = (int)(sampleRate * (beatDuration * 2.2f));

            for (int s = 0; s < noteDurationSamples; s++)
            {
                int sampleIdx = startSample + s;
                if (sampleIdx >= totalSamples) break;

                float tNote = (float)s / sampleRate;
                float tGlobal = (float)sampleIdx / sampleRate;

                // Timbre rico de Kalimba tropical: fundamental + oitava + harmônico ímpar + toque de sino
                float kEnvelope = Mathf.Exp(-tNote * 3.6f) * (1f - Mathf.Exp(-tNote * 280f));
                float kSound = (
                    Mathf.Sin(2f * Mathf.PI * note.freq * tGlobal) * 1.0f +
                    Mathf.Sin(4f * Mathf.PI * note.freq * tGlobal) * 0.38f +
                    Mathf.Sin(6f * Mathf.PI * note.freq * tGlobal) * 0.12f +
                    Mathf.Sin(5.42f * 2f * Mathf.PI * note.freq * tGlobal) * 0.08f
                ) * kEnvelope * 0.26f;

                musicData[sampleIdx] += kSound;
            }
        }

        // 4. Cross-fade suave de 0.8s nas bordas para loop contínuo infinito e sem estalo
        int musicCrossfade = (int)(sampleRate * 0.8f);
        for (int i = 0; i < musicCrossfade; i++)
        {
            float blend = (float)i / musicCrossfade;
            int tailIdx = totalSamples - musicCrossfade + i;
            float mixed = Mathf.Lerp(musicData[tailIdx], musicData[i], blend);
            musicData[i] = mixed;
            musicData[tailIdx] = mixed;
        }

        cozyMusicClip = AudioClip.Create("CozyIslandMelody", totalSamples, 1, sampleRate, false);
        cozyMusicClip.SetData(musicData, 0);
    }

    // ==========================================
    // GERAÇÃO DE TEXTURAS PROCEDURAIS PARA UI
    // ==========================================

    private void GenerateProceduralTextures()
    {
        if (texPanelNavy != null) return;

        // Painel Navy Glassmorphism com borda dourada sutil
        texPanelNavy = MakeRoundedBoxTexture(48, 48, 14, new Color(0.06f, 0.12f, 0.18f, 0.88f), new Color(1.0f, 0.84f, 0.35f, 0.45f), 2);

        // Painel Modal
        texPanelModal = MakeRoundedBoxTexture(48, 48, 14, new Color(0.05f, 0.10f, 0.15f, 0.95f), new Color(1.0f, 0.84f, 0.35f, 0.6f), 2);

        // Botão Dourado Primário (Normal, Hover, Ativo)
        texBtnGoldNormal = MakeRoundedBoxTexture(48, 48, 12, new Color(0.96f, 0.74f, 0.18f, 1f), new Color(1f, 0.88f, 0.45f, 1f), 1);
        texBtnGoldHover = MakeRoundedBoxTexture(48, 48, 12, new Color(1.0f, 0.84f, 0.32f, 1f), new Color(1f, 1f, 0.8f, 1f), 2);
        texBtnGoldActive = MakeRoundedBoxTexture(48, 48, 12, new Color(0.85f, 0.62f, 0.12f, 1f), new Color(0.7f, 0.5f, 0.1f, 1f), 1);

        // Botão Secundário Slate (Normal, Hover, Ativo)
        texBtnSlateNormal = MakeRoundedBoxTexture(48, 48, 12, new Color(0.12f, 0.22f, 0.32f, 0.90f), new Color(0.28f, 0.45f, 0.62f, 0.5f), 1);
        texBtnSlateHover = MakeRoundedBoxTexture(48, 48, 12, new Color(0.18f, 0.32f, 0.46f, 0.98f), new Color(1.0f, 0.84f, 0.35f, 0.75f), 2);
        texBtnSlateActive = MakeRoundedBoxTexture(48, 48, 12, new Color(0.08f, 0.16f, 0.24f, 1f), new Color(0.28f, 0.45f, 0.62f, 0.8f), 1);

        // Botão Vermelho de Sair (Normal, Hover, Ativo)
        texBtnRedNormal = MakeRoundedBoxTexture(48, 48, 12, new Color(0.42f, 0.12f, 0.15f, 0.90f), new Color(0.85f, 0.25f, 0.28f, 0.6f), 1);
        texBtnRedHover = MakeRoundedBoxTexture(48, 48, 12, new Color(0.65f, 0.16f, 0.20f, 0.98f), new Color(1.0f, 0.45f, 0.45f, 0.9f), 2);
        texBtnRedActive = MakeRoundedBoxTexture(48, 48, 12, new Color(0.30f, 0.08f, 0.10f, 1f), new Color(0.6f, 0.15f, 0.18f, 0.8f), 1);
    }

    private static Texture2D MakeRoundedBoxTexture(int width, int height, int radius, Color fillColor, Color borderColor, int borderWidth = 0)
    {
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        tex.hideFlags = HideFlags.DontSave;
        Color[] pixels = new Color[width * height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float dx = 0;
                if (x < radius) dx = radius - x;
                else if (x >= width - radius) dx = x - (width - radius - 1);

                float dy = 0;
                if (y < radius) dy = radius - y;
                else if (y >= height - radius) dy = y - (height - radius - 1);

                float dist = Mathf.Sqrt(dx * dx + dy * dy);

                if (dist > radius)
                {
                    float alphaEdge = Mathf.Clamp01(radius + 1f - dist);
                    pixels[y * width + x] = new Color(fillColor.r, fillColor.g, fillColor.b, fillColor.a * alphaEdge);
                }
                else if (borderWidth > 0 && (dist > radius - borderWidth || x < borderWidth || x >= width - borderWidth || y < borderWidth || y >= height - borderWidth))
                {
                    pixels[y * width + x] = borderColor;
                }
                else
                {
                    pixels[y * width + x] = fillColor;
                }
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }

    private void InitializeStyles()
    {
        if (stylesInitialized && stylePanelNavy != null) return;
        GenerateProceduralTextures();

        stylePanelNavy = new GUIStyle(GUI.skin.box)
        {
            normal = { background = texPanelNavy, textColor = Color.white },
            border = new RectOffset(14, 14, 14, 14),
            padding = new RectOffset(20, 20, 20, 20),
            alignment = TextAnchor.MiddleCenter
        };

        stylePanelModal = new GUIStyle(GUI.skin.box)
        {
            normal = { background = texPanelModal, textColor = Color.white },
            border = new RectOffset(14, 14, 14, 14),
            padding = new RectOffset(24, 24, 24, 24),
            alignment = TextAnchor.UpperCenter
        };

        // Botão Dourado Primário
        styleBtnGold = new GUIStyle(GUI.skin.button)
        {
            normal = { background = texBtnGoldNormal, textColor = new Color(0.14f, 0.08f, 0.02f) },
            hover = { background = texBtnGoldHover, textColor = new Color(0.12f, 0.06f, 0.0f) },
            active = { background = texBtnGoldActive, textColor = Color.black },
            border = new RectOffset(12, 12, 12, 12),
            fontSize = 16,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };

        // Botão Slate Secundário
        styleBtnSlate = new GUIStyle(GUI.skin.button)
        {
            normal = { background = texBtnSlateNormal, textColor = Color.white },
            hover = { background = texBtnSlateHover, textColor = new Color(1.0f, 0.90f, 0.4f) },
            active = { background = texBtnSlateActive, textColor = Color.white },
            border = new RectOffset(12, 12, 12, 12),
            fontSize = 14,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };

        // Botão Vermelho de Saída
        styleBtnRed = new GUIStyle(GUI.skin.button)
        {
            normal = { background = texBtnRedNormal, textColor = new Color(1f, 0.88f, 0.88f) },
            hover = { background = texBtnRedHover, textColor = Color.white },
            active = { background = texBtnRedActive, textColor = Color.white },
            border = new RectOffset(12, 12, 12, 12),
            fontSize = 14,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };

        // Título Principal Estilizado
        styleTitle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 38,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        styleTitle.normal.textColor = new Color(1.0f, 0.85f, 0.28f);

        styleTitleShadow = new GUIStyle(styleTitle);
        styleTitleShadow.normal.textColor = new Color(0.02f, 0.05f, 0.08f, 0.65f);

        // Subtítulo
        styleSubtitle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 15,
            fontStyle = FontStyle.Normal,
            alignment = TextAnchor.MiddleCenter
        };
        styleSubtitle.normal.textColor = new Color(0.85f, 0.92f, 0.98f);

        // Dica
        styleHint = new GUIStyle(GUI.skin.label)
        {
            fontSize = 12,
            fontStyle = FontStyle.Italic,
            alignment = TextAnchor.MiddleCenter
        };
        styleHint.normal.textColor = new Color(0.75f, 0.85f, 0.90f, 0.8f);

        // Créditos
        styleCredits = new GUIStyle(GUI.skin.label)
        {
            fontSize = 12,
            alignment = TextAnchor.MiddleCenter
        };
        styleCredits.normal.textColor = new Color(1.0f, 0.88f, 0.5f, 0.75f);

        // HUD em Jogo
        styleHudBox = new GUIStyle(GUI.skin.box)
        {
            normal = { background = texPanelNavy, textColor = Color.white },
            border = new RectOffset(14, 14, 14, 14)
        };

        styleHudCoins = new GUIStyle(GUI.skin.label)
        {
            fontSize = 16,
            fontStyle = FontStyle.Bold
        };
        styleHudCoins.normal.textColor = new Color(1.0f, 0.85f, 0.2f);

        styleHudSub = new GUIStyle(GUI.skin.label)
        {
            fontSize = 12
        };
        styleHudSub.normal.textColor = new Color(0.9f, 0.9f, 0.9f);

        stylesInitialized = true;
    }

    // ==========================================
    // RENDERIZAÇÃO DA INTERFACE (OnGUI)
    // ==========================================

    void OnGUI()
    {
        InitializeStyles();

        bool hasModal = showControlsModal || showExitConfirmModal;

        // 1. MENU INICIAL (com efeito suave de fade in/out)
        if (menuAlpha > 0.005f && (currentState == GameState.MainMenu || currentState == GameState.Transitioning))
        {
            Color oldGuiColor = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, menuAlpha);

            GUI.enabled = !hasModal;
            DrawMainMenu();
            GUI.enabled = true;

            GUI.color = oldGuiColor;
        }

        // 2. HUD DE GAMEPLAY (visível apenas em Playing e Victory)
        if (currentState == GameState.Playing || currentState == GameState.Victory)
        {
            DrawGameplayHud();
        }

        // 3. MENU DE PAUSA & OPÇÕES (acionado ao apertar ESC durante o jogo)
        if (currentState == GameState.Paused)
        {
            GUI.enabled = !hasModal;
            DrawPauseMenu();
            GUI.enabled = true;
        }

        // 4. TELA DE VITÓRIA
        if (currentState == GameState.Victory)
        {
            GUI.enabled = !hasModal;
            DrawVictoryPanel();
            GUI.enabled = true;
        }

        // 5. MODAIS GLOBAIS (desenhados por último, por cima de tudo)
        if (showControlsModal)
        {
            GUI.enabled = true;
            int oldDepth = GUI.depth;
            GUI.depth = -100;
            DrawControlsModal();
            GUI.depth = oldDepth;
        }
        else if (showExitConfirmModal)
        {
            GUI.enabled = true;
            int oldDepth = GUI.depth;
            GUI.depth = -100;
            DrawExitConfirmModal();
            GUI.depth = oldDepth;
        }
    }

    private void DrawMainMenu()
    {
        // Animação de flutuação vertical no título
        float floatOffset = Mathf.Sin(Time.time * 2.2f) * 5f;

        float screenW = Screen.width;
        float screenH = Screen.height;

        // Dimensões do Card Central do Menu
        float cardW = 380f;
        float cardH = 375f;
        float cardX = (screenW - cardW) * 0.5f;
        float cardY = (screenH - cardH) * 0.5f + 35f;

        // Posição do Título (acima do card, flutuando)
        float titleY = cardY - 105f + floatOffset;

        // Sombra do título
        GUI.Label(new Rect(cardX + 2, titleY + 2, cardW, 46), "TINY ISLAND", styleTitleShadow);
        // Título principal
        GUI.Label(new Rect(cardX, titleY, cardW, 46), "TINY ISLAND", styleTitle);

        // Subtítulo
        GUI.Label(new Rect(cardX, titleY + 48, cardW, 24), "Uma Micro-Aventura Tropical", styleSubtitle);

        // Painel Central translúcido
        GUI.Box(new Rect(cardX, cardY, cardW, cardH), "", stylePanelNavy);

        // Botões do Menu Inicial
        float btnW = 280f;
        float btnX = cardX + (cardW - btnW) * 0.5f;
        float startY = cardY + 26f;

        // Botão 1: Começar Expedição (Dourado vibrante)
        if (GUI.Button(new Rect(btnX, startY, btnW, 44), "COMEÇAR EXPEDIÇÃO", styleBtnGold))
        {
            StartExpedition();
        }

        // Botão 2: Como Jogar (Slate elegante)
        if (GUI.Button(new Rect(btnX, startY + 52, btnW, 36), "Como Jogar", styleBtnSlate))
        {
            PlayMenuClickSound();
            showControlsModal = true;
        }

        // Botão 3: Som Ativado / Mudo
        string soundLabel = isAudioMuted ? "Som: Desativado" : "Som: Ativado";
        if (GUI.Button(new Rect(btnX, startY + 96, btnW, 36), soundLabel, styleBtnSlate))
        {
            ToggleAudio();
        }

        // Botão 4: Clima (Dia / Pôr do Sol Dourado)
        string timeLabel = (IslandAtmosphereManager.Instance != null && IslandAtmosphereManager.Instance.currentTimeOfDay == IslandAtmosphereManager.TimeOfDayPreset.Sunset)
            ? "Clima: Pôr do Sol Dourado"
            : "Clima: Dia Ensolarado";
        if (GUI.Button(new Rect(btnX, startY + 140, btnW, 36), timeLabel, styleBtnSlate))
        {
            if (IslandAtmosphereManager.Instance != null)
            {
                IslandAtmosphereManager.Instance.ToggleTimeOfDay();
            }
        }

        // Botão 5: Sair do Jogo
        if (GUI.Button(new Rect(btnX, startY + 184, btnW, 36), "Sair do Jogo", styleBtnRed))
        {
            PlayMenuClickSound();
            showExitConfirmModal = true;
        }

        // Dica de teclado / controle
        GUI.Label(new Rect(cardX, startY + 232, cardW, 22), "[ESPAÇO] Iniciar   |   [T] Pôr do Sol   |   [ESC] Sair", styleHint);

        // Créditos com o nome do criador
        GUI.Label(new Rect(cardX, startY + 280, cardW, 24), "Tiny Island v1.3 - Feito por Edu Braga", styleCredits);
    }

    private void DrawControlsModal()
    {
        float screenW = Screen.width;
        float screenH = Screen.height;

        float modalW = 500f;
        float modalH = 360f;
        float modalX = (screenW - modalW) * 0.5f;
        float modalY = (screenH - modalH) * 0.5f;
        Rect modalRect = new Rect(modalX, modalY, modalW, modalH);

        // Fundo escuro que bloqueia cliques na interface de trás
        Color prevColor = GUI.color;
        GUI.color = new Color(0.02f, 0.05f, 0.08f, 0.75f);
        GUI.DrawTexture(new Rect(0, 0, screenW, screenH), Texture2D.whiteTexture);
        GUI.color = prevColor;

        // Se o jogador clicar fora do card modal no fundo escuro, fecha suavemente
        Event e = Event.current;
        if (e != null && e.type == EventType.MouseDown && !modalRect.Contains(e.mousePosition))
        {
            showControlsModal = false;
            PlayMenuClickSound();
            e.Use();
            return;
        }

        GUI.Box(modalRect, "", stylePanelModal);

        // Cabeçalho do Modal
        GUIStyle modalHeader = new GUIStyle(styleTitle) { fontSize = 24 };
        modalHeader.normal.textColor = new Color(1f, 0.88f, 0.35f);
        GUI.Label(new Rect(modalX, modalY + 22, modalW, 32), "GUIA DO EXPLORADOR", modalHeader);

        // Texto do Objetivo
        GUIStyle bodyBold = new GUIStyle(GUI.skin.label) { fontSize = 14, fontStyle = FontStyle.Bold };
        bodyBold.normal.textColor = new Color(1f, 0.84f, 0.2f);

        GUIStyle bodyNormal = new GUIStyle(GUI.skin.label) { fontSize = 13, wordWrap = true };
        bodyNormal.normal.textColor = Color.white;

        GUI.Label(new Rect(modalX + 35, modalY + 68, modalW - 70, 22), "OBJETIVO:", bodyBold);
        GUI.Label(new Rect(modalX + 35, modalY + 92, modalW - 70, 48),
            $"Explore o arquipélago (Farol, Píer, Veleiro, Ponte, Acampamento, Recife e Ruínas) para encontrar as {targetCoins} moedas de ouro e abrir o Baú do Tesouro!", bodyNormal);

        // Controles
        GUI.Label(new Rect(modalX + 35, modalY + 148, modalW - 70, 22), "CONTROLES:", bodyBold);
        GUI.Label(new Rect(modalX + 35, modalY + 172, modalW - 70, 68),
            "- W, A, S, D ou Setas / Left Stick: Caminhar pela ilha\n- ESPAÇO / Botão A: Pular nas pedras, barco e colinas\n- Sem estresse: Cair no mar não dá dano - você volta são e salvo!", bodyNormal);

        // Botão Fechar Modal
        float btnW = 200f;
        float btnH = 44f;
        float btnX = modalX + (modalW - btnW) * 0.5f;
        float btnY = modalY + modalH - 64f;

        if (GUI.Button(new Rect(btnX, btnY, btnW, btnH), "Entendido", styleBtnGold))
        {
            PlayMenuClickSound();
            showControlsModal = false;
        }
    }

    private void DrawExitConfirmModal()
    {
        float screenW = Screen.width;
        float screenH = Screen.height;

        float modalW = 420f;
        float modalH = 205f;
        float modalX = (screenW - modalW) * 0.5f;
        float modalY = (screenH - modalH) * 0.5f;
        Rect modalRect = new Rect(modalX, modalY, modalW, modalH);

        Color prevColor = GUI.color;
        GUI.color = new Color(0.02f, 0.05f, 0.08f, 0.75f);
        GUI.DrawTexture(new Rect(0, 0, screenW, screenH), Texture2D.whiteTexture);
        GUI.color = prevColor;

        Event e = Event.current;
        if (e != null && e.type == EventType.MouseDown && !modalRect.Contains(e.mousePosition))
        {
            showExitConfirmModal = false;
            PlayMenuClickSound();
            e.Use();
            return;
        }

        GUI.Box(modalRect, "", stylePanelModal);

        GUIStyle exitHeader = new GUIStyle(styleTitle) { fontSize = 22 };
        exitHeader.normal.textColor = new Color(1f, 0.84f, 0.35f);
        GUI.Label(new Rect(modalX, modalY + 22, modalW, 30), "SAIR DO JOGO", exitHeader);

        GUIStyle confirmMsg = new GUIStyle(GUI.skin.label)
        {
            fontSize = 14,
            alignment = TextAnchor.MiddleCenter
        };
        confirmMsg.normal.textColor = Color.white;
        GUI.Label(new Rect(modalX + 20, modalY + 68, modalW - 40, 24), "Tem certeza de que deseja fechar o jogo?", confirmMsg);

        float btnW = 150f;
        float gap = 20f;
        float totalW = btnW * 2 + gap;
        float startX = modalX + (modalW - totalW) * 0.5f;
        float btnY = modalY + 120f;

        if (GUI.Button(new Rect(startX, btnY, btnW, 42), "Sim, Sair", styleBtnRed))
        {
            QuitGame();
        }

        if (GUI.Button(new Rect(startX + btnW + gap, btnY, btnW, 42), "Cancelar", styleBtnSlate))
        {
            PlayMenuClickSound();
            showExitConfirmModal = false;
        }
    }

    private void DrawGameplayHud()
    {
        int boxWidth = 350;
        int boxHeight = 68;
        GUI.Box(new Rect(20, 20, boxWidth, boxHeight), "", styleHudBox);

        GUI.Label(new Rect(34, 26, 320, 26), $"Moedas: {totalItemsCollected} / {targetCoins}", styleHudCoins);

        if (!isVictory)
        {
            if (lastCollectDisplayTimer > 0f && !string.IsNullOrEmpty(lastCollectedItemName))
            {
                lastCollectDisplayTimer -= Time.deltaTime;
                styleHudSub.normal.textColor = new Color(1f, 0.92f, 0.45f);
                GUI.Label(new Rect(34, 52, 320, 24), $"+1: {lastCollectedItemName}", styleHudSub);
            }
            else
            {
                styleHudSub.normal.textColor = new Color(0.9f, 0.9f, 0.9f);
                GUI.Label(new Rect(34, 52, 320, 24), "Explore a ilha! [ESPAÇO] para Pular", styleHudSub);
            }
        }
        else
        {
            styleHudSub.normal.textColor = new Color(0.3f, 1f, 0.4f);
            GUI.Label(new Rect(34, 52, 320, 24), "O Baú do Tesouro no Santuário abriu!", styleHudSub);
        }

        // Indicador discreto de tempo no topo direito + atalho de clima e pausa
        GUIStyle timeCornerStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 13,
            alignment = TextAnchor.MiddleRight
        };
        timeCornerStyle.normal.textColor = new Color(1f, 1f, 1f, 0.90f);
        string timeIcon = (IslandAtmosphereManager.Instance != null && IslandAtmosphereManager.Instance.currentTimeOfDay == IslandAtmosphereManager.TimeOfDayPreset.Sunset) ? "🌅 Pôr do Sol" : "☀️ Dia";
        GUI.Label(new Rect(Screen.width - 340, 20, 315, 24), $"Tempo: {gameTimer:F1}s   |   [T] {timeIcon}   |   [ESC] Menu", timeCornerStyle);
    }

    private void DrawPauseMenu()
    {
        float screenW = Screen.width;
        float screenH = Screen.height;

        float panelW = 380f;
        float panelH = 415f;
        float panelX = (screenW - panelW) * 0.5f;
        float panelY = (screenH - panelH) * 0.5f;

        GUI.Box(new Rect(panelX, panelY, panelW, panelH), "", stylePanelNavy);

        GUIStyle pauseHeader = new GUIStyle(styleTitle)
        {
            fontSize = 24
        };
        pauseHeader.normal.textColor = new Color(1f, 0.88f, 0.35f);
        GUI.Label(new Rect(panelX, panelY + 22, panelW, 32), "MENU DE PAUSA", pauseHeader);

        float btnW = 280f;
        float btnX = panelX + (panelW - btnW) * 0.5f;
        float startY = panelY + 68f;

        // 1. Continuar Expedição
        if (GUI.Button(new Rect(btnX, startY, btnW, 40), "Continuar Expedição", styleBtnGold))
        {
            PlayMenuClickSound();
            currentState = GameState.Playing;
        }

        // 2. Alternar Clima / Horário (Dia / Pôr do Sol)
        string atmosphereLabel = (IslandAtmosphereManager.Instance != null && IslandAtmosphereManager.Instance.currentTimeOfDay == IslandAtmosphereManager.TimeOfDayPreset.Sunset)
            ? "Mudar para Dia Ensolarado"
            : "Mudar para Pôr do Sol";
        if (GUI.Button(new Rect(btnX, startY + 46, btnW, 36), atmosphereLabel, styleBtnSlate))
        {
            if (IslandAtmosphereManager.Instance != null)
            {
                IslandAtmosphereManager.Instance.ToggleTimeOfDay();
            }
        }

        // 3. Alternar Som
        string soundLabel = isAudioMuted ? "Som: Desativado" : "Som: Ativado";
        if (GUI.Button(new Rect(btnX, startY + 88, btnW, 36), soundLabel, styleBtnSlate))
        {
            ToggleAudio();
        }

        // 4. Guia de Controles
        if (GUI.Button(new Rect(btnX, startY + 130, btnW, 36), "Controles & Objetivo", styleBtnSlate))
        {
            PlayMenuClickSound();
            showControlsModal = true;
        }

        // 5. Reiniciar Ilha
        if (GUI.Button(new Rect(btnX, startY + 172, btnW, 36), "Reiniciar Ilha", styleBtnSlate))
        {
            PlayMenuClickSound();
            RestartGame();
        }

        // 6. Menu Principal
        if (GUI.Button(new Rect(btnX, startY + 214, btnW, 36), "Menu Principal", styleBtnSlate))
        {
            PlayMenuClickSound();
            ReturnToMainMenu();
        }

        // 7. Sair do Jogo
        if (GUI.Button(new Rect(btnX, startY + 256, btnW, 36), "Sair do Jogo", styleBtnRed))
        {
            PlayMenuClickSound();
            showExitConfirmModal = true;
        }

        // Dica
        GUI.Label(new Rect(panelX, startY + 304, panelW, 20), "Pressione [ESC] para retornar ao jogo", styleHint);
    }

    private void DrawVictoryPanel()
    {
        int panelW = 460;
        int panelH = 260;
        int screenX = (Screen.width - panelW) / 2;
        int screenY = (Screen.height - panelH) / 2 - 20;

        GUI.Box(new Rect(screenX, screenY, panelW, panelH), "", stylePanelNavy);

        GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 24,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        titleStyle.normal.textColor = new Color(1f, 0.84f, 0f);
        GUI.Label(new Rect(screenX, screenY + 20, panelW, 35), "PARABÉNS!", titleStyle);

        GUIStyle msgStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 15,
            alignment = TextAnchor.MiddleCenter
        };
        msgStyle.normal.textColor = Color.white;
        GUI.Label(new Rect(screenX, screenY + 58, panelW, 30), "Você descobriu o Segredo de Tiny Island!", msgStyle);

        GUIStyle timeStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 14,
            fontStyle = FontStyle.Italic,
            alignment = TextAnchor.MiddleCenter
        };
        timeStyle.normal.textColor = new Color(0.9f, 0.9f, 0.9f);
        GUI.Label(new Rect(screenX, screenY + 92, panelW, 25), $"Tempo de Expedição: {gameTimer:F1} segundos", timeStyle);

        // Botões de Ação na Vitória
        float btnW = 190f;
        float gap = 16f;
        float totalBtnW = btnW * 2 + gap;
        float startX = screenX + (panelW - totalBtnW) * 0.5f;
        float btnY = screenY + 130f;

        if (GUI.Button(new Rect(startX, btnY, btnW, 44), "Jogar Novamente", styleBtnGold))
        {
            PlayMenuClickSound();
            RestartGame();
        }

        if (GUI.Button(new Rect(startX + btnW + gap, btnY, btnW, 44), "Menu Inicial", styleBtnSlate))
        {
            PlayMenuClickSound();
            ReturnToMainMenu();
        }

        // Botão de Sair do Jogo na Vitória
        float quitW = 200f;
        if (GUI.Button(new Rect(screenX + (panelW - quitW) * 0.5f, btnY + 54, quitW, 36), "Sair do Jogo", styleBtnRed))
        {
            PlayMenuClickSound();
            showExitConfirmModal = true;
        }

        if (showExitConfirmModal)
        {
            DrawExitConfirmModal();
        }
    }
}
