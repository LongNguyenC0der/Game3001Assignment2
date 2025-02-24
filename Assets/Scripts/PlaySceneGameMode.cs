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
    private Tile currentlyHoveredTile = null;

    private void Start()
    {
        player = Instantiate<GameObject>(playerPrefab, new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity);
        player.SetActive(false);
    }

    private void Update()
    {
        if(bIsDebugView && currentlyHoveredTile)
        {
            if (Input.GetMouseButtonDown(0))
            {
                SetUpActor();
            }
        }

        if (Input.GetKeyDown(KeyCode.H))
        {
            bIsDebugView = !bIsDebugView;
            if (!bIsDebugView) currentlyHoveredTile = null;
            OnDebugViewToggled?.Invoke(this, new OnDebugViewToggledEventArgs { bIsDebugView = this.bIsDebugView });
        }
        //if (Input.GetKeyDown(KeyCode.Keypad0) || Input.GetKeyDown(KeyCode.Alpha0))
        //{
        //    playMode = EPlayMode.Standby;
        //    player.SetActive(false);
        //    target.SetActive(false);
        //    enemy.SetActive(false);
        //}
    }

    private void FixedUpdate()
    {
        // Only run on debug view mode
        if (bIsDebugView)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            // If RayCast hit anything
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                // If it hits a Tile
                if (hit.collider.gameObject.TryGetComponent<Tile>(out Tile tile))
                {
                    // If the currentlyHoveredTile is not the tile that is currently being hovered on screen.
                    if (currentlyHoveredTile != tile)
                    {
                        // If the currentlyHoveredTile is not null, which means we are unhovereing from a currently selected tile.
                        if (currentlyHoveredTile) currentlyHoveredTile.ExitBeingHovered();

                        // Set the currentlyHoveredTile to the tile that RayCast hit.
                        currentlyHoveredTile = tile;
                        currentlyHoveredTile.BeingHovered();
                    }
                }
                // If the thing getting hit is not a Tile, which is unlikely, since we only have Tiles on screen
                else
                {
                    if (currentlyHoveredTile)
                    {
                        currentlyHoveredTile.ExitBeingHovered();
                        currentlyHoveredTile = null;
                    }
                }
            }
            // If Raycast not hitting anything
            else
            {
                if(currentlyHoveredTile)
                {
                    currentlyHoveredTile.ExitBeingHovered();
                    currentlyHoveredTile = null;
                }
            }
        }
    }

    private void SetUpActor()
    {
        player.SetActive(false);

        Vector3 newPos = new Vector3(
            currentlyHoveredTile.transform.position.x + 0.5f,
            currentlyHoveredTile.transform.position.y,
            currentlyHoveredTile.transform.position.z - 0.5f
            );
        player.transform.position = newPos;

        player.SetActive(true);
        //SoundManager.Instance.PlayEffectSound();
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