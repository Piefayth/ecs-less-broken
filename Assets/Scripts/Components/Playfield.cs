using Unity.Entities;
using Unity.Rendering;
using Unity.Mathematics;
using Unity.Collections;

public struct Playfield : ISharedComponentData {
    public int width, height;
    public int index;
    public float3 origin;
    public MeshInstanceRenderer look;
}