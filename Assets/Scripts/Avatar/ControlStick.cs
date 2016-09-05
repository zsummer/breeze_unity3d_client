using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using Proto4z;


public class ControlStick : MonoBehaviour
{

    UnityEngine.EventSystems.EventSystem _event;
    Camera _mainCamera;

    public Image strick;
    Vector3 _originStrick;
    bool _isStrick = false;

    AvatarController _control;

    Transform _skillButtons = null;
    void Start ()
    {
        _mainCamera = Camera.main;
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
        Facade.GetSingleton<NetController>().Send<ChangeModeIDReq>(new ChangeModeIDReq(Facade.GetSingleton<ModelMgr>().GetNextModelID(Facade.AvatarInfo.modeID)));
    }
    void AvatarAttack()
    {
        _control.CrossAttack();
    }
    void OnChangeModeIDResp(ChangeModeIDResp resp)
    {
        if (resp.retCode == (ushort)ERROR_CODE.EC_SUCCESS)
        {
            Facade.CreateAvatar(resp.modeID);
        }
    }
    void CheckStrick(Vector3 position)
    {
        var dis = Vector3.Distance(position, _originStrick);
        if (dis < 0.1f)
        {
            _control.moveType = MoveType.MT_IDLE;
            return;
        }
        if (dis > 40)
        {
            position = _originStrick + (position - _originStrick) * (40 / dis);
        }
        var pos = (position - _originStrick) / 5;
        pos.z = pos.y;
        pos.y = 0;
        _control.targetPos = pos;
        _control.moveType = MoveType.MT_HANDLE;
        strick.transform.position = position;
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (_control == null && Facade.AvatarMode != null)
        {
            _control = Facade.AvatarMode.gameObject.GetComponent<AvatarController>();
        }
        if (_control == null)
        {
            return;
        }
        if (Facade.AvatarMode == null || Facade.AvatarInfo == null)
        {
            if (_control.moveType != MoveType.MT_IDLE)
            {
                _control.moveType = MoveType.MT_IDLE;
            }
            return;
        }
        if (_skillButtons == null)
        {
            var skillButtonsRes = Resources.Load<GameObject>("Guis/SkillButtons/SkillButtons");
            if (skillButtonsRes != null)
            {
                _skillButtons = Instantiate(skillButtonsRes).transform;
                _skillButtons.SetParent(GameObject.Find("UGUI").transform, false);
                _skillButtons.gameObject.SetActive(true);
                _skillButtons.Find("ChangeModelSkill").GetComponent<Button>().onClick.AddListener(delegate () { ChangeAvatarModel(); });
                _skillButtons.Find("AttackSkill").GetComponent<Button>().onClick.AddListener(delegate () { AvatarAttack(); });

            }
            else
            {
                Debug.LogError("can't Instantiate [Prefabs/Guis/SkillButtons/SkillButtons].");
            }
        }


        if (true)
        {
            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");
            if (Math.Abs(h) > 0.1 || Math.Abs(v) > 0.1)
            {
                _control.targetPos = new Vector3(h, 0, v) * 10;
                _control.moveType = MoveType.MT_HANDLE;
            }
            if (_control.moveType == MoveType.MT_HANDLE && Math.Abs(h) < 0.1 && Math.Abs(v) < 0.1)
            {
                _control.moveType = MoveType.MT_IDLE;
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
                    Debug.Log(hit3D.transform.gameObject.name + _control.targetPos);
                    _control.targetPos = hit3D.point;
                    _control.moveType = MoveType.MT_TARGET;
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
