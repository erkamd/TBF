using UnityEngine;

public class Ball : MonoBehaviour
{
    public static Ball Instance { get; private set; }
    public Vector2Int gridPosition;

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
            sphere.transform.SetParent(transform, false);
            sphere.transform.localScale = Vector3.one * 0.5f;
        }
    }

    public void MoveTo(Vector2Int cell)
    {
        gridPosition = cell;
        transform.position = GridManager.Instance.CellToWorld(cell) + Vector3.up * 0.25f;
    }
}
