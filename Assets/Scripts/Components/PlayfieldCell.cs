using Unity.Entities;
using Unity.Mathematics;

public struct PlayfieldCell : IComponentData {
    public int4 playfieldPosition;
}