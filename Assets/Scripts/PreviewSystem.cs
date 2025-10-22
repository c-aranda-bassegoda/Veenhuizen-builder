using System;
using Unity.VisualScripting;
using UnityEngine;
using static BuildingScriptableObject;

public class PreviewSystem : MonoBehaviour
{
    private float previewYOffset = 0.06f;

    private GameObject buildingPreview;

    [SerializeField] private Material previewMaterialPrefab;
    private Material previewMaterial;

    private void Awake()
    {
        previewMaterial = new Material(previewMaterialPrefab);
    }

    public void StartPlacementPreview(BuildingScriptableObject buildingSO)
    {
        buildingPreview = Instantiate(buildingSO.prefab, new Vector3(0,0,0), Quaternion.Euler(0, buildingSO.GetRotationAngle(buildingSO.Direction), 0));
        PreparePreview(buildingPreview);
    }

    private void PreparePreview(GameObject buildingPreview)
    {
        Renderer[] renderers = buildingPreview.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            Material[] materials = renderer.materials;
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = previewMaterial;
            }
            renderer.materials = materials;
        }
    }

    public void StopPlacementPreview()
    {
        Destroy(buildingPreview);
    }

    public void UpdatePreview(Vector3 worldPosition, bool validity)
    {
        MovePreview(worldPosition);
        ApplyFeedback(validity);
    }

    private void ApplyFeedback(bool validity)
    {
        Color c = validity ? Color.blue : Color.red;
        c.a = 0.5f;
        previewMaterial.color = c;
    }

    private void MovePreview(Vector3 worldPosition)
    {
        buildingPreview.transform.position = new Vector3(worldPosition.x, worldPosition.y + previewYOffset, worldPosition.z);
    }
}
