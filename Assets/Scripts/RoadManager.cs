using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.PlayerSettings;

public class RoadManager : MonoBehaviour
{
    [SerializeField] Mesh roadStraight, roadTurn, roadCrossroad, roadIntsct3;
    public Dictionary<Vector2Int, MeshFilter> placedRoads = new();
    [SerializeField] GameObject roadPrefab;
    [SerializeField] LayerMask roadTestLayer;
    [SerializeField] GridBuildingSystem gridBuildingSystem;
    [SerializeField] EconomyManager economyManager;

    public void Update()
    {

    }

    public void PlaceRoad(Vector2Int roadPos, MeshFilter newRoadMesh)
    {
        if (!placedRoads.ContainsKey(roadPos))
        {
            placedRoads.Add(roadPos, newRoadMesh);
            UpdateRoads(roadPos);
        }
    }

    public void RemoveRoad(Vector2Int roadPos)
    {
        if (placedRoads.ContainsKey(roadPos))
        {
            //Destroy(GetPlacedRoad(roadPos));
            placedRoads.Remove(roadPos);
            UpdateRoads(roadPos);
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
    public void UpdateRoads(Vector2Int roadPos, bool placingRoad = true)
    {
        List<Vector2Int> roadsToUpdate = GetAdjacentRoadPositions(roadPos);
        if(placingRoad) roadsToUpdate.Add(roadPos);
        CheckRoadConnectionOnBuild(roadPos);

        foreach (Vector2Int pos in roadsToUpdate)
        {
            MeshFilter roadToUpdate = GetPlacedRoad(pos);
            if(roadToUpdate == null) continue;

            Debug.Log($"Checking adjacent positions for {pos.x}, {pos.y}");

            //Sorted: Up, Down, Right, Left
            List<Vector2Int> adjacentRoadPositions = GetAdjacentRoadPositions(pos);
            List<bool> isRoadThere = FindAdjacentObjects(adjacentRoadPositions, pos);

            string roadConfig = CheckAdjacentRoads(isRoadThere);
            UpdateRoad(roadToUpdate, roadConfig);
        }
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

    private void CheckRoadConnectionOnBuild(Vector2Int pos)
    {
        List<PlacedObject> connectedObjects = GetConnectedObjects(pos, false);

        foreach(PlacedObject connectedObject in connectedObjects)
        {
            foreach (PlacedObject addingConnectedObject in connectedObjects)
            {
                if( (connectedObject != addingConnectedObject) && (!connectedObject.connectedObjects.Contains(addingConnectedObject)) )
                {
                    connectedObject.connectedObjects.Add(addingConnectedObject);
                }
            }

            UpdateObjectNotConnectedWarning(connectedObject);
        }
    }

    public void CheckRoadConnectionOnDelete(Vector2Int pos)
    {
        List<PlacedObject> connectedObjects = GetConnectedObjects(pos, true);

        Debug.Log($"Objects connected to {pos}: {connectedObjects.Count}");

        Dictionary<Vector2Int, PlacedObject> connectedObjectPositions = new();

        foreach(PlacedObject obj in connectedObjects)
        {
            Vector2Int objPos = obj.GetOrigin();
            connectedObjectPositions.Add(objPos, obj);

            //Debug.Log($"Connected Objects: {obj.name} at {objPos}");
        }

        foreach(KeyValuePair<Vector2Int, PlacedObject> obj in connectedObjectPositions)
        {
            List<PlacedObject> currentConnectedObjects = GetConnectedObjects(obj.Key, false);
            obj.Value.connectedObjects.Clear();
            Debug.Log($"Clearing {obj.Value.name}");
            foreach(PlacedObject _obj in currentConnectedObjects)
            {
                if (obj.Value != _obj)
                {
                    obj.Value.connectedObjects.Add(_obj);
                    Debug.Log($"Adding: {_obj.name} to {obj.Value.name} at {obj.Key}");
                }
            }

            UpdateObjectNotConnectedWarning(obj.Value);
        }
    }

    private List<PlacedObject> GetConnectedObjects(Vector2Int pos, bool isDelete)
    {
        Grid<GridObject> grid = gridBuildingSystem.GetGrid();
        bool skipFirstObjectCheck = isDelete;

        List<Vector2Int> checkedPositions = new();
        List<Vector2Int> uncheckedPositions = new();
        List<PlacedObject> connectedObjects = new();

        uncheckedPositions.Add(pos);

        while (uncheckedPositions.Count > 0)
        {
            List<Vector2Int> newPositionsTempList = new();
            List<Vector2Int> oldPositionsTempList = new();

            foreach (Vector2Int uncheckedPos in uncheckedPositions)
            {
                Debug.Log($"Checking New Position: {uncheckedPos}");
                oldPositionsTempList.Add(uncheckedPos);
                checkedPositions.Add(uncheckedPos);

                GridObject gridObject = grid.GetGridObj(uncheckedPos.x, uncheckedPos.y);
                PlacedObject placedObject = null;

                if (gridObject != null)
                {
                    placedObject = gridObject.GetPlacedObject();
                }

                if (placedObject == null && !skipFirstObjectCheck) continue;

                if (!skipFirstObjectCheck)
                {
                    if (placedObject.name != "Road" && !connectedObjects.Contains(placedObject))
                    {
                        //Debug.Log($"Adding New Position Object: {uncheckedPos} , {placedObject.name}");
                        connectedObjects.Add(placedObject);
                    }
                }
                skipFirstObjectCheck = false;

                List<Vector2Int> adjacentRoadPositions = GetAdjacentRoadPositions(uncheckedPos);

                foreach (Vector2Int adjRoadPos in adjacentRoadPositions)
                {
                    if (!checkedPositions.Contains(adjRoadPos))
                    {
                        newPositionsTempList.Add(adjRoadPos);
                    }
                }
            }

            foreach (Vector2Int tempPos in oldPositionsTempList)
            {
                uncheckedPositions.Remove(tempPos);
            }

            foreach (Vector2Int tempPos in newPositionsTempList)
            {
                uncheckedPositions.Add(tempPos);
            }
        }

        return connectedObjects;
    }

    public void UpdateObjectNotConnectedWarning(PlacedObject obj)
    {
        BuildingScriptableObject buildingSO = obj.GetScriptableObject();
        if(obj.connectedObjects.Count > 0)
        {
            obj.exclamationMark.SetActive(false);
            Debug.Log($"Connected: {obj.name}");
            if (buildingSO != null) economyManager.HandleNewBuilding(buildingSO, obj);
        }
        else
        {
            obj.exclamationMark.SetActive(true);
            if(buildingSO != null) economyManager.HandleRemovedBuilding(buildingSO, obj);
        }
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

    MeshFilter GetPlacedRoad(Vector2Int roadPos)
    {
        if(placedRoads.ContainsKey(roadPos))
        {
            return placedRoads[roadPos];
        }
        else return null;
    }
}
