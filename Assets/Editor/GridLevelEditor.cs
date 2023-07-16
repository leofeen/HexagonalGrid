using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GridLevel))]
public class GridLevelEditor : Editor
{
    GridLevel level;
    bool needsRepaint = false;
    Vector2Int mouseOverGridTile;
    SelectionInfo selectionInfo;

    void OnEnable()
    {
        level = target as GridLevel;
        mouseOverGridTile = Vector2Int.zero;
        Undo.undoRedoPerformed += level.RefreshPlacedObjectsList;

        selectionInfo.isSelectionActive = false;
    }

    void OnSceneGUI()
    {
        Event guiEvent = Event.current;

        if (guiEvent.type == EventType.Repaint)
        {
            DrawGrid();
        }
        else if (guiEvent.type == EventType.Layout)
        {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
        }
        else
        {
            HandleInput(guiEvent);
            if (needsRepaint)
            {
                HandleUtility.Repaint();
            }
        }      
    }

    void HandleInput(Event guiEvent)
    {
        Ray mouseRay = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition);
        float distanceToGridPlane = (level.gridPlaneHeight - mouseRay.origin.z) / mouseRay.direction.z;
        Vector3 mousePoint = mouseRay.GetPoint(distanceToGridPlane);

        if ((guiEvent.type == EventType.MouseDown || guiEvent.type == EventType.MouseDrag) 
            && guiEvent.button == 0 && guiEvent.modifiers == EventModifiers.None)
        {
            HandleLeftMouse(mousePoint);
        }
        else if ((guiEvent.type == EventType.MouseDown || guiEvent.type == EventType.MouseDrag) 
            && guiEvent.button == 0 && guiEvent.modifiers == EventModifiers.Shift)
        {
            HandleShiftLeftMouse(mousePoint);
        }
        else if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && guiEvent.modifiers == EventModifiers.Control)
        {
            HandleCtrlLeftMouseDown(mousePoint);
        }
        else if (guiEvent.type == EventType.MouseDrag && guiEvent.button == 0 && guiEvent.modifiers == EventModifiers.Control)
        {
            HandleCtrlLeftMouseDrag(mousePoint);
        }
        else if (guiEvent.type == EventType.MouseDown && guiEvent.button == 1)
        {
            HandleRightMouseDown(mousePoint);
        }

        UpdateMouseOverGridTile(mousePoint); 
    }

    void HandleRightMouseDown(Vector3 mousePoint)
    {
        selectionInfo.isSelectionActive = false;
    }

    void HandleCtrlLeftMouseDown(Vector3 mousePoint)
    {
        selectionInfo.isSelectionActive = true;
        Vector2Int mousePointIndicies = level.WorldPointToIndicies(mousePoint);
        selectionInfo.selectionStartIndicies = mousePointIndicies;
        selectionInfo.selectionEndIndicies = mousePointIndicies;
    }

    void HandleCtrlLeftMouseDrag(Vector3 mousePoint)
    {
        selectionInfo.selectionEndIndicies = level.WorldPointToIndicies(mousePoint);
    }

    void HandleLeftMouse(Vector3 mousePoint)
    {
        GameObject gameObject;

        if (!selectionInfo.isSelectionActive)
        {
            gameObject = level.PlaceSelectedObject(mousePoint);
            Undo.RegisterCreatedObjectUndo(gameObject, "Place object on grid");
            needsRepaint = true;
            return;
        }

        selectionInfo.isSelectionActive = false;

        for (int x = Mathf.Min(selectionInfo.selectionStartIndicies.x, selectionInfo.selectionEndIndicies.x);
                x <= Mathf.Max(selectionInfo.selectionStartIndicies.x, selectionInfo.selectionEndIndicies.x); x++)
        {
            for (int y = Mathf.Min(selectionInfo.selectionStartIndicies.y, selectionInfo.selectionEndIndicies.y);
                    y <= Mathf.Max(selectionInfo.selectionStartIndicies.y, selectionInfo.selectionEndIndicies.y); y++)
            {
                gameObject = level.PlaceSelectedObject(new Vector2Int(x, y));
                Undo.RegisterCreatedObjectUndo(gameObject, "Place object on grid");
            }
        }
        
        needsRepaint = true;
    }

    void HandleShiftLeftMouse(Vector3 mousePoint)
    {
        if (!selectionInfo.isSelectionActive)
        {
            level.DeleteObject(mousePoint);
            needsRepaint = true;
            return;
        }

        selectionInfo.isSelectionActive = false;

        for (int x = Mathf.Min(selectionInfo.selectionStartIndicies.x, selectionInfo.selectionEndIndicies.x);
                x <= Mathf.Max(selectionInfo.selectionStartIndicies.x, selectionInfo.selectionEndIndicies.x); x++)
        {
            for (int y = Mathf.Min(selectionInfo.selectionStartIndicies.y, selectionInfo.selectionEndIndicies.y);
                    y <= Mathf.Max(selectionInfo.selectionStartIndicies.y, selectionInfo.selectionEndIndicies.y); y++)
            {
                level.DeleteObject(new Vector2Int(x, y));
            }
        }
        
        needsRepaint = true;
    }

    void DrawGrid()
    {
        for (int q = -level.numberOfHexesOnSide + 1; q <= level.numberOfHexesOnSide - 1; q++)
        {
            for (int r = -level.numberOfHexesOnSide + 1; r <= level.numberOfHexesOnSide - 1; r++)
            {
                if (Mathf.Abs(-q - r) <= level.numberOfHexesOnSide - 1)
                {
                    DrawHexagon(q, r, level.gridLinesColor);
                }
            }
        }

        DrawSelection();

        needsRepaint = false;
    }

    void DrawSelection()
    {
        if (!selectionInfo.isSelectionActive)
        {
            DrawHexagon(mouseOverGridTile, level.mouseOverOutlineColor);
            return;
        }

        int minQ = Mathf.Min(selectionInfo.selectionStartIndicies.x, selectionInfo.selectionEndIndicies.x);
        int maxQ = Mathf.Max(selectionInfo.selectionStartIndicies.x, selectionInfo.selectionEndIndicies.x);
        int minR = Mathf.Min(selectionInfo.selectionStartIndicies.y, selectionInfo.selectionEndIndicies.y);
        int maxR = Mathf.Max(selectionInfo.selectionStartIndicies.y, selectionInfo.selectionEndIndicies.y);

        for (int q = minQ; q <= maxQ; q++)
        {
            for (int r = minR; r <= maxR; r++)
            {
                DrawHexagon(q, r, level.mouseOverOutlineColor);
            }
        }        
    }

    void DrawHexagon(int q, int r, Color linesColor)
    {
        DrawHexagon(new Vector2Int(q, r), linesColor);
    }

    void DrawHexagon(Vector2Int indicies, Color linesColor)
    {
        Vector3 center = level.IndiciesToWorldPoint(indicies);
        Vector3[] hexagonCorners = new Vector3[7];

        for (int i = 0; i <= 5; i++)
        {
            hexagonCorners[i] = GetPointyHexagonCorner(center, i);
        }

        hexagonCorners[6] = GetPointyHexagonCorner(center, 0);

        Handles.color = linesColor;
        Handles.DrawPolyLine(hexagonCorners);
    }

    Vector3 GetPointyHexagonCorner(Vector3 center, int i)
    {
        float angleDeg = 60 * i - 30;
        float angleRad = angleDeg * Mathf.Deg2Rad;
        return new Vector3(center.x + level.hexSize * Mathf.Cos(angleRad), center.y + level.hexSize * Mathf.Sin(angleRad), center.z);
    }

    void UpdateMouseOverGridTile(Vector3 mousePosition)
    {
        Vector2Int currentIndicies = level.WorldPointToIndicies(mousePosition);

        if (currentIndicies != mouseOverGridTile)
        {
            mouseOverGridTile = currentIndicies;
            needsRepaint = true;
        }
    }

    public override void OnInspectorGUI()
    {
        EditorGUILayout.HelpBox("ЛКМ - разместить\nShift + ЛКМ - удалить\nCtrl + ЛКМ - выделить область\nПКМ - снять выделение", MessageType.None, true);

        if (DrawDefaultInspector())
        {
            level.RecalculatePositions();
        }

        GUIStyle verticalBoxStyle = new GUIStyle();
        verticalBoxStyle.margin = new RectOffset(0, 0, 20, 20);

        EditorGUILayout.BeginVertical(verticalBoxStyle);

        string selectedObjectText = level.avaibleObjects[level.selectedIndex].objectName == "" ? level.avaibleObjects[level.selectedIndex].prefab.name : level.avaibleObjects[level.selectedIndex].objectName;
        GUILayout.Label($"Выбранный элемент: {selectedObjectText}");

        GUILayout.Space(10);

        GUILayout.Label("Выбор элемента"); 
        for (int i = 0; i < level.avaibleObjects.Count; i++)
        {
            string buttonText = level.avaibleObjects[i].objectName == "" ? level.avaibleObjects[i].prefab.name : level.avaibleObjects[i].objectName;
            
            if (GUILayout.Button(buttonText))
            {
                level.selectedIndex = i;
            }
        }
        EditorGUILayout.EndVertical();

        if (GUILayout.Button("Clear Grid"))
        {
            level.ClearGrid();
        }
    }

    public struct SelectionInfo
    {
        public bool isSelectionActive;
        public Vector2Int selectionStartIndicies;
        public Vector2Int selectionEndIndicies;
    }
}
