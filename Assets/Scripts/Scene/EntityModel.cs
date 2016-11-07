using UnityEngine;
using System.Collections;
using System;
using Proto4z;
public enum MoveType
{
    MT_IDLE,
    MT_HANDLE,
    MT_TARGET,
}


public class EntityModel : MonoBehaviour
{
    public Proto4z.EntityFullData _info;
    private float _modelHeight = 5;
    private AnimationState _free;
    private AnimationState _runned;
    private AnimationState _attack;
    private AnimationState _death;
    private Animation _anim;



    private float _startMoveTime = 0;
    private Vector3 _startMovePosition;
    private bool _prediction = false;
    private Camera _mainCamera;

    private EntityModel _mainPlayer;

    void Start()
    {
        _anim = GetComponent<Animation>();
        _free = _anim["free"];
        _free.wrapMode = WrapMode.Loop;

        _runned = _anim["walk"];
        _runned.wrapMode = WrapMode.Loop;

        _attack = _anim["attack"];
        _death = _anim["death"];
        
        
        _startMoveTime = Time.realtimeSinceStartup;
        _startMovePosition = transform.position;

        foreach (Camera camera in Camera.allCameras)
        {
            if (camera.name == "SceneCamera")
            {
                _mainCamera = camera;
                break;
            }
        }
        if (_mainCamera == null)
        {
            //_mainCamera = Camera.main;
        }
    }
    public void RefreshMoveInfo(Proto4z.EntityMove mv)
    {
        _startMovePosition = transform.position;
        _startMoveTime = Time.realtimeSinceStartup;
        _prediction = false;
        float magnitude = 0;
        if (mv.waypoints.Count > 0 &&  Math.Abs(mv.realSpeed - mv.expectSpeed) < 1.0f)
        {
            var server = new Vector3((float)mv.position.x, 0, (float)mv.position.y);
            var dst = new Vector3((float)mv.waypoints[0].x, 0, (float)mv.waypoints[0].y);
            var last = new Vector3((float)_info.entityMove.position.x, 0, (float)_info.entityMove.position.y);
            server = Vector3.Normalize(server - last);
            dst = Vector3.Normalize(dst - last);
            last = server - dst;
            magnitude = last.magnitude;
            if (last.magnitude < 0.01)
            {
                _prediction = true;
            }
        }
        if (!_prediction)
        {
            Debug.LogWarning("move prediction false. last frame magnitude=" + magnitude);
        }


        _info.entityMove = mv;
    }
    public void PlayAttack()
    {
        _anim.CrossFade(_attack.name);
    }
    public void PlayDeath()
    {
        _anim.CrossFade(_death.name);
    }
    public void PlayFree()
    {
        _anim.CrossFade(_free.name);
    }
    public void DoAttack()
    {
        float a = transform.rotation.eulerAngles.y;
        Vector3 dir = transform.rotation * Vector3.forward ;
        Vector3 src = transform.position - dir * 1.0f;
        Vector3 target = transform.position + dir * 7.0f;
        target.y += 2f;
        src.y += 2f;
        Debug.DrawLine(src, target, Color.yellow, 1.0f);

        Facade._serverProxy.SendToScene(new UseSkillReq(Facade._entityID, 1, 0, new EPosition(target.x, target.z)));
    }
    void OnGUI()
    {

        Vector3 worldPosition = new Vector3(transform.position.x, transform.position.y + _modelHeight, transform.position.z);
        Vector2 position = Camera.main.WorldToScreenPoint(worldPosition);
        if (true)
        {
            worldPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z);
            position = _mainCamera.WorldToScreenPoint(worldPosition);
            position = new Vector2(position.x, Screen.height - position.y);
            GUI.Box(new Rect(position, new Vector2(20, 20)), "c");

            worldPosition = new Vector3((float)_info.entityMove.position.x, transform.position.y, (float)_info.entityMove.position.y);
            
            position = _mainCamera.WorldToScreenPoint(worldPosition);
            position = new Vector2(position.x, Screen.height - position.y);
            GUI.Box(new Rect(position, new Vector2(20, 20)), "s");
        }
        worldPosition = new Vector3(transform.position.x, transform.position.y + _modelHeight, transform.position.z);
        position = Camera.main.WorldToScreenPoint(worldPosition);
        position = new Vector2(position.x, Screen.height - position.y);

        GUIStyle st = new GUIStyle();
        st.normal.textColor = Color.red;
        st.normal.background = null;
        st.fontSize = (int)(Screen.height * GameOption._fontSizeScreeHeightRate);

        string text = "";
        Vector2 textSize = new Vector2();

        if (_info.entityInfo.etype == (ushort)Proto4z.EntityType.ENTITY_PLAYER)
        {
            if (Facade._entityID == _info.entityInfo.eid)
            {
                st.normal.textColor = Color.yellow;
            }
            else if (_mainPlayer != null && _mainPlayer._info.entityInfo.camp != _info.entityInfo.camp)
            {
                st.normal.textColor = Color.red;
            }
            text = _info.baseInfo.avatarName;
            textSize = GUI.skin.label.CalcSize(new GUIContent(text)) * st.fontSize / GUI.skin.font.fontSize;
            GUI.Label(new Rect(position.x - (textSize.x / 2), position.y - textSize.y, textSize.x, textSize.y), text, st);
        }
        
        int curHP = (int)(_info.entityInfo.curHP /100.0f *5);
        if (curHP > 5)
        {
            curHP = 5;
        }
        text = _info.entityInfo.curHP.ToString();
        text += ":";

        for (int i = 0; i < curHP; i++)
        {
            text += "■";
        }
        for (int i = 0; i < 5 - curHP; i++)
        {
            text += "□";
        }
        st.normal.textColor = Color.red;
        if (Facade._entityID == _info.entityInfo.eid)
        {
            st.normal.textColor = Color.yellow;
        }
        
        textSize = GUI.skin.label.CalcSize(new GUIContent(text)) * st.fontSize / GUI.skin.font.fontSize;
        GUI.Label(new Rect(position.x - (textSize.x / 2), position.y - textSize.y - textSize.y, textSize.x, textSize.y), text, st);
    }
    void FixedUpdate()
    {
        //check main player 
        if (Facade._entityID != 0 && (_mainPlayer == null || _mainPlayer._info.entityInfo.eid != Facade._entityID))
        {
            _mainPlayer = Facade._sceneManager.GetEntity(Facade._entityID);
        }

        if (_info.entityInfo.state == (ushort) Proto4z.EntityState.ENTITY_STATE_ACTIVE 
            &&_info.entityMove.action == (ushort)Proto4z.MoveAction.MOVE_ACTION_IDLE
            && (_anim.IsPlaying(_runned.name) || (_anim.clip.name != _death.name  &&!_anim.isPlaying)))
        {
            PlayFree();
        }
        Vector3 serverPosition = new Vector3((float)_info.entityMove.position.x, transform.position.y, (float)_info.entityMove.position.y);
        var dist = Vector3.Distance(transform.position, serverPosition);
        if (dist < 0.1f && _info.entityMove.action == (ushort)Proto4z.MoveAction.MOVE_ACTION_IDLE)
        {
            return;
        }
        if (_info.entityInfo.state != (ushort)Proto4z.EntityState.ENTITY_STATE_ACTIVE)
        {
            transform.position = serverPosition;
            return;
        }
        var old = transform.position;
        if (_prediction &&  (serverPosition - transform.position).magnitude < GameOption._CompensationSpeed)
        {
            transform.position = Vector3.MoveTowards(transform.position, serverPosition, (float)_info.entityMove.expectSpeed * Time.fixedDeltaTime);
        }
        else
        {
            transform.position = Vector3.Lerp(_startMovePosition, serverPosition, (Time.realtimeSinceStartup - _startMoveTime) / GameOption._ServerFrameInterval);
        }



        if (_info.entityInfo.state == (ushort)Proto4z.EntityState.ENTITY_STATE_ACTIVE
            && (_info.entityMove.action == (ushort)Proto4z.MoveAction.MOVE_ACTION_FOLLOW
            || _info.entityMove.action == (ushort)Proto4z.MoveAction.MOVE_ACTION_PATH))
        {
            if (_anim.IsPlaying(_free.name) || !_anim.isPlaying)
            {
                _anim.CrossFade(_runned.name, 0.2f);
            }
            if (_info.entityMove.waypoints.Count > 0)
            {
                var face = new Vector3((float)_info.entityMove.waypoints[0].x, 0, (float)_info.entityMove.waypoints[0].y) - transform.position;
                var euler = Vector3.Angle(face, Vector3.forward);
                if (face.x < 0f)
                {
                    euler = 360f - euler;
                }
                var targetRotation = Quaternion.Euler(0, euler, 0);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * 5);
            }

        }

    }

}
