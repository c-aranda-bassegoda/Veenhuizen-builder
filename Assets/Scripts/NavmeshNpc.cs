using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class NavmeshNpc : MonoBehaviour
{
    NavMeshAgent agent;
    PlacedObject originBuilding;
    string desiredBuilding, currentTargetBuilding;
    bool movingToTarget;

    float startY;
    [SerializeField] Image charImage;
    [SerializeField] float yBobTarget;
    [SerializeField] float bobSpeed;
    bool goingUp;
    bool isFindingTarget;
    bool hasTarget;
    bool stopCoroutine;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
    }

    private void Update()
    {
        if (!agent.pathPending) // Make sure the agent has a path
        {
            if(agent.remainingDistance > 2)
            {
                if(!isFindingTarget)
                {
                    StartCoroutine(TryFindTarget(true));
                }
                MoveAnimations();
            }
            else if (hasTarget)
            {
                Debug.Log("Found target");
                stopCoroutine = true;
                hasTarget = false;
                isFindingTarget = false;
            }
        }
    }

    void MoveAnimations()
    {
        if(goingUp)
        {
            if (charImage.transform.position.y < startY + yBobTarget) charImage.transform.position += new Vector3(0, bobSpeed * Time.deltaTime, 0);
            else goingUp = false;
        }
        else
        {
            if (charImage.transform.position.y > startY) charImage.transform.position += new Vector3(0, -bobSpeed * Time.deltaTime, 0);
            else goingUp = true;
        }
    }

    public void SetOrigin(PlacedObject _building)
    {
        originBuilding = _building;
        SetDesiredBuilding("Boerderij");
        //Debug.Log($"Set {gameObject.name} target to Farm");
        StartCoroutine(TryFindTarget(false));
    }

    public void SetDesiredBuilding(string buildingName)
    {
        desiredBuilding = buildingName;
    }

    // Update is called once per frame
    public void SetNavmeshTarget(Vector3 targetPos)
    {
        startY = charImage.transform.position.y;
        agent.SetDestination(targetPos);
        hasTarget = true;
    }
   

    IEnumerator TryFindTarget(bool findBetterPath)
    {
        bool foundTarget = false;
        isFindingTarget = true;
        stopCoroutine = false;

        while (!foundTarget)
        {
            List<PlacedObject> buildingsOfDesiredType = GridBuildingSystem.instance.GetBuildingsOfType(desiredBuilding);
            Debug.Log($"Buildings of type: {buildingsOfDesiredType.Count}");

            Dictionary<PlacedObject, float> accessibleBuildings = new();
            foreach(PlacedObject obj in buildingsOfDesiredType)
            {
                float distanceToObj = GetPathDistance(obj.transform.GetChild(0).position);
                if (distanceToObj >= 0)
                {
                    accessibleBuildings.Add(obj, distanceToObj);
                }
            }
            if (accessibleBuildings.Count > 0)
            {
                List<PlacedObject> accessibleBuildingsByDistance = accessibleBuildings.OrderBy(x => x.Value).Select(x => x.Key).ToList();
                PlacedObject closestAccessibleBuilding = accessibleBuildingsByDistance[0];
                Debug.Log($"Found nearest building for {gameObject.name}: {closestAccessibleBuilding.name}");

                if(!findBetterPath || (currentTargetBuilding != desiredBuilding))
                {
                    SetNavmeshTarget(closestAccessibleBuilding.transform.GetChild(0).position);
                    currentTargetBuilding = desiredBuilding;
                    foundTarget = true;
                }
                else
                {
                    if (agent.remainingDistance < 1000)
                    {
                        Debug.Log($"Considering better path: {GetPathDistance(accessibleBuildingsByDistance[0].transform.GetChild(0).position)} vs {agent.remainingDistance}");
                        if (GetPathDistance(accessibleBuildingsByDistance[0].transform.GetChild(0).position) < agent.remainingDistance)
                        {
                            SetNavmeshTarget(closestAccessibleBuilding.transform.GetChild(0).position);
                            currentTargetBuilding = desiredBuilding;
                            foundTarget = true;
                        }
                    }
                }
            }
            else
            {
                Debug.Log($"No accessible building found for {gameObject.name}");
            }

            if (stopCoroutine) break;

            yield return new WaitForSeconds(1);
        }

        isFindingTarget = false;
        yield return null;
    }

    public float GetPathDistance(Vector3 targetPosition)
    {
        NavMeshPath path = new NavMeshPath();
        if (!agent.CalculatePath(targetPosition, path) || path.status != NavMeshPathStatus.PathComplete)
            return -1f;  // Return -1 if unreachable

        float distance = 0f;

        // Sum the distances between path points
        for (int i = 0; i < path.corners.Length - 1; i++)
        {
            distance += Vector3.Distance(path.corners[i], path.corners[i + 1]);
        }

        return distance;
    }
}
