using UnityEngine;

public class SaveSystemConfig : MonoBehaviour
{
    private static SaveSystemConfig instance;

    [SerializeField] private bool overwriteFiles;
    public static bool OverwriteFiles { get { return instance.overwriteFiles; } }

    [SerializeField] private bool deleteSaves;
    public static bool DeleteSaves { get { return instance.deleteSaves; } }

    private void Awake()
    {
        instance = this;
    }
}
