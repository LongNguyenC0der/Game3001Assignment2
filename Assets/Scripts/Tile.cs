using TMPro;
using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField] private TMP_Text costText;
    [SerializeField] private int cost = 1;
    [SerializeField] private GameObject overlayGO;
    [SerializeField] private GameObject selectableGO;

    private void Start()
    {
        costText.text = cost.ToString();
        costText.gameObject.SetActive(false);

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
}
