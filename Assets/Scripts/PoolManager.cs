using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.U2D.IK;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance;
    
    public GameObject objectToPool;
    public int amountToPool = 20;
    private List<GameObject> pool;

    private void Awake() => Instance = this;

    // Start is called before the first frame update
    void Start()
    {
        pool = Helpers.CreatePool(objectToPool)(amountToPool);
        foreach (var obj in pool)
        {
            obj.transform.parent = transform;
        }
    }

    public GameObject GetObject()
    {
        foreach (var obj in pool)
        {
            if (obj.activeSelf == false)
            {
                obj.SetActive(true);
                return obj;
            }
        }
        return null;
    }
}
