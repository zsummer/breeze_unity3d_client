using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

public enum MoveType
{
    MT_IDLE,
    MT_HANDLE,
    MT_TARGET,
}


public class ControlStick : MonoBehaviour
{
    public Vector3 targetPos { get { return _targetPos; } }
    public MoveType moveType { get { return _moveType; } set { _moveType = value; } }

    public static ControlStick instance;

    UnityEngine.EventSystems.EventSystem _event;
    MoveType _moveType = MoveType.MT_IDLE;
    Vector3 _targetPos; // 
    Camera _mainCamera;

    public Image image1;
    public Image image2;
    bool isPress = false;

    void Start ()
    {
        _mainCamera = Camera.main;
        _event = UnityEngine.EventSystems.EventSystem.current;
        instance = this;
        Init();
	}
    void Init()
    {
        image2.transform.position = image1.transform.position;
        var color = image1.color;
        color.a = 10;
        image1.color = color;
        image2.color = image1.color;
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (Facade._avatarID == 0)
        {
            if (_moveType != MoveType.MT_IDLE)
            {
                _moveType = MoveType.MT_IDLE;
            }
            return;
        }

        if (true)
        {
            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");
            if (Math.Abs(h) > 0.1 || Math.Abs(v) > 0.1)
            {
                _targetPos = new Vector3(h, 0, v) * 10;
                _moveType = MoveType.MT_HANDLE;
            }
            if (_moveType == MoveType.MT_HANDLE && Math.Abs(h) < 0.1 && Math.Abs(v) < 0.1)
            {
                _moveType = MoveType.MT_IDLE;
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (!_event.IsPointerOverGameObject())
            {
                Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit3D = new RaycastHit();
                Physics.Raycast(ray, out hit3D, 100);
                if (hit3D.transform != null && hit3D.transform.name == "Terrain")
                {
                    Debug.Log(hit3D.transform.gameObject.name);
                    _targetPos = hit3D.point;
                    _moveType = MoveType.MT_TARGET;
                }
            }
            else
            {

            }
        }

    }
}
