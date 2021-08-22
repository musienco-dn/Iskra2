using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CapFramerate : MonoBehaviour
{
    [SerializeField] Texture2D cursorTexture;
    [SerializeField] Texture2D cursorTextureChange;

    private CursorMode cursorMode = CursorMode.Auto;
    private Vector2 hotSpot = Vector2.zero;
    void OnMouseEnter()
    {
        Cursor.SetCursor(cursorTexture, hotSpot, cursorMode);
    }

    void OnMouseExit()
    {
        Cursor.SetCursor(cursorTextureChange, Vector2.zero, cursorMode);
    }

    void Start()
    {
        Cursor.SetCursor(cursorTexture, Vector2.zero, CursorMode.ForceSoftware);
        //Application.targetFrameRate = 62;

    }

    // Update is called once per frame
    void Update()
    {

    }
}