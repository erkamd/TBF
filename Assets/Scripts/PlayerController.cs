using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public ActionMenu actionMenu;
    private AgentController selected;

    private void Update()
    {
        if (!GameManager.Instance.IsPlayerTurn)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit))
            {
                var agent = hit.collider.GetComponentInParent<AgentController>();
                if (agent != null && GameManager.Instance.PlayerAgents.Contains(agent))
                {
                    SelectAgent(agent);
                }
            }
        }
    }

    private void SelectAgent(AgentController agent)
    {
        if (agent.actionPoints <= 0)
            return;

        selected = agent;
        actionMenu.Open(agent);
    }
}
