using UnityEngine;

public class ToolController : MonoBehaviour
{
    [Header("Tool Settings")]
    [SerializeField] private float toolRadius = 1.0f;
    [SerializeField] private float toolSoftness = 0.5f;
    
    private void Start()
    {
        UpdateToolScale();
    }
    
    private void OnValidate()
    {
        UpdateToolScale();
    }
    
    private void UpdateToolScale()
    {
        // Scale tool visual representation based on radius
        transform.localScale = Vector3.one * toolRadius; // Direct radius scaling
    }
    
    public Transform GetToolTransform()
    {
        return transform;
    }
    
    public float GetToolRadius()
    {
        return toolRadius;
    }
    
    public float GetToolSoftness()
    {
        return toolSoftness;
    }
    
    public void SetToolRadius(float newRadius)
    {
        toolRadius = newRadius;
        UpdateToolScale();
    }
    
    public void SetToolSoftness(float newSoftness)
    {
        toolSoftness = newSoftness;
    }
}
