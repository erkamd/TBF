using UnityEngine;
using UnityEngine.EventSystems;

public class GridCell : MonoBehaviour, IPointerClickHandler
{
    public Vector2Int gridPosition;

    // This method is called by Unity when the object is clicked
    public void OnPointerClick(PointerEventData eventData)
    {
        // Only respond to right mouse button clicks
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            ClickCallBack();
        }
        //else if (eventData.button == PointerEventData.InputButton.Left)
        //{
        //    GameManager.Instance.playerController.actionMenu.Close();
        //    GameManager.Instance.playerController.selected = null;
        //}
    }

    public void ClickCallBack()
    {
        Debug.Log("clicked: " + gridPosition);
        GameManager.Instance.OnGridCellClicked(gridPosition);
    }
}
