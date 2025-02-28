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

    private PlaySceneGameMode playSceneGameMode;

    private float f = -1.0f;
    private float g = -1.0f;
    private float h = -1.0f;

    public int Row { get; set; }
    public int Col { get; set; }
    public int Cost { get; set; }
    public float F
    {
        get => f;
        set
        {
            f = value;
            UpdateInfoText();
        }
    }
    public float G
    {
        get => g;
        set
        {
            g = value;
            UpdateInfoText();
        }
    }
    public float H
    {
        get => h;
        set
        {
            h = value;
            UpdateInfoText();
        }
    }

    private bool bIsSelectable = true;
    private bool bIsDebugView = false;

    private void Start()
    {
        playSceneGameMode = FindFirstObjectByType<PlaySceneGameMode>();
        originalMaterial = cubeMesh.material;

        UpdateInfoText();
        infoText.gameObject.SetActive(false);

        bIsSelectable = Cost < (int)GridMap.ETileType.WALL;

        playSceneGameMode.OnDebugViewToggled += PlaySceneGameMode_OnDebugViewToggled;
    }

    private void OnDestroy()
    {
        if(playSceneGameMode)
        {
            playSceneGameMode.OnDebugViewToggled -= PlaySceneGameMode_OnDebugViewToggled;
        }
    }

    private void PlaySceneGameMode_OnDebugViewToggled(object sender, PlaySceneGameMode.OnDebugViewToggledEventArgs e)
    {
        this.bIsDebugView = e.bIsDebugView;
        infoText.gameObject.SetActive(bIsDebugView);
        overlayGO.SetActive(bIsDebugView);
    }

    public void BeingHovered()
    {
        if (bIsDebugView) overlayGO.SetActive(false);
        selectableGO.SetActive(true);
    }

    public void ExitBeingHovered()
    {
        if (bIsDebugView) overlayGO.SetActive(true);
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
        F = -1.0f;
        G = -1.0f;
        H = -1.0f;
    }

    public void UpdateInfoText()
    {
        infoText.text = $"P: ({Row},{Col})\n" +
            $"Cost: {Cost}\n" +
            $"F: {(F < 0 ? "N/A" : F)}\n" +
            $"G: {(G < 0 ? "N/A" : G)}\n" +
            $"H: {(H < 0 ? "N/A" : H)}\n" +
            $"{(GridMap.ETileType)Cost}";
    }

    public bool IsSelectable() { return bIsSelectable; }
    public string GetInfoText() { return infoText.text; }
}
