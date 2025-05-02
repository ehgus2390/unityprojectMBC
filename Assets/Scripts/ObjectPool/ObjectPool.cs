using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class ObjectPool<T> where T: Component
{
    private readonly Stack<T> pool = new Stack<T>();
    private readonly T prefab;
    private readonly Transform parent;

    public ObjectPool(T prefab, int initCount, Transform parent = null)
    {
        this .prefab = prefab;
        this .parent = parent;
        for (int i = 0; i < initCount; i++)
        {
            T obj = GameObject.Instantiate (prefab,parent);
            obj.gameObject.SetActive(false);
            pool.Push (obj);
        }
    }
    public T Get()
    {
        if (pool.Count>0)
        {
            T obj = pool.Pop ();
            obj.gameObject.SetActive(true);
            return obj;
        }
        T newObj = GameObject.Instantiate (prefab, parent) ;
        return newObj;
    }

    public void ReturnPool(T obj)
    {
        obj.gameObject.SetActive(false);
        pool.Push (obj);
    }

    void Start()
    {
        
    }



    // Update is called once per frame
    void Update()
    {
        
    }
}
