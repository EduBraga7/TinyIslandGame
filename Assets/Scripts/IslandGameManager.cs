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

    [Header("Cronômetro")]
    public float gameTimer = 0f;

    [Header("Menu e Animações")]
    public float menuAlpha = 1f;
    public bool showControlsModal = false;
    public bool showExitConfirmModal = false;
    public bool isAudioMuted = false;

    // Componente de Áudio para efeitos sonoros procedurais
    private static AudioSource audioSource;
    private static AudioClip coinSound;
    private static AudioClip victorySound;
    private static AudioClip menuClickSound;
    private static AudioClip startSound;

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

        // Cria o emissor de áudio se não existir
        audioSource = gameObject.GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        // Gera sons procedurais sintetizados matematicamente
        GenerateProceduralAudio();

        // Gera texturas suaves para interface minimalista
        GenerateProceduralTextures();
    }

    void Start()
    {
        // Inicia no Menu Principal por padrão
        currentState = GameState.MainMenu;
        menuAlpha = 1f;

        CollectibleItem[] allCoins = FindObjectsByType<CollectibleItem>();
        if (allCoins != null && allCoins.Length > 0)
        {
            targetCoins = allCoins.Length;
        }
    }

    void Update()
    {
        // Fading suave da interface do menu
        if (currentState == GameState.MainMenu)
        {
            menuAlpha = Mathf.MoveTowards(menuAlpha, 1f, Time.deltaTime * 3.5f);

            // Atalho para iniciar (Espaço / Enter / Gamepad A) quando nenhum modal estiver ativo
            if (!showControlsModal && !showExitConfirmModal && WasStartHotkeyPressed())
            {
                StartExpedition();
            }

            // Tecla ESC no Menu Principal
            if (WasPauseHotkeyPressed())
            {
                if (showControlsModal)
                {
                    showControlsModal = false;
                    PlayMenuClickSound();
                }
                else
                {
                    showExitConfirmModal = !showExitConfirmModal;
                    PlayMenuClickSound();
                }
            }
        }
        else if (currentState == GameState.Transitioning)
        {
            menuAlpha = Mathf.MoveTowards(menuAlpha, 0f, Time.deltaTime * 2.2f);
        }

        // Cronômetro apenas durante o gameplay ativo
        if (currentState == GameState.Playing)
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
                if (showControlsModal)
                {
                    showControlsModal = false;
                    PlayMenuClickSound();
                }
                else if (showExitConfirmModal)
                {
                    showExitConfirmModal = false;
                    PlayMenuClickSound();
                }
                else
                {
                    currentState = GameState.Playing;
                    PlayMenuClickSound();
                }
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
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void RestartGame()
    {
        showControlsModal = false;
        showExitConfirmModal = false;
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

        // 1. MENU INICIAL (com efeito suave de fade in/out)
        if (menuAlpha > 0.005f && (currentState == GameState.MainMenu || currentState == GameState.Transitioning))
        {
            Color oldGuiColor = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, menuAlpha);

            DrawMainMenu();

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
            DrawPauseMenu();
        }

        // 4. TELA DE VITÓRIA
        if (currentState == GameState.Victory)
        {
            DrawVictoryPanel();
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
        float cardH = 340f;
        float cardX = (screenW - cardW) * 0.5f;
        float cardY = (screenH - cardH) * 0.5f + 40f;

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
        float startY = cardY + 30f;

        // Botão 1: Começar Expedição (Dourado vibrante)
        if (GUI.Button(new Rect(btnX, startY, btnW, 46), "COMEÇAR EXPEDIÇÃO", styleBtnGold))
        {
            StartExpedition();
        }

        // Botão 2: Como Jogar (Slate elegante)
        if (GUI.Button(new Rect(btnX, startY + 56, btnW, 38), "Como Jogar", styleBtnSlate))
        {
            PlayMenuClickSound();
            showControlsModal = true;
        }

        // Botão 3: Som Ativado / Mudo
        string soundLabel = isAudioMuted ? "Som: Desativado" : "Som: Ativado";
        if (GUI.Button(new Rect(btnX, startY + 104, btnW, 38), soundLabel, styleBtnSlate))
        {
            ToggleAudio();
        }

        // Botão 4: Sair do Jogo
        if (GUI.Button(new Rect(btnX, startY + 152, btnW, 38), "Sair do Jogo", styleBtnRed))
        {
            PlayMenuClickSound();
            showExitConfirmModal = true;
        }

        // Dica de teclado / controle
        GUI.Label(new Rect(cardX, startY + 204, cardW, 22), "[ESPAÇO] Iniciar   |   [ESC] Sair", styleHint);

        // Créditos com o nome do criador
        GUI.Label(new Rect(cardX, startY + 255, cardW, 24), "Tiny Island v1.1 - Feito por Edu Braga", styleCredits);

        // MODAIS
        if (showControlsModal)
        {
            DrawControlsModal();
        }
        else if (showExitConfirmModal)
        {
            DrawExitConfirmModal();
        }
    }

    private void DrawControlsModal()
    {
        float screenW = Screen.width;
        float screenH = Screen.height;

        float modalW = 480f;
        float modalH = 330f;
        float modalX = (screenW - modalW) * 0.5f;
        float modalY = (screenH - modalH) * 0.5f;

        GUI.Box(new Rect(modalX, modalY, modalW, modalH), "", stylePanelModal);

        // Cabeçalho do Modal
        GUIStyle modalHeader = new GUIStyle(styleTitle)
        {
            fontSize = 22
        };
        modalHeader.normal.textColor = new Color(1f, 0.88f, 0.35f);
        GUI.Label(new Rect(modalX, modalY + 20, modalW, 30), "GUIA DO EXPLORADOR", modalHeader);

        // Texto do Objetivo
        GUIStyle bodyBold = new GUIStyle(GUI.skin.label)
        {
            fontSize = 14,
            fontStyle = FontStyle.Bold
        };
        bodyBold.normal.textColor = new Color(1f, 0.84f, 0.2f);

        GUIStyle bodyNormal = new GUIStyle(GUI.skin.label)
        {
            fontSize = 13,
            wordWrap = true
        };
        bodyNormal.normal.textColor = Color.white;

        GUI.Label(new Rect(modalX + 30, modalY + 65, modalW - 60, 22), "OBJETIVO:", bodyBold);
        GUI.Label(new Rect(modalX + 30, modalY + 88, modalW - 60, 42),
            $"Explore o arquipélago (Farol, Píer, Veleiro, Ponte, Acampamento, Recife e Ruínas) para encontrar as {targetCoins} moedas de ouro e abrir o Baú do Tesouro!", bodyNormal);

        // Controles
        GUI.Label(new Rect(modalX + 30, modalY + 140, modalW - 60, 22), "CONTROLES:", bodyBold);
        GUI.Label(new Rect(modalX + 30, modalY + 162, modalW - 60, 62),
            "- W, A, S, D ou Setas / Left Stick: Caminhar pela ilha\n- ESPAÇO / Botão A: Pular nas pedras, barco e colinas\n- Sem estresse: Cair no mar não dá dano - você volta são e salvo!", bodyNormal);

        // Botão Fechar Modal
        if (GUI.Button(new Rect(modalX + (modalW - 180) * 0.5f, modalY + modalH - 58, 180, 40), "Entendido", styleBtnGold))
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

        GUI.Box(new Rect(modalX, modalY, modalW, modalH), "", stylePanelModal);

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

        // Indicador discreto de tempo no topo direito + atalho de pausa
        GUIStyle timeCornerStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 13,
            alignment = TextAnchor.MiddleRight
        };
        timeCornerStyle.normal.textColor = new Color(1f, 1f, 1f, 0.85f);
        GUI.Label(new Rect(Screen.width - 240, 20, 215, 24), $"Tempo: {gameTimer:F1}s   |   [ESC] Menu", timeCornerStyle);
    }

    private void DrawPauseMenu()
    {
        float screenW = Screen.width;
        float screenH = Screen.height;

        float panelW = 380f;
        float panelH = 370f;
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

        // 2. Alternar Som
        string soundLabel = isAudioMuted ? "Som: Desativado" : "Som: Ativado";
        if (GUI.Button(new Rect(btnX, startY + 48, btnW, 36), soundLabel, styleBtnSlate))
        {
            ToggleAudio();
        }

        // 3. Guia de Controles
        if (GUI.Button(new Rect(btnX, startY + 92, btnW, 36), "Controles & Objetivo", styleBtnSlate))
        {
            PlayMenuClickSound();
            showControlsModal = true;
        }

        // 4. Reiniciar Ilha
        if (GUI.Button(new Rect(btnX, startY + 136, btnW, 36), "Reiniciar Ilha", styleBtnSlate))
        {
            PlayMenuClickSound();
            RestartGame();
        }

        // 5. Menu Principal
        if (GUI.Button(new Rect(btnX, startY + 180, btnW, 36), "Menu Principal", styleBtnSlate))
        {
            PlayMenuClickSound();
            ReturnToMainMenu();
        }

        // 6. Sair do Jogo
        if (GUI.Button(new Rect(btnX, startY + 224, btnW, 36), "Sair do Jogo", styleBtnRed))
        {
            PlayMenuClickSound();
            showExitConfirmModal = true;
        }

        // Dica
        GUI.Label(new Rect(panelX, startY + 272, panelW, 20), "Pressione [ESC] para retornar ao jogo", styleHint);

        // Modais sobre o menu de pausa
        if (showControlsModal)
        {
            DrawControlsModal();
        }
        else if (showExitConfirmModal)
        {
            DrawExitConfirmModal();
        }
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
