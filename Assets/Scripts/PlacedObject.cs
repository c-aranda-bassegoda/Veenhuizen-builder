using System;
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
            placedObject.worldPosition = worldPosition;
            placedObject.name = placedObjectSO.name;
            placedObject.isModule = placedObjectSO.module;
        }
        else
        {
            GameObject corePrefab = placedObjectSO.modules[0];
            GameObject coreObj = Instantiate(
                corePrefab,
                worldPosition,
                Quaternion.Euler(0, placedObjectSO.GetRotationAngle(dir), 0)
            );

            PlacedObject corePlaced = coreObj.GetComponent<PlacedObject>();
            corePlaced.placedSctiptableObject = placedObjectSO;
            corePlaced.origin = origin;
            corePlaced.dir = dir;
            corePlaced.worldPosition = worldPosition;
            corePlaced.name = placedObjectSO.name + "_Core";
            corePlaced.isModule = false;
            corePlaced.modules = new List<PlacedObject>();

            Vector3[] offsets =
            {
                new Vector3(+2, 0, +2),
                new Vector3(+2, 0, -2),
                new Vector3(-2, 0, +2),
                new Vector3(-2, 0, -2),
            };

            foreach (var offset in offsets)
            {
                Vector3 rotatedOffset =
                    Quaternion.Euler(0, placedObjectSO.GetRotationAngle(dir), 0) * offset;

                Vector3 modulePos = worldPosition + rotatedOffset;

                GameObject modObj = Instantiate(
                    placedObjectSO.modules[1],
                    modulePos,
                    Quaternion.Euler(0, placedObjectSO.GetRotationAngle(dir), 0)
                );

                PlacedObject modPlaced = modObj.GetComponent<PlacedObject>();
                modPlaced.placedSctiptableObject = placedObjectSO;
                modPlaced.origin = origin;
                modPlaced.dir = dir;
                modPlaced.worldPosition = modulePos;
                modPlaced.name = placedObjectSO.name + "_Module";
                modPlaced.isModule = true;

                corePlaced.modules.Add(modPlaced);
            }
        }

        return placedObject;
    }
    private BuildingScriptableObject placedSctiptableObject;
    private Vector2Int origin;
    private BuildingScriptableObject.Dir dir;
    private Vector3 worldPosition;


    public GameObject exclamationMark;
    public List<PlacedObject> connectedObjects;
    public List<PlacedObject> modules;
    public bool isModule;

    public PlacedObject Repalace(BuildingScriptableObject placedObjectSO, bool inInstitution)
    {
        Vector3 pos = this.worldPosition;
        Vector2Int orig = this.origin;
        BuildingScriptableObject.Dir direction = this.dir;

        GameObject oldObject = this.gameObject;

        PlacedObject placedObject = null;
        GameObject prefab = (inInstitution ? placedObjectSO.modulePrefab : placedObjectSO.prefab);
        GameObject placedObjTransform = Instantiate(prefab, this.worldPosition, Quaternion.Euler(0, placedObjectSO.GetRotationAngle(this.dir), 0));

        placedObject = placedObjTransform.GetComponent<PlacedObject>();
        placedObject.placedSctiptableObject = placedObjectSO;
        placedObject.origin = this.origin;
        placedObject.dir = this.dir;
        placedObject.worldPosition = this.worldPosition;
        placedObject.name = placedObjectSO.name;
        placedObject.isModule = placedObjectSO.module;

        Destroy(oldObject);

        return placedObject;
    }
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

    internal BuildingScriptableObject.Dir GetDir()
    {
        return dir;
    }
}
