using System.Collections.Generic;
using UnityEngine;

public static class Pathfinding
{
    private static readonly Vector2Int[] Directions =
    {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right,
        new Vector2Int(1,1),
        new Vector2Int(1,-1),
        new Vector2Int(-1,1),
        new Vector2Int(-1,-1)
    };

    private static bool IsValidCell(Vector2Int cell)
    {
        return cell.x >= 0 && cell.x < GridManager.Instance.width &&
               cell.y >= 0 && cell.y < GridManager.Instance.height;
    }

    public static Vector2Int NextStep(Vector2Int start, Vector2Int goal)
    {
        if (start == goal) return start;

        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        queue.Enqueue(start);
        cameFrom[start] = start;

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            foreach (var dir in Directions)
            {
                var next = current + dir;
                if (!IsValidCell(next))
                    continue;
                if (GameManager.Instance.IsCellOccupied(next) && next != goal)
                    continue;
                if (cameFrom.ContainsKey(next))
                    continue;
                cameFrom[next] = current;
                if (next == goal)
                {
                    queue.Clear();
                    break;
                }
                queue.Enqueue(next);
            }
        }

        if (!cameFrom.ContainsKey(goal))
            return start;

        var step = goal;
        while (cameFrom[step] != start)
            step = cameFrom[step];
        return step;
    }
}
