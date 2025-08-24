using UnityEngine;

public class GridBox : MonoBehaviour
{
    [SerializeField] private Vector2Int gridCoordinate;

    private void OnMouseDown()
    {
        if (GameStateManager.Instance.IsOnline())
        {
            GameManager.Instance.ClickedOnGridBoxRpc(gridCoordinate, transform.position, GameManager.Instance.GetLocalPlayerType());
        }
        else
        {
            GameManager.Instance.ClickedOnGridBoxRemote(gridCoordinate, transform.position, GameManager.Instance.GetLocalPlayerType());
        }

    }

    public Vector2Int GetGridCoordinate()
    {
        return gridCoordinate;
    }

    public Vector2 GetGridPosition()
    {
        return transform.position;
    }
}
