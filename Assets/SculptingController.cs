using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class SculptingController : MonoBehaviour
{
    [Header("Cylinder Parameters")]
    [SerializeField] private float radius = 1f;
    [SerializeField] private float height = 2f;
    [SerializeField] private int radialSegments = 8;
    [SerializeField] private int heightSegments = 4;
    
    [Header("Material")]
    [SerializeField] private Material material;
    
    
    [Header("Caps")]
    [SerializeField] private int capRadialRings = 3;
    
    [Header("Tool Deformation")]
    [SerializeField] private ToolController toolController;
    [SerializeField] private bool restoreShape = false;
    [SerializeField] private float density = 1.0f;
    
    
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private Mesh mesh;
    
    // Store original vertex positions for deformation
    private Vector3[] originalVertices;
    private Vector3[] currentVertices;
    
    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        mesh = new Mesh();
        mesh.name = "Generated Cylinder";
        meshFilter.mesh = mesh;
        
        // Apply material if assigned
        if (material != null)
        {
            meshRenderer.material = material;
        }
    }
    
    private void Start()
    {
        GenerateCylinder();
    }
    
    private void Update()
    {
        if (toolController != null && originalVertices != null)
        {
            ApplyToolDeformation();
        }
    }
    
    
    public void GenerateCylinder()
    {
        if (radialSegments < 3) radialSegments = 3;
        if (heightSegments < 1) heightSegments = 1;
        if (capRadialRings < 1) capRadialRings = 1;
        
        // Calculate vertex count
        int cylinderVertexCount = (radialSegments + 1) * (heightSegments + 1);
        int capVertexCount = 2 + capRadialRings * 2 * (radialSegments + 1); // 2 centers + 2 rings
        
        int totalVertexCount = cylinderVertexCount + capVertexCount;
        
        Vector3[] vertices = new Vector3[totalVertexCount];
        Vector3[] normals = new Vector3[totalVertexCount];
        Vector2[] uvs = new Vector2[totalVertexCount];
        
        // Generate cylinder vertices
        int vertexIndex = 0;
        for (int y = 0; y <= heightSegments; y++)
        {
            float yPos = (float)y / heightSegments * height - height * 0.5f;
            
            for (int x = 0; x <= radialSegments; x++)
            {
                float angle = (float)x / radialSegments * Mathf.PI * 2f;
                float xPos = Mathf.Cos(angle) * radius;
                float zPos = Mathf.Sin(angle) * radius;
                
                vertices[vertexIndex] = new Vector3(xPos, yPos, zPos);
                normals[vertexIndex] = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
                uvs[vertexIndex] = new Vector2((float)x / radialSegments, (float)y / heightSegments);
                
                vertexIndex++;
            }
        }
        
        // Generate cap vertices
        // Generate top cap vertices
        vertices[vertexIndex] = new Vector3(0, height * 0.5f, 0);
        normals[vertexIndex] = Vector3.up;
        uvs[vertexIndex] = new Vector2(0.5f, 0.5f);
        vertexIndex++;
        
        // Top cap rings
        for (int ring = 1; ring <= capRadialRings; ring++)
        {
            float ringRadius = (float)ring / capRadialRings * radius;
            
            for (int x = 0; x <= radialSegments; x++)
            {
                float angle = (float)x / radialSegments * Mathf.PI * 2f;
                float xPos = Mathf.Cos(angle) * ringRadius;
                float zPos = Mathf.Sin(angle) * ringRadius;
                
                vertices[vertexIndex] = new Vector3(xPos, height * 0.5f, zPos);
                
                // Calculate proper normal - should be up for top cap
                normals[vertexIndex] = Vector3.up;
                
                uvs[vertexIndex] = new Vector2((Mathf.Cos(angle) + 1) * 0.5f, (Mathf.Sin(angle) + 1) * 0.5f);
                
                vertexIndex++;
            }
        }
        
        // Generate bottom cap vertices
        vertices[vertexIndex] = new Vector3(0, -height * 0.5f, 0);
        normals[vertexIndex] = Vector3.down;
        uvs[vertexIndex] = new Vector2(0.5f, 0.5f);
        vertexIndex++;
        
        // Bottom cap rings
        for (int ring = 1; ring <= capRadialRings; ring++)
        {
            float ringRadius = (float)ring / capRadialRings * radius;
            
            for (int x = 0; x <= radialSegments; x++)
            {
                float angle = (float)x / radialSegments * Mathf.PI * 2f;
                float xPos = Mathf.Cos(angle) * ringRadius;
                float zPos = Mathf.Sin(angle) * ringRadius;
                
                vertices[vertexIndex] = new Vector3(xPos, -height * 0.5f, zPos);
                
                // Calculate proper normal - should be down for bottom cap
                normals[vertexIndex] = Vector3.down;
                
                uvs[vertexIndex] = new Vector2((Mathf.Cos(angle) + 1) * 0.5f, (Mathf.Sin(angle) + 1) * 0.5f);
                
                vertexIndex++;
            }
        }
        
        // Calculate triangle count
        int cylinderTriangleCount = radialSegments * heightSegments * 6;
        int capTriangleCount = capRadialRings * radialSegments * 6 * 2; // Both caps
        
        int totalTriangleCount = cylinderTriangleCount + capTriangleCount;
        int[] triangles = new int[totalTriangleCount];
        
        // Generate cylinder triangles
        int triangleIndex = 0;
        for (int y = 0; y < heightSegments; y++)
        {
            for (int x = 0; x < radialSegments; x++)
            {
                int current = y * (radialSegments + 1) + x;
                int next = current + radialSegments + 1;
                
                // First triangle
                triangles[triangleIndex] = current;
                triangles[triangleIndex + 1] = next;
                triangles[triangleIndex + 2] = current + 1;
                
                // Second triangle
                triangles[triangleIndex + 3] = current + 1;
                triangles[triangleIndex + 4] = next;
                triangles[triangleIndex + 5] = next + 1;
                
                triangleIndex += 6;
            }
        }
        
        // Generate cap triangles
        // Generate top cap triangles
        int topCenterIndex = cylinderVertexCount;
        int topRingStartIndex = topCenterIndex + 1;
        
        // Generate triangles for each ring
        for (int ring = 0; ring < capRadialRings; ring++)
        {
            int currentRingStart = topRingStartIndex + ring * (radialSegments + 1);
            int nextRingStart = topRingStartIndex + (ring + 1) * (radialSegments + 1);
            
            // If this is the innermost ring, connect to center
            if (ring == 0)
            {
                for (int x = 0; x < radialSegments; x++)
                {
                    triangles[triangleIndex] = topCenterIndex;
                    triangles[triangleIndex + 1] = currentRingStart + x + 1;
                    triangles[triangleIndex + 2] = currentRingStart + x;
                    
                    triangleIndex += 3;
                }
            }
            
            // If this is not the outermost ring, connect to next ring
            if (ring < capRadialRings - 1)
            {
                for (int x = 0; x < radialSegments; x++)
                {
                    int current = currentRingStart + x;
                    int currentNext = currentRingStart + x + 1;
                    int next = nextRingStart + x;
                    int nextNext = nextRingStart + x + 1;
                    
                    // First triangle - correct winding order
                    triangles[triangleIndex] = current;
                    triangles[triangleIndex + 1] = currentNext;
                    triangles[triangleIndex + 2] = next;
                    
                    // Second triangle - correct winding order
                    triangles[triangleIndex + 3] = currentNext;
                    triangles[triangleIndex + 4] = nextNext;
                    triangles[triangleIndex + 5] = next;
                    
                    triangleIndex += 6;
                }
            }
        }
        
        // Generate bottom cap triangles
        int bottomCenterIndex = cylinderVertexCount + 1 + capRadialRings * (radialSegments + 1);
        int bottomRingStartIndex = bottomCenterIndex + 1;
        
        // Generate triangles for each ring
        for (int ring = 0; ring < capRadialRings; ring++)
        {
            int currentRingStart = bottomRingStartIndex + ring * (radialSegments + 1);
            int nextRingStart = bottomRingStartIndex + (ring + 1) * (radialSegments + 1);
            
            // If this is the innermost ring, connect to center
            if (ring == 0)
            {
                for (int x = 0; x < radialSegments; x++)
                {
                    triangles[triangleIndex] = bottomCenterIndex;
                    triangles[triangleIndex + 1] = currentRingStart + x;
                    triangles[triangleIndex + 2] = currentRingStart + x + 1;
                    
                    triangleIndex += 3;
                }
            }
            
            // If this is not the outermost ring, connect to next ring
            if (ring < capRadialRings - 1)
            {
                for (int x = 0; x < radialSegments; x++)
                {
                    int current = currentRingStart + x;
                    int currentNext = currentRingStart + x + 1;
                    int next = nextRingStart + x;
                    int nextNext = nextRingStart + x + 1;
                    
                    // First triangle - correct winding order for bottom cap
                    triangles[triangleIndex] = current;
                    triangles[triangleIndex + 1] = next;
                    triangles[triangleIndex + 2] = currentNext;
                    
                    // Second triangle - correct winding order for bottom cap
                    triangles[triangleIndex + 3] = currentNext;
                    triangles[triangleIndex + 4] = next;
                    triangles[triangleIndex + 5] = nextNext;
                    
                    triangleIndex += 6;
                }
            }
        }
        
        // Assign data to mesh
        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        
        mesh.RecalculateBounds();
        
        // Store original vertices for deformation
        originalVertices = new Vector3[vertices.Length];
        currentVertices = new Vector3[vertices.Length];
        System.Array.Copy(vertices, originalVertices, vertices.Length);
        System.Array.Copy(vertices, currentVertices, vertices.Length);
    }
    
    private void ApplyToolDeformation()
    {
        Vector3 toolPosition = transform.InverseTransformPoint(toolController.GetToolTransform().position);
        Vector3[] deformedVertices = new Vector3[originalVertices.Length];
        
        // First pass: calculate individual vertex deformations
        Vector3[] individualDeformations = new Vector3[originalVertices.Length];
        
        for (int i = 0; i < originalVertices.Length; i++)
        {
            Vector3 vertexPos = restoreShape ? originalVertices[i] : currentVertices[i];
            Vector3 direction = vertexPos - toolPosition;
            float distance = direction.magnitude;
            
            if (distance < toolController.GetToolRadius())
            {
                // Calculate deformation strength based on distance and softness
                float normalizedDistance = distance / toolController.GetToolRadius();
                float deformationStrength = Mathf.Pow(1.0f - normalizedDistance, toolController.GetToolSoftness());
                
                // Push vertex away from tool
                Vector3 pushDirection = direction.normalized;
                float pushDistance = deformationStrength * (toolController.GetToolRadius() - distance);
                
                individualDeformations[i] = pushDirection * pushDistance;
            }
            else
            {
                individualDeformations[i] = Vector3.zero;
            }
        }
        
        // Second pass: apply density-based neighbor influence
        for (int i = 0; i < originalVertices.Length; i++)
        {
            Vector3 vertexPos = restoreShape ? originalVertices[i] : currentVertices[i];
            Vector3 totalDeformation = individualDeformations[i];
            
            if (density > 0)
            {
                // Find nearby vertices and apply density influence
                for (int j = 0; j < originalVertices.Length; j++)
                {
                    if (i != j && individualDeformations[j] != Vector3.zero)
                    {
                        Vector3 otherVertexPos = restoreShape ? originalVertices[j] : currentVertices[j];
                        float neighborDistance = Vector3.Distance(vertexPos, otherVertexPos);
                        
                        // Calculate influence based on distance and density
                        float influenceStrength = Mathf.Exp(-neighborDistance * density);
                        totalDeformation += individualDeformations[j] * influenceStrength;
                    }
                }
            }
            
            deformedVertices[i] = vertexPos + totalDeformation;
        }
        
        // Update current vertices and apply to mesh
        currentVertices = deformedVertices;
        mesh.vertices = deformedVertices;
        mesh.RecalculateNormals();
    }
    

    private void OnValidate()
    {
        if (Application.isPlaying && mesh != null)
        {
            GenerateCylinder();
        }
    }
    
    // Public methods for runtime modification
    public void SetRadius(float newRadius)
    {
        radius = newRadius;
        GenerateCylinder();
    }
    
    public void SetHeight(float newHeight)
    {
        height = newHeight;
        GenerateCylinder();
    }
    
    public void SetRadialSegments(int newRadialSegments)
    {
        radialSegments = newRadialSegments;
        GenerateCylinder();
    }
    
    public void SetHeightSegments(int newHeightSegments)
    {
        heightSegments = newHeightSegments;
        GenerateCylinder();
    }
    
    public void SetMaterial(Material newMaterial)
    {
        material = newMaterial;
        if (meshRenderer != null)
        {
            meshRenderer.material = material;
        }
    }
    
    
    public void SetCapRadialRings(int newCapRadialRings)
    {
        capRadialRings = newCapRadialRings;
        GenerateCylinder();
    }
    
    public void SetToolController(ToolController newToolController)
    {
        toolController = newToolController;
    }
    
    public void SetRestoreShape(bool newRestoreShape)
    {
        restoreShape = newRestoreShape;
    }
    
    public void SetDensity(float newDensity)
    {
        density = newDensity;
    }
    
    
}
