using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class PlacedObject : MonoBehaviour
{
    public static PlacedObject Create(Vector3 worldPosition, Vector2Int origin, BuildingScriptableObject.Dir dir, BuildingScriptableObject placedObjectSO, bool inInstitution)
    {
        GameObject placedObjTransform;
        PlacedObject placedObject = null;
        if (!placedObjectSO.modular)
        {
            GameObject prefab = (inInstitution ? placedObjectSO.modulePrefab : placedObjectSO.prefab);
            placedObjTransform = Instantiate(prefab, worldPosition, Quaternion.Euler(0, placedObjectSO.GetRotationAngle(dir), 0));

            placedObject = placedObjTransform.GetComponent<PlacedObject>();
            placedObject.placedSctiptableObject = placedObjectSO;
            placedObject.origin = origin;
            placedObject.dir = dir;
            placedObject.name = placedObjectSO.name;
            placedObject.isModule = placedObjectSO.module;
        } else
        {

        }

        return placedObject;
    }
    private BuildingScriptableObject placedSctiptableObject;
    private Vector2Int origin;
    private BuildingScriptableObject.Dir dir;

    public GameObject exclamationMark;
    public List<PlacedObject> connectedObjects;
    public bool isModule;

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
