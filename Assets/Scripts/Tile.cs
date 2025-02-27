using TMPro;
using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField] private TMP_Text infoText;
    [SerializeField] private GameObject overlayGO;
    [SerializeField] private GameObject selectableGO;
    [SerializeField] private MeshRenderer cubeMesh;
    [SerializeField] private Material exploredMaterial;
    [SerializeField] private Material retracedMaterial;
    private Material originalMaterial;

    private int row;
    private int col;
    private int cost;

    private bool bIsSelectable = true;

    private void Start()
    {
        originalMaterial = cubeMesh.material;

        infoText.text = $"P: ({row},{col})\n" +
            $"Cost: {cost}\n" +
            $"F:\n" +
            $"G:\n" +
            $"H:\n" +
            $"{(GridMap.ETileType)cost}";
        infoText.gameObject.SetActive(false);

        bIsSelectable = cost < (int)GridMap.ETileType.WALL;

        FindFirstObjectByType<PlaySceneGameMode>().OnDebugViewToggled += PlaySceneGameMode_OnDebugViewToggled;
    }

    private void PlaySceneGameMode_OnDebugViewToggled(object sender, PlaySceneGameMode.OnDebugViewToggledEventArgs e)
    {
        infoText.gameObject.SetActive(e.bIsDebugView);
        overlayGO.SetActive(e.bIsDebugView);
        //selectableGO.SetActive(false);
    }

    public void BeingHovered()
    {
        //overlayGO.SetActive(false);
        selectableGO.SetActive(true);
    }

    public void ExitBeingHovered()
    {
        //overlayGO.SetActive(true);
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

    public int GetRow() { return row; }
    public void SetRow(int newRow) { row = newRow; }
    public int GetCol() { return col; }
    public void SetCol(int newCol) { col = newCol; }
    public int GetCost() { return cost; }
    public void SetCost(int newCost) { cost = newCost; }
    public bool IsSelectable() { return bIsSelectable; }
    public string GetInfoText() { return infoText.text; }
}
