using TMPro;
using UnityEngine;

public class AgentController : MonoBehaviour
{
    public AgentStats stats = new AgentStats();
    public Vector2Int gridPosition;
    public bool hasBall;
    public int actionPoints;
    public int jerseyNumber;
    public Color agentColor = Color.white; // Default to white

    public TextMeshPro jerseyText;

    private void Start()
    {
        if (GetComponent<MeshRenderer>() == null)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.SetParent(transform, false);

            // Assign color to material
            MeshRenderer meshRenderer = cube.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
                meshRenderer.material.color = agentColor;
        }

        jerseyText.text = jerseyNumber.ToString();

        ResetActionPoints();
    }

    public void Initialize(Vector2Int cell)
    {
        gridPosition = cell;
        transform.position = GridManager.Instance.CellToWorld(cell);
    }

    public void MoveTo(Vector2Int cell)
    {
        gridPosition = cell;
        transform.position = GridManager.Instance.CellToWorld(cell);
        if (hasBall)
        {
            Ball.Instance.MoveTo(cell);
        }
    }

    public void ResetActionPoints()
    {
        actionPoints = stats.speed;
    }

    public bool SpendActionPoints(int amount)
    {
        if (actionPoints < amount)
            return false;
        actionPoints -= amount;
        return true;
    }
}
