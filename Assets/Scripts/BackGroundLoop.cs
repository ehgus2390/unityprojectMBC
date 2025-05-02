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
        //���� ��ġ���� ���������� ���α���*2��ŭ �̵�
        Vector3 offset = new Vector3(width*REPEATCOUNT, 0,0f);
        //���� ��ġ���� ������ ��ŭ ���ؼ� ���ο� ��ġ�� �̵�
        transform.position += offset;

    }
    // Update is called once per frame
}
