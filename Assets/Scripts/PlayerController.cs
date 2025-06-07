using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public ActionMenu actionMenu;
    public ImmediateActionMenu immediateMenu;

    [System.NonSerialized]
    public bool selectionLocked = false;

    [System.NonSerialized]
    public AgentController selected;

    private void Update()
    {
        if (selectionLocked)
            return;

        if (Input.GetMouseButtonUp(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit))
            {
                var agent = hit.collider.GetComponentInParent<AgentController>();
                if (agent != null && agent == GameManager.Instance.CurrentAgent)
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
