using System.Collections;
using System.Collections.Generic;
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

    public void Update()
    {
        //if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
        //{
        //    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        //    RaycastHit hit;

        //    // Raycast into the scene
        //    if (Physics.Raycast(ray, out hit, 100f, roadTestLayer))
        //    {
        //        Vector2Int roadPos = new();
        //        roadPos.x = (int)Mathf.Round(hit.point.x);
        //        roadPos.y = (int)Mathf.Round(hit.point.z);

        //        if (Input.GetMouseButton(0))
        //        {
        //            if (!placedRoads.ContainsKey(roadPos))
        //            {
        //                GameObject newRoad = Instantiate(roadPrefab, new Vector3(roadPos.x, 2, roadPos.y), Quaternion.identity);
        //                newRoad.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
        //                MeshFilter roadMeshFilter = newRoad.GetComponent<MeshFilter>();

        //                placedRoads.Add(roadPos, roadMeshFilter);
        //                UpdateRoads(roadPos);
        //            }
        //        }
        //        else if (Input.GetMouseButton(1))
        //        {
        //            if (placedRoads.ContainsKey(roadPos))
        //            {
        //                Destroy(GetPlacedRoad(roadPos));
        //                placedRoads.Remove(roadPos);
        //                UpdateRoads(roadPos);
        //            }
        //        }
        //    }
        //}
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
        List<bool> isRoadThere = FindAdjacentRoads(adjacentRoadPositions, newPosition);
        string roadConfig = CheckAdjacentRoads(isRoadThere);
        UpdateRoad(roadToUpdate, roadConfig);
    }
    public void UpdateRoads(Vector2Int roadPos, bool placingRoad = true)
    {
        List<Vector2Int> roadsToUpdate = GetAdjacentRoadPositions(roadPos);
        if(placingRoad) roadsToUpdate.Add(roadPos);

        foreach (Vector2Int pos in roadsToUpdate)
        {
            Debug.Log($"{pos.x}, {pos.y}");
        }

        foreach (Vector2Int pos in roadsToUpdate)
        {
            MeshFilter roadToUpdate = GetPlacedRoad(pos);
            if(roadToUpdate == null) continue;

            Debug.Log($"Checking adjacent positions for {pos.x}, {pos.y}");

            //Sorted: Up, Down, Right, Left
            List<Vector2Int> adjacentRoadPositions = GetAdjacentRoadPositions(pos);
            List<bool> isRoadThere = FindAdjacentRoads(adjacentRoadPositions, pos);

            string roadConfig = CheckAdjacentRoads(isRoadThere);
            UpdateRoad(roadToUpdate, roadConfig);
        }
    }

    List<bool> FindAdjacentRoads(List<Vector2Int> adjacentRoadPositions, Vector2Int pos)
    {
        List<bool> isRoadThere = new() { false, false, false, false };
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
                    isRoadThere[i] = true;
                }
            }
            else
            {
                Debug.Log($"No object found at {adjacentRoadPositions[i].x}, {adjacentRoadPositions[i].y}");
            }
        }
        return isRoadThere;
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

    string CheckAdjacentRoads(List<bool> isRoadThere)
    {
        string roadConfig = "";

        bool isRoadUp = isRoadThere[0];
        bool isRoadDown = isRoadThere[1];
        bool isRoadRight = isRoadThere[2];
        bool isRoadLeft = isRoadThere[3];

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
