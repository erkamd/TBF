using UnityEngine;

public class Ball : MonoBehaviour
{
    public static Ball Instance { get; private set; }
    public Vector2Int gridPosition;
    private Vector2 velocity = Vector2.zero;
    private bool isTravelling = false;

    [SerializeField]
    private float friction = 0.9f;

    private const float stopThreshold = 0.05f;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // create a simple sphere visual if none present
        if (GetComponentInChildren<MeshRenderer>() == null)
        {
            var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);

            MeshRenderer meshRenderer = sphere.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
                meshRenderer.material.color = new Color(1.0f, 1.0f, 1.0f, 0.2f);

            sphere.GetComponent<Collider>().enabled = false;

            sphere.transform.SetParent(transform, false);
            sphere.transform.localPosition = new Vector3(0, 0, -1);
            sphere.transform.localScale = Vector3.one;
        }
    }

    public void MoveTo(Vector2Int cell)
    {
        bool cameFromTravel = isTravelling;

        gridPosition = cell;
        transform.position = GridManager.Instance.CellToWorld(cell);

        isTravelling = false;
        velocity = Vector2.zero;
    }

    // Call this to start a pass
    public void PassTo(Vector2Int targetCell, bool hard)
    {
        if (isTravelling)
            return;

        Vector3 targetWorld = GridManager.Instance.CellToWorld(targetCell);
        Vector3 startWorld = transform.position;
        Vector2 delta = new Vector2(targetWorld.x - startWorld.x, targetWorld.y - startWorld.y);

        velocity = delta * (1f - friction);
        if (hard)
            velocity *= 1.5f;

        isTravelling = true;

        foreach (var a in GameManager.Instance.AllAgents)
            a.hasBall = false;
    }

    public void AdvanceWithVelocity()
    {
        if (!isTravelling)
            return;

        // Store previous grid cell
        Vector2Int prevCell = gridPosition;

        // Move the ball
        transform.position += (Vector3)velocity;
        velocity *= friction;

        // Calculate new grid cell
        Vector2Int newCell = new Vector2Int(
            Mathf.RoundToInt(transform.position.x / GridManager.Instance.cellSize),
            Mathf.RoundToInt(transform.position.y / GridManager.Instance.cellSize));

        // Check all cells between prevCell and newCell, but skip the starting cell
        bool first = true;
        foreach (var cell in GetCellsOnLine(prevCell, newCell))
        {
            if (first)
            {
                first = false;
                continue; // Skip the starting cell
            }

            gridPosition = cell;

            // Check for agent collision during travel
            var agent = GameManager.Instance.GetAgentAtCell(cell);
            if (agent != null)
            {
                foreach (var a in GameManager.Instance.AllAgents)
                    a.hasBall = false;

                isTravelling = false;
                velocity = Vector2.zero;
                transform.position = GridManager.Instance.CellToWorld(cell);
                GameManager.Instance.TriggerImmediateAction(agent);
                return;
            }
        }

        gridPosition = newCell;

        int side;
        if (GridManager.Instance.IsGoalCell(newCell, out side))
        {
            isTravelling = false;
            velocity = Vector2.zero;
            GameManager.Instance.GoalScored(side);
            MoveTo(newCell);
            return;
        }

        bool bounceX = false;
        bool bounceY = false;

        // X axis bounce, but check for goal cells at left/right
        if (newCell.x < 0)
        {
            if (!GridManager.Instance.IsGoalCell(new Vector2Int(-1, newCell.y), out _))
            {
                bounceX = true;
                newCell.x = 0;
            }
        }
        else if (newCell.x >= GridManager.Instance.width)
        {
            if (!GridManager.Instance.IsGoalCell(new Vector2Int(GridManager.Instance.width, newCell.y), out _))
            {
                bounceX = true;
                newCell.x = GridManager.Instance.width - 1;
            }
        }

        // Y axis bounce, but check for goal cells at top/bottom
        if (newCell.y < 0)
        {
            if (!GridManager.Instance.IsGoalCell(new Vector2Int(newCell.x, -1), out _))
            {
                bounceY = true;
                newCell.y = 0;
            }
        }
        else if (newCell.y >= GridManager.Instance.height)
        {
            if (!GridManager.Instance.IsGoalCell(new Vector2Int(newCell.x, GridManager.Instance.height), out _))
            {
                bounceY = true;
                newCell.y = GridManager.Instance.height - 1;
            }
        }

        if (bounceX) velocity.x = -velocity.x;
        if (bounceY) velocity.y = -velocity.y;

        if (bounceX || bounceY)
        {
            transform.position = GridManager.Instance.CellToWorld(newCell);
            gridPosition = newCell;
        }

        if (velocity.magnitude < stopThreshold)
        {
            MoveTo(newCell);
        }
    }

    // Bresenham's line algorithm for grid traversal
    private static System.Collections.Generic.IEnumerable<Vector2Int> GetCellsOnLine(Vector2Int from, Vector2Int to)
    {
        int x0 = from.x, y0 = from.y;
        int x1 = to.x, y1 = to.y;
        int dx = Mathf.Abs(x1 - x0), dy = Mathf.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            yield return new Vector2Int(x0, y0);
            if (x0 == x1 && y0 == y1) break;
            int e2 = 2 * err;
            if (e2 > -dy) { err -= dy; x0 += sx; }
            if (e2 < dx) { err += dx; y0 += sy; }
        }
    }

    public bool IsTravelling()
    {
        return isTravelling;
    }

    public Vector2 Velocity => velocity;
}
