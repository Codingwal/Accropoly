using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class HappinessConfigAuthoring : MonoBehaviour
{
    [SerializeField] private float defaultHappiness;

    [Header("The impact on happiness of different factors")]
    [SerializeField] private float homeless;
    [SerializeField] private float hasElectricity;
    [SerializeField] private float noElectricity;
    [SerializeField] private float employed;
    [SerializeField] private float unemployed;
    public class Baker : Baker<HappinessConfigAuthoring>
    {
        public override void Bake(HappinessConfigAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new HappinessConfig
            {
                defaultHappiness = authoring.defaultHappiness,
                homeless = authoring.homeless,
                hasElectricity = authoring.hasElectricity,
                noElectricity = authoring.noElectricity,
                employed = authoring.employed,
                unemployed = authoring.unemployed,
            });
        }
    }
}
public struct HappinessConfig : IComponentData
{
    public float defaultHappiness; // The happiness before any other factor is taken into account

    // The impact on happiness of different factors
    public float homeless;
    public float hasElectricity;
    public float noElectricity;
    public float employed;
    public float unemployed;
}
