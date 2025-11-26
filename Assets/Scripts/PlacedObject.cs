using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class PlacedObject : MonoBehaviour
{
    public static PlacedObject Create(Vector3 worldPosition, Vector2Int origin, BuildingScriptableObject.Dir dir, BuildingScriptableObject placedObjectSO)
    {
        GameObject placedObjTransform = Instantiate(placedObjectSO.prefab, worldPosition, Quaternion.Euler(0, placedObjectSO.GetRotationAngle(dir), 0));
        
        PlacedObject placedObject = placedObjTransform.GetComponent<PlacedObject>();
        placedObject.placedSctiptableObject = placedObjectSO;
        placedObject.origin = origin;
        placedObject.dir = dir;
        placedObject.name = placedObjectSO.name;

        return placedObject;
    }
    private BuildingScriptableObject placedSctiptableObject;
    private Vector2Int origin;
    private BuildingScriptableObject.Dir dir;

    public GameObject exclamationMark;
    public List<PlacedObject> connectedObjects;

    [Header("People")]
    [SerializeField] NPC person;
    [SerializeField] int personAmount;
    [SerializeField] float timeBetweenSpawns;
    List<NPC> associatedPeople;
    Transform buildingOrigin;
    bool spawnedPeople;

    [SerializeField] private float moveToCentreMultiplier = 0.01f;
    [SerializeField] private float moveToPlaceMultiplier = 0.01f;
    [SerializeField] private float matchVelocityMultiplier = 0.125f;
    [SerializeField] private float distanceMultiplier = 5f;
    [SerializeField] private float minimumBoidDistance = 10f;

    [SerializeField] private Vector3 minMoveBounds;
    [SerializeField] private Vector3 maxMoveBounds;

    public void OnPlace()
    {
        Debug.Log("building start");
        buildingOrigin = transform.GetChild(0);

        minMoveBounds = buildingOrigin.position + (buildingOrigin.forward * 5) + (-buildingOrigin.right * 5);
        maxMoveBounds = buildingOrigin.position + (buildingOrigin.forward * 10) + (-buildingOrigin.right * 5);
        StartCoroutine(SpawnPeople());
    }

    IEnumerator SpawnPeople()
    {
        associatedPeople = new();

        for(int i = 0; i < personAmount; i++)
        {
            NPC newNpc = Instantiate(person, buildingOrigin.position, transform.rotation);
            associatedPeople.Add(newNpc);
            newNpc.SetOrigin(this);
            yield return new WaitForSeconds(timeBetweenSpawns);
        }
    }

    private void FixedUpdate()
    {
        MoveNPCs();
    }

    void MoveNPCs()
    {
        if (associatedPeople == null) return;

        Vector3 v1, v2, v3, v4;
        foreach (NPC _npc in associatedPeople)
        {
            v1 = Rule1(_npc);
            v2 = Rule2(_npc);
            v3 = Rule3(_npc);
            v4 = Rule4(_npc);

            _npc.SetVelocity(v1, v2, v3, v4);
            _npc.MoveNpc();
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
        Destroy(gameObject);
    }
    //Make the boids move as a group
    Vector3 Rule1(NPC npc)
    {
        Vector3 percievedCentreOfMass = new Vector3(0, 0, 0);

        foreach (NPC _npc in associatedPeople)
        {
            if (_npc.gameObject != _npc.gameObject)
            {
                percievedCentreOfMass = percievedCentreOfMass + _npc.transform.position;
            }
        }
        percievedCentreOfMass = percievedCentreOfMass / (associatedPeople.Count - 1);

        if (float.IsNaN(((percievedCentreOfMass - npc.transform.position) * moveToCentreMultiplier).x)) return Vector3.zero;
        if (float.IsNaN(((percievedCentreOfMass - npc.transform.position) * moveToCentreMultiplier).y)) return Vector3.zero;
        if (float.IsNaN(((percievedCentreOfMass - npc.transform.position) * moveToCentreMultiplier).z)) return Vector3.zero;

        Debug.Log($"Rule 1: {(percievedCentreOfMass - npc.transform.position) * moveToCentreMultiplier}");
        return (percievedCentreOfMass - npc.transform.position) * moveToCentreMultiplier;
    }

    //Seperate the boids from eachother
    Vector3 Rule2(NPC npc)
    {
        Vector3 displacement = new Vector3(0, 0, 0);
        foreach (NPC _npc in associatedPeople)
        {
            if (npc.gameObject != _npc.gameObject)
            {
                if (Vector2.Distance(_npc.transform.position, npc.transform.position) < minimumBoidDistance)
                {
                    displacement = (displacement - (_npc.transform.position - npc.transform.position)) * distanceMultiplier;
                }
            }
        }

        Debug.Log($"Rule 2: {displacement}");
        return displacement;
    }

    //Match velocity of other boids
    Vector3 Rule3(NPC npc)
    {
        if (associatedPeople.Count <= 1) return Vector3.zero;

        Vector3 percievedVelocity = new Vector3(0, 0, 0);
        foreach (NPC _npc in associatedPeople)
        {
            if (npc.gameObject != _npc.gameObject)
            {
                percievedVelocity += _npc.GetVelocity();
            }
        }

        percievedVelocity = percievedVelocity / (associatedPeople.Count - 1);


        if(float.IsNaN(percievedVelocity.x * matchVelocityMultiplier)) return Vector3.zero;
        if (float.IsNaN(percievedVelocity.y * matchVelocityMultiplier)) return Vector3.zero;
        if (float.IsNaN(percievedVelocity.z * matchVelocityMultiplier)) return Vector3.zero;

        Debug.Log($"Rule 3: {percievedVelocity * matchVelocityMultiplier}");
        return percievedVelocity * matchVelocityMultiplier;
    }

    //Make sure the boids stay within bounds
    Vector3 Rule4(NPC npc)
    {
        Vector3 boundsCorrection = new Vector3(0, 0, 0);

        if (npc.transform.position.x < minMoveBounds.x) boundsCorrection.x = 10;
        else if (npc.transform.position.x > maxMoveBounds.x) boundsCorrection.x = -10;
        if (npc.transform.position.y < minMoveBounds.y) boundsCorrection.y = 10;
        else if (npc.transform.position.y > maxMoveBounds.y) boundsCorrection.y = -10;
        if (npc.transform.position.z < minMoveBounds.z) boundsCorrection.z = 10;
        else if (npc.transform.position.z > maxMoveBounds.z) boundsCorrection.z = -10;

        Debug.Log($"Rule 4: {boundsCorrection}");
        return boundsCorrection;
    }
}
