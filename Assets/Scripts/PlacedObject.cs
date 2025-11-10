using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class PlacedObject : MonoBehaviour
{
    public static PlacedObject Create(Vector3 worldPosition, Vector2Int origin, BuildingScriptableObject.Dir dir, BuildingScriptableObject placedObjectSO)
    {
        GameObject placedObjTransform = Instantiate(placedObjectSO.prefab, worldPosition, Quaternion.Euler(0, placedObjectSO.GetRotationAngle(dir), 0));
        
        PlacedObject placedObject = placedObjTransform.GetComponent<PlacedObject>();
        placedObject.placedSctiptableObject = placedObjectSO;
        placedObject.origin = origin;
        placedObject.dir = dir;
        placedObject.name = placedObjectSO.name;

        return placedObject;
    }
    private BuildingScriptableObject placedSctiptableObject;
    private Vector2Int origin;
    private BuildingScriptableObject.Dir dir;

    public GameObject exclamationMark;
    public List<PlacedObject> connectedObjects;

    public BuildingScriptableObject GetScriptableObject() {  return placedSctiptableObject; }
    public Vector2Int GetOrigin() { return origin; }
    public List<Vector2Int> GetGridPositionList()
    {
        return placedSctiptableObject.GetGridPositionList(origin, dir);
    }
    public void Destructor()
    {
        Destroy(gameObject);
    }
}
