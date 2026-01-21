using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
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
            if (prefab == null) Debug.LogError("null prefab");
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
            placedObject.nonWorkingPopulation = placedObjectSO.nonWorkingPopulation;
            placedObject.possibleNpcTypes = placedObjectSO.possibleNpcTypes;
        }
        else
        {
            GameObject corePrefab = placedObjectSO.coreModularBuilding;
            if (corePrefab == null) Debug.LogError("null core");
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
            placedObject.nonWorkingPopulation = placedObjectSO.nonWorkingPopulation;
            placedObject.possibleNpcTypes = placedObjectSO.possibleNpcTypes;

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
                        BuildingScriptableObject.Dir modDir = BuildingScriptableObject.Dir.Down;
                        float rotY = placedObjectSO.GetRotationAngle(dir) + 0f;
                        Vector2Int fix = new Vector2Int(0, 0); // left
                        if (z == 0) { rotY = 270f; ; fix = new Vector2Int(cellSize, 0); modDir = BuildingScriptableObject.Dir.Right; }  // bottom
                        else if (x == w - 1) { rotY = 180f; fix = new Vector2Int(cellSize, cellSize); modDir = BuildingScriptableObject.Dir.Up; }    // right
                        else if (z == h - 1) { rotY = 90f; fix = new Vector2Int(0, cellSize); modDir = BuildingScriptableObject.Dir.Left; }  // top

                        offsetsInfo.Add(new OffsetInfo(new Vector3(x * cellSize, 0, z * cellSize), new Vector3(fix.x, 0, fix.y), rotY, modDir));
                    }
                }
            }

            for (int i = 0; i < placedObjectSO.modules.Count; i++)
            {
                Quaternion R1 = Quaternion.Euler(0, offsetsInfo[i].rotationY, 0);
                Quaternion R2 = Quaternion.Euler(0, placedObjectSO.GetRotationAngle(dir), 0);
                Vector3 T = R2 * offsetsInfo[i].fix + offsetsInfo[i].offset;

                Vector3 rotatedOffset = T;

                Quaternion moduleRot = (R2 * R1);
                Vector3 modulePos = worldPosition + rotatedOffset;

                if (placedObjectSO.modules[i].prefab == null) Debug.LogError("null module");
                GameObject modObj = Instantiate(
                    placedObjectSO.modules[i].prefab,
                    modulePos,
                    moduleRot
                );

                PlacedObject modPlaced = modObj.GetComponent<PlacedObject>();
                modPlaced.placedSctiptableObject = placedObjectSO.modules[i];
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
    public Vector3 worldPosition;
    public Quaternion worldRotation;

    private Vector2Int gridPos;
    public GameObject exclamationMark;
    public List<PlacedObject> modules;
    public bool isModule;
    public PlacedObject parent;
    public bool inInstitution;
    private Grid<GridObject> gridObject;
    int nonWorkingPopulation;
    List<string> possibleNpcTypes;

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
        placedObject.nonWorkingPopulation = placedObjectSO.nonWorkingPopulation;
        placedObject.possibleNpcTypes = placedObjectSO.possibleNpcTypes;

        if (this.parent != null)
            this.parent.modules.Add(placedObject);

        oldObject.GetComponent<PlacedObject>().RemoveAllPeople();
        Destroy(oldObject);
        placedObject.OnPlace();

        return placedObject;
    }
    [Header("People")]
    [HideInInspector] public float personAmount;
    [SerializeField] float timeBetweenSpawns;
    [SerializeField] List<NavmeshNpc> associatedWorkingPeople, associatedNonWorkingPeople;
    [SerializeField] List<NavmeshNpc> workingPeople;
    [SerializeField] Transform buildingOrigin;
    [SerializeField] TextMeshProUGUI farmlandCount;
    public Dictionary<PlacedObject, NavmeshNpc> adjacentFarmlandWorked;
    public bool isConnectedFarmland;
    bool spawnedPeople;
    int connectedFarmlandCountInText;
    public PlacedObject farm;

    public void Update()
    {
        if(placedSctiptableObject != null)
        {
            if (placedSctiptableObject.name == "Boerderij")
            {
                if (connectedFarmlandCountInText != adjacentFarmlandWorked.Count)
                {
                    connectedFarmlandCountInText = adjacentFarmlandWorked.Count;
                    farmlandCount.text = $"{connectedFarmlandCountInText}/10";
                }
            }
        }
    }
    public void OnPlace()
    {
        if (exclamationMark != null)
        {
            exclamationMark.transform.rotation = Quaternion.identity;
            //exclamationMark.SetActive(true);
        }
        Debug.Log("building start");

        buildingOrigin = transform.GetChild(0);

        if (gameObject.tag == "Farmland") buildingOrigin.rotation = Quaternion.Euler(0, 90, 0);
        else if (gameObject.tag != "Road") buildingOrigin.rotation = Quaternion.Euler(0, GetScriptableObject().GetRotationAngle(dir), 0);
        

        Debug.Log("Not error1");
        SpawnPeople();
        Debug.Log("Not error2");

        if (placedSctiptableObject.name == "Boerderij")
        {
            farmlandCount.gameObject.transform.parent.gameObject.SetActive(true);
            if (adjacentFarmlandWorked == null) ConnectFarmland(true);
        }

        CacheRenderers();

        Debug.Log($"Registering building: {name}");
        NPCManager.instance.RegisterBuilding(this, true);
    } 

    public List<PlacedObject> ConnectFarmland(bool createNewList)
    {
        if(createNewList) adjacentFarmlandWorked = new();
        List<PlacedObject> newFarmland = RoadManager.instance.FindConnectedFarmsOrFarmland(origin, true);

        Debug.Log($"Farm at {origin} looking for new farmland: {newFarmland.Count} adjacent farms found");

        foreach (PlacedObject farmland in newFarmland)
        {
            if ((!farmland.isConnectedFarmland) && (adjacentFarmlandWorked.Count < 10) && (!adjacentFarmlandWorked.ContainsKey(farmland)))
            {
                farmland.isConnectedFarmland = true;
                farmland.farm = this;
                adjacentFarmlandWorked.Add(farmland, null);
                farmland.exclamationMark.SetActive(false);
            }
        }

        Debug.Log($"Created farmland list for {gameObject.name}: {adjacentFarmlandWorked.Count}");
        return adjacentFarmlandWorked.Keys.ToList();

    }

    public PlacedObject GetFreeFarmland()
    {
        foreach(KeyValuePair<PlacedObject, NavmeshNpc> kvp in adjacentFarmlandWorked)
        {
            if (kvp.Value == null) return kvp.Key;
        }

        return null;
    }

    public int SendPeopleToWork(int _amount)
    {
        if (associatedWorkingPeople.Count <= 0) return 0;
        int sentPeople = 0;

        Debug.Log($"People Maximum: {_amount}");

        NavmeshNpc unemployedNpc = null;
        if (workingPeople == null)
        {
            workingPeople = new List<NavmeshNpc>();
        }
        else
        {
            foreach(NavmeshNpc npc in associatedWorkingPeople)
            {
                if(!workingPeople.Contains(npc))
                {
                    unemployedNpc = npc;
                    break;
                }
            }
        }
        if (unemployedNpc == null) return 0;

        PlacedObject objectToCheck;
        if (inInstitution) objectToCheck = parent;
        else objectToCheck = this;

        if(objectToCheck == null) return 0;

        List<PlacedObject> connectedFarms = RoadManager.instance.GetConnectedFarms(objectToCheck);  

        if(connectedFarms.Count < 1) return 0;
  
        //implement this on npc

        foreach (NavmeshNpc npc in associatedWorkingPeople)
        {
            if (sentPeople >= _amount) break;
            if (workingPeople.Contains(npc)) continue;

            List<PlacedObject> emptyConnectedFarms = new();
            foreach (PlacedObject farm in connectedFarms)
            {
                if (farm.GetFreeFarmland() != null)
                {
                    emptyConnectedFarms.Add(farm);
                }
            }

            Debug.Log($"Empty connected farms: {emptyConnectedFarms.Count}");

            if(emptyConnectedFarms.Count < 1) break;

            PlacedObject targetFarm = npc.GetClosestObjectFromList(emptyConnectedFarms);

            if (targetFarm.adjacentFarmlandWorked == null) ConnectFarmland(true);

            PlacedObject targetFarmland = targetFarm.GetFreeFarmland();

            if (targetFarm.adjacentFarmlandWorked.ContainsKey(targetFarmland))
            {
                Debug.Log("NPC WORK UPDATE: ADD");
                targetFarm.adjacentFarmlandWorked[targetFarmland] = npc;
                npc.SetNavmeshTarget(targetFarmland.transform.position, targetFarmland, targetFarm);
                NPCManager.instance.ChangeNpcStatus(npc, true);
                workingPeople.Add(npc);
                sentPeople++;
            }
            else Debug.LogWarning($"Adjacent farmland not in dictionary for {targetFarm.gameObject.name}");
        }

        return sentPeople;
    }

    public int GetPeopleFromWork(int _amount)
    {
        //Get people out of working people list and back to the building
        int peopleSentBack = 0;
        List<NavmeshNpc> peopleSentBackList = new();
        Debug.Log($"Sending People Back: {_amount}");

        foreach(NavmeshNpc npc in workingPeople)
        {
            if (peopleSentBack < _amount)
            {
                peopleSentBackList.Add(npc);
                peopleSentBack++;
            }
            else break;
        }

        foreach(NavmeshNpc npc in peopleSentBackList)
        {
            SendNpcBack(npc, false);
        }

        //Return amount of people that stopped working
        return peopleSentBack;
    }

    public void SendNpcBack(NavmeshNpc npc, bool teleport)
    {
        if (npc == null) return;
        workingPeople.Remove(npc);
        NPCManager.instance.ChangeNpcStatus(npc, false);

        if (npc.destinationBuilding != null)
        {
            if (npc.destinationBuilding.name == "Akker" && npc.destinationBuilding != null) FreeFarmland(npc.targetFarm, npc.destinationBuilding);
        }
        npc.destinationBuilding = null;
        Debug.Log("NPC WORK UPDATE: REMOVE");
        npc.ReturnHome(teleport);
    }

    public void FreeFarmland(PlacedObject farm, PlacedObject farmland)
    {
        farm.adjacentFarmlandWorked[farmland] = null;
    }

    void SpawnPeople()
    {
        associatedWorkingPeople = new();
        associatedNonWorkingPeople = new();


        Debug.Log($"Spawning {personAmount} people");

        for (int j = 0; j < 2; j++)
        {
            int npcsSpawnedOfType = 0;
            int randomTypeInt = 0;

            float intToCheck = personAmount;
            if (j == 1) intToCheck = nonWorkingPopulation;

            int switchInt = Mathf.FloorToInt(intToCheck / possibleNpcTypes.Count);

            for (int i = 0; i < intToCheck; i++)
            {
                NavmeshNpc person = null;
                if (possibleNpcTypes.Count == 1) person = NPCManager.instance.GetRandomNpcOfType(possibleNpcTypes[0]);
                if (possibleNpcTypes.Count > 1)
                {
                    if (npcsSpawnedOfType == switchInt)
                    {
                        npcsSpawnedOfType = 0;
                        randomTypeInt++;
                        if (randomTypeInt > possibleNpcTypes.Count - 1) randomTypeInt = UnityEngine.Random.Range(0, possibleNpcTypes.Count);
                    }

                    person = NPCManager.instance.GetRandomNpcOfType(possibleNpcTypes[randomTypeInt]);
                    npcsSpawnedOfType++;
                }


                NavmeshNpc newNpc = null;
                if (j == 0)
                {
                    if(isModule) newNpc = Instantiate(person, parent.transform.GetChild(0).position + new Vector3(0, 2.2f, 0), Quaternion.Euler(0, 0, 0));
                    else newNpc = Instantiate(person, transform.GetChild(0).position + new Vector3(0, 2.2f, 0), Quaternion.Euler(0, 0, 0));

                    newNpc.shouldIdleInGesticht = true;
                    associatedWorkingPeople.Add(newNpc);
                    NPCManager.instance.RegisterNpc(newNpc, true);
                }
                if (j == 1)
                {
                    if (!isModule)
                    {
                        newNpc = Instantiate(person, buildingOrigin.parent.position + new Vector3(0, 2.2f, 0), Quaternion.Euler(0, 0, 0));
                        newNpc.shouldRotate = true;
                    }
                    else
                    {
                        newNpc = Instantiate(person, buildingOrigin.parent.GetChild(0).position + new Vector3(0, 2.2f, 0), Quaternion.Euler(0, 0, 0));
                        newNpc.shouldIdleInGesticht = true;
                    }
                    associatedNonWorkingPeople.Add(newNpc);
                }
                newNpc.SetOrigin(this);
            }
        }
    }

    public BuildingScriptableObject GetScriptableObject() {  return placedSctiptableObject; }
    public Vector2Int GetOrigin() { return origin; }
    public List<Vector2Int> GetGridPositionList()
    {
        return placedSctiptableObject.GetGridPositionList(origin, dir);
    }

    public BuildingScriptableObject Destructor()
    {
        if(name == "Akker")
        {
            if(farm != null)
            {
                if(farm.adjacentFarmlandWorked.ContainsKey(this)) farm.adjacentFarmlandWorked.Remove(this);
            }
        }

        if(name == "Boerderij")
        {
            foreach(KeyValuePair<PlacedObject, NavmeshNpc> kvp in adjacentFarmlandWorked)
            {
                kvp.Key.farm = null;
                kvp.Key.isConnectedFarmland = false;
            }
        }

        //if (isModule && parent != null)
        //{
        //    return parent.Destructor();
        //}
        int associatedPeopleAmt = 0;
        if (associatedWorkingPeople != null) associatedPeopleAmt = associatedWorkingPeople.Count;
        if (modules != null)
        {
            foreach (PlacedObject module in modules)
            {
                GridBuildingSystem.instance.RemoveModule(module);
                Debug.Log("Destroying module");
            }
        }

        RemoveAllPeople();
        Debug.Log("111order: first");

        Destroy(gameObject);
        return placedSctiptableObject;
    }

    public void RemoveAllPeople()
    {
        foreach (NavmeshNpc _npc in associatedWorkingPeople)
        {
            Destroy(_npc.gameObject);
            NPCManager.instance.RegisterNpc(_npc, false);
        }
        foreach (NavmeshNpc _npc in associatedNonWorkingPeople)
        {
            Destroy(_npc.gameObject);
        }
        associatedWorkingPeople.Clear();
        associatedNonWorkingPeople.Clear();

        NPCManager.instance.RegisterBuilding(this, false);
    }

    internal BuildingScriptableObject.Dir GetDir()
    {
        return dir;
    }

    private Renderer[] renderers;
    private MaterialPropertyBlock mpb;
    private bool isHighlighted;

    private static readonly int BaseColorID = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorID = Shader.PropertyToID("_Color");

    private void CacheRenderers()
    {
        renderers = GetComponentsInChildren<Renderer>(true);
        mpb = new MaterialPropertyBlock();
    }

    public void Highlight(Color tint)
    {
        if (isHighlighted) return;

        foreach (Renderer r in renderers)
        {
            if (!r)
                continue;

            r.GetPropertyBlock(mpb);

            if (r.sharedMaterial.HasProperty(BaseColorID))
            {
                mpb.SetColor(BaseColorID, tint);
            }
            else if (r.sharedMaterial.HasProperty(ColorID))
            {
                mpb.SetColor(ColorID, tint);
            }

            r.SetPropertyBlock(mpb);
        }

        isHighlighted = true;
    }

    public void Unhighlight()
    {
        if (!isHighlighted) return;

        foreach (Renderer r in renderers)
        {
            if (r != null) 
                r.SetPropertyBlock(null);
        }

        isHighlighted = false;
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
