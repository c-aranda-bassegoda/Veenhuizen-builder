using UnityEngine;

[CreateAssetMenu(fileName = "BuildingScriptableObject", menuName = "Scriptable Objects/BuildingScriptableObject")]
public class BuildingScriptableObject : ScriptableObject
{
    public new string name;
    public Transform prefab;
    public Transform visual;
    public int width;
    public int height;

    
}
