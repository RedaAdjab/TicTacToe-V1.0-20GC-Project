using System.Collections.Generic;
using UnityEngine;

public class GridBoxList : MonoBehaviour
{
    public static GridBoxList Instance { get; private set; }

    [SerializeField] private List<GridBox> gridBoxList;


    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Only one Instance Can be Active");
            return;
        }
        Instance = this;   
    }

    public Vector2 GetGridWorldPosition(Vector2 gridCoordinate)
    {
        foreach (GridBox gridBox in gridBoxList)
        {
            if (gridCoordinate == gridBox.GetGridCoordinate())
            {
                return gridBox.GetGridPosition();
            }
        }
        return Vector2.zero;
    }
}
