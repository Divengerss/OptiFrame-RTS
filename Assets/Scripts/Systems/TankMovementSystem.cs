using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public partial struct TankMovementSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var dt = SystemAPI.Time.DeltaTime;

        // Create a NativeList to hold all DefensiveBuilding positions.
        using var defensiveBuildingPositions = new NativeList<float3>(Allocator.TempJob);

        // Query all entities with the DefensiveBuilding tag and store their positions.
        foreach (var transform in SystemAPI.Query<RefRO<LocalTransform>>().WithAll<DefensiveBuilding>())
        {
            var position = transform.ValueRO.Position;
            // Ignore Y position for DefensiveBuildings.
            position.y = 0;
            defensiveBuildingPositions.Add(position);
        }

        // For each entity having a LocalTransform and Tank component,
        // exclude the player tank from the query.
        foreach (var (transform, entity) in SystemAPI.Query<RefRW<LocalTransform>>()
                .WithAll<Tank>()
                .WithNone<Player>()
                .WithEntityAccess())
        {
            float3 currentPosition = transform.ValueRO.Position;
            currentPosition.y = 0; // Ignore Y position of the tank.

            if (defensiveBuildingPositions.Length > 0)
            {
                // Find the nearest DefensiveBuilding.
                float3 nearestPosition = defensiveBuildingPositions[0];
                float nearestDistanceSq = math.distancesq(currentPosition, nearestPosition);

                for (int i = 1; i < defensiveBuildingPositions.Length; i++)
                {
                    float3 candidatePosition = defensiveBuildingPositions[i];
                    float candidateDistanceSq = math.distancesq(currentPosition, candidatePosition);

                    if (candidateDistanceSq < nearestDistanceSq)
                    {
                        nearestPosition = candidatePosition;
                        nearestDistanceSq = candidateDistanceSq;
                    }
                }

                // Calculate direction to the nearest DefensiveBuilding.
                float3 direction = math.normalize(nearestPosition - currentPosition);

                // Update the LocalTransform position and rotation towards the target.
                transform.ValueRW.Position += direction * dt * 5.0f;
                transform.ValueRW.Rotation = quaternion.LookRotationSafe(direction, math.up());
            }
            else
            {
                // No DefensiveBuilding found, do not move.
                continue;
            }
        }

        // Handle turret rotation for each tank.
        var spin = quaternion.RotateY(SystemAPI.Time.DeltaTime * math.PI);

        foreach (var tank in SystemAPI.Query<RefRW<Tank>>())
        {
            var trans = SystemAPI.GetComponentRW<LocalTransform>(tank.ValueRO.Turret);

            // Add a rotation around the Y axis (relative to the parent).
            trans.ValueRW.Rotation = math.mul(spin, trans.ValueRO.Rotation);
        }
    }
}
