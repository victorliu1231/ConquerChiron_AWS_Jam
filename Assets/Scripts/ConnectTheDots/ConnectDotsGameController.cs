using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ConnectDotsGameController : MonoBehaviour
{
    internal static ConnectDotsGameController instance;

    internal List<Line> lines = new List<Line>();
    internal List<Cell> cells = new List<Cell>();

    Cell currentCell;

    Cell lastCell;

    int movesCount = 0;

    [SerializeField] GameObject winScreen;

    internal static bool isGameActive = true;

    private void Awake()
    {
        instance = this;
    }

    /// <summary>
    /// ran when the player moves their mouse into a cell area while clicking
    /// </summary>
    /// <param name="cell">The cell that we just entered</param>
    internal void ChangeCurrentCell(Cell cell)
    {
        //are we currently clicking anything?
        if(currentCell== null)
        {
            //is the cell we are clicking a head?
            if (cell.isHead)
            {
                currentCell = cell;
                cell.line.Clear();
                cell.line.Add(cell);

                if(lastCell == null || !(lastCell.isHead && lastCell.typeIndex == cell.typeIndex))
                {
                    movesCount++;
                }
                lastCell = cell;
            }
            //we already have a line here
            else if (cell.isSelected)
            {
                currentCell = cell;

                if (cell.post != null) cell.post.UnSet();

                if(lastCell == null || !lastCell.IsConnected(cell))
                {
                    movesCount++;
                }

                lastCell = cell;
            }
            else
            {
                InputHandler.instance.CancelClick();
            }
        }
        //yes, we are dragging our mouse
        else
        {
            //ensure that the cell is a direct neighbor
            if((currentCell.x==cell.x && (currentCell.y+1==cell.y || currentCell.y - 1 == cell.y)) || 
                (currentCell.y == cell.y && (currentCell.x + 1 == cell.x || currentCell.x - 1 == cell.x)))
            {
                //ensure that the cell is not the head of another color
                if(!currentCell.Equals(cell) && !(cell.isHead && cell.typeIndex != currentCell.typeIndex))
                {
                    currentCell.line.Add(cell);
                    currentCell = cell;
                }
            }
            else
            {
                //InputHandler.instance.CancelClick(); // This line of code breaks the game but was in original GitHub code
            }
        }
    }

    /// <summary>
    /// Check if the game is finished
    /// </summary>
    internal void CheckGame()
    {
        currentCell = null;
        lastCell = null;

        foreach (Line line in lines) if (!line.IsConnected()) return;

        if (CountFilledCells() < cells.Count) return;

        isGameActive = false;
        winScreen.SetActive(true);
    }

    /// <summary>
    /// Count the number of cells that are selected
    /// </summary>
    /// <returns></returns>
    int CountFilledCells()
    {
        int count = 0;
        foreach (Cell cell in cells) if (cell.isSelected) count++;
        return count;
    }
}
