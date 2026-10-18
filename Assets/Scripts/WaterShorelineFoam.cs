using UnityEngine;

/// <summary>
/// Anima a espuma costeira onde o mar encontra a areia da praia e pedras,
/// simulando o ritmo suave da maré com expansão física e respiração de transparência.
/// Mapeado para o pilar de Direção de Arte do ROADMAP.md (v1.2).
/// </summary>
public class WaterShorelineFoam : MonoBehaviour
{
    [Header("Parâmetros da Maré (Respiração das Ondas)")]
    [Tooltip("Velocidade do ciclo da onda (em radianos por segundo)")]
    [Range(0.2f, 3.0f)]
    public float waveSpeed = 0.90f;

    [Tooltip("Variação da escala da espuma durante a maré")]
    [Range(0.005f, 0.15f)]
    public float waveScaleAmplitude = 0.035f;

    [Tooltip("Offset de fase para criar assincronia natural entre as ilhas e pedras")]
    public float phaseOffset = 0f;

    [Header("Opacidade e Luz")]
    [Tooltip("Se deve oscilar a transparência da espuma com o quebrar da onda")]
    public bool animateAlpha = true;

    [Tooltip("Transparência mínima quando a onda recua para o oceano")]
    [Range(0.0f, 1.0f)]
    public float minAlpha = 0.45f;

    [Tooltip("Transparência máxima quando a onda quebra na praia")]
    [Range(0.0f, 1.0f)]
    public float maxAlpha = 0.95f;

    [Tooltip("Leve oscilação vertical da espuma acompanhando a maré")]
    [Range(0.0f, 0.05f)]
    public float verticalBobAmount = 0.008f;

    private Vector3 initialScale;
    private Vector3 initialPosition;
    private Material foamMaterial;
    private Color initialColor;

    void Start()
    {
        initialScale = transform.localScale;
        initialPosition = transform.position;

        Renderer rend = GetComponent<Renderer>();
        if (rend != null)
        {
            // Cria instância do material em runtime para permitir controle independente de cor/alfa
            foamMaterial = rend.material;
            if (foamMaterial.HasProperty("_BaseColor"))
            {
                initialColor = foamMaterial.GetColor("_BaseColor");
            }
            else if (foamMaterial.HasProperty("_Color"))
            {
                initialColor = foamMaterial.GetColor("_Color");
            }
            else
            {
                initialColor = Color.white;
            }
        }
    }

    void Update()
    {
        float time = Time.time * waveSpeed + phaseOffset;
        float wave = (Mathf.Sin(time) + 1f) * 0.5f; // 0..1 suave

        // 1. Expansão e retração sutil da linha de espuma sobre a areia
        float scaleFactor = 1f + Mathf.Lerp(-waveScaleAmplitude, waveScaleAmplitude, wave);
        transform.localScale = new Vector3(initialScale.x * scaleFactor, initialScale.y, initialScale.z * scaleFactor);

        // 2. Leve oscilação de altura sincronizada com a maré
        if (verticalBobAmount > 0.0001f)
        {
            float yOffset = Mathf.Lerp(-verticalBobAmount, verticalBobAmount, wave);
            transform.position = new Vector3(initialPosition.x, initialPosition.y + yOffset, initialPosition.z);
        }

        // 3. Respiração de opacidade (ao recuar fica tênue; ao avançar fica bem branca e nítida)
        if (animateAlpha && foamMaterial != null)
        {
            float alpha = Mathf.Lerp(minAlpha, maxAlpha, wave);
            Color c = initialColor;
            c.a = alpha;

            if (foamMaterial.HasProperty("_BaseColor"))
            {
                foamMaterial.SetColor("_BaseColor", c);
            }
            else if (foamMaterial.HasProperty("_Color"))
            {
                foamMaterial.SetColor("_Color", c);
            }
        }
    }

    void OnDestroy()
    {
        if (foamMaterial != null)
        {
            Destroy(foamMaterial);
        }
    }
}
