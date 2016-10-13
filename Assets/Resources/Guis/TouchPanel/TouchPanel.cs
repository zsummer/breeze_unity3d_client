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
    bool _isHandle = false;
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
        if (_isStrick)
        {
            _isStrick = false;
            strick.gameObject.SetActive(false);
            var req = new MoveReq();
            req.eid = Facade._entityID;
            req.action = (ushort)Proto4z.MoveAction.MOVE_ACTION_IDLE;
            req.clientPos = new Proto4z.EPoint(_control.transform.position.x, _control.transform.position.z);
            req.dstPos.x = _control.transform.position.x;
            req.dstPos.y = _control.transform.position.z;
            Facade.GetSingleton<ServerProxy>().SendToScene(req);
        }
    }
    void ChangeAvatarModel()
    {
        Facade.GetSingleton<ServerProxy>().SendToGame<ChangeModeIDReq>(new ChangeModeIDReq(Facade.GetSingleton<ModelDict>().GetNextModelID(Facade._avatarInfo.modeID)));
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
            return;
        }
        if (dis > 40)
        {
            position = _originStrick + (position - _originStrick) * (40 / dis);
        }

        var pos = (position - _originStrick) / 5;
        pos.z = pos.y;
        pos.y = 0;
        strick.transform.position = position;

        var req = new MoveReq();
        req.eid = Facade._entityID;
        req.action = (ushort)Proto4z.MoveAction.MOVE_ACTION_PATH;
        req.clientPos = new Proto4z.EPoint(_control.transform.position.x, _control.transform.position.z);
        req.dstPos.x = _control.transform.position.x;
        req.dstPos.y = _control.transform.position.z;
        req.dstPos.x += pos.x;
        req.dstPos.y += pos.z;
        Facade.GetSingleton<ServerProxy>().SendToScene(req);

        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Facade._entityID == 0)
        {
            return;
        }
        if (_control != null && _control._info.entityInfo.eid != Facade._entityID)
        {
            _control = null;
        }
        if (_control == null)
        {
            _control = Facade._gameScene.GetEntity(Facade._entityID);
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
            var req = new MoveReq();
            req.eid = Facade._entityID;
            req.clientPos = new Proto4z.EPoint(_control.transform.position.x, _control.transform.position.z);
            req.dstPos.x = _control.transform.position.x;
            req.dstPos.y = _control.transform.position.z;

            if (Math.Abs(h) > 0.1 || Math.Abs(v) > 0.1)
            {
                _isHandle = true;
                req.action = (ushort)Proto4z.MoveAction.MOVE_ACTION_PATH;
                req.dstPos.x += h * 10;
                req.dstPos.y += v * 10;
                Facade.GetSingleton<ServerProxy>().SendToScene(req);
            }
            else if (_isHandle && _control._info.entityMove.action != (ushort) Proto4z.MoveAction.MOVE_ACTION_IDLE)
            {
                _isHandle = false;
                req.action = (ushort)Proto4z.MoveAction.MOVE_ACTION_IDLE;
                Facade.GetSingleton<ServerProxy>().SendToScene(req);
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
                    var req = new MoveReq();
                    req.eid = Facade._entityID;
                    req.action = (ushort)Proto4z.MoveAction.MOVE_ACTION_PATH;
                    req.clientPos = new Proto4z.EPoint(_control.transform.position.x, _control.transform.position.z);
                    req.dstPos.x = hit3D.point.x;
                    req.dstPos.y = hit3D.point.z;
                    Facade.GetSingleton<ServerProxy>().SendToScene(req);
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
