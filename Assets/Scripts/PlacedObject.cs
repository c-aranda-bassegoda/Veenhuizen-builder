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
            placedObject.personAmount = placedObjectSO.population;
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
            placedObject.personAmount = placedObjectSO.population;

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
        placedObject.personAmount = placedObjectSO.population;

        if (this.parent != null)
            this.parent.modules.Add(placedObject);

        Destroy(oldObject);
        placedObject.OnPlace();

        return placedObject;
    }
    [Header("People")]
    [SerializeField] NavmeshNpc person;
    [HideInInspector] public float personAmount;
    [SerializeField] float timeBetweenSpawns;
    [SerializeField] List<NavmeshNpc> associatedPeople;
    List<NavmeshNpc> workingPeople;
    [SerializeField] Transform buildingOrigin;
    public Dictionary<PlacedObject, bool> adjacentFarmlandWorked;
    public bool isConnectedFarmland;
    bool spawnedPeople;

    public void OnPlace()
    {
        if (exclamationMark != null) exclamationMark.transform.rotation = Quaternion.identity;
        Debug.Log("building start");
        buildingOrigin = transform.GetChild(0);
        if (gameObject.tag == "Farmland") buildingOrigin.rotation = Quaternion.Euler(0, 90, 0);
        else if (gameObject.tag != "Road") buildingOrigin.rotation = Quaternion.Euler(0, GetScriptableObject().GetRotationAngle(dir), 0);

        if (placedSctiptableObject.name == "Boerderij")
        {
            adjacentFarmlandWorked = new();
            //Debug.Log($"Building origin: {origin}");
            List<PlacedObject> newFarmland = RoadManager.instance.FindConnectedFarmsOrFarmland(origin, true);

            foreach(PlacedObject farmland in newFarmland)
            {
                if((!farmland.isConnectedFarmland) && (adjacentFarmlandWorked.Count < 10))
                {
                    farmland.isConnectedFarmland = true;
                    adjacentFarmlandWorked.Add(farmland, false);
                }
            }

            Debug.Log($"Created farmland list for {gameObject.name}: {adjacentFarmlandWorked.Count}");
        }
        if(placedSctiptableObject.name == "Akker")
        {
            List<PlacedObject> newFarms = RoadManager.instance.FindConnectedFarmsOrFarmland(origin, false);

            foreach(PlacedObject farm in newFarms)
            {
                if(farm.adjacentFarmlandWorked.Count < 10)
                {
                    adjacentFarmlandWorked = new();
                    List<PlacedObject> newFarmland = RoadManager.instance.FindConnectedFarmsOrFarmland(farm.GetOrigin(), true);

                    foreach (PlacedObject farmland in newFarmland)
                    {
                        if ((!farmland.isConnectedFarmland) && (adjacentFarmlandWorked.Count < 10))
                        {
                            farmland.isConnectedFarmland = true;
                            adjacentFarmlandWorked.Add(farmland, false);
                        }
                    }

                    farm.adjacentFarmlandWorked = adjacentFarmlandWorked;
                    Debug.Log($"Created farmland list for {farm.gameObject.name}: {farm.adjacentFarmlandWorked.Count}");
                }
            }
        }
        if (personAmount > 0) NPCManager.instance.RegisterBuilding(this);

        StartCoroutine(SpawnPeople());
    } 

    public PlacedObject GetFreeFarmland()
    {
        foreach(KeyValuePair<PlacedObject, bool> kvp in adjacentFarmlandWorked)
        {
            if (!kvp.Value) return kvp.Key;
        }

        return null;
    }

    public int SendPeopleToWork(int _amount)
    {
        if (associatedPeople.Count <= 0) return 0;

        NavmeshNpc unemployedNpc = null;
        if (workingPeople == null)
        {
            workingPeople = new List<NavmeshNpc>();
        }
        else
        {
            foreach(NavmeshNpc npc in associatedPeople)
            {
                if(!workingPeople.Contains(npc))
                {
                    unemployedNpc = npc;
                    break;
                }
            }
        }
        if (unemployedNpc == null) return 0;

        
        List<PlacedObject> connectedFarms = RoadManager.instance.GetConnectedFarms(this);
        List<PlacedObject> fullFarms = new();
        foreach(PlacedObject farm in connectedFarms)
        {
            if(farm.GetFreeFarmland() == null)
            {
                fullFarms.Add(farm);
            }
        }
        foreach(PlacedObject farm in fullFarms) connectedFarms.Remove(farm);

        if(connectedFarms.Count < 1) return 0;

        //implement this on npc
        PlacedObject targetFarmland = unemployedNpc.GetClosestObjectFromList(connectedFarms);

        for (int i = 0; i < _amount;)
        {
            bool foundNpc = false;
            //Get new npc from associated people (one thats not already working) and send it to work
            foreach (NavmeshNpc npc in associatedPeople)
            {
                foundNpc = false;
                if (workingPeople.Contains(npc)) continue;

                npc.SetNavmeshTarget(targetFarmland.transform.position);
                adjacentFarmlandWorked[targetFarmland] = true;

                //Dictionary<PlacedObject, float> accessibleBuildings = npc.CanFindTarget();

                //Debug.Log($"Accessible buildings for {npc.name}: {accessibleBuildings.Count}");

                //if (accessibleBuildings.Count < 1) continue;
                //else
                //{
                //    workingPeople.Add(npc);
                //    npc.FindTarget(accessibleBuildings);
                //    i++;
                //    foundNpc = true;
                //    break;
                //}
            }

            if (!foundNpc) break;
        }

        return workingPeople.Count;
    }

    public int GetPeopleFromWork(int _amount)
    {
        //Get people out of working people list and back to the building

        //Return amount of people that stopped working
        return _amount;
    }

    IEnumerator SpawnPeople()
    {
        associatedPeople = new();

        for(int i = 0; i < personAmount; i++)
        {
            NavmeshNpc newNpc = Instantiate(person, buildingOrigin.position + buildingOrigin.TransformDirection(new Vector3(0, 5, -0)), Quaternion.Euler(0, 0, 0));
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
