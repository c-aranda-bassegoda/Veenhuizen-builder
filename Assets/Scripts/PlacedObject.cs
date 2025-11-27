using System.Collections;
using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class PlacedObject : MonoBehaviour
{
    public static PlacedObject Create(Vector3 worldPosition, Vector2Int origin, BuildingScriptableObject.Dir dir, BuildingScriptableObject placedObjectSO, bool inInstitution)
    {
        GameObject placedObjTransform;
        PlacedObject placedObject = null;
        if (!placedObjectSO.modular)
        {
            GameObject prefab = (inInstitution ? placedObjectSO.modulePrefab : placedObjectSO.prefab);
            placedObjTransform = Instantiate(prefab, worldPosition, Quaternion.Euler(0, placedObjectSO.GetRotationAngle(dir), 0));

            placedObject = placedObjTransform.GetComponent<PlacedObject>();
            placedObject.placedSctiptableObject = placedObjectSO;
            placedObject.origin = origin;
            placedObject.dir = dir;
            placedObject.worldPosition = worldPosition;
            placedObject.name = placedObjectSO.name;
            placedObject.isModule = placedObjectSO.module;
        }
        else
        {
            GameObject corePrefab = placedObjectSO.modules[0];
            GameObject coreObj = Instantiate(
                corePrefab,
                worldPosition,
                Quaternion.Euler(0, placedObjectSO.GetRotationAngle(dir), 0)
            );

            PlacedObject corePlaced = coreObj.GetComponent<PlacedObject>();
            corePlaced.placedSctiptableObject = placedObjectSO;
            corePlaced.origin = origin;
            corePlaced.dir = dir;
            corePlaced.worldPosition = worldPosition;
            corePlaced.name = placedObjectSO.name + "_Core";
            corePlaced.isModule = false;
            corePlaced.modules = new List<PlacedObject>();

            Vector3[] offsets =
            {
                new Vector3(+2, 0, +2),
                new Vector3(+2, 0, -2),
                new Vector3(-2, 0, +2),
                new Vector3(-2, 0, -2),
            };

            foreach (var offset in offsets)
            {
                Vector3 rotatedOffset =
                    Quaternion.Euler(0, placedObjectSO.GetRotationAngle(dir), 0) * offset;

                Vector3 modulePos = worldPosition + rotatedOffset;

                GameObject modObj = Instantiate(
                    placedObjectSO.modules[1],
                    modulePos,
                    Quaternion.Euler(0, placedObjectSO.GetRotationAngle(dir), 0)
                );

                PlacedObject modPlaced = modObj.GetComponent<PlacedObject>();
                modPlaced.placedSctiptableObject = placedObjectSO;
                modPlaced.origin = origin;
                modPlaced.dir = dir;
                modPlaced.worldPosition = modulePos;
                modPlaced.name = placedObjectSO.name + "_Module";
                modPlaced.isModule = true;

                corePlaced.modules.Add(modPlaced);
            }
        }

        return placedObject;
    }
    private BuildingScriptableObject placedSctiptableObject;
    private Vector2Int origin;
    private BuildingScriptableObject.Dir dir;
    private Vector3 worldPosition;


    public GameObject exclamationMark;
    public List<PlacedObject> connectedObjects;
    public List<PlacedObject> modules;
    public bool isModule;

    public PlacedObject Repalace(BuildingScriptableObject placedObjectSO, bool inInstitution)
    {
        Vector3 pos = this.worldPosition;
        Vector2Int orig = this.origin;
        BuildingScriptableObject.Dir direction = this.dir;

        GameObject oldObject = this.gameObject;

        PlacedObject placedObject = null;
        GameObject prefab = (inInstitution ? placedObjectSO.modulePrefab : placedObjectSO.prefab);
        GameObject placedObjTransform = Instantiate(prefab, this.worldPosition, Quaternion.Euler(0, placedObjectSO.GetRotationAngle(this.dir), 0));

        placedObject = placedObjTransform.GetComponent<PlacedObject>();
        placedObject.placedSctiptableObject = placedObjectSO;
        placedObject.origin = this.origin;
        placedObject.dir = this.dir;
        placedObject.worldPosition = this.worldPosition;
        placedObject.name = placedObjectSO.name;
        placedObject.isModule = placedObjectSO.module;

        Destroy(oldObject);

        return placedObject;
    }
    [Header("People")]
    [SerializeField] NPC person;
    [SerializeField] int personAmount;
    [SerializeField] float timeBetweenSpawns;
    [SerializeField] List<NPC> associatedPeople;
    [SerializeField] Transform buildingOrigin;
    bool spawnedPeople;

    [SerializeField] private float moveToCentreMultiplier = 0.01f;
    [SerializeField] private float moveToPlaceMultiplier = 0.01f;
    [SerializeField] private float matchVelocityMultiplier = 0.125f;
    [SerializeField] private float minimumBoidDistance = 3f;

    [SerializeField] private Vector3 minMoveBounds;
    [SerializeField] private Vector3 maxMoveBounds;

    public void OnPlace()
    {
        Debug.Log("building start");
        buildingOrigin = transform.GetChild(0);

        minMoveBounds = buildingOrigin.position + buildingOrigin.TransformDirection(new Vector3(+10, 0, -10));
        maxMoveBounds = buildingOrigin.position + buildingOrigin.TransformDirection(new Vector3(-10, 0, +10));

        StartCoroutine(SpawnPeople());
    }

    IEnumerator SpawnPeople()
    {
        associatedPeople = new();

        for(int i = 0; i < personAmount; i++)
        {
            NPC newNpc = Instantiate(person, buildingOrigin.position + new Vector3(0, 3, 0), transform.rotation);
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
        int associatedPeopleAmt = associatedPeople.Count;
        foreach(NPC _npc in associatedPeople)
        {
            Destroy(_npc.gameObject);
        }
        associatedPeople.Clear();

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

        //Debug.Log($"Rule 1: {(percievedCentreOfMass - npc.transform.position) * moveToCentreMultiplier}");
        percievedCentreOfMass.y = 0;
        return (percievedCentreOfMass - npc.transform.position) * moveToCentreMultiplier;
    }

    //Seperate the boids from eachother
    Vector3 Rule2(NPC npc)
    {
        Vector3 displacement = new Vector3(0, 0, 0);
        foreach (NPC _npc in associatedPeople)
        {
            if (npc != _npc)
            {
                if (Vector3.Distance(_npc.transform.position, npc.transform.position) < minimumBoidDistance)
                {
                    displacement = displacement - (_npc.transform.position - npc.transform.position);
                    //if(associatedPeople.IndexOf(npc) == 0) Debug.Log($"Rule 2: {displacement} ({Vector3.Distance(_npc.transform.position, npc.transform.position)})");
                }
            }
        }

        displacement.y = 0;
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

        percievedVelocity.y = 0;
        //Debug.Log($"Rule 3: {percievedVelocity * matchVelocityMultiplier}");
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

        //Debug.Log($"Rule 4: {boundsCorrection}");
        boundsCorrection.y = 0;
        return boundsCorrection;
    }

    internal BuildingScriptableObject.Dir GetDir()
    {
        return dir;
    }
}
