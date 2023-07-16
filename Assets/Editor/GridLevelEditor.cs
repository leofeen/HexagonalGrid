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
        float xShift = 0.5f * level.gridSize.x;
        float yShift = 0.5f * level.gridSize.y;
        Vector2Int maxIndiciesDrawn = new Vector2Int(level.maxCoordinateLinesDrawn / 2, level.maxCoordinateLinesDrawn / 2);

        for (int i = 0; i < level.maxCoordinateLinesDrawn / 2; i++)
        {
            Vector3 positiveCoordinate = level.IndiciesToWorldPoint(i, i) + new Vector3(xShift, yShift, 0);
            Vector3 negativeCoordinate = level.IndiciesToWorldPoint(-i, -i) + new Vector3(xShift, yShift, 0);

            Handles.color = level.gridLinesColor;
            Handles.DrawLine(new Vector3(-maxIndiciesDrawn.x + xShift, positiveCoordinate.y, 0), new Vector3(maxIndiciesDrawn.x + xShift, positiveCoordinate.y, 0));
            Handles.DrawLine(new Vector3(positiveCoordinate.x, -maxIndiciesDrawn.y + yShift, 0), new Vector3(positiveCoordinate.x, maxIndiciesDrawn.y + yShift, 0));
            Handles.DrawLine(new Vector3(-maxIndiciesDrawn.x + xShift, negativeCoordinate.y, 0), new Vector3(maxIndiciesDrawn.x + xShift, negativeCoordinate.y, 0));
            Handles.DrawLine(new Vector3(negativeCoordinate.x, -maxIndiciesDrawn.y + yShift, 0), new Vector3(negativeCoordinate.x, maxIndiciesDrawn.y + yShift, 0));
        }

        if (selectionInfo.isSelectionActive)
        {
            Vector3[] rectangleCoordinates = FindRectangleWorldVerts(selectionInfo.selectionStartIndicies, selectionInfo.selectionEndIndicies);

            Handles.DrawSolidRectangleWithOutline(rectangleCoordinates, level.mouseOverFillColor, level.mouseOverOutlineColor);
        }
        else if (mouseOverGridTile.x <= maxIndiciesDrawn.x && mouseOverGridTile.y <= maxIndiciesDrawn.y
            && mouseOverGridTile.x >= -maxIndiciesDrawn.x && mouseOverGridTile.y >= -maxIndiciesDrawn.y)
        {
            Vector3[] rectangleCoordinates = FindRectangleWorldVerts(mouseOverGridTile, mouseOverGridTile);

            Handles.DrawSolidRectangleWithOutline(rectangleCoordinates, level.mouseOverFillColor, level.mouseOverOutlineColor);
        }

        needsRepaint = false;
    }

    Vector3[] FindRectangleWorldVerts(Vector2Int startIndicies, Vector2Int endIndicies)
    {
        float xShift = 0.5f * level.gridSize.x;
        float yShift = 0.5f * level.gridSize.y;

        Vector3 startWorldPoint = level.IndiciesToWorldPoint(startIndicies);
        Vector3 endWorldPoint = level.IndiciesToWorldPoint(endIndicies);

        Vector3 leftUp = new Vector3(Mathf.Min(startWorldPoint.x, endWorldPoint.x), Mathf.Max(startWorldPoint.y, endWorldPoint.y), startWorldPoint.z);
        Vector3 rightDown = new Vector3(Mathf.Max(startWorldPoint.x, endWorldPoint.x), Mathf.Min(startWorldPoint.y, endWorldPoint.y), startWorldPoint.z);
        Vector3 rightUp = new Vector3(Mathf.Max(startWorldPoint.x, endWorldPoint.x), Mathf.Max(startWorldPoint.y, endWorldPoint.y), startWorldPoint.z);
        Vector3 leftDown = new Vector3(Mathf.Min(startWorldPoint.x, endWorldPoint.x), Mathf.Min(startWorldPoint.y, endWorldPoint.y), startWorldPoint.z);

        return new Vector3[]
        {
            rightUp + xShift*Vector3.right + yShift*Vector3.up,
            rightDown + xShift*Vector3.right + yShift*Vector3.down,
            leftDown + xShift*Vector3.left + yShift*Vector3.down,
            leftUp + xShift*Vector3.left + yShift*Vector3.up,
        };
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

        if (GUILayout.Button("Recalculate Grid Size"))
        {
            level.RecalculateGridSize();
            level.RecalculatePositions();
        }

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
