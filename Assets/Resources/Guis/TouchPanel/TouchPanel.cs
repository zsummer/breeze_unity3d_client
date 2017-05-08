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
    Vector3 _lastDirt;
    int _strickTouch = -1;
    EntityModel _control;
    float _lastSendMove = 0.0f;
    void Start ()
    {
        _event = UnityEngine.EventSystems.EventSystem.current;
    }
    void BeginStrick(Vector3 position, int touch)
    {
        _strickTouch = touch;
        _originStrick = position;
        strick.gameObject.SetActive(true);
        strick.transform.position = position;
    }
    void EndStrick()
    {
        if (_strickTouch >= 0)
        {
            _strickTouch = -1;
            strick.gameObject.SetActive(false);
            var req = new MoveReq();
            req.eid = Facade._entityID;
            req.action = (ushort)Proto4z.MOVE_ACTION.MOVE_ACTION_IDLE;
            req.clientPos = new Proto4z.EPosition(_control.transform.position.x, _control.transform.position.z);
			req.waypoints.Add (new EPosition (_control.transform.position.x, _control.transform.position.z));
            Facade._serverProxy.SendToScene(req);
            Debug.Log("client stop move EndStrick");
        }
    }



    void CheckStrick(int touchCount)
    {
        if (_strickTouch < 0)
        {
            return;
        }
        if (touchCount == 0 
            || Input.GetTouch(_strickTouch).phase == TouchPhase.Canceled
            || Input.GetTouch(_strickTouch).phase == TouchPhase.Ended)
        {
            EndStrick();
            return;
        }

        var dist = Vector3.Distance(Input.GetTouch(_strickTouch).position, _originStrick);
        if (dist < 0.3f)
        {
            return;
        }
        if (dist > Screen.width * GameOption._TouchRedius)
        {
            dist = Screen.width * GameOption._TouchRedius;
        }
        var dir = Vector3.Normalize((Vector3)Input.GetTouch(_strickTouch).position - _originStrick);
        strick.transform.position = _originStrick + (dir * dist);


        
        dir.z = dir.y;
        dir.y = 0;
        dir *= 20;
        if (_lastDirt == null || Vector3.Distance(dir, _lastDirt) > Math.Sin( 10.0 * Math.PI/360.0)* 20)
        {
            _lastDirt = dir;
        }
        else if (Time.realtimeSinceStartup - _lastSendMove < 0.2f)
        {
            return;
        }
        EntityModel player = Facade._sceneManager.GetEntity(Facade._entityID);
        if (player == null ||  !player.isCanMove())
        {
            if (player._info.mv.action != (ushort)MOVE_ACTION.MOVE_ACTION_IDLE)
            {
                var stopMove = new MoveReq();
                stopMove.eid = Facade._entityID;
                stopMove.action = (ushort)Proto4z.MOVE_ACTION.MOVE_ACTION_IDLE;
                stopMove.clientPos = new Proto4z.EPosition(_control.transform.position.x, _control.transform.position.z);
                Facade._serverProxy.SendToScene(stopMove);
            }
            return;
        }

        Vector3 dst = _control.transform.position + _lastDirt;
        _lastSendMove = Time.realtimeSinceStartup;

        //Debug.Log("Send Move");
        var req = new MoveReq();
        req.eid = Facade._entityID;
        req.action = (ushort)Proto4z.MOVE_ACTION.MOVE_ACTION_PATH;
        req.clientPos = new Proto4z.EPosition(_control.transform.position.x, _control.transform.position.z);
		req.waypoints.Add (new EPosition (dst.x, dst.z));
        Facade._serverProxy.SendToScene(req);

        //Debug.Log("used time=" + (Time.realtimeSinceStartup - _lastSendMove));
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Facade._entityID == 0)
        {
            return;
        }
        if (_control != null && _control._info.state.eid != Facade._entityID)
        {
            _control = null;
        }
        if (_control == null)
        {
            _control = Facade._sceneManager.GetEntity(Facade._entityID);
            foreach(Camera camera in Camera.allCameras)
            {
                if (camera.name == "SceneCamera")
                {
                    _mainCamera = camera;
                    break;
                }
            }
            _strickTouch = -1;
        }

        /*
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
                req.action = (ushort)Proto4z.MOVE_ACTION.MOVE_ACTION_PATH;
                req.dstPos.x += h * 10;
                req.dstPos.y += v * 10;
                Facade._serverProxy.SendToScene(req);
            }
            else if (_isHandle && _control._info.mv.action != (ushort) Proto4z.MOVE_ACTION.MOVE_ACTION_IDLE)
            {
                _isHandle = false;
                req.action = (ushort)Proto4z.MOVE_ACTION.MOVE_ACTION_IDLE;
                Facade._serverProxy.SendToScene(req);
            }
        }
        */

        int touchCount = Input.touchCount;
        if (_strickTouch < 0 && touchCount > 0)
        {
            for (int i = 0; i < touchCount; i++)
            {
                if(Input.GetTouch(i).phase == TouchPhase.Began 
                    && RectTransformUtility.RectangleContainsScreenPoint((transform as RectTransform), new Vector2(Input.GetTouch(i).position.x, Input.GetTouch(i).position.y)))
                {
                    BeginStrick(Input.GetTouch(i).position, i);
                    break;
                }
            }
        }
        if (_strickTouch >= 0 )
        {
            CheckStrick(touchCount);
        }

        if (Input.GetMouseButtonDown(0))
        {
            if(!_event.IsPointerOverGameObject())
            {
#if UNITY_EDITOR 

                Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit3D = new RaycastHit();
                Physics.Raycast(ray, out hit3D, 100);
                if (hit3D.transform != null && hit3D.transform.name == "Terrain")
                {
                    var req = new MoveReq();
                    req.eid = Facade._entityID;
                    req.action = (ushort)Proto4z.MOVE_ACTION.MOVE_ACTION_PATH;
                    req.clientPos = new Proto4z.EPosition(_control.transform.position.x, _control.transform.position.z);
					req.waypoints.Add (new EPosition (hit3D.point.x, hit3D.point.z));
                    Facade._serverProxy.SendToScene(req);
                }
#endif
            }

        }


    }
}
