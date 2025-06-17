using System.Collections;
using System.Collections.Generic;
using Components;
using UnityEngine;

public class BillboardMaterials : MonoBehaviour
{
    [SerializeField] private SerializableDictionary<BillboardInfo.Problems, Material> materials;
    private static BillboardMaterials instance;
    public static SerializableDictionary<BillboardInfo.Problems, Material> Materials { get { Debug.Assert(instance != null); return instance.materials; } }
    private void Awake()
    {
        instance = this;
    }
}
