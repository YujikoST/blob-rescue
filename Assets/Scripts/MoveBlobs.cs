using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Helpers
{
    // -- The juicy stuff --

    public static Func<float, Func<GameObject, float>>
        GrowBlob = eatedArea => blob =>
        {
            var blobArea = GetBlobArea(blob);
            var newArea = blobArea + eatedArea;

            return ReplaceBlobArea(newArea)(blob);
        };

    public static Func<float, Func<GameObject, float>>
        ShrinkBlob = shrinkedArea => blob =>
        {
            var blobArea = GetBlobArea(blob);
            var newArea = blobArea - shrinkedArea;

            if (newArea < MIN_BLOB_AREA)
            {
                return 0;
            }

            return ReplaceBlobArea(newArea)(blob);
        };

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

    private static readonly Func<float, Func<GameObject, float>>
        ReplaceBlobArea = newArea => blob =>
        {
            var newDiameter = GetDiameter(newArea);
            var currentDiameter = blob.transform.localScale.x;
            blob.transform.localScale = new Vector3(newDiameter, newDiameter, DEFAULT_Z);
            
            var diameterDifference = currentDiameter - newDiameter;
            return diameterDifference;
        };
}