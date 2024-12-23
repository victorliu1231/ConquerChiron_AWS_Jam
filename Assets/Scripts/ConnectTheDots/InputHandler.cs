using UnityEditor.Experimental.GraphView;
using UnityEngine;

/// <summary>
/// A typical InputHandler captures the input from the player and allows them to control the game
/// </summary>
public class InputHandler : MonoBehaviour
{
    internal static InputHandler instance;

    [SerializeField] Camera mainCamera;
    [SerializeField] Transform mousePositionTrackingCube;
    [SerializeField] BoxCollider mouseCubeBoxCollider;

    internal bool isClicking = false;
    bool hasReleased = true;

    [SerializeField] float heightOffset = -1.59f;
    [SerializeField] float widthOffset = 0;

    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        if (GameController.isGameActive)
        {
            if (Input.GetMouseButtonDown(0)) isClicking = true;
            else CancelClick();
        }
    }

    /// <summary>
    /// Cancels the players click so the mouse position no longer affects the grid
    /// </summary>
    internal void CancelClick()
    {
        GameController.instance.CheckGame();
        isClicking = false;
    }
}