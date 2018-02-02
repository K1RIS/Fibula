using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Utility
{
    static public Vector3 convertMousePositionToVector3(string layer)
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100, LayerMask.GetMask(layer)))
        {
            return new Vector3((int)(hit.point.x) + 0.5f, hit.point.y + 0.5f, (int)(hit.point.z) + 0.5f);
        }
        return Vector3.zero;
    }

    static public bool canvasClicked()
    {
        GameObject hit = raycastThatDetectsUI();
        if (hit == null || hit.tag == "IgnoreThisCanvas")
        {
            return false;
        }
        return true;
    }

    static public GameObject raycastThatDetectsUI()
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current);
        pointerData.position = Input.mousePosition;
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        if (results.Count == 0)
        {
            return null;
        }
        return results[0].gameObject;
    }
}