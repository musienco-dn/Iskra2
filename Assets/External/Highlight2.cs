using UnityEngine;
using System.Collections;
public class Highlight2 : MonoBehaviour
{
    private GameObject mSelectedObject;
    public Material SimpleMat;
    public Material HighlightedMat;
    public GameObject SelectedObject
    {
        get
        {
            return mSelectedObject;
        }
        set
        {
            // get old game object
            GameObject goOld = mSelectedObject;     // assign new game object
            mSelectedObject = value;
            // if this object is the same - just not process this
            if (goOld == mSelectedObject)
            {
                return;
            }
            // set material to non-selected object
            if (goOld != null)
            {
                goOld.GetComponent<Renderer>().material = SimpleMat;
            }
            // set material to selected object
            if (mSelectedObject != null)
            {
                mSelectedObject.GetComponent<Renderer>().material = HighlightedMat;
            }
        }
    }
    public void Update()
    {
        SelectObjectByMousePos();
    }
    void SelectObjectByMousePos()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            GameObject rayCastedGO = hit.collider.gameObject;
            // select object
            this.SelectedObject = rayCastedGO;
        }
        else
            this.SelectedObject = null;
    }
}