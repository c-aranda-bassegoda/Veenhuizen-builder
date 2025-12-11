using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static Testing;

public class PlacedObject : MonoBehaviour
{
    public static PlacedObject Create(Vector3 worldPosition, Vector2Int origin, BuildingScriptableObject.Dir dir, BuildingScriptableObject placedObjectSO, bool inInstitution)
    {
        GameObject placedObjTransform;
        PlacedObject placedObject = null;
        if (!placedObjectSO.modular)
        {
            GameObject prefab = (inInstitution ? placedObjectSO.modulePrefab : placedObjectSO.prefab);
            placedObjTransform = Instantiate(prefab, worldPosition, Quaternion.identity);

            placedObject = placedObjTransform.GetComponent<PlacedObject>();
            placedObject.placedSctiptableObject = placedObjectSO;
            placedObject.origin = origin;
            placedObject.dir = dir;
            placedObject.worldPosition = worldPosition;
            placedObject.name = placedObjectSO.name;
            placedObject.isModule = placedObjectSO.module;
            placedObject.inInstitution = false;
        }
        else
        {
            GameObject corePrefab = placedObjectSO.modules[0];
            placedObjTransform = Instantiate(
                corePrefab,
                worldPosition,
                Quaternion.identity
            );

            placedObject = placedObjTransform.GetComponent<PlacedObject>();
            placedObject.placedSctiptableObject = placedObjectSO;
            placedObject.origin = origin;
            placedObject.dir = dir;
            placedObject.worldPosition = worldPosition;
            placedObject.name = placedObjectSO.name;
            placedObject.isModule = false;
            placedObject.modules = new List<PlacedObject>();
            placedObject.inInstitution = false;

            List<OffsetInfo> offsetsInfo = new List<OffsetInfo>();
            int cellSize = 10;
            int w = placedObjectSO.width;
            int h = placedObjectSO.height;

            for (int x = 0; x < w; x++)
            {
                for (int z = 0; z < h; z++)
                {
                    bool isBoundary = (x == 0 || x == w - 1 || z == 0 || z == h - 1);
                    bool isCorner = (x == 0 || x == w - 1) && (z == 0 || z == h - 1);

                    if (isBoundary && !isCorner)
                    {
                        BuildingScriptableObject.Dir modDir = BuildingScriptableObject.Dir.Left;
                        float rotY = placedObjectSO.GetRotationAngle(dir) + 0f;
                        Vector2Int fix = new Vector2Int(0, 0); // left
                        if (z == 0) { rotY = 270f; ; fix = new Vector2Int(cellSize, 0); modDir = BuildingScriptableObject.Dir.Down; }  // bottom
                        else if (x == w - 1) { rotY = 180f; fix = new Vector2Int(cellSize, cellSize); modDir = BuildingScriptableObject.Dir.Right; }    // right
                        else if (z == h - 1) { rotY = 90f; fix = new Vector2Int(0, cellSize); modDir = BuildingScriptableObject.Dir.Up; }  // top

                        offsetsInfo.Add(new OffsetInfo(new Vector3(x * cellSize, 0, z * cellSize), new Vector3(fix.x, 0, fix.y), rotY, modDir));
                    }
                }
            }

            for (int i = 0; i < placedObjectSO.modules.Count - 1; i++)
            {
                Quaternion R1 = Quaternion.Euler(0, offsetsInfo[i].rotationY, 0);
                Quaternion R2 = Quaternion.Euler(0, placedObjectSO.GetRotationAngle(dir), 0);
                Vector3 T = R2 * offsetsInfo[i].fix + offsetsInfo[i].offset;

                Vector3 rotatedOffset = T;

                Quaternion moduleRot = (R2 * R1);
                Vector3 modulePos = worldPosition + rotatedOffset;

                GameObject modObj = Instantiate(
                    placedObjectSO.modules[i + 1],
                    modulePos,
                    moduleRot
                );

                PlacedObject modPlaced = modObj.GetComponent<PlacedObject>();
                modPlaced.placedSctiptableObject = placedObjectSO;
                modPlaced.origin = origin;
                modPlaced.dir = offsetsInfo[i].dir;
                modPlaced.worldPosition = modulePos;
                modPlaced.worldRotation = moduleRot;
                modPlaced.name = placedObjectSO.name + "_module";
                modPlaced.isModule = true;
                modPlaced.parent = placedObject;
                modPlaced.inInstitution = true;

                placedObject.modules.Add(modPlaced);
            }
        }

        return placedObject;
    }
    private BuildingScriptableObject placedSctiptableObject;
    [SerializeField] private Vector2Int origin;
    private BuildingScriptableObject.Dir dir;
    private Vector3 worldPosition;
    private Quaternion worldRotation;

    private Vector2Int gridPos;
    public GameObject exclamationMark;
    public List<PlacedObject> modules;
    public bool isModule;
    public PlacedObject parent;
    public bool inInstitution;
    private Grid<GridObject> gridObject;

    public PlacedObject Replace(BuildingScriptableObject placedObjectSO)
    {
        Vector3 pos = this.worldPosition;
        Vector2Int orig = this.origin;
        BuildingScriptableObject.Dir direction = this.dir;

        if (this.parent != null)
            this.parent.modules.Remove(this);
        GameObject oldObject = this.gameObject;

        PlacedObject placedObject = null;
        GameObject prefab = (inInstitution ? placedObjectSO.modulePrefab : placedObjectSO.prefab);
        GameObject placedObjTransform = Instantiate(prefab, this.worldPosition, this.worldRotation);

        placedObject = placedObjTransform.GetComponent<PlacedObject>();
        placedObject.placedSctiptableObject = placedObjectSO;
        placedObject.origin = this.origin;
        placedObject.dir = this.dir;
        placedObject.worldPosition = this.worldPosition;
        placedObject.worldRotation = this.worldRotation;
        placedObject.name = placedObjectSO.name + "_module";
        placedObject.isModule = placedObjectSO.module;
        placedObject.parent = this.parent;
        placedObject.inInstitution = this.inInstitution;

        if (this.parent != null)
            this.parent.modules.Add(placedObject);

        Destroy(oldObject);

        return placedObject;
    }
    [Header("People")]
    [SerializeField] NavmeshNpc person;
    [SerializeField] int personAmount;
    [SerializeField] float timeBetweenSpawns;
    [SerializeField] List<NavmeshNpc> associatedPeople;
    [SerializeField] Transform buildingOrigin;
    bool spawnedPeople;

    [SerializeField] private float moveToCentreMultiplier = 0.01f;
    [SerializeField] private float moveToPlaceMultiplier = 0.01f;
    [SerializeField] private float matchVelocityMultiplier = 0.125f;
    [SerializeField] private float minimumBoidDistance = 3f;

    //[SerializeField] private Vector3 minMoveBounds;
    //[SerializeField] private Vector3 maxMoveBounds;

    public void OnPlace()
    {
        if(exclamationMark != null) exclamationMark.transform.rotation = Quaternion.identity;
        Debug.Log("building start");
        buildingOrigin = transform.GetChild(0);
        if(gameObject.tag == "Farmland") buildingOrigin.rotation = Quaternion.Euler(0, 90, 0);
        else if (gameObject.tag != "Road") buildingOrigin.rotation = Quaternion.Euler(0, GetScriptableObject().GetRotationAngle(dir), 0);

        StartCoroutine(SpawnPeople());
    } 

    IEnumerator SpawnPeople()
    {
        associatedPeople = new();

        for(int i = 0; i < personAmount; i++)
        {
            NavmeshNpc newNpc = Instantiate(person, buildingOrigin.position + buildingOrigin.TransformDirection(new Vector3(0, 5, -0)), transform.rotation);
            associatedPeople.Add(newNpc);
            newNpc.SetOrigin(this);
            yield return new WaitForSeconds(timeBetweenSpawns);
        }
    }

    public BuildingScriptableObject GetScriptableObject() {  return placedSctiptableObject; }
    public Vector2Int GetOrigin() { return origin; }
    public List<Vector2Int> GetGridPositionList()
    {
        return placedSctiptableObject.GetGridPositionList(origin, dir);
    }

    public void Destructor()
    {
        if (isModule && parent != null)
        {
            parent.Destructor();
            return; 
        }
        int associatedPeopleAmt = 0;
        if (associatedPeople != null) associatedPeopleAmt = associatedPeople.Count;
        if (modules != null)
        {
            foreach (PlacedObject module in modules)
            {
                Destroy(module.gameObject);
            }
        }
        foreach (NavmeshNpc _npc in associatedPeople)
        {
            Destroy(_npc.gameObject);
        }
        associatedPeople.Clear();

        Destroy(gameObject);
    }

    internal BuildingScriptableObject.Dir GetDir()
    {
        return dir;
    }
}
public struct OffsetInfo
{
    public Vector3 offset;
    public Vector3 fix;
    public float rotationY;
    public BuildingScriptableObject.Dir dir;

    public OffsetInfo(Vector3 offset, Vector3 fix, float rotationY, BuildingScriptableObject.Dir dir)
    {
        this.offset = offset;
        this.fix = fix;
        this.rotationY = rotationY;
        this.dir = dir;
    }
}
