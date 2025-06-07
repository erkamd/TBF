using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    public static Ball Instance { get; private set; }
    public Vector2Int gridPosition;
    private Queue<Vector2Int> travelPath = new Queue<Vector2Int>();
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

        // Build path (straight line, diagonal allowed)
        travelPath.Clear();
        Vector2Int current = gridPosition;
        while (current != targetCell)
        {
            current = GetNextStep(current, targetCell);
            travelPath.Enqueue(current);
        }

        isTravelling = true;
        StartCoroutine(TravelPathCoroutine());
    }

    private IEnumerator TravelPathCoroutine()
    {
        while (travelPath.Count > 0)
        {
            Vector2Int next = travelPath.Dequeue();
            MoveTo(next);
            // Wait for a turn or a fixed time (simulate turn-based)
            yield return new WaitForSeconds(0.5f); // Replace with turn event if you have one
        }
        isTravelling = false;
    }

    private Vector2Int GetNextStep(Vector2Int current, Vector2Int target)
    {
        int dx = target.x - current.x;
        int dy = target.y - current.y;
        int stepX = dx == 0 ? 0 : (dx > 0 ? 1 : -1);
        int stepY = dy == 0 ? 0 : (dy > 0 ? 1 : -1);

        // Move diagonally if possible
        if (dx != 0 && dy != 0)
            return new Vector2Int(current.x + stepX, current.y + stepY);
        if (dx != 0)
            return new Vector2Int(current.x + stepX, current.y);
        if (dy != 0)
            return new Vector2Int(current.x, current.y + stepY);
        return current;
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
