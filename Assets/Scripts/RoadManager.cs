using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RoadManager : MonoBehaviour
{
    [SerializeField] GameObject roadStraight, roadTurn, roadIntsct4, roadIntsct3;
    public Dictionary<Vector2Int, GameObject> placedRoads;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        roadPositions = new();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void UpdateRoads(Vector2Int roadPos)
    {
        List<Vector2Int> roadsToUpdate = GetAdjacentRoadPositions(roadPos);
        roadsToUpdate.Add(roadPos);

        foreach(Vector2Int pos in roadsToUpdate)
        {
            GameObject roadToUpdate = GetPlacedRoad(pos);
            if(roadToUpdate == null) continue;

            //Sorted: Up, Down, Right, Left
            List<Vector2Int> adjacentRoadPositions = GetAdjacentRoadPositions(pos);
            List<bool> isRoadThere = new() {false, false, false, false};

            for(int i = 0; i < adjacentRoadPositions.Count; i++)
            {
                Vector2Int adjPos = adjacentRoadPositions[i];
                GameObject adjRoad = GetPlacedRoad(adjPos);
                if(adjRoad != null)
                {
                    isRoadThere[i] = true;
                }
            }

            string roadConfig = CheckAdjacentRoads(isRoadThere);
            UpdateRoad(roadToUpdate, roadConfig);
        }
    }

    void UpdateRoad(GameObject road, string config)
    {
        //4 way intersection
        if(config == "crossroad" || config == "zero")
        {

        }

        //straight road vertical
        else if(config == "oneUp" || config == "oneDown" || config == "upDown")
        {

        }

        //straight road horizontal
        else if(config == "oneLeft" || config == "oneRight" || config == "rightLeft")
        {

        }

        //turn: ^ >
        else if(config == "downRight")
        {

        }

        //turn ^ <
        else if(config == "downLeft")
        {

        }

        //turn: > ^ 
        else if(config == "upLeft")
        {

        }

        //turn: < ^ 
        else if(config == "upRight")
        {

        }

        //3 way, not up
        else if(config == "notUp")
        {

        }

        //3 way, not down
        else if(config == "notDown")
        {

        }

        //3 way, not right
        else if(config == "notRight")
        {

        }

        //3 way, not left
        else if(config == "notLeft")
        {

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
        else if(!isRoadUp && isRoadDown && isRoadRight && isRoadLeft) roadConfig = "crossroad";
    }

    List<Vector2Int> GetAdjacentRoadPositions(Vector2Int roadPos)
    {
        List<Vector2Int> adjacentRoads = new();

        Vector2Int roadUp = roadPos;
        roadUp.y + 1;
        adjacentRoads.Add(roadUp);

        Vector2Int roadDown = roadPos;
        roadUp.y - 1;
        adjacentRoads.Add(roadDown);

        Vector2Int roadRight = roadPos;
        roadRight.x + 1;
        adjacentRoads.Add(roadRight);

        Vector2Int roadLeft = roadPos;
        roadRight.x - 1;
        adjacentRoads.Add(roadLeft);

        return adjacentRoads;
    }

    GameObject GetPlacedRoad(Vector2Int roadPos)
    {
        if(placedRoads[roadPos] != null)
        {
            return placedRoads[roadPos];
        }
        else return null;
    }
}
