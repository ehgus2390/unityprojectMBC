using UnityEngine;
using System.Collections;

public class FogController : MonoBehaviour
{
    [Header("Fog Settings")]
    public bool enableFog = true;
    public Color fogColor = Color.gray;
    public FogMode fogMode = FogMode.Exponential;
    public float fogDensity = 0.01f;
    public float fogStartDistance = 0f;
    public float fogEndDistance = 300f;

    [Header("Dynamic Fog")]
    public bool useDynamicFog = false;
    public float minFogDensity = 0.005f;
    public float maxFogDensity = 0.02f;
    public float fogChangeSpeed = 0.5f;

    [Header("Zone Transition")]
    public float defaultTransitionSpeed = 1f;
    private Color targetFogColor;
    private float targetFogDensity;
    private FogMode targetFogMode;
    private bool isInFogZone = false;
    private Coroutine transitionCoroutine;

    private void Start()
    {
        // 초기 안개 설정
        targetFogColor = fogColor;
        targetFogDensity = fogDensity;
        targetFogMode = fogMode;
        UpdateFogSettings();
    }

    private void Update()
    {
        if (useDynamicFog && !isInFogZone)
        {
            // 동적으로 안개 밀도 변경 (안개 존에 있을 때는 비활성화)
            float targetDensity = Mathf.Lerp(minFogDensity, maxFogDensity,
                (Mathf.Sin(Time.time * fogChangeSpeed) + 1f) * 0.5f);
            fogDensity = Mathf.Lerp(fogDensity, targetDensity, Time.deltaTime);
            UpdateFogSettings();
        }
    }

    public void EnterFogZone(FogZone zone)
    {
        isInFogZone = true;
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
        }
        transitionCoroutine = StartCoroutine(TransitionToFogZone(zone));
    }

    public void ExitFogZone()
    {
        isInFogZone = false;
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
        }
        transitionCoroutine = StartCoroutine(TransitionToDefaultFog());
    }

    private IEnumerator TransitionToFogZone(FogZone zone)
    {
        targetFogColor = zone.zoneFogColor;
        targetFogDensity = zone.zoneFogDensity;
        if (zone.useCustomFogMode)
        {
            targetFogMode = zone.customFogMode;
        }

        float elapsedTime = 0f;
        Color startColor = fogColor;
        float startDensity = fogDensity;
        FogMode startMode = fogMode;

        while (elapsedTime < zone.transitionSpeed)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / zone.transitionSpeed;

            fogColor = Color.Lerp(startColor, targetFogColor, t);
            fogDensity = Mathf.Lerp(startDensity, targetFogDensity, t);
            if (zone.useCustomFogMode)
            {
                fogMode = targetFogMode;
            }

            UpdateFogSettings();
            yield return null;
        }
    }

    private IEnumerator TransitionToDefaultFog()
    {
        float elapsedTime = 0f;
        Color startColor = fogColor;
        float startDensity = fogDensity;
        FogMode startMode = fogMode;

        while (elapsedTime < defaultTransitionSpeed)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / defaultTransitionSpeed;

            fogColor = Color.Lerp(startColor, targetFogColor, t);
            fogDensity = Mathf.Lerp(startDensity, targetFogDensity, t);
            fogMode = targetFogMode;

            UpdateFogSettings();
            yield return null;
        }
    }

    public void UpdateFogSettings()
    {
        // 안개 활성화/비활성화
        RenderSettings.fog = enableFog;

        if (enableFog)
        {
            // 안개 설정 적용
            RenderSettings.fogColor = fogColor;
            RenderSettings.fogMode = fogMode;

            if (fogMode == FogMode.Linear)
            {
                RenderSettings.fogStartDistance = fogStartDistance;
                RenderSettings.fogEndDistance = fogEndDistance;
            }
            else
            {
                RenderSettings.fogDensity = fogDensity;
            }
        }
    }

    // 안개 설정을 동적으로 변경하는 메서드들
    public void SetFogDensity(float density)
    {
        fogDensity = Mathf.Clamp(density, 0f, 1f);
        UpdateFogSettings();
    }

    public void SetFogColor(Color color)
    {
        fogColor = color;
        UpdateFogSettings();
    }

    public void ToggleFog()
    {
        enableFog = !enableFog;
        UpdateFogSettings();
    }
}