using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;

public class CannonBallAuthoring : MonoBehaviour
{
    class Baker : Baker<CannonBallAuthoring>
    {
        public override void Bake(CannonBallAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);

            // By default, components are zero-initialized,
            // so the Velocity field of CannonBall will be float3.zero.
            AddComponent<CannonBall>(entity);
            AddComponent<HDRPMaterialPropertyBaseColor>(entity);
        }
    }
}

public struct CannonBall : IComponentData
{
    public float3 Velocity;
}