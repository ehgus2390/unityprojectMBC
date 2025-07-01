using UnityEngine;

public class CubeController : MonoBehaviour
{
    [Range(1f, 100f)]
    public float speed = 20.0f;

    private void Update()
    {
        transform.position += Time.deltaTime * -transform.forward * speed;
    }
}
