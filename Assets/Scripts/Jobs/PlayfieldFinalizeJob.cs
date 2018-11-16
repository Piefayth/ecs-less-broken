using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections.LowLevel.Unsafe;

public struct PlayfieldFinalizeJob : IJobParallelFor {
    public EntityCommandBuffer.Concurrent commandBuffer;

    [ReadOnly]
    public EntityArray playfields;
    
    public void Execute(int index) {
        commandBuffer.RemoveComponent<Uninitialized>(index, playfields[index]);
    }
}