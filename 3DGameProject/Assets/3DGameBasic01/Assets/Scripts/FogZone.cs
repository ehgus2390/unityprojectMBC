using UnityEngine;

public class FogZone : MonoBehaviour
{
    [Header("Fog Zone Settings")]
    public Color zoneFogColor = Color.gray;
    public float zoneFogDensity = 0.01f;
    public float transitionSpeed = 1f;
    public bool useCustomFogMode = false;
    public FogMode customFogMode = FogMode.Exponential;

    private FogController fogController;
    private bool isPlayerInZone = false;

    private void Start()
    {
        fogController = FindObjectOfType<FogController>();
        if (fogController == null)
        {
            Debug.LogError("FogController not found in the scene!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInZone = true;
            if (fogController != null)
            {
                fogController.EnterFogZone(this);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInZone = false;
            if (fogController != null)
            {
                fogController.ExitFogZone();
            }
        }
    }

    private void OnDrawGizmos()
    {
        // 에디터에서 안개 영역을 시각화
        Gizmos.color = new Color(zoneFogColor.r, zoneFogColor.g, zoneFogColor.b, 0.3f);
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawCube(Vector3.zero, Vector3.one);
    }
}