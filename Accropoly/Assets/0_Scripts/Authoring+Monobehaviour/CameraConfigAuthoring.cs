using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class CameraConfigAuthoring : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed;

    [Header("Rotation")]
    [SerializeField] private float rotationSpeed;

    [Header("Zooming")]
    [SerializeField] private float zoomSpeed;
    [SerializeField] private float minDistance;
    [SerializeField] private float maxDistance;

    [Header("Looking")]
    [SerializeField] private float lookSpeed;
    [SerializeField] private float minAngle;
    [SerializeField] private float maxAngle;

    [Header("Sprinting")]
    [SerializeField] private float sprintSpeedMultiplier;
    public class Baker : Baker<CameraConfigAuthoring>
    {
        public override void Bake(CameraConfigAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new CameraConfig
            {
                moveSpeed = authoring.moveSpeed,
                rotationSpeed = authoring.rotationSpeed,
                zoomSpeed = authoring.zoomSpeed,
                minDistance = authoring.minDistance,
                maxDistance = authoring.maxDistance,
                lookSpeed = authoring.lookSpeed,
                minAngle = authoring.minAngle,
                maxAngle = authoring.maxAngle,
                sprintSpeedMultiplier = authoring.sprintSpeedMultiplier
            });
        }
    }
}
public struct CameraConfig : IComponentData
{
    public float moveSpeed;

    public float rotationSpeed;

    public float zoomSpeed;
    public float minDistance;
    public float maxDistance;

    public float lookSpeed;
    public float minAngle;
    public float maxAngle;

    public float sprintSpeedMultiplier;
}