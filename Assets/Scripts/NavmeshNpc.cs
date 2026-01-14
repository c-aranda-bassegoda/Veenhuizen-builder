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
    public PlacedObject destinationBuilding, targetFarm;
    public Vector3 navmeshDestination;
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
    bool isHome;

    int frameCount;

    void Start()
    {
        isHome = true;
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
    }

    private void Update()
    {
        if (!agent.pathPending) // Make sure the agent has a path
        {
            if(agent.remainingDistance > 2)
            {
                //if(!isFindingTarget)
                //{
                //    StartCoroutine(TryFindTarget(true));
                //}
            }
            else if (hasTarget)
            {
                Debug.Log("Found target");
                stopCoroutine = true;
                hasTarget = false;
                isFindingTarget = false;
            }
        }

        if(!isHome)
        {
            MoveAnimations();
            CheckConnection();
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

    void CheckConnection()
    {
        frameCount++;
        if(frameCount >= 15)
        {
            frameCount = 0;
            if(!IsConnectedToOrigin())
            {
                originBuilding.SendNpcBack(this, true);
            }
            else if (!IsConnectedToTarget())
            {
                originBuilding.SendNpcBack(this, false);
                SetNavmeshTarget(originBuilding.transform.GetChild(0).position, originBuilding);
            }
        }
    }

    public bool IsConnectedToOrigin()
    {
        float pathDistance = GetPathDistance(originBuilding.transform.GetChild(0).position);
        Debug.Log($"Is Connected To Origin: {pathDistance}");
        if (pathDistance == -1)
        {
            return false;
        }
        return true;
    }

    public bool IsConnectedToTarget()
    {
        if(destinationBuilding == null) return false;
        float pathDistance = GetPathDistance(navmeshDestination);
        Debug.Log($"Is Connected To Target: {pathDistance}");
        if (pathDistance == -1)
        {
            return false;
        }
        return true;
    }

    public void SetOrigin(PlacedObject _building)
    {
        originBuilding = _building;
        SetDesiredBuilding("Akker");
        //Debug.Log($"Set {gameObject.name} target to Farm");
        //StartCoroutine(TryFindTarget(false));
    }

    public Dictionary<PlacedObject, float> CanFindTarget()
    {
        List<PlacedObject> buildingsOfDesiredType = GridBuildingSystem.instance.GetBuildingsOfType(desiredBuilding);
        Debug.Log($"Buildings of type: {buildingsOfDesiredType.Count}");

        Dictionary<PlacedObject, float> accessibleBuildings = new();
        foreach (PlacedObject obj in buildingsOfDesiredType)
        {
            Transform childObj = obj.transform.GetChild(0);
            if(childObj != null)
            {
                float distanceToObj = GetPathDistance(obj.transform.GetChild(0).position);
                if (distanceToObj >= 0)
                {
                    accessibleBuildings.Add(obj, distanceToObj);
                }
            }
            else
            {
                Debug.LogWarning($"Child null: {obj.name}");
            }
        }

        return accessibleBuildings;
    }

    public void FindTarget(Dictionary<PlacedObject, float> _accessibleBuildings)
    {
        StartCoroutine(TryFindTarget(false, _accessibleBuildings));
    }

    public void SetDesiredBuilding(string buildingName)
    {
        desiredBuilding = buildingName;
    }

    public void SetNavmeshTarget(Vector3 targetPos, PlacedObject building, PlacedObject _targetFarm = null)
    {
        startY = charImage.transform.position.y;
        agent.SetDestination(targetPos);
        destinationBuilding = building;
        navmeshDestination = targetPos;
        if (_targetFarm != null) targetFarm = _targetFarm;
        hasTarget = true;
        isHome = false;
        agent.isStopped = false;
    }

    public void ReturnHome()
    {
        agent.isStopped = true;
        agent.ResetPath();
        hasTarget = false;
        isHome = true;
        targetFarm = null;
        Debug.Log($"Sending agent back to {originBuilding.transform.GetChild(0).position}");
        agent.Warp(originBuilding.transform.GetChild(0).position);
    }

    public PlacedObject GetClosestObjectFromList(List<PlacedObject> objects)
    {
        if(objects.Count == 0)
        {
            Debug.LogWarning("No objects in list to get closest");
            return null;
        }
        Debug.Log($"Object list count: {objects.Count}");
        Dictionary<PlacedObject, float> objectDict = new();
        foreach (PlacedObject obj in objects)
        {
            float distanceToObj = GetPathDistance(obj.transform.GetChild(0).position);
            objectDict.Add(obj, distanceToObj);
            
        }
        Debug.Log($"Object dict count: {objectDict.Count}");
        List<PlacedObject> objectsByDistance = objectDict.OrderBy(x => x.Value).Select(x => x.Key).ToList();
        PlacedObject closestObject = objectsByDistance[0];

        return closestObject;
    }
   
    IEnumerator TryFindTarget(bool findBetterPath, Dictionary<PlacedObject, float> _accessibleBuildings = null)
    {
        bool foundTarget = false;
        isFindingTarget = true;
        stopCoroutine = false;

        while (!foundTarget)
        {
            Dictionary<PlacedObject, float> accessibleBuildings = new();

            if (_accessibleBuildings == null)
            {
                List<PlacedObject> buildingsOfDesiredType = GridBuildingSystem.instance.GetBuildingsOfType(desiredBuilding);
                Debug.Log($"Buildings of type: {buildingsOfDesiredType.Count}");

                foreach (PlacedObject obj in buildingsOfDesiredType)
                {
                    float distanceToObj = GetPathDistance(obj.transform.GetChild(0).position);
                    if (distanceToObj >= 0)
                    {
                        accessibleBuildings.Add(obj, distanceToObj);
                    }
                }
            }
            else
            {
                accessibleBuildings = _accessibleBuildings;
            }
            if (accessibleBuildings.Count > 0)
            {
                List<PlacedObject> accessibleBuildingsByDistance = accessibleBuildings.OrderBy(x => x.Value).Select(x => x.Key).ToList();
                PlacedObject closestAccessibleBuilding = accessibleBuildingsByDistance[0];
                Debug.Log($"Found nearest building for {gameObject.name}: {closestAccessibleBuilding.name}");

                if (!findBetterPath || (currentTargetBuilding != desiredBuilding))
                {
                    SetNavmeshTarget(closestAccessibleBuilding.transform.GetChild(0).position, closestAccessibleBuilding);
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
                            SetNavmeshTarget(closestAccessibleBuilding.transform.GetChild(0).position, closestAccessibleBuilding);
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
