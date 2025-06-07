using UnityEngine;

public class Ball : MonoBehaviour
{
    public static Ball Instance { get; private set; }
    public Vector2Int gridPosition;
    private Vector2Int velocity = Vector2Int.zero;
    private int remainingSteps = 0;
    private bool isTravelling = false;

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
                meshRenderer.material.color = Color.white;

            sphere.GetComponent<Collider>().enabled = false;

            sphere.transform.SetParent(transform, false);
            sphere.transform.localPosition = new Vector3(0, 0, -1);
            sphere.transform.localScale = Vector3.one * 0.5f;
        }
    }

    public void MoveTo(Vector2Int cell)
    {
        gridPosition = cell;
        transform.position = GridManager.Instance.CellToWorld(cell);

        var agent = GameManager.Instance.GetAgentAtCell(cell);
        if (agent != null)
        {
            foreach (var a in GameManager.Instance.AllAgents)
                a.hasBall = false;

            agent.hasBall = true;
            isTravelling = false;
            velocity = Vector2Int.zero;
            remainingSteps = 0;
        }
    }

    // Call this to start a pass
    public void PassTo(Vector2Int targetCell)
    {
        if (isTravelling)
            return;

        // If already at or next to the target, just move instantly
        if (IsNeighbor(gridPosition, targetCell))
        {
            MoveTo(targetCell);
            return;
        }

        int dx = targetCell.x - gridPosition.x;
        int dy = targetCell.y - gridPosition.y;
        int stepX = dx == 0 ? 0 : (dx > 0 ? 1 : -1);
        int stepY = dy == 0 ? 0 : (dy > 0 ? 1 : -1);
        velocity = new Vector2Int(stepX, stepY);
        remainingSteps = Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dy));
        isTravelling = true;
    }

    public void AdvanceWithVelocity()
    {
        if (!isTravelling)
            return;

        if (remainingSteps > 0)
        {
            Vector2Int next = gridPosition + velocity;
            next.x = Mathf.Clamp(next.x, 0, GridManager.Instance.rows - 1);
            next.y = Mathf.Clamp(next.y, 0, GridManager.Instance.columns - 1);
            MoveTo(next);
            remainingSteps--;
        }

        if (remainingSteps <= 0)
        {
            isTravelling = false;
            velocity = Vector2Int.zero;
        }
    }

    private bool IsNeighbor(Vector2Int a, Vector2Int b)
    {
        int dx = Mathf.Abs(a.x - b.x);
        int dy = Mathf.Abs(a.y - b.y);
        return dx <= 1 && dy <= 1 && (dx + dy) > 0;
    }

    public bool IsTravelling()
    {
        return isTravelling;
    }
}
