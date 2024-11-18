using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Authoring
{
    public class Happiness : MonoBehaviour
    {
        [SerializeField] private float defaultHappiness;

        [Header("The impact on happiness of different factors")]
        [SerializeField] private float homeless;
        [SerializeField] private float hasElectricity;
        [SerializeField] private float noElectricity;
        [SerializeField] private float employed;
        [SerializeField] private float unemployed;
        public class Baker : Baker<Happiness>
        {
            public override void Bake(Happiness authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new ConfigComponents.Happiness
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
}
namespace ConfigComponents
{
    public struct Happiness : IComponentData
    {
        public float defaultHappiness; // The happiness before any other factor is taken into account

        // The impact on happiness of different factors
        public float homeless;
        public float hasElectricity;
        public float noElectricity;
        public float employed;
        public float unemployed;
    }
}