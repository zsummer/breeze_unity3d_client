using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using Proto4z;


public class TouchPanel : MonoBehaviour
{

    UnityEngine.EventSystems.EventSystem _event;
    Camera _mainCamera;

    public Image strick;
    Vector3 _originStrick;
    bool _isStrick = false;

    EntityModel _control;

    void Start ()
    {
        _event = UnityEngine.EventSystems.EventSystem.current;
        Facade.GetSingleton<Dispatcher>().AddListener("ChangeModeIDResp", (Action<ChangeModeIDResp>)OnChangeModeIDResp);
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
    void ChangeAvatarModel()
    {
        Facade.GetSingleton<ServerProxy>().Send<ChangeModeIDReq>(new ChangeModeIDReq(Facade.GetSingleton<ModelDict>().GetNextModelID(Facade._avatarInfo.modeID)));
    }
    void AvatarAttack()
    {
        _control.CrossAttack();
    }
    void OnChangeModeIDResp(ChangeModeIDResp resp)
    {
        if (resp.retCode == (ushort)ERROR_CODE.EC_SUCCESS)
        {
            //Facade._gameScene.CreateEntityByAvatarID(Facade._avatarInfo.avatarID);
        }
    }
    void CheckStrick(Vector3 position)
    {
        var dis = Vector3.Distance(position, _originStrick);
        if (dis < 0.1f)
        {
            _control._moveType = MoveType.MT_IDLE;
            return;
        }
        if (dis > 40)
        {
            position = _originStrick + (position - _originStrick) * (40 / dis);
        }
        var pos = (position - _originStrick) / 5;
        pos.z = pos.y;
        pos.y = 0;
        _control._targetPos = pos;
        _control._moveType = MoveType.MT_HANDLE;
        strick.transform.position = position;
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Facade._entityID == 0)
        {
            return;
        }
        if (_control != null && _control._eid != Facade._entityID)
        {
            _control = null;
        }
        if (_control == null)
        {
            _control = Facade._gameScene.GetEntity(Facade._entityID).model;
            foreach(Camera camera in Camera.allCameras)
            {
                if (camera.name == "SceneCamera")
                {
                    _mainCamera = camera;
                    break;
                }
            }
        }
            

        if (true)
        {
            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");
            if (Math.Abs(h) > 0.1 || Math.Abs(v) > 0.1)
            {
                _control._targetPos = new Vector3(h, 0, v) * 10;
                _control._moveType = MoveType.MT_HANDLE;
            }
            if (_control._moveType == MoveType.MT_HANDLE && Math.Abs(h) < 0.1 && Math.Abs(v) < 0.1)
            {
                _control._moveType = MoveType.MT_IDLE;
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
                    Debug.Log(hit3D.transform.gameObject.name + _control._targetPos);
                    _control._targetPos = hit3D.point;
                    _control._moveType = MoveType.MT_TARGET;
                }
            }
            else if (RectTransformUtility.RectangleContainsScreenPoint((transform as RectTransform), new Vector2(Input.mousePosition.x, Input.mousePosition.y)))
            {
                BeginStrick(Input.mousePosition);
            }
        }
        if (Input.GetMouseButtonUp(1))
        {
            ChangeAvatarModel();
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
