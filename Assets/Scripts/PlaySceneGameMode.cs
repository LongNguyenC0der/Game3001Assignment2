using System;
using UnityEngine;

public enum EPlayMode : byte
{
    Standby,
    LinearSeek,
    LinearFlee,
    LinearArrive,
    LinearAvoid
}

public class PlaySceneGameMode : MonoBehaviour
{
    public class OnDebugViewToggledEventArgs : EventArgs { public bool bIsDebugView; }
    public event EventHandler<OnDebugViewToggledEventArgs> OnDebugViewToggled;

    private const float BOUNDARY = 7.0f;
    private const float MOVE_SPEED = 3.0f;
    private const float TURN_SPEED = 100.0f;

    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject targetPrefab;
    [SerializeField] private GameObject enemyPrefab;
    private GameObject player;
    private GameObject target;
    private GameObject enemy;

    private EPlayMode playMode;

    private bool bIsDebugView = false;

    private void Start()
    {
        player = Instantiate<GameObject>(playerPrefab, new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            bIsDebugView = !bIsDebugView;
            OnDebugViewToggled?.Invoke(this, new OnDebugViewToggledEventArgs { bIsDebugView = this.bIsDebugView });
        }

        if (Input.GetKeyDown(KeyCode.Keypad0) || Input.GetKeyDown(KeyCode.Alpha0))
        {
            playMode = EPlayMode.Standby;
            player.SetActive(false);
            target.SetActive(false);
            enemy.SetActive(false);
        }
        else if (Input.GetKeyDown(KeyCode.Keypad1) || Input.GetKeyDown(KeyCode.Alpha1))
        {
            playMode = EPlayMode.LinearSeek;
            SetUpActors(playMode);
        }
        else if (Input.GetKeyDown(KeyCode.Keypad2) || Input.GetKeyDown(KeyCode.Alpha2))
        {
            playMode = EPlayMode.LinearFlee;
            SetUpActors(playMode);
        }
        else if (Input.GetKeyDown(KeyCode.Keypad3) || Input.GetKeyDown(KeyCode.Alpha3))
        {
            playMode = EPlayMode.LinearArrive;
            SetUpActors(playMode);
        }
        else if (Input.GetKeyDown(KeyCode.Keypad4) || Input.GetKeyDown(KeyCode.Alpha4))
        {
            playMode = EPlayMode.LinearAvoid;
            SetUpActors(playMode);
        }
    }

    private void SetUpActors(EPlayMode playMode)
    {
        player.SetActive(false);
        target.SetActive(false);
        enemy.SetActive(false);

        switch (playMode)
        {
            case EPlayMode.Standby:
                break;
            case EPlayMode.LinearSeek:
            case EPlayMode.LinearArrive:
                player.transform.position = GetRandomPosition();
                target.transform.position = GetRandomPosition();
                player.SetActive(true);
                target.SetActive(true);
                SoundManager.Instance.PlayEffectSound();
                break;
            case EPlayMode.LinearFlee:
                player.transform.position = GetRandomPosition();
                enemy.transform.position = GetRandomPosition();
                player.SetActive(true);
                enemy.SetActive(true);
                SoundManager.Instance.PlayEffectSound();
                break;
            case EPlayMode.LinearAvoid:
                player.transform.position = GetRandomPosition();
                target.transform.position = GetRandomPosition();
                enemy.transform.position = player.transform.position + ((target.transform.position - player.transform.position) / 2.0f);
                player.SetActive(true);
                target.SetActive(true);
                enemy.SetActive(true);
                SoundManager.Instance.PlayEffectSound();
                break;
            default:
                break;
        }
    }

    private Vector3 GetRandomPosition()
    {
        return new Vector3(UnityEngine.Random.Range(-BOUNDARY, BOUNDARY), 0.0f, UnityEngine.Random.Range(-BOUNDARY, BOUNDARY));
    }

    private void LinearSeek()
    {
        Vector3 direction = (target.transform.position - player.transform.position).normalized;
        Vector3 distanceToMove = MOVE_SPEED * Time.deltaTime * direction;
        player.transform.position += distanceToMove;
        Turning(direction);
    }

    private void Turning(Vector3 direction)
    {
        float currentAngle = player.transform.eulerAngles.y;
        float desiredAngle = Vector3.SignedAngle(player.transform.forward, direction, Vector3.up); // Return relative angle
        float newAngle = Mathf.MoveTowardsAngle(currentAngle, currentAngle + desiredAngle, TURN_SPEED * Time.deltaTime); // 2 absolute angles
        player.transform.rotation = Quaternion.Euler(0.0f, newAngle, 0.0f);
    }
}
