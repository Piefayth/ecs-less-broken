using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Transforms;

public struct PlayfieldCellCreationJob : IJobParallelFor {
    public EntityCommandBuffer.Concurrent commandBuffer;

    [ReadOnly]
    public NativeList<CellData> positions;

    [ReadOnly]
    public EntityArchetype playfieldCellArchetype;

    [ReadOnly]
    public SharedComponentDataArray<Playfield> playfieldArray;

    [ReadOnly]
    public NativeHashMap<int, int> playfieldIndex;

    public void Execute(int index) {
        commandBuffer.CreateEntity(index, playfieldCellArchetype);
        commandBuffer.SetComponent(index, new Scale {
            Value = new float3(1.0f, 1.0f, 1.0f)
        });

        playfieldIndex.TryGetValue(positions[index].position.x, out int playfield);

        commandBuffer.SetComponent(index, new Position { // should consider the playfield origin, spacing between grid tiles, etc
            Value = new float3(positions[index].position.x, positions[index].position.y, positions[index].position.z),
        });

        commandBuffer.SetComponent(index, new PlayfieldCell {
            playfieldPosition = new int4(positions[index].position.x, positions[index].position.y, positions[index].position.z, playfield)
        });

        commandBuffer.SetSharedComponent(index, playfieldArray[playfield].look);
    }
}