# UnitySculptingPrototype - Interactive Sculpting System

## Technology Description

UnitySculptingPrototype is a system for interactive 3D sculpting in Unity that simulates working with clay or plasticine. The system allows real-time deformation of cylindrical objects using a virtual tool.

### Main Components

#### 1. **SculptingController** - Sculpting Controller

Main component responsible for generating and deforming 3D mesh.

**Cylinder Parameters:**

- `Radius` - cylinder radius
- `Height` - cylinder height
- `Radial Segments` - number of segments around circumference
- `Height Segments` - number of segments along height
- `Cap Radial Rings` - number of radial rings in caps

**Material Parameters:**

- `Restore Shape` - whether to restore shape when tool moves away (false = plasticine, true = rubber)
- `Density` - material density (affects mutual attraction of neighboring points)

#### 2. **ToolController** - Tool Controller

Manages sculpting tool parameters.

**Tool Parameters:**

- `Tool Radius` - tool influence radius
- `Tool Softness` - tool softness (0-1, where 1 = very soft)

#### 3. **Rotation** - Rotation Script

Simple component for rotating object around Y axis.

**Parameters:**

- `Rotation Speed` - rotation speed in degrees per second

## How to Use

### Scene Setup

1. **Create sculpting cylinder:**

   - Create empty GameObject
   - Add `MeshFilter` and `MeshRenderer` components
   - Add `SculptingController` component
   - Assign material in `Material` field

2. **Create tool:**

   - Create empty GameObject (e.g., sphere)
   - Add `ToolController` component
   - Configure tool radius and softness
   - Add visual representation (e.g., child sphere)

3. **Setup interaction:**
   - In `SculptingController` assign `ToolController` in `Tool Controller` field
   - Configure material parameters (`Restore Shape`, `Density`)

### Configuration Parameters

#### SculptingController

**Cylinder Parameters:**

- `Radius` (1.0) - cylinder size
- `Height` (2.0) - cylinder height
- `Radial Segments` (8) - circumference detail
- `Height Segments` (4) - height detail

**Caps:**

- `Cap Radial Rings` (3) - number of rings in caps

**Tool Deformation:**

- `Tool Controller` - reference to ToolController
- `Restore Shape` (false) - restore shape
- `Density` (1.0) - material density

#### ToolController

**Tool Settings:**

- `Tool Radius` (1.0) - influence radius
- `Tool Softness` (0.5) - tool softness

#### Rotation

- `Rotation Speed` (90) - Y axis rotation speed

### Controls

1. **Tool movement:**

   - Move GameObject with `ToolController` in scene
   - Cylinder will deform in real-time

2. **Parameter adjustment:**

   - Change parameters in Inspector during play
   - Results apply instantly

3. **Rotation:**
   - Add `Rotation` component for automatic rotation
   - Configure rotation speed

### Configuration Examples

#### Soft Clay

- `Restore Shape`: false
- `Density`: 2.0
- `Tool Softness`: 0.8

#### Hard Plasticine

- `Restore Shape`: false
- `Density`: 0.5
- `Tool Softness`: 0.3

#### Rubber

- `Restore Shape`: true
- `Density`: 1.0
- `Tool Softness`: 0.6

### Technical Details

#### Deformation Algorithm

1. **Individual deformation calculation:**

   - For each vertex calculate distance to tool
   - Apply formula considering tool radius and softness

2. **Density application:**

   - Each vertex influences neighboring vertices
   - Influence strength depends on distance and `Density` parameter
   - Uses exponential decay function

3. **Mesh update:**
   - Deformed vertices applied to mesh
   - Normals recalculated for correct lighting

#### Performance

- System optimized for real-time operation
- Vertex count affects performance
- Recommended to use reasonable segment count

### Possible Extensions

- Adding various tool types
- Material layer system
- Sculpture save/load
- Export to various formats
- VR support

## License

Project created for educational purposes.
