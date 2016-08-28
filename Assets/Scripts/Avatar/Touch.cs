using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Touch : MonoBehaviour {

    RaycastHit _hit3D = new RaycastHit();
    Camera _mainCamera;
    void Start ()
    {
        _mainCamera = Camera.main;

	}

	void Update ()
    {
        Check2DTouch();
        //Check3DTouch();
	}





    void Check3DTouch()
    {
        Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
        Physics.Raycast(ray, out _hit3D, 100);
        if (_hit3D.transform != null)
        {
            Debug.Log(_hit3D.point + _hit3D.transform.gameObject.name);
        }
    }
    void Check2DTouch()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
            Physics.Raycast(ray, out _hit3D, 100);
            if (_hit3D.transform != null)
            {
                Debug.Log(_hit3D.point + _hit3D.transform.gameObject.name);
            }
            Debug.Log("" + Input.touchCount + Input.mousePosition + _hit3D.point + _hit3D.transform.gameObject.name);
        }
    }

}
