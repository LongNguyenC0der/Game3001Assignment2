using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class PlaySceneGameMode : MonoBehaviour
{
    public class OnDebugViewToggledEventArgs : EventArgs { public bool bIsDebugView; }
    public event EventHandler<OnDebugViewToggledEventArgs> OnDebugViewToggled;

    private const float MOVE_SPEED = 2.0f;
    private const float TURN_SPEED = 100.0f;

    [SerializeField] private TMP_Text totalCostText;
    [SerializeField] private TMP_Text pathInfoText;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject targetPrefab;
    private GameObject player;
    private GameObject target;

    private bool bIsFindingPath = false;
    private bool bCanMove = false;
    private bool bIsDebugView = false;
    private Tile currentlyHoveredTile = null;

    private GridMap gridMap;
    [SerializeField] private int iterations;

    private List<Tile> path = new List<Tile>();
    private Stack<Tile> waypoints = new Stack<Tile>();

    private void Start()
    {
        gridMap = FindFirstObjectByType<GridMap>();
        player = Instantiate<GameObject>(playerPrefab, new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity);
        target = Instantiate<GameObject>(targetPrefab, new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity);
        player.SetActive(false);
        target.SetActive(false);
        totalCostText.text = "Total Path Cost: ...";
        pathInfoText.text = string.Empty;
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
                SoundManager.Instance.PlayFanfare();
                bCanMove = false;
            }
        }

        // Setting start and end tile with mouse clicks
        // We also don't want users to change start and end tile while the algorithm is running because it can be a bit funky and I don't want to deal with that
        // Also...The same thing with while the character is moving
        // You can do whatever you want otherwise
        if (bIsDebugView && currentlyHoveredTile && currentlyHoveredTile.IsSelectable())
        {
            if (!bIsFindingPath && !bCanMove && Input.GetMouseButtonDown(0))
            {
                SetUpActor(currentlyHoveredTile);
                gridMap.SetStartTile(currentlyHoveredTile);

                // If there's a path already, meaning there is a change of start tile. Proceed to pathfinding again!
                if (path.Count > 0)
                {
                    PathFinding();
                }
            }
            else if (!bIsFindingPath && !bCanMove && Input.GetMouseButtonDown(1))
            {
                // Can't be the same with start tile
                if (gridMap.GetStartTile() != currentlyHoveredTile)
                {
                    SetUpGoal();
                    gridMap.SetEndTile(currentlyHoveredTile);

                    // If there's a path already, meaning there is a change of end tile. Proceed to pathfinding again!
                    if (path.Count > 0)
                    {
                        // Reset actor back to the start position in case we change the end tile after the actor has moved.
                        if (HasPlayerMovedFromStartingLocation()) SetUpActor(gridMap.GetStartTile());

                        PathFinding();
                    }
                }
                // Else display error if wanted.
            }
        }

        // Reset
        if (Input.GetKeyDown(KeyCode.R))
        {
            SoundManager.Instance.PlayButtonSound();

            player.SetActive(false);
            target.SetActive(false);
            currentlyHoveredTile = null;
            gridMap.ResetAllTiles(true);
            path.Clear();
            totalCostText.text = "Total Path Cost: ...";
            pathInfoText.text = string.Empty;

            // Pretty annoying since we're not using objects pooling, we probably should...
            // Have to delay the DebugInvoke event in case we're reseting while in debug mode. Just to give the newly created tiles a brief moment to subcribe back in.
            DelayDebugInvoke();
        }

        // Find Shortest Path
        else if (!bCanMove && Input.GetKeyDown(KeyCode.F))
        {
            SoundManager.Instance.PlayButtonSound();

            if (gridMap.GetStartTile())
            {
                if (HasPlayerMovedFromStartingLocation()) SetUpActor(gridMap.GetStartTile());
            }
            
            PathFinding();
        }

        // Debug View Toggle
        else if (Input.GetKeyDown(KeyCode.H))
        {
            SoundManager.Instance.PlayButtonSound();

            bIsDebugView = !bIsDebugView;
            OnDebugViewToggled?.Invoke(this, new OnDebugViewToggledEventArgs { bIsDebugView = this.bIsDebugView });
        }

        // Move actor to the goal
        else if (Input.GetKeyDown(KeyCode.M))
        {
            SoundManager.Instance.PlayButtonSound();

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
                if (HasPlayerMovedFromStartingLocation()) SetUpActor(gridMap.GetStartTile());

                bCanMove = true;
            }
        }
    }

    private void FixedUpdate()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        // If RayCast hit anything
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // If it hits a Tile and the Tile is selectable (A.K.A not a Wall)
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
                    pathInfoText.text = currentlyHoveredTile.GetInfoText();
                }
            }
            // If the thing getting hit is not a Tile, which is unlikely, since we only have Tiles on screen
            else
            {
                if (currentlyHoveredTile)
                {
                    currentlyHoveredTile.ExitBeingHovered();
                    currentlyHoveredTile = null;
                    pathInfoText.text = string.Empty;
                }
            }
        }
        // If Raycast not hitting anything
        else
        {
            if (currentlyHoveredTile)
            {
                currentlyHoveredTile.ExitBeingHovered();
                currentlyHoveredTile = null;
                pathInfoText.text = string.Empty;
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
        SoundManager.Instance.PlayButtonSound();
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
        SoundManager.Instance.PlayButtonSound();
    }

    private IEnumerator FindShortestPath()
    {
        bIsFindingPath = true;

        while (true)
        {
            iterations++;

            path = Pathing.AStarPathing(gridMap.GetStartTile(), gridMap.GetEndTile(), gridMap.GetTileList(), iterations, gridMap, out float totalPathCost);

            if (path.Count > 0)
            {
                foreach (Tile tile in path)
                {
                    tile.BeingRetraced();
                }
                totalCostText.text = $"Total Path Cost: {totalPathCost}";
                bIsFindingPath = false;
                yield break;
            }

            yield return new WaitForSeconds(0.05f);
        }
    }

    private void PathFinding()
    {
        if (gridMap.GetStartTile() && gridMap.GetStartTile())
        {
            iterations = 0;
            StopAllCoroutines();
            gridMap.ResetAllTiles(false);
            StartCoroutine(FindShortestPath());
            totalCostText.text = "Total Path Cost: ...";
        }
    }

    private async void DelayDebugInvoke()
    {
        await Task.Delay(10); // 10 ms = 0.01s
        OnDebugViewToggled?.Invoke(this, new OnDebugViewToggledEventArgs { bIsDebugView = this.bIsDebugView });
    }

    private bool HasPlayerMovedFromStartingLocation()
    {
        return Vector3.Distance(player.transform.position, gridMap.GetStartTile().transform.position) >= 1f;
    }

    private bool MovePlayer(Vector3 target)
    {
        // Offset because of how we set up the Cube(Tile)
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
            SoundManager.Instance.PlayStepSound();
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