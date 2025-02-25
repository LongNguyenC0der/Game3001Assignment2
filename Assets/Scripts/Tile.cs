using TMPro;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public int row;
    public int col;
    [SerializeField] private TMP_Text costText;
    [SerializeField] private int cost = 1;
    [SerializeField] private GameObject overlayGO;
    [SerializeField] private GameObject selectableGO;

    [SerializeField] private MeshRenderer cubeMesh;
    [SerializeField] private Material exploredMaterial;
    [SerializeField] private Material retracedMaterial;
    private Material originalMaterial;

    private bool bIsSelectable = true;

    private void Start()
    {
        originalMaterial = cubeMesh.material;

        costText.text = cost.ToString();
        costText.gameObject.SetActive(false);

        bIsSelectable = cost < (int)GridMap.ETileType.WALL;

        FindFirstObjectByType<PlaySceneGameMode>().OnDebugViewToggled += PlaySceneGameMode_OnDebugViewToggled;
    }

    private void PlaySceneGameMode_OnDebugViewToggled(object sender, PlaySceneGameMode.OnDebugViewToggledEventArgs e)
    {
        costText.gameObject.SetActive(e.bIsDebugView);
        overlayGO.SetActive(e.bIsDebugView);
        selectableGO.SetActive(false);
    }

    public void BeingHovered()
    {
        overlayGO.SetActive(false);
        selectableGO.SetActive(true);
    }

    public void ExitBeingHovered()
    {
        overlayGO.SetActive(true);
        selectableGO.SetActive(false);
    }

    public void BeingExplored()
    {
        cubeMesh.material = exploredMaterial;
    }

    public void BeingRetraced()
    {
        cubeMesh.material = retracedMaterial;
    }

    public void ResetTile()
    {
        cubeMesh.material = originalMaterial;
    }

    public int GetCost() { return cost; }
    public void SetCost(int newCost) { cost = newCost; }
    public bool IsSelectable() { return bIsSelectable; }
}
