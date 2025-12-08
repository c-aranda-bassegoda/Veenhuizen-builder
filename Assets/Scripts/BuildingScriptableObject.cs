using System.Collections.Generic;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Timeline;

[CreateAssetMenu(fileName = "BuildingScriptableObject", menuName = "Scriptable Objects/BuildingScriptableObject")]
public class BuildingScriptableObject : ScriptableObject
{
    public static Dir GetNextDir(Dir dir)
    {
        switch (dir)
        {
            case Dir.Down: return Dir.Left;
            case Dir.Left: return Dir.Up;
            case Dir.Up: return Dir.Right;
            case Dir.Right: 
            default: return Dir.Down;
        }
    }

    public enum Dir
    {
        Down,
        Left,
        Up,
        Right,
    }

    public new string name;
    // id?
    public GameObject prefab;
    public GameObject modulePrefab;
    public GameObject moduleCore;
    public List<BuildingScriptableObject> moduleBSOs;
    public List<Vector2Int> modulePositions;
    public int width;
    public int height;
    public float hapiness;
    public float control;
    public float buildCost;
    public float yearlyCost;
    public float yearlyEarnings;
    public float yearlyFoodCost;
    public float yearlyFoodEarnings;
    public int maxPlacements;
    [DoNotSerialize] public bool module;
    public bool modular;
    public float GetControl() { return control; }
    public float GetHapiness() { return hapiness; }
    public Dir Direction { get; set; }

    public int GetRotationAngle(Dir dir)
    {
        switch (dir)
        {
            default: 
            case Dir.Down: return 0;
            case Dir.Left: return 90;
            case Dir.Up: return 180;
            case Dir.Right: return 270;
        }
    }

    public Vector2Int GetRotationOffset(Dir dir)
    {
        switch (dir)
        {
            default:
            case Dir.Down: return new Vector2Int(0,0);
            case Dir.Left: return new Vector2Int(0,width);
            case Dir.Up: return new Vector2Int(width, height);
            case Dir.Right: return new Vector2Int(height, 0);
        }
    }
    public List<Vector2Int> GetGridPositionList(Vector2Int offset, Dir dir)
    {
        Debug.Log($"Offset is: {offset}");
        List<Vector2Int> gridPositionList = new List<Vector2Int>();
        int helpWidth = 0, helpHeight = 0;
        switch (dir)
        {
            case Dir.Down:
            case Dir.Up:
                helpWidth = width;
                helpHeight = height;
                break;
            case Dir.Left:
            case Dir.Right:
                helpWidth = height;
                helpHeight = width;
                break;
        }
        for (int i = 0; i < helpWidth; i++)
        {
            for (int j = 0; j < helpHeight; j++)
            {
                gridPositionList.Add(offset + new Vector2Int(i, j));
            }
        }
        return gridPositionList;
    }
}
