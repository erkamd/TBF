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
        gridPosition = cell;
        transform.position = GridManager.Instance.CellToWorld(cell);

        var agent = GameManager.Instance.GetAgentAtCell(cell);
        if (agent != null)
        {
            foreach (var a in GameManager.Instance.AllAgents)
                a.hasBall = false;

            agent.hasBall = true;
            isTravelling = false;
            velocity = Vector2.zero;
        }
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

        transform.position += (Vector3)velocity;
        velocity *= friction;

        Vector2Int cell = new Vector2Int(
            Mathf.RoundToInt(transform.position.x / GridManager.Instance.cellSize),
            Mathf.RoundToInt(transform.position.y / GridManager.Instance.cellSize));

        cell.x = Mathf.Clamp(cell.x, 0, GridManager.Instance.rows - 1);
        cell.y = Mathf.Clamp(cell.y, 0, GridManager.Instance.columns - 1);

        gridPosition = cell;

        if (velocity.magnitude < stopThreshold)
        {
            MoveTo(cell);
        }
    }

    public bool IsTravelling()
    {
        return isTravelling;
    }
}
