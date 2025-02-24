using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartSceneGameMode : MonoBehaviour
{
    [SerializeField] private Button playButton;

    private void Start()
    {
        playButton.onClick.AddListener(() =>
        {
            SoundManager.Instance.PlayEffectSound();
            SceneManager.LoadScene(1);
        });
    }
}
