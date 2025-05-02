using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackGroundLoop : MonoBehaviour
{
    private float width;
    private const int REPEATCOUNT = 2;
    private BoxCollider2D backGroundCollider;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    private void Awake()
    {
        backGroundCollider = GetComponent<BoxCollider2D>();
        width = backGroundCollider.size.x;
    }
    void Update()
    {
        if (transform.position.x<= -width)
        {
            Reposition();
        }
        
    }

    private void Reposition()
    {
        //현재 위치에서 오른쪽으로 가로길이*2만큼 이동
        Vector3 offset = new Vector3(width*REPEATCOUNT, 0,0f);
        //현재 위치에서 오프셋 만큼 더해서 새로운 위치로 이동
        transform.position += offset;

    }
    // Update is called once per frame
}
