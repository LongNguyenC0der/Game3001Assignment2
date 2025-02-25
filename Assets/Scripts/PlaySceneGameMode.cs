using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    private bool bIsDebugView = false;
    private Tile currentlyHoveredTile = null;

    private GridMap gridMap;
    public int iterations;

    private void Start()
    {
        gridMap = FindFirstObjectByType<GridMap>();
        player = Instantiate<GameObject>(playerPrefab, new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity);
        target = Instantiate<GameObject>(targetPrefab, new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity);
        player.SetActive(false);
        target.SetActive(false);
    }

    private void Update()
    {
        if(bIsDebugView && currentlyHoveredTile)
        {
            if (Input.GetMouseButtonDown(0))
            {
                SetUpActor();
                gridMap.start = currentlyHoveredTile;
            }
            else if (Input.GetMouseButtonDown(1))
            {
                // Select the goal tile here
                // Can't be the same with start tile
                if (gridMap.start != currentlyHoveredTile)
                {
                    SetUpGoal();
                    gridMap.end = currentlyHoveredTile;
                }
                // Display error if wanted.
            }
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            player.SetActive(false);
            target.SetActive(false);
            currentlyHoveredTile = null;
            gridMap.ResetAllTiles();
        }

        else if (Input.GetKeyDown(KeyCode.F))
        {
            if (gridMap.start && gridMap.end)
            {
                iterations = 0;
                StopAllCoroutines();
                StartCoroutine(FindShortestPath());
            }
        }

        else if (Input.GetKeyDown(KeyCode.H))
        {
            bIsDebugView = !bIsDebugView;
            if (!bIsDebugView) currentlyHoveredTile = null;
            OnDebugViewToggled?.Invoke(this, new OnDebugViewToggledEventArgs { bIsDebugView = this.bIsDebugView });
        }
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
                        // Test adjacent
                        //foreach (Tile t in Pathing.Adjacent(currentlyHoveredTile, gridMap.GetTileList(), GridMap.ROWS, GridMap.COLUMNS))
                        //{
                        //    t.BeingHovered();
                        //}
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

    private void SetUpGoal()
    {
        target.SetActive(false);

        Vector3 newPos = new Vector3(
            currentlyHoveredTile.transform.position.x + 0.5f,
            currentlyHoveredTile.transform.position.y,
            currentlyHoveredTile.transform.position.z - 0.5f
            );
        target.transform.position = newPos;

        target.SetActive(true);
        //SoundManager.Instance.PlayEffectSound();
    }

    private IEnumerator FindShortestPath()
    {
        while(true)
        {
            iterations++;

            List<Tile> path = Pathing.Dijkstra(gridMap.start, gridMap.end, gridMap.GetTileList(), iterations, gridMap);

            if (path.Count > 0)
            {
                foreach (Tile tile in path)
                {
                    tile.BeingRetraced();
                }
                yield break;
            }

            yield return new WaitForSeconds(0.1f);
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