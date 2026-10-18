using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Controla o ciclo de iluminação e atmosfera da ilha (Dia Ensolarado vs. Pôr do Sol Dourado / Golden Hour).
/// Mapeado para o marco v1.3 do ROADMAP.md.
/// Realiza transição suave entre cores, ângulos solares, sombras e intensidades de luz do Farol e Fogueira.
/// </summary>
public class IslandAtmosphereManager : MonoBehaviour
{
    public static IslandAtmosphereManager Instance { get; private set; }

    public enum TimeOfDayPreset
    {
        Day,
        Sunset
    }

    [Header("Estado da Atmosfera")]
    [Tooltip("Define se a ilha está sob a luz do dia ou no pôr do sol dourado")]
    public TimeOfDayPreset currentTimeOfDay = TimeOfDayPreset.Day;

    [Tooltip("Duração da transição cinematográfica entre dia e pôr do sol")]
    [Range(0.5f, 5.0f)]
    public float transitionDuration = 1.8f;

    [Header("Referências de Luz da Cena")]
    public Light sunLight;
    public Light lighthouseLight;
    public Light campfireLight;

    [Header("Pós-Processamento URP (Volume Global)")]
    public Volume globalVolume;
    private Bloom postBloom;
    private ColorAdjustments postColorAdj;
    private WhiteBalance postWhiteBalance;
    private Vignette postVignette;

    [Header("Configurações: Dia Ensolarado (☀️)")]
    public Vector3 daySunEuler = new Vector3(50f, -30f, 0f);
    public Color daySunColor = new Color(1.0f, 0.98f, 0.92f);
    public float daySunIntensity = 2.0f;
    public Color daySkyAmbient = new Color(0.65f, 0.78f, 0.95f);
    public Color dayEquatorAmbient = new Color(0.45f, 0.60f, 0.72f);
    public Color dayGroundAmbient = new Color(0.22f, 0.30f, 0.22f);
    public float dayLighthouseIntensity = 1.5f;
    public float dayCampfireIntensity = 2.4f;

    [Header("Configurações: Pôr do Sol Dourado (🌅)")]
    [Tooltip("Sol baixo no horizonte para criar sombras longas e expressivas nas praias")]
    public Vector3 sunsetSunEuler = new Vector3(14f, -55f, 0f);
    public Color sunsetSunColor = new Color(1.0f, 0.52f, 0.16f);
    public float sunsetSunIntensity = 2.4f;
    public Color sunsetSkyAmbient = new Color(0.92f, 0.48f, 0.32f);
    public Color sunsetEquatorAmbient = new Color(0.70f, 0.32f, 0.45f);
    public Color sunsetGroundAmbient = new Color(0.32f, 0.16f, 0.12f);
    public float sunsetLighthouseIntensity = 6.5f;
    public float sunsetCampfireIntensity = 5.2f;

    private float transitionProgress = 0f; // 0 = Day, 1 = Sunset
    private float targetProgress = 0f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;

        FindSceneLights();
    }

    void Start()
    {
        FindSceneLights();
        ApplyLightingImmediate(currentTimeOfDay == TimeOfDayPreset.Sunset ? 1f : 0f);
        targetProgress = (currentTimeOfDay == TimeOfDayPreset.Sunset) ? 1f : 0f;
        transitionProgress = targetProgress;
    }

    public void FindSceneLights()
    {
        // 1. Busca a luz direcional do Sol
        if (sunLight == null)
        {
            GameObject sunObj = GameObject.Find("Directional Light");
            if (sunObj != null)
            {
                sunLight = sunObj.GetComponent<Light>();
            }
            if (sunLight == null)
            {
                Light[] allLights = FindObjectsByType<Light>();
                foreach (Light l in allLights)
                {
                    if (l.type == LightType.Directional)
                    {
                        sunLight = l;
                        break;
                    }
                }
            }
        }

        // 2. Busca a luz do Farol Tropical
        if (lighthouseLight == null)
        {
            GameObject lhObj = GameObject.Find("LighthouseBeaconLight");
            if (lhObj != null) lighthouseLight = lhObj.GetComponent<Light>();
        }

        // 3. Busca a luz da fogueira do Acampamento
        if (campfireLight == null)
        {
            GameObject fireObj = GameObject.Find("CampfireLight");
            if (fireObj != null) campfireLight = fireObj.GetComponent<Light>();
        }

        // 4. Busca o Volume de Pós-Processamento Global da Cena
        if (globalVolume == null)
        {
            globalVolume = FindAnyObjectByType<Volume>();
        }
        if (globalVolume != null && globalVolume.sharedProfile != null)
        {
            globalVolume.sharedProfile.TryGet(out postBloom);
            globalVolume.sharedProfile.TryGet(out postColorAdj);
            globalVolume.sharedProfile.TryGet(out postWhiteBalance);
            globalVolume.sharedProfile.TryGet(out postVignette);
        }
    }

    void Update()
    {
        // Atalho rápido no teclado (tecla T ou L) ou D-Pad para alternar a atmosfera a qualquer momento
        if (WasToggleHotkeyPressed())
        {
            ToggleTimeOfDay();
        }

        // Interpolação suave contínua
        if (Mathf.Abs(transitionProgress - targetProgress) > 0.001f)
        {
            float speed = 1f / Mathf.Max(0.1f, transitionDuration);
            transitionProgress = Mathf.MoveTowards(transitionProgress, targetProgress, Time.deltaTime * speed);
            float smoothT = Mathf.SmoothStep(0f, 1f, transitionProgress);
            ApplyLightingInterpolated(smoothT);
        }

        // Se o pôr do sol estiver ativo, a luz do farol faz uma varredura rotativa suave pelo mar
        if (lighthouseLight != null && transitionProgress > 0.3f)
        {
            lighthouseLight.transform.Rotate(Vector3.up, 32f * Time.deltaTime, Space.World);
        }
    }

    private bool WasToggleHotkeyPressed()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && (Keyboard.current.tKey.wasPressedThisFrame || Keyboard.current.lKey.wasPressedThisFrame))
        {
            return true;
        }
        if (Gamepad.current != null && Gamepad.current.dpad.up.wasPressedThisFrame)
        {
            return true;
        }
        return false;
#else
        return Input.GetKeyDown(KeyCode.T) || Input.GetKeyDown(KeyCode.L);
#endif
    }

    public void ToggleTimeOfDay()
    {
        if (currentTimeOfDay == TimeOfDayPreset.Day)
        {
            SetTimeOfDay(TimeOfDayPreset.Sunset);
        }
        else
        {
            SetTimeOfDay(TimeOfDayPreset.Day);
        }
    }

    public void SetTimeOfDay(TimeOfDayPreset newTime)
    {
        currentTimeOfDay = newTime;
        targetProgress = (newTime == TimeOfDayPreset.Sunset) ? 1f : 0f;

        if (IslandGameManager.Instance != null)
        {
            IslandGameManager.PlayMenuClickSound();
        }

        string modeName = (newTime == TimeOfDayPreset.Sunset) ? "Pôr do Sol Dourado" : "Dia Ensolarado";
        Debug.Log($"<color=orange>[TinyIsland]</color> Atmosfera alterada para: <b>{modeName}</b>!");
    }

    private void ApplyLightingInterpolated(float t)
    {
        if (sunLight != null)
        {
            Quaternion rotDay = Quaternion.Euler(daySunEuler);
            Quaternion rotSunset = Quaternion.Euler(sunsetSunEuler);
            sunLight.transform.rotation = Quaternion.Slerp(rotDay, rotSunset, t);

            sunLight.color = Color.Lerp(daySunColor, sunsetSunColor, t);
            sunLight.intensity = Mathf.Lerp(daySunIntensity, sunsetSunIntensity, t);
        }

        // Iluminação de Ambiente Trilight e Flat
        Color sky = Color.Lerp(daySkyAmbient, sunsetSkyAmbient, t);
        Color equator = Color.Lerp(dayEquatorAmbient, sunsetEquatorAmbient, t);
        Color ground = Color.Lerp(dayGroundAmbient, sunsetGroundAmbient, t);

        RenderSettings.ambientSkyColor = sky;
        RenderSettings.ambientEquatorColor = equator;
        RenderSettings.ambientGroundColor = ground;
        RenderSettings.ambientLight = sky;

        // Farol Tropical
        if (lighthouseLight != null)
        {
            lighthouseLight.intensity = Mathf.Lerp(dayLighthouseIntensity, sunsetLighthouseIntensity, t);
            lighthouseLight.range = Mathf.Lerp(18f, 26f, t);
        }

        // Fogueira do Acampamento
        if (campfireLight != null)
        {
            campfireLight.intensity = Mathf.Lerp(dayCampfireIntensity, sunsetCampfireIntensity, t);
            campfireLight.range = Mathf.Lerp(9f, 13f, t);
        }

        // Pós-Processamento URP Dinâmico (Luz e Cor Cinematográfica)
        if (postBloom != null)
        {
            postBloom.intensity.value = Mathf.Lerp(0.85f, 1.45f, t);
            postBloom.threshold.value = Mathf.Lerp(1.05f, 0.92f, t);
            postBloom.tint.value = Color.Lerp(new Color(1.0f, 0.94f, 0.82f), new Color(1.0f, 0.72f, 0.38f), t);
        }

        if (postColorAdj != null)
        {
            postColorAdj.colorFilter.value = Color.Lerp(new Color(1.0f, 0.98f, 0.94f), new Color(1.0f, 0.75f, 0.52f), t);
            postColorAdj.saturation.value = Mathf.Lerp(18f, 28f, t);
            postColorAdj.contrast.value = Mathf.Lerp(14f, 19f, t);
            postColorAdj.postExposure.value = Mathf.Lerp(0.22f, 0.28f, t);
        }

        if (postWhiteBalance != null)
        {
            postWhiteBalance.temperature.value = Mathf.Lerp(8f, 32f, t);
        }

        if (postVignette != null)
        {
            postVignette.intensity.value = Mathf.Lerp(0.20f, 0.28f, t);
        }
    }

    private void ApplyLightingImmediate(float t)
    {
        ApplyLightingInterpolated(t);
    }
}
