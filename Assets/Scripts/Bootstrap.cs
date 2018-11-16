using Unity.Entities;
using UnityEngine;
using Unity.Rendering;

public class Bootstrap : ComponentSystem {
    public EntityArchetype uninitializedPlayfieldArchetype;

    EntityManager entityManager;
    bool initialized = false;

    protected override void OnUpdate() {
        if (!initialized) {
            initialized = true;
            Initialize();
        }
    }

    protected void Initialize() {
        Settings settings = (Settings)GameObject.FindObjectOfType(typeof(Settings));
        entityManager = World.GetOrCreateManager<EntityManager>();
        uninitializedPlayfieldArchetype = entityManager.CreateArchetype(typeof(Playfield), typeof(Uninitialized));
        Entity playfield = entityManager.CreateEntity(uninitializedPlayfieldArchetype);
        entityManager.SetSharedComponentData(playfield, new Playfield {
            height = settings.playfieldHeight,
            width = settings.playfieldWidth,
            look = GetLookFromPrefab(settings.playfieldCellPrefab),
            index = 0,
        });
    }

    public static MeshInstanceRenderer GetLookFromPrefab(GameObject prefab) {
        MeshInstanceRenderer result = prefab.GetComponent<MeshInstanceRendererComponent>().Value;
        prefab.SetActive(false);
        return result;
    }
}
