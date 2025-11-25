using UnityEngine;

public class NPC : MonoBehaviour
{
    PlacedObject associatedBuilding;
    Vector3 originPosition;

    public void SetOrigin(PlacedObject _associatedBuilding)
    {
        associatedBuilding = _associatedBuilding;
        originPosition = associatedBuilding.transform.forward * 2;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
