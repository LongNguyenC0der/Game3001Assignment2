using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlaySceneGameMode : MonoBehaviour
{
    public class OnDebugViewToggledEventArgs : EventArgs { public bool bIsDebugView; }
    public event EventHandler<OnDebugViewToggledEventArgs> OnDebugViewToggled;

    private const float BOUNDARY = 7.0f;
    private const float MOVE_SPEED = 2.0f;
    private const float TURN_SPEED = 100.0f;

    [SerializeField] private TMP_Text totalCostText;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject targetPrefab;
    private GameObject player;
    private GameObject target;

    private bool bCanMove = false;
    private bool bIsDebugView = false;
    private Tile currentlyHoveredTile = null;

    private GridMap gridMap;
    public int iterations;

    private List<Tile> path = new List<Tile>();
    private Stack<Tile> waypoints = new Stack<Tile>();

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
        if (bCanMove)
        {
            if (waypoints.TryPeek(out Tile waypoint))
            {
                if (MovePlayer(waypoint.transform.position))
                {
                    waypoints.Pop();
                }
            }
            else
            {
                bCanMove = false;
            }
        }

        // Setting start and end tile with mouse clicks
        if (bIsDebugView && currentlyHoveredTile)
        {
            if (Input.GetMouseButtonDown(0))
            {
                SetUpActor(currentlyHoveredTile);
                gridMap.start = currentlyHoveredTile;
            }
            else if (Input.GetMouseButtonDown(1))
            {
                // Can't be the same with start tile
                if (gridMap.start != currentlyHoveredTile)
                {
                    SetUpGoal();
                    gridMap.end = currentlyHoveredTile;
                }
                // Display error if wanted.
            }
        }

        // Reset
        if (Input.GetKeyDown(KeyCode.R))
        {
            player.SetActive(false);
            target.SetActive(false);
            currentlyHoveredTile = null;
            gridMap.ResetAllTiles(true);
            totalCostText.text = "Total Path Cost:";
        }

        // Find Shortest Path
        else if (Input.GetKeyDown(KeyCode.F))
        {
            if (gridMap.start && gridMap.end)
            {
                iterations = 0;
                StopAllCoroutines();
                gridMap.ResetAllTiles(false);
                StartCoroutine(FindShortestPath());
            }
        }

        // Debug View Toggle
        else if (Input.GetKeyDown(KeyCode.H))
        {
            bIsDebugView = !bIsDebugView;
            if (!bIsDebugView) currentlyHoveredTile = null;
            OnDebugViewToggled?.Invoke(this, new OnDebugViewToggledEventArgs { bIsDebugView = this.bIsDebugView });
        }

        // Move actor to the goal
        else if (Input.GetKeyDown(KeyCode.M))
        {
            if (path.Count > 0)
            {
                // Since our path retraces from end to start, using a Stack is perfect here.
                // So we don't have to reverse the path nor keep track of indexes.
                waypoints.Clear();
                foreach (Tile tile in path)
                {
                    waypoints.Push(tile);
                }

                // If the player is not at the start location, it means the user wants to redo the movement again
                // So we'll just teleport the player back to the start position
                if (Vector3.Distance(player.transform.position, waypoints.Peek().transform.position) >= 1f )
                {
                    SetUpActor(waypoints.Peek());
                }

                bCanMove = true;
            }
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

    private void SetUpActor(Tile tile)
    {
        player.SetActive(false);

        Vector3 newPos = new Vector3(
            tile.transform.position.x + 0.5f,
            tile.transform.position.y,
            tile.transform.position.z - 0.5f
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

            path = Pathing.Dijkstra(gridMap.start, gridMap.end, gridMap.GetTileList(), iterations, gridMap, out float totalPathCost);

            if (path.Count > 0)
            {
                foreach (Tile tile in path)
                {
                    tile.BeingRetraced();
                }
                totalCostText.text = $"Total Path Cost: {totalPathCost}";
                yield break;
            }

            yield return new WaitForSeconds(0.05f);
        }
    }

    private Vector3 GetRandomPosition()
    {
        return new Vector3(UnityEngine.Random.Range(-BOUNDARY, BOUNDARY), 0.0f, UnityEngine.Random.Range(-BOUNDARY, BOUNDARY));
    }

    private bool MovePlayer(Vector3 target)
    {
        // Offset because of how we set up the Cube
        Vector3 offset = new Vector3(
            target.x + 0.5f,
            target.y,
            target.z - 0.5f
            );

        //Seek
        Vector3 direction = (offset - player.transform.position).normalized;
        Vector3 distanceToMove = MOVE_SPEED * Time.deltaTime * direction;
        player.transform.position += distanceToMove;
        Turning(direction);
        
        if (Vector3.Distance(player.transform.position, offset) <= 0.01f)
        {
            return true;
        }

        return false;
    }

    private void Turning(Vector3 direction)
    {
        float currentAngle = player.transform.eulerAngles.y;
        float desiredAngle = Vector3.SignedAngle(player.transform.forward, direction, Vector3.up); // Return relative angle
        float newAngle = Mathf.MoveTowardsAngle(currentAngle, currentAngle + desiredAngle, TURN_SPEED * Time.deltaTime); // 2 absolute angles
        player.transform.rotation = Quaternion.Euler(0.0f, newAngle, 0.0f);
    }
}