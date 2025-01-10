//using UnityEditor.Experimental.GraphView;
using UnityEngine;

/// <summary>
/// A typical InputHandler captures the input from the player and allows them to control the game
/// </summary>
public class InputHandler : MonoBehaviour
{
    internal static InputHandler instance;
    internal bool isClicking = false;

    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        if (ConnectDotsGameController.isGameActive)
        {
            if (!Input.GetMouseButton(0)) CancelClick();
        }
    }

    /// <summary>
    /// Cancels the players click so the mouse position no longer affects the grid
    /// </summary>
    internal void CancelClick()
    {
        ConnectDotsGameController.instance.CheckGame();
    }
}