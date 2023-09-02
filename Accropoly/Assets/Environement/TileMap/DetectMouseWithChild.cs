using UnityEngine;

public class DetectMouseWithChild : MonoBehaviour
{
    private IMapTile parentMapTileScript;

    private new Renderer renderer;
    private Color defaultColor;
    [SerializeField] private bool changeColor = true;
    [SerializeField] private Color placeableColor = Color.green;
    [SerializeField] private Color notPlaceableColor = Color.red;
    private void Awake()
    {
        parentMapTileScript = transform.parent.GetComponent<IMapTile>();

        renderer = GetComponent<Renderer>();
        defaultColor = renderer.material.color;

        parentMapTileScript.ChildsDefaultColor += () => renderer.material.color = defaultColor;
        parentMapTileScript.ChildsPlaceableColor += () => renderer.material.color = changeColor ? placeableColor : defaultColor;
        parentMapTileScript.ChildsNotPlaceableColor += () => renderer.material.color = changeColor ? notPlaceableColor : defaultColor;
    }
    private void OnMouseEnter()
    {
        parentMapTileScript.OnMouseEnterChild();
    }
    private void OnMouseExit()
    {
        parentMapTileScript.OnMouseExitChild();
    }
}
