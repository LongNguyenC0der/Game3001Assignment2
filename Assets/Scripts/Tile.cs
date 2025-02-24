using TMPro;
using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField] private TMP_Text costText;
    [SerializeField] private int cost = 1;
    [SerializeField] private GameObject overlayGO;

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
    }
}
