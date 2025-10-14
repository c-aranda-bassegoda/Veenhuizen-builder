using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class PlacedObject : MonoBehaviour
{
    public static PlacedObject Create(Vector3 worldPosition, Vector2Int origin, BuildingScriptableObject placedObjectSO)
    {
        Transform placedObjTransform = Instantiate(placedObjectSO.prefab, worldPosition, Quaternion.identity);
        
        PlacedObject placedObject = placedObjTransform.GetComponent<PlacedObject>();
        placedObject.placedSctiptableObject = placedObjectSO;
        placedObject.origin = origin;

        return placedObject;
    }
    private BuildingScriptableObject placedSctiptableObject;
    private Vector2Int origin;

    public List<Vector2Int> GetGridPositionList()
    {
        return placedSctiptableObject.GetGridPositionList(origin);
    }
    public void Destructor()
    {
        Destroy(gameObject);
    }
}
