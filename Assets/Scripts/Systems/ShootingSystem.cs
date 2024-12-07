using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateBefore(typeof(TransformSystemGroup))]
public partial struct ShootingSystem : ISystem
{
    private float timer;

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        timer -= SystemAPI.Time.DeltaTime;
        if (timer > 0)
        {
            return;
        }
        timer = 0.3f;

        var config = SystemAPI.GetSingleton<Config>();
        var ballTransform = state.EntityManager.GetComponentData<LocalTransform>(config.CannonBallPrefab);

        foreach (var (tower, transform) in SystemAPI.Query<RefRO<Tower>, RefRO<LocalToWorld>>())
        {
            // Find the nearest enemy
            float3 towerPosition = transform.ValueRO.Position;
            Entity? nearestEnemy = null;
            float nearestDistance = float.MaxValue;

            foreach (var (enemyTransform, enemyEntity) in SystemAPI.Query<RefRO<LocalTransform>>()
                        .WithAll<Tank>()
                        .WithNone<Player>()
                        .WithEntityAccess())
            {
                float3 enemyPosition = enemyTransform.ValueRO.Position;
                float distance = math.distance(towerPosition, enemyPosition);

                if (distance < 40f && distance < nearestDistance) // Within range and closer
                {
                    nearestEnemy = enemyEntity;
                    nearestDistance = distance;
                }
            }

            // If a target is found, spawn and aim the cannonball
            if (nearestEnemy.HasValue)
            {
                Entity cannonBallEntity = state.EntityManager.Instantiate(config.CannonBallPrefab);

                var cannonTransform = state.EntityManager.GetComponentData<LocalToWorld>(tower.ValueRO.Spawner);
                ballTransform.Position = cannonTransform.Position;

                // Set position of the cannonball
                state.EntityManager.SetComponentData(cannonBallEntity, ballTransform);

                // Aim at the nearest enemy
                float3 enemyPosition = state.EntityManager.GetComponentData<LocalTransform>(nearestEnemy.Value).Position;
                float3 direction = math.normalize(enemyPosition - cannonTransform.Position);

                state.EntityManager.SetComponentData(cannonBallEntity, new CannonBall
                {
                    Velocity = direction * 12.0f // Adjust speed as needed
                });
            }
        }
    }
}
