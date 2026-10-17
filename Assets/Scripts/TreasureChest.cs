using System.Collections;
using UnityEngine;

/// <summary>
/// Controla o Baú do Tesouro Lendário na ilha.
/// Mantém o baú fechado até que todas as 6 moedas sejam coletadas.
/// Ao abrir, rotaciona a tampa suavemente e dispara efeitos comemorativos.
/// </summary>
public class TreasureChest : MonoBehaviour
{
    public static TreasureChest Instance { get; private set; }

    [Header("Componentes do Baú")]
    public Transform lidTransform;
    public ParticleSystem openParticles;
    public Light treasureLight;

    [Header("Animação de Abertura")]
    public float targetOpenAngle = -100f;
    public float openDuration = 1.2f;

    [Header("Status")]
    public bool isOpen = false;

    private Quaternion closedRotation;
    private Quaternion openRotation;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        // Se a tampa não foi vinculada no inspector, busca pelo nome no modelo
        if (lidTransform == null)
        {
            Transform found = transform.Find("Chest_Lid");
            if (found != null) lidTransform = found;
            else lidTransform = transform.Find("Chest_Base/Chest_Lid");
        }

        if (lidTransform != null)
        {
            closedRotation = lidTransform.localRotation;
            openRotation = closedRotation * Quaternion.Euler(targetOpenAngle, 0f, 0f);
        }

        // Luz dourada suave inicialmente desligada ou fraca
        if (treasureLight != null)
        {
            treasureLight.enabled = false;
        }
    }

    public void OpenChest()
    {
        if (isOpen) return;
        isOpen = true;

        StartCoroutine(AnimateOpenRoutine());
    }

    private IEnumerator AnimateOpenRoutine()
    {
        // Toca som de fanfarra de vitória
        IslandGameManager.PlayVictorySound();

        // Ativa luz dourada que emerge de dentro do baú
        if (treasureLight != null)
        {
            treasureLight.enabled = true;
        }

        // Ativa efeito de partículas/fogos se houver
        if (openParticles != null)
        {
            openParticles.Play();
        }

        if (lidTransform != null)
        {
            float elapsed = 0f;
            while (elapsed < openDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / openDuration);
                // Curva suave com efeito de mola leve (SmoothStep)
                float smoothT = Mathf.SmoothStep(0f, 1f, t);
                lidTransform.localRotation = Quaternion.Slerp(closedRotation, openRotation, smoothT);
                yield return null;
            }
            lidTransform.localRotation = openRotation;
        }
    }
}
