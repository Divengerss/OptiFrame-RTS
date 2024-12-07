using Unity.Entities;
using UnityEngine;

public class TowerAuthoring : MonoBehaviour
{
    public GameObject Spawner;
    
    class Baker : Baker<TowerAuthoring>
    {
        public override void Bake(TowerAuthoring authoring)
        {
            // GetEntity returns the Entity baked from the GameObject
            var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            AddComponent(entity, new Tower
            {
                Spawner = GetEntity(authoring.Spawner, TransformUsageFlags.Dynamic),
            });
        }
    }
}

// A component that will be added to the root entity of every Tower.
public struct Tower : IComponentData
{
    public Entity Spawner;
}