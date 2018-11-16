using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;

[BurstCompile]
public struct NativeQueueToNativeListJob<T> : IJob where T : struct {
    public NativeQueue<T> queue;
    public NativeList<T> out_list;

    public void Execute() {
        int count = queue.Count;

        for (int i = 0; i < count; ++i)
            out_list.Add(queue.Dequeue());
    }
}