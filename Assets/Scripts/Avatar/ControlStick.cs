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

    public Image strick;
    Vector3 _originStrick;
    bool _isStrick = false;

    void Start ()
    {
        _mainCamera = Camera.main;
        _event = UnityEngine.EventSystems.EventSystem.current;
        instance = this;
	}
    void BeginStrick(Vector3 position)
    {
        _isStrick = true;
        _originStrick = position;
        strick.gameObject.SetActive(true);
        strick.transform.position = position;
    }
    void EndStrick()
    {
        _isStrick = false;
        strick.gameObject.SetActive(false);
    }
    void CheckStrick(Vector3 position)
    {
        var dis = Vector3.Distance(position, _originStrick);
        if (dis < 0.1f)
        {
            _moveType = MoveType.MT_IDLE;
            return;
        }
        if (dis > 40)
        {
            position = _originStrick + (position - _originStrick) * (40 / dis);
        }
        _targetPos = (position - _originStrick)/5;
        _targetPos.z = _targetPos.y;
        _targetPos.y = 0;
        _moveType = MoveType.MT_HANDLE;
        strick.transform.position = position;
        
    }

    // Update is called once per frame
    void Update ()
    {
        if (Facade.AvatarMode == null || Facade.AvatarInfo == null)
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
            else if (true)
            {
                BeginStrick(Input.mousePosition);
            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            EndStrick();
        }
        if (_isStrick)
        {
            CheckStrick(Input.mousePosition);
        }
    }
}
