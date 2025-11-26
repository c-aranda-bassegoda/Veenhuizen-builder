using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class NPC : MonoBehaviour
{
    PlacedObject associatedBuilding;
    private Vector3 velocity;
    [SerializeField] private float slowdownMultiplier = .125f;

    public void SetOrigin(PlacedObject _associatedBuilding)
    {
        associatedBuilding = _associatedBuilding;
    }

    public void SetVelocity(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
    {
        velocity += v1 + v2 + v3 + v4;
        velocity *= slowdownMultiplier;
    }

    public Vector3 GetVelocity()
    {
        return velocity;
    }

    public void MoveNpc()
    {
        transform.position += velocity;
    }
}
