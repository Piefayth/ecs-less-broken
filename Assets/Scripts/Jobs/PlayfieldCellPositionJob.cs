using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;

[BurstCompile]
public struct PlayfieldCellPositionJob : IJobParallelFor {
    [ReadOnly]
    public int width;
    public int height;

    [ReadOnly]
    public int playfieldIndex;

    public NativeQueue<CellData>.Concurrent cellDatas;

    public void Execute(int index) {
        int z = 0, x = 0;
        if (index > 0) {
            z = index / width;
            x = index % width;
        }

        cellDatas.Enqueue(new CellData {
            position = new int4(x, 0, z, playfieldIndex),
            index = index,
        });
    }
}