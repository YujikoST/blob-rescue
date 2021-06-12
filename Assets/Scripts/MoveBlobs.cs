using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Helpers
{
    // -- The juicy stuff --

    public static float GrowBlob(GameObject blob, float eatedArea)
    {
        var blobArea = GetBlobArea(blob);
        var newArea = blobArea + eatedArea;

        var diameter = GetDiameter(newArea);
        var difference = Mathf.Abs(diameter - blob.transform.localScale.x);

        blob.transform.localScale = new Vector3(diameter, diameter, DEFAULT_Z);

        return difference;
    }

    public static float ShrinkBlob(GameObject blob, float shrinkedArea)
    {
        var blobArea = GetBlobArea(blob);
        var newArea = blobArea - shrinkedArea;

        if (newArea < MIN_BLOB_AREA)
        {
            return 0;
        }

        var diameter = GetDiameter(newArea);
        var difference = Mathf.Abs(diameter - blob.transform.localScale.x);

        blob.transform.localScale = new Vector3(diameter, diameter, DEFAULT_Z);
        return difference;
    }

    public static readonly Func<GameObject, Func<int, List<GameObject>>>
        CreatePool = gameObject => amount =>
            Enumerable
                .Repeat(0, amount)
                .Select(InstantiateUnactive(gameObject))
                .ToList();


    // -- helper functions and data --


    private static readonly float DEFAULT_Z = 0;
    private static readonly float MIN_BLOB_AREA = 0.5f;

    private static readonly Func<float, float>
        GetArea = diameter =>
            Mathf.PI * Mathf.Pow(diameter / 2, 2);

    private static readonly Func<GameObject, float>
        GetBlobArea = blob =>
            GetArea(blob.transform.localScale.x);

    private static readonly Func<float, float>
        GetDiameter = area =>
            Mathf.Sqrt(area / Mathf.PI) * 2;


    private static readonly Func<GameObject, Func<int, GameObject>>
        InstantiateUnactive = gameObject => _ =>
        {
            var instance = UnityEngine.Object.Instantiate(gameObject);
            instance.SetActive(false);
            return instance;
        };
}