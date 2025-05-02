using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ScrolObeject : MonoBehaviour
{
    private float speed = 5f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.left * speed * Time.deltaTime);
        //if (!GameManager)
        //{
            
        //}
    }
}
