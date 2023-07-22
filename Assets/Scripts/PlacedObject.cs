using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlacedObject : MonoBehaviour
{
    [HideInInspector]
    public int id;
    [HideInInspector]
    public Vector2Int indiciesOnGrid;
    [HideInInspector]
    public int associatedGridObjectIndex;

    public Vector2 perObjectOffset = Vector2.zero;

    public Vector3 position {
        get {
            if (_position == null)
            {
                _position = transform.position;
            }
            return (Vector3) _position + perObjectOffset.ToXY();
        }

        set {
            _position = value;
            transform.position = (Vector3) value + perObjectOffset.ToXY();
        }
    }

    Vector3? _position = null;

    public void UpdateOffset()
    {
        if (_position == null)
        {
            _position = transform.position;
        }
        position = (Vector3) _position;
    }

    #if UNITY_EDITOR
    void OnValidate()
    {
        UpdateOffset();
    }
    #endif
}
