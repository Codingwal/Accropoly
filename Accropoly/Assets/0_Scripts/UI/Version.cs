using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Version : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI versionNumber;

    private void Start()
    {
        string version = "v0.1.1-dev1";
        versionNumber.text = version;
    }
}