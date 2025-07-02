using UnityEngine;

public class CubeController : MonoBehaviour
{
    [Range(1f, 100f)]
    public float speed = 20.0f;

    public Transform SliceObj;
    private void Update()
    {
        transform.position += Time.deltaTime * -transform.forward * speed;
    }
}
