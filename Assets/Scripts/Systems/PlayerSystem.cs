using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public partial struct PlayerSystem : ISystem
{
    // Because OnUpdate accesses a managed object (the camera), we cannot Burst compile 
    // this method, so we don't use the [BurstCompile] attribute here.
    public void OnUpdate(ref SystemState state)
    {
        // Get player input
        var movement = new float3(
            Input.GetAxis("Horizontal"),
            0,
            Input.GetAxis("Vertical")
        );
        movement *= SystemAPI.Time.DeltaTime;

        foreach (var playerTransform in SystemAPI.Query<RefRW<LocalTransform>>().WithAll<Player>())
        {
            var playerRotation = playerTransform.ValueRO.Rotation;
            var relativeMovement = math.mul(playerRotation, movement);
            playerTransform.ValueRW.Position += relativeMovement * -2f;

            // Move the camera to follow the player
            var cameraTransform = Camera.main.transform;
            cameraTransform.position = playerTransform.ValueRO.Position;
            cameraTransform.position -= 8.0f * cameraTransform.forward;  // raise the camera by an offset
        }
    }
}
