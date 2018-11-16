using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Rendering;
using System;

public class PlayfieldInitializationBarrier : BarrierSystem {}
public class PlayfieldFinalizeBarrier : BarrierSystem {}

[UpdateAfter(typeof(Bootstrap))]
public class PlayfieldInitilizationSystem : JobComponentSystem {
    public EntityArchetype playfieldCellArchetype;

    [Inject] PlayfieldInitializationBarrier initBarrier;
    [Inject] PlayfieldFinalizeBarrier finalizeBarrier;

    EntityManager entityManager;
    Entity playfieldSettingsEntity;

    ComponentGroup uninitializedPlayfieldGroup;
    bool initialized = false;

    NativeQueue<CellData> positions;
    NativeList<CellData> resultPositions;
    NativeHashMap<int, int> playfieldIndex;

    protected override JobHandle OnUpdate(JobHandle inputDeps) {
        if (!initialized) {
            Initialize();
            initialized = true;
        }

        if (uninitializedPlayfieldGroup.CalculateLength() == 0) {
            return inputDeps;
        }

        resultPositions.Clear();
        playfieldIndex.Clear();

        SharedComponentDataArray<Playfield> playfieldArray = uninitializedPlayfieldGroup.GetSharedComponentDataArray<Playfield>();
        EntityArray entityArray = uninitializedPlayfieldGroup.GetEntityArray();
        Playfield playfield = entityManager.GetSharedComponentData<Playfield>(entityArray[0]);
        for (int i = 0; i < playfieldArray.Length; i++) {
            playfieldIndex.TryAdd(playfieldArray[i].index, i);
        }

        JobHandle combinedHandle = new JobHandle();
        int totalNumCells = 0;
        for (int i = 0; i < playfieldArray.Length; i++) {
            int size = playfieldArray[i].width * playfieldArray[i].height;
            totalNumCells += size;
            JobHandle pHandle = new PlayfieldCellPositionJob {
                width = playfieldArray[i].width,
                height = playfieldArray[i].height,
                playfieldIndex = i,
                cellDatas = positions.ToConcurrent(),
            }.Schedule(size, 64, inputDeps);

            if (i == 0) {
                combinedHandle = pHandle;
            } else {
                combinedHandle = JobHandle.CombineDependencies(combinedHandle, pHandle);
            }
        }

        JobHandle qlHandle = new NativeQueueToNativeListJob<CellData> {
            queue = positions,
            out_list = resultPositions,
        }.Schedule(combinedHandle);

        JobHandle pijHandle = new PlayfieldCellCreationJob {
            commandBuffer = initBarrier.CreateCommandBuffer().ToConcurrent(),
            positions = resultPositions,
            playfieldCellArchetype = playfieldCellArchetype,
            playfieldArray = playfieldArray,
            playfieldIndex = playfieldIndex,
        }.Schedule(totalNumCells, 64, qlHandle);

        JobHandle pfjHandle = new PlayfieldFinalizeJob {
            commandBuffer = finalizeBarrier.CreateCommandBuffer().ToConcurrent(),
            playfields = entityArray,
        }.Schedule(entityArray.Length, 64, pijHandle);

        return pfjHandle;
    }

    protected void Initialize() {
        entityManager = World.GetOrCreateManager<EntityManager>();
        positions = new NativeQueue<CellData>(Allocator.Persistent);
        resultPositions = new NativeList<CellData>(Allocator.Persistent);
        playfieldIndex = new NativeHashMap<int, int>(50, Allocator.Persistent);
        uninitializedPlayfieldGroup = GetComponentGroup(typeof(Playfield), typeof(Uninitialized));
        playfieldCellArchetype = entityManager.CreateArchetype(typeof(PlayfieldCell), typeof(Position), typeof(Scale), typeof(MeshInstanceRenderer));
    }

    protected override void OnDestroyManager() {
        positions.Dispose();
        resultPositions.Dispose();
        playfieldIndex.Dispose();
    }
}

public struct CellData {
    public int4 position;
    public int index;
}