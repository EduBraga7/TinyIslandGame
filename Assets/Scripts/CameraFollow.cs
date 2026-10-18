using System;
using UnityEngine;

/// <summary>
/// Câmera isométrica suave que acompanha o jogador com ângulo e distância configuráveis,
/// e suporta modo de órbita cinematográfica contemplativa no Menu Inicial com transição suave.
/// </summary>
public class CameraFollow : MonoBehaviour
{
    public static CameraFollow Instance { get; private set; }

    public enum CameraMode
    {
        MenuOrbit,
        TransitioningToGameplay,
        GameplayFollow,
        TransitioningToMenu
    }

    [Header("Modo de Operação")]
    public CameraMode mode = CameraMode.MenuOrbit;

    [Header("Alvo de Gameplay")]
    [Tooltip("Arraste o objeto do Jogador (Player) aqui")]
    public Transform target;

    [Header("Posicionamento Isométrico (Gameplay)")]
    [Tooltip("Distância e altura da câmera em relação ao jogador")]
    public Vector3 offset = new Vector3(0f, 6.2f, -5.5f);

    [Tooltip("Suavidade da perseguição (quanto menor, mais rápida)")]
    public float smoothTime = 0.15f;

    [Header("Órbita Cinematográfica (Menu Inicial)")]
    public Vector3 orbitCenter = new Vector3(0f, 0.6f, 0f);
    public float orbitDistance = 18f;
    public float orbitHeight = 9.5f;
    public float orbitSpeed = 7.5f; // Graus por segundo
    public float currentOrbitAngle = 45f;

    [Header("Transição Suave")]
    public float transitionDuration = 1.35f;

    private Vector3 currentVelocity = Vector3.zero;
    private float transitionTimer = 0f;
    private Vector3 transitionStartPos;
    private Quaternion transitionStartRot;
    private Action onTransitionComplete;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        // Se começar no modo de órbita do menu, já posiciona imediatamente para evitar pulos de câmera
        if (mode == CameraMode.MenuOrbit)
        {
            transform.position = CalculateOrbitPosition(currentOrbitAngle);
            transform.LookAt(orbitCenter + Vector3.up * 0.8f);
        }
        else if (mode == CameraMode.GameplayFollow && target != null)
        {
            transform.position = target.position + offset;
            transform.LookAt(target.position + Vector3.up * 1f);
        }
    }

    void LateUpdate()
    {
        switch (mode)
        {
            case CameraMode.MenuOrbit:
                UpdateMenuOrbit();
                break;

            case CameraMode.TransitioningToGameplay:
                UpdateTransitionToGameplay();
                break;

            case CameraMode.GameplayFollow:
                UpdateGameplayFollow();
                break;

            case CameraMode.TransitioningToMenu:
                UpdateTransitionToMenu();
                break;
        }
    }

    private void UpdateMenuOrbit()
    {
        currentOrbitAngle += orbitSpeed * Time.deltaTime;
        if (currentOrbitAngle >= 360f) currentOrbitAngle -= 360f;

        transform.position = CalculateOrbitPosition(currentOrbitAngle);
        transform.LookAt(orbitCenter + Vector3.up * 0.8f);
    }

    private void UpdateTransitionToGameplay()
    {
        transitionTimer += Time.deltaTime;
        float t = Mathf.Clamp01(transitionTimer / transitionDuration);
        float smoothT = Mathf.SmoothStep(0f, 1f, t);

        Vector3 targetPos = (target != null ? target.position : Vector3.zero);
        Vector3 desiredPos = targetPos + offset;
        Quaternion desiredRot = Quaternion.LookRotation((targetPos + Vector3.up * 1f) - desiredPos);

        transform.position = Vector3.Lerp(transitionStartPos, desiredPos, smoothT);
        transform.rotation = Quaternion.Slerp(transitionStartRot, desiredRot, smoothT);

        if (t >= 1f)
        {
            mode = CameraMode.GameplayFollow;
            currentVelocity = Vector3.zero;
            Action callback = onTransitionComplete;
            onTransitionComplete = null;
            callback?.Invoke();
        }
    }

    private void UpdateTransitionToMenu()
    {
        transitionTimer += Time.deltaTime;
        float t = Mathf.Clamp01(transitionTimer / transitionDuration);
        float smoothT = Mathf.SmoothStep(0f, 1f, t);

        Vector3 desiredPos = CalculateOrbitPosition(currentOrbitAngle);
        Quaternion desiredRot = Quaternion.LookRotation((orbitCenter + Vector3.up * 0.8f) - desiredPos);

        transform.position = Vector3.Lerp(transitionStartPos, desiredPos, smoothT);
        transform.rotation = Quaternion.Slerp(transitionStartRot, desiredRot, smoothT);

        if (t >= 1f)
        {
            mode = CameraMode.MenuOrbit;
            Action callback = onTransitionComplete;
            onTransitionComplete = null;
            callback?.Invoke();
        }
    }

    private void UpdateGameplayFollow()
    {
        if (target == null) return;

        // Posição desejada somando a posição do jogador ao offset
        Vector3 desiredPosition = target.position + offset;

        // Movimento suave usando SmoothDamp para evitar solavancos
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, smoothTime);

        // Aponta a câmera levemente acima dos pés do jogador (centro do corpo)
        transform.LookAt(target.position + Vector3.up * 1f);
    }

    public Vector3 CalculateOrbitPosition(float angleDegrees)
    {
        float rad = angleDegrees * Mathf.Deg2Rad;
        float x = orbitCenter.x + Mathf.Cos(rad) * orbitDistance;
        float z = orbitCenter.z + Mathf.Sin(rad) * orbitDistance;
        float y = orbitCenter.y + orbitHeight;
        return new Vector3(x, y, z);
    }

    public void StartTransitionToGameplay(Action onComplete = null)
    {
        mode = CameraMode.TransitioningToGameplay;
        transitionTimer = 0f;
        transitionStartPos = transform.position;
        transitionStartRot = transform.rotation;
        onTransitionComplete = onComplete;
    }

    public void StartTransitionToMenu(Action onComplete = null)
    {
        mode = CameraMode.TransitioningToMenu;
        transitionTimer = 0f;
        transitionStartPos = transform.position;
        transitionStartRot = transform.rotation;
        onTransitionComplete = onComplete;
    }
}

