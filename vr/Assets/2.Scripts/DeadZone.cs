using UnityEngine;

public class DeadZone : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.transform.CompareTag("Cube"))
        {
            Destroy(collision.gameObject);
        }
    }
}
