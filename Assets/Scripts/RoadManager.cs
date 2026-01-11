using System;
using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

public class RoadManager : MonoBehaviour
{
    [SerializeField] Mesh roadStraight, roadTurn, roadCrossroad, roadIntsct3;
    public Dictionary<Vector2Int, MeshFilter> placedRoads = new();
    [SerializeField] GameObject roadPrefab;
    [SerializeField] LayerMask roadTestLayer;
    [SerializeField] GridBuildingSystem gridBuildingSystem;
    [SerializeField] EconomyManager economyManager;
    [SerializeField] NavMeshSurface navMeshSurface;
    public static RoadManager instance;

    public List<List<PlacedObject>> connectedObjectGroups = new();

    private void Awake()
    {
        instance = this;
    }

    public void Update()
    {
        //Debug stuff
        if(Input.GetKeyDown(KeyCode.L))
        {
            Debug.Log($"Connected Objects:");
            foreach (List<PlacedObject> objGroup in connectedObjectGroups)
            {
                Debug.Log($"Connected Objects: List {connectedObjectGroups.IndexOf(objGroup)}:");
                foreach (PlacedObject obj in objGroup)
                {
                    Debug.Log($"Connected Objects: {obj.name}:");
                }
            }

            //Grid<GridObject> grid = gridBuildingSystem.GetGrid();
            //List<PlacedObject> directlyConnectedObjects = new();

            //GridObject currentGridObject = grid.GetGridObj(2, 2);
            //PlacedObject currentPlacedObject = currentGridObject.GetPlacedObject();

            //Debug.Log($"test1: {currentPlacedObject.gameObject.name}");
        }
    }

    public void PlaceRoad(Vector2Int roadPos, MeshFilter newRoadMesh)
    {
        if (!placedRoads.ContainsKey(roadPos))
        {
            placedRoads.Add(roadPos, newRoadMesh);
            UpdateConnections(roadPos);
        }
    }

    public void RemoveRoad(Vector2Int roadPos)
    {
        if (placedRoads.ContainsKey(roadPos))
        {
            //Destroy(GetPlacedRoad(roadPos));
            placedRoads.Remove(roadPos);
            UpdateConnections(roadPos);
        }
    }

    public void UpdatePreviewRoad(GameObject previewRoad, Vector2Int newPosition)
    {
        MeshFilter roadToUpdate = previewRoad.transform.GetChild(0).gameObject.GetComponent<MeshFilter>();
        List<Vector2Int> adjacentRoadPositions = GetAdjacentRoadPositions(newPosition);
        List<bool> isRoadThere = FindAdjacentObjects(adjacentRoadPositions, newPosition);
        string roadConfig = CheckAdjacentRoads(isRoadThere);
        UpdateRoad(roadToUpdate, roadConfig);
    }
    public void UpdateConnections(Vector2Int roadPos, bool placingRoad = true)
    {
        ConnectNewObject(roadPos);

        List<Vector2Int> roadsToUpdate = GetAdjacentRoadPositions(roadPos);
        if (placingRoad) roadsToUpdate.Add(roadPos);
        //CheckRoadConnectionOnBuild(roadPos);

        foreach (Vector2Int pos in roadsToUpdate)
        {
            MeshFilter roadToUpdate = GetPlacedRoad(pos);
            if (roadToUpdate == null) continue;

            Debug.Log($"Checking adjacent positions for {pos.x}, {pos.y}");

            //Sorted: Up, Down, Right, Left
            List<Vector2Int> adjacentRoadPositions = GetAdjacentRoadPositions(pos);
            List<bool> isRoadThere = FindAdjacentObjects(adjacentRoadPositions, pos);

            string roadConfig = CheckAdjacentRoads(isRoadThere);
            Debug.Log($"New roadconfig for pos {pos} = {roadConfig}");
            UpdateRoad(roadToUpdate, roadConfig);
        }

        StartCoroutine(DelayNewNavmesh());
    }

    IEnumerator DelayNewNavmesh()
    {
        yield return new WaitForEndOfFrame();
        Debug.Log("Building new navmesh");
        navMeshSurface.BuildNavMesh();
    }

    List<bool> FindAdjacentObjects(List<Vector2Int> adjacentRoadPositions, Vector2Int pos)
    {
        List<bool> isObjectThere = new() { false, false, false, false };
        for (int i = 0; i < adjacentRoadPositions.Count; i++)
        {
            Grid<GridObject> grid = gridBuildingSystem.GetGrid();
            GridObject gridObject = grid.GetGridObj(adjacentRoadPositions[i].x, adjacentRoadPositions[i].y);

            //Vector2Int adjPos = adjacentRoadPositions[i];
            //MeshFilter adjRoad = GetPlacedRoad(adjPos);
            if (gridObject != null)
            {
                if (!gridObject.CanPlace())
                {
                    Debug.Log($"Adjacent road found for {pos.x}, {pos.y} at {adjacentRoadPositions[i].x}, {adjacentRoadPositions[i].y}");
                    isObjectThere[i] = true;
                }
            }
            else
            {
                Debug.Log($"No object found at {adjacentRoadPositions[i].x}, {adjacentRoadPositions[i].y}");
            }
        }
        return isObjectThere;
    }


    public void DisconnectObject(Vector2Int pos)
    {
        /*
         I think the old objects position doesnt get cleared from the grid before this check so nothing ever changes in terms of connectivity
         */
        Grid<GridObject> grid = gridBuildingSystem.GetGrid();
        GridObject currentGridObject = grid.GetGridObj(pos.x, pos.y);
        PlacedObject currentPlacedObject = currentGridObject.GetPlacedObject();

        int oldObjectCount = 0;
        foreach(List<PlacedObject> objGroup in connectedObjectGroups)
        {
            if(objGroup.Contains(currentPlacedObject))
            {
                oldObjectCount = objGroup.Count;
                objGroup.Remove(currentPlacedObject);
            }
        }

        int oldListAmount = connectedObjectGroups.Count;

        List<Vector2Int> checkedPositions = new();
        List<Vector2Int> adjPositionsToCheck = GetAdjacentRoadPositions(pos);
        List<Vector2Int> adjObjectPositions = new();

        List<PlacedObject> adjObjects = new() { currentPlacedObject };

        foreach (Vector2Int adjPosToCheck in adjPositionsToCheck)
        {
            Debug.Log($"DisconnectObject: checking {adjPosToCheck})");

            GridObject adjGridObject = grid.GetGridObj(adjPosToCheck.x, adjPosToCheck.y);
            if (adjGridObject == null) continue;
            PlacedObject adjPlacedObject = adjGridObject.GetPlacedObject();
            if (adjPlacedObject != null)
            {
                adjObjectPositions.Add(adjPosToCheck);
                adjObjects.Add(adjPlacedObject);
            }
        }

        //List<Vector2Int> positionsWithObjects = new();

        bool foundAllPositions = false;

        foreach (Vector2Int adjPosToCheck in adjPositionsToCheck)
        {
            int foundAdjPositions = 0;

            List<PlacedObject> objectsInNewGroup = new();
            List<Vector2Int> uncheckedPositions = new() { adjPosToCheck };

            while (uncheckedPositions.Count > 0)
            {
                List<Vector2Int> newPositionsTempList = new();
                List<Vector2Int> oldPositionsTempList = new();

                foreach (Vector2Int uncheckedPos in uncheckedPositions)
                {
                    //Debug.Log($"Checking New Position: {uncheckedPos}");
                    checkedPositions.Add(uncheckedPos);
                    oldPositionsTempList.Add(uncheckedPos);

                    if (uncheckedPos == pos) continue;

                    GridObject gridObject = grid.GetGridObj(uncheckedPos.x, uncheckedPos.y);
                    PlacedObject placedObject = null;

                    if (gridObject != null)
                    {
                        placedObject = gridObject.GetPlacedObject();

                        if (placedObject != null)
                        {
                            //positionsWithObjects.Add(uncheckedPos);

                            if(adjObjectPositions.Contains(uncheckedPos))
                            {
                                foundAdjPositions++;
                                Debug.Log($"Found adjacent position: {uncheckedPos}");
                                if (foundAdjPositions == adjObjectPositions.Count) foundAllPositions = true;
                            }

                            if (foundAllPositions)
                            {
                                Debug.Log($"Found all positions, restoring groups to old");
                                break;
                            }
                            objectsInNewGroup.Add(placedObject);

                            List<Vector2Int> newAdjacentPositions = GetAdjacentRoadPositions(uncheckedPos);
                            foreach(Vector2Int newAdjPos in newAdjacentPositions)
                            {
                                if(!checkedPositions.Contains(newAdjPos) && (pos != newAdjPos)) newPositionsTempList.Add(newAdjPos);
                            }
                        }
                    }
                }
                if (foundAllPositions) break;

                foreach (Vector2Int tempPos in oldPositionsTempList)
                {
                    uncheckedPositions.Remove(tempPos);
                }

                foreach (Vector2Int tempPos in newPositionsTempList)
                {
                    if(adjPositionsToCheck.Contains(tempPos)) uncheckedPositions.Insert(0, tempPos);
                    else uncheckedPositions.Add(tempPos);
                }
            }

            if (foundAllPositions) break;

            Debug.Log($"DisconnectObject: Old List Amount: {connectedObjectGroups.Count}");

            if (objectsInNewGroup.Count > 0)
            {
                List<PlacedObject> newObjectList = new();
                newObjectList.AddRange(objectsInNewGroup);
                connectedObjectGroups.Add(newObjectList);

                Debug.Log($"DisconnectObject: new list: {newObjectList.Count}");
                foreach (PlacedObject obj in newObjectList)
                {
                    Debug.Log($"DisconnectObject: new object: {obj.name}");
                    UpdateObjectNotConnectedWarning(obj, newObjectList);
                }

                if (newObjectList.Count == (oldObjectCount - 1)) break;
            }
        }

        if (foundAllPositions)
        {
            if(connectedObjectGroups.Count > oldListAmount)
            { 
                connectedObjectGroups.RemoveRange(oldListAmount, connectedObjectGroups.Count - oldListAmount);
            }
        }
        else
        {
            List<List<PlacedObject>> listsToRemove = new();

            foreach (PlacedObject adjObj in adjObjects)
            {
                //Debug.Log($"DisconnectObject: Checking {adjObj}");
                foreach (List<PlacedObject> objGroup in connectedObjectGroups)
                {
                    if (objGroup.Contains(adjObj))
                    {
                        if (adjObjects.IndexOf(adjObj) == 0) oldObjectCount = objGroup.Count;
                        if (!listsToRemove.Contains(objGroup))
                        {
                            listsToRemove.Add(objGroup);
                            break;
                        }
                    }
                }
            }

            foreach (List<PlacedObject> listToRemove in listsToRemove)
            {
                connectedObjectGroups.Remove(listToRemove);
                Debug.Log($"Removing object group, new amount {connectedObjectGroups.Count}");
            }
        }

        StartCoroutine(DelayNewNavmesh());
    }

    private void ConnectNewObject(Vector2Int pos)
    {
        Grid<GridObject> grid = gridBuildingSystem.GetGrid();
        List<PlacedObject> directlyConnectedObjects = new();
        List<List<PlacedObject>> groupsToMerge = new();

        GridObject currentGridObject = grid.GetGridObj(pos.x, pos.y);
        PlacedObject currentPlacedObject = currentGridObject.GetPlacedObject();

        if(currentPlacedObject.inInstitution) currentPlacedObject = currentPlacedObject.parent;
        List<Vector2Int> adjacentRoadPositions = null;
        if (currentPlacedObject.name == "Gesticht")
        {
            adjacentRoadPositions = GetAdjacentRoadPositionsGesticht(currentPlacedObject.GetOrigin());
        }
        else
        {
            adjacentRoadPositions = GetAdjacentRoadPositions(pos);
        }

        foreach (Vector2Int gridPos in adjacentRoadPositions)
        {
            GridObject gridObject = grid.GetGridObj(gridPos.x, gridPos.y);

            if (gridObject != null)
            {
                PlacedObject placedObject = gridObject.GetPlacedObject();
                if (placedObject != null)
                {
                    if(placedObject.inInstitution)
                    {
                        if(!directlyConnectedObjects.Contains(placedObject.parent)) directlyConnectedObjects.Add(placedObject.parent);
                    }
                    else if(placedObject.name == "Gesticht")
                    {
                        if(!directlyConnectedObjects.Contains(placedObject)) directlyConnectedObjects.Add(placedObject);
                    }
                    else directlyConnectedObjects.Add(placedObject);
                }
            }
        }

        Debug.Log($"Connected: connected objects: {directlyConnectedObjects.Count}");
        bool debugthingy = false;
        List<PlacedObject> firstObjGroup = null;

        foreach (PlacedObject placedObject in directlyConnectedObjects)
        {
            foreach (List<PlacedObject> placedObjGroup in connectedObjectGroups)
            {
                if (placedObjGroup.Contains(placedObject))
                {
                    if (firstObjGroup == null)
                    {
                        firstObjGroup = placedObjGroup;
                        if (!groupsToMerge.Contains(firstObjGroup)) groupsToMerge.Add(firstObjGroup);
                    }

                    if(placedObjGroup != firstObjGroup)
                    {
                        if(!groupsToMerge.Contains(placedObjGroup)) groupsToMerge.Add(placedObjGroup);

                        debugthingy = true;
                    }
                }
            }
            if (!debugthingy) Debug.Log($"Connected: object not in group");
        }

        if (groupsToMerge.Count > 1)
        {
            foreach (List<PlacedObject> objGroup in groupsToMerge)
            {
                if (groupsToMerge.IndexOf(objGroup) != 0)
                {
                    groupsToMerge[0].AddRange(objGroup);
                    connectedObjectGroups.Remove(objGroup);
                }
            }
            groupsToMerge[0].Add(currentPlacedObject);
            foreach (PlacedObject placedObject in groupsToMerge[0])
            {
                UpdateObjectNotConnectedWarning(placedObject, groupsToMerge[0]);
            }

            //Debug.Log($"ConnectNewObject: merged {groupsToMerge.Count} groups into one: ");
            Debug.Log($"Connected Object: Merging, Adding new obj to list {connectedObjectGroups.IndexOf(groupsToMerge[0])}");
            foreach (PlacedObject placedObject in groupsToMerge[0]) Debug.Log($"ConnectNewObject: {placedObject.name}");
        }
        else if (groupsToMerge.Count == 1)
        {
            groupsToMerge[0].Add(currentPlacedObject);
            foreach(PlacedObject placedObject in groupsToMerge[0])
            {
                UpdateObjectNotConnectedWarning(placedObject, groupsToMerge[0]);
            }

            //Debug.Log("ConnectNewObject: added to 1 existing group: ");
            Debug.Log($"Connected Object: Adding new obj to list {connectedObjectGroups.IndexOf(groupsToMerge[0])}");
            foreach (PlacedObject placedObject in groupsToMerge[0]) Debug.Log($"ConnectNewObject: {placedObject.name}");
        }
        else
        {
            connectedObjectGroups.Add(new List<PlacedObject> { currentPlacedObject });
            UpdateObjectNotConnectedWarning(currentPlacedObject, connectedObjectGroups[connectedObjectGroups.Count - 1]);

            //Debug.Log("ConnectNewObject: new group created");
            Debug.Log($"Connected Object: New group, Adding new obj to list {connectedObjectGroups.Count - 1}");
        }

        //UpdateObjectNotConnectedWarning(currentPlacedObject);
    }

    //Get farms or farmland only connected by other farm and farmland
    public List<PlacedObject> FindConnectedFarmsOrFarmland(Vector2Int pos, bool findFarmland)
    {
        Grid<GridObject> grid = gridBuildingSystem.GetGrid();
        List<PlacedObject> directlyConnectedObjects = new();

        GridObject currentGridObject = grid.GetGridObj(pos.x, pos.y);
        PlacedObject currentPlacedObject = currentGridObject.GetPlacedObject();

        List<Vector2Int> uncheckedPositions = new List<Vector2Int>() { pos };
        List<Vector2Int> checkedPositions = new();

        while (uncheckedPositions.Count > 0)
        {
            Vector2Int newPos = uncheckedPositions[0];
            List<Vector2Int> newAdjacentPositions = GetAdjacentRoadPositions(newPos);

            uncheckedPositions.Remove(newPos);
            checkedPositions.Add(newPos);

            foreach(Vector2Int newAdjPos in newAdjacentPositions)
            {
                if (checkedPositions.Contains(newAdjPos)) continue;
                checkedPositions.Add(newAdjPos);

                GridObject adjGridObject = grid.GetGridObj(newAdjPos.x, newAdjPos.y);
                if (adjGridObject == null) continue;
                PlacedObject adjPlacedObject = adjGridObject.GetPlacedObject();
                if(adjPlacedObject == null) continue;

                if(findFarmland)
                {
                    if (adjPlacedObject.name == "Boerderij")
                    {
                        uncheckedPositions.Add(newAdjPos);
                    }
                    else if (adjPlacedObject.name == "Akker")
                    {
                        uncheckedPositions.Add(newAdjPos);
                        directlyConnectedObjects.Add(adjPlacedObject);
                    }
                }
                else
                {
                    if (adjPlacedObject.name == "Boerderij")
                    {
                        directlyConnectedObjects.Add(adjPlacedObject);
                        uncheckedPositions.Add(newAdjPos);
                    }
                    else if (adjPlacedObject.name == "Akker")
                    {
                        uncheckedPositions.Add(newAdjPos);
                    }
                }
            }
        }

        return directlyConnectedObjects;
    }

    //Returns farms that are connected via road to origin building
    public List<PlacedObject> GetConnectedFarms(PlacedObject originBuilding)
    {
        List<PlacedObject> targetObjectGroup = new();
        foreach (List<PlacedObject> objGroup in connectedObjectGroups)
        {
            if (objGroup.Contains(originBuilding)) targetObjectGroup = objGroup;
        }
        List<PlacedObject> connectedFarms = new();
        foreach(PlacedObject obj in targetObjectGroup)
        {
            if(obj != null)
            {
                Debug.Log($"Object in {originBuilding.name} object group: {obj.name}");
                if (obj.name == "Boerderij") connectedFarms.Add(obj);
            }
        }
        return connectedFarms;
    }

    private List<PlacedObject> GetConnectedObjects(Vector2Int pos)
    {
        Grid<GridObject> grid = gridBuildingSystem.GetGrid();
        GridObject currentGridObject = grid.GetGridObj(pos.x, pos.y);
        PlacedObject currentPlacedObject = currentGridObject.GetPlacedObject();

        foreach(List<PlacedObject> objGroup in connectedObjectGroups)
        {
            if(objGroup.Contains(currentPlacedObject)) return objGroup;
        }
        throw new Exception("Object not in any group");
    }

    public void UpdateObjectNotConnectedWarning(PlacedObject obj, List<PlacedObject> objGroup)
    {
        BuildingScriptableObject buildingSO = obj.GetScriptableObject();
        int connectedBuildingCount = 0;

        foreach (PlacedObject connectedObj in objGroup)
        {
            //Custom logic depending on object ideally
            if(connectedObj != null)
            {
                if (!connectedObj.gameObject.CompareTag("Road")) connectedBuildingCount++;
            }
        }

        //Debug.Log($"ConnectNewObject: {connectedBuildingCount} buildings connected");

        if (connectedBuildingCount > 1)
        {
            if (obj.exclamationMark != null)
            {
                if (obj.gameObject.CompareTag("Farmland"))
                {
                    List<PlacedObject> connectedFarms = GetConnectedFarms(obj);
                    Debug.Log($"Found {connectedFarms.Count} farms adjacent to farmland");
                    
                    foreach(PlacedObject farm in connectedFarms)
                    {
                        StartCoroutine(DelayedWarningUpdate(farm, obj));
                        //if(farm.adjacentFarmlandWorked == null)
                        //{
                        //    Debug.LogWarning($"Adjacent Farmland Null for {farm.GetOrigin()}");
                        //    continue;
                        //}
                        //if(farm.adjacentFarmlandWorked.ContainsKey(obj))
                        //{
                        //    obj.exclamationMark.SetActive(false);
                        //    break;
                        //}
                    }
                    
                }
                else obj.exclamationMark.SetActive(false);
            }
            Debug.Log($"Connected: {obj.name}");
            if (buildingSO != null) economyManager.HandleNewConnectedBuilding(buildingSO, obj);
        }
        else
        {
            Debug.Log($"Found no buildings connected to {obj.name}");
            if (obj.exclamationMark != null) obj.exclamationMark.SetActive(true);
            if(buildingSO != null) economyManager.HandleDisconnectedBuilding(buildingSO, obj);
        }
    }

    IEnumerator DelayedWarningUpdate(PlacedObject farm, PlacedObject farmland)
    {
        int initialFarmlandCount = farm.adjacentFarmlandWorked.Count;
        int framesWaited = 0;
        while ((farm.adjacentFarmlandWorked == null) || (initialFarmlandCount == farm.adjacentFarmlandWorked.Count))
        {
            yield return null;
            if (!farmland.exclamationMark.activeSelf) break;

            framesWaited++;
            if(framesWaited == 5)
            {
                Debug.LogWarning("Broke out of delayed warning update");
                break;
            }
        }

        if(farmland.exclamationMark.activeSelf)
        {
            if (farm.adjacentFarmlandWorked.ContainsKey(farmland))
            {
                farmland.exclamationMark.SetActive(false);
            }
            else
            {
                Debug.Log("Farmland isnt being worked by farm");
            }
        }

        yield return null;
    }


    void UpdateRoad(MeshFilter road, string config)
    {
        //4 way intersection
        if(config == "crossroad" || config == "zero")
        {
            road.mesh = roadCrossroad;
        }

        //straight road vertical
        else if(config == "oneUp" || config == "oneDown" || config == "upDown")
        {
            road.mesh = roadStraight;
            road.transform.rotation = Quaternion.identity;
        }

        //straight road horizontal
        else if(config == "oneLeft" || config == "oneRight" || config == "rightLeft")
        {
            road.mesh = roadStraight;
            road.transform.rotation = Quaternion.Euler(0, 90, 0);
        }

        //turn: ^ >
        else if(config == "downRight")
        {
            road.mesh = roadTurn;
            road.transform.rotation = Quaternion.Euler(0, 180, 0);
        }

        //turn ^ <
        else if(config == "downLeft")
        {
            road.mesh = roadTurn;
            road.transform.rotation = Quaternion.Euler(0, 270, 0);
        }

        //turn: > ^ 
        else if(config == "upLeft")
        {
            road.mesh = roadTurn;
            road.transform.rotation = Quaternion.identity;
        }

        //turn: < ^ 
        else if(config == "upRight")
        {
            road.mesh = roadTurn;
            road.transform.rotation = Quaternion.Euler(0, 90, 0);
        }

        //3 way, not up
        else if(config == "notUp")
        {
            road.mesh = roadIntsct3;
            road.transform.rotation = Quaternion.Euler(0, 270, 0);
        }

        //3 way, not down
        else if(config == "notDown")
        {
            road.mesh = roadIntsct3;
            road.transform.rotation = Quaternion.Euler(0, 90, 0);
        }

        //3 way, not right
        else if(config == "notRight")
        {
            road.mesh = roadIntsct3;
            road.transform.rotation = Quaternion.identity;
        }

        //3 way, not left
        else if(config == "notLeft")
        {
            road.mesh = roadIntsct3;
            road.transform.rotation = Quaternion.Euler(0, 180, 0);
        }
    }

    string CheckAdjacentRoads(List<bool> isObjectThere)
    {
        string roadConfig = "";

        bool isRoadUp = isObjectThere[0];
        bool isRoadDown = isObjectThere[1];
        bool isRoadRight = isObjectThere[2];
        bool isRoadLeft = isObjectThere[3];

        //No surrounding roads, crossroads
        if(!isRoadUp && !isRoadDown && !isRoadRight && !isRoadLeft) roadConfig = "zero";

        //1 surrounding road (up), keep straight
        else if(isRoadUp && !isRoadDown && !isRoadRight && !isRoadLeft) roadConfig = "oneUp";

        //1 surrounding road (down), keep straight
        else if(!isRoadUp && isRoadDown && !isRoadRight && !isRoadLeft) roadConfig = "oneDown";

        //1 surrounding road (right)
        else if(!isRoadUp && !isRoadDown && isRoadRight && !isRoadLeft) roadConfig = "oneRight";

        //1 surrounding road (left)
        else if(!isRoadUp && !isRoadDown && !isRoadRight && isRoadLeft) roadConfig = "oneLeft";

        //2 surrounding roads (up, down)
        else if(isRoadUp && isRoadDown && !isRoadRight && !isRoadLeft) roadConfig = "upDown";

        //2 surrounding roads (up, right)
        else if(isRoadUp && !isRoadDown && isRoadRight && !isRoadLeft) roadConfig = "upRight";

        //2 surrounding roads (up, left)
        else if(isRoadUp && !isRoadDown && !isRoadRight && isRoadLeft) roadConfig = "upLeft";

        //2 surrounding roads (down, right)
        else if(!isRoadUp && isRoadDown && isRoadRight && !isRoadLeft) roadConfig = "downRight";

        //2 surrounding roads (down, left)
        else if(!isRoadUp && isRoadDown && !isRoadRight && isRoadLeft) roadConfig = "downLeft";

        //2 surrounding roads (left, right)
        else if(!isRoadUp && !isRoadDown && isRoadRight && isRoadLeft) roadConfig = "rightLeft";

        //3 surrounding roads (not up)
        else if(!isRoadUp && isRoadDown && isRoadRight && isRoadLeft) roadConfig = "notUp";

        //3 surrounding roads (not down)
        else if(isRoadUp && !isRoadDown && isRoadRight && isRoadLeft) roadConfig = "notDown";

        //3 surrounding roads (not right)
        else if(isRoadUp && isRoadDown && !isRoadRight && isRoadLeft) roadConfig = "notRight";

        //3 surrounding roads (not left)
        else if(isRoadUp && isRoadDown && isRoadRight && !isRoadLeft) roadConfig = "notLeft";

        //4 surrounding roads, crossroads
        else if(isRoadUp && isRoadDown && isRoadRight && isRoadLeft) roadConfig = "crossroad";

        Debug.Log($"Config for road: {roadConfig} ({isRoadUp}, {isRoadDown}, {isRoadRight}, {isRoadLeft}");

        return roadConfig;
    }

    List<Vector2Int> GetAdjacentRoadPositions(Vector2Int roadPos)
    {
        List<Vector2Int> adjacentRoads = new();

        Vector2Int roadUp = roadPos;
        roadUp.y += 1;
        adjacentRoads.Add(roadUp);

        Vector2Int roadDown = roadPos;
        roadDown.y -= 1;
        adjacentRoads.Add(roadDown);

        Vector2Int roadRight = roadPos;
        roadRight.x += 1;
        adjacentRoads.Add(roadRight);

        Vector2Int roadLeft = roadPos;
        roadLeft.x -= 1;
        adjacentRoads.Add(roadLeft);

        return adjacentRoads;
    }

    List<Vector2Int> GetAdjacentRoadPositionsGesticht(Vector2Int origin)
    {
        List<Vector2Int> adjacentRoads = new();

        Vector2Int downLeft = origin;
        downLeft.y -= 1;
        adjacentRoads.Add(downLeft);

        Vector2Int downMiddle = origin;
        downLeft.y -= 1;
        downLeft.x += 1;
        adjacentRoads.Add(downMiddle);

        Vector2Int downRight = origin;
        downLeft.y -= 1;
        downLeft.x += 2;
        adjacentRoads.Add(downLeft);

        Vector2Int leftDown = origin;
        leftDown.x -= 1;
        adjacentRoads.Add(leftDown);

        Vector2Int leftMiddle = origin;
        leftMiddle.x -= 1;
        leftMiddle.y += 1;
        adjacentRoads.Add(leftMiddle);

        Vector2Int leftUp = origin;
        leftUp.x -= 1;
        leftUp.y += 2;
        adjacentRoads.Add(leftUp);

        Vector2Int rightDown = origin;
        rightDown.x += 3;
        adjacentRoads.Add(rightDown);

        Vector2Int rightMiddle = origin;
        rightMiddle.x += 3;
        rightMiddle.y += 1;
        adjacentRoads.Add(rightMiddle);

        Vector2Int rightUp = origin;
        rightUp.x += 3;
        rightUp.y += 2;
        adjacentRoads.Add(rightUp);

        Vector2Int upLeft = origin;
        upLeft.y += 3;
        adjacentRoads.Add(upLeft);

        Vector2Int upMiddle = origin;
        upMiddle.y += 3;
        upMiddle.x += 1;
        adjacentRoads.Add(upMiddle);

        Vector2Int upRight = origin;
        upRight.x += 3;
        upMiddle.x += 2;
        adjacentRoads.Add(upRight);

        return adjacentRoads;
    }

    MeshFilter GetPlacedRoad(Vector2Int roadPos)
    {
        if(placedRoads.ContainsKey(roadPos))
        {
            return placedRoads[roadPos];
        }
        else return null;
    }
}
