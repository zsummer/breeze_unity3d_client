using UnityEngine;
using System.Collections;
using System;

public enum MoveType
{
    MT_IDLE,
    MT_HANDLE,
    MT_TARGET,
}


public class EntityModel : MonoBehaviour
{
    public Proto4z.EntityFullData _info;
    private float _modelHeight = 6;
    private AnimationState _free;
    private AnimationState _runned;
    private AnimationState _attack;
    private Animation _anim;




    private float _startMoveTime = 0;
    private float _lastFrameTime = 0;
    private Vector3 _startMovePosition;



    private EntityModel _mainPlayer;

    void Start()
    {
        _anim = GetComponent<Animation>();
        _free = _anim["free"];
        _runned = _anim["walk"];
        _attack = _anim["attack"];
        _free.wrapMode = WrapMode.Loop;
        _runned.wrapMode = WrapMode.Loop;
        _lastFrameTime = GameOption._ServerFrameInterval;
        _startMoveTime = Time.realtimeSinceStartup;
        _startMovePosition = transform.position;

    }
    public void RefreshMoveInfo(Proto4z.EntityMove mv)
    {
        _startMovePosition = transform.position;
        _lastFrameTime = Time.realtimeSinceStartup - _startMoveTime;
        _startMoveTime = Time.realtimeSinceStartup;
        _info.entityMove = mv;
    }
    public void CrossAttack()
    {
        _anim.CrossFade(_attack.name);
    }
    void OnGUI()
    {
        if (_info.entityInfo.etype != (ushort)Proto4z.EntityType.ENTITY_AVATAR)
        {
            return;
        }
        Vector3 worldPosition = new Vector3(transform.position.x, transform.position.y + _modelHeight, transform.position.z);
        Vector2 position = Camera.main.WorldToScreenPoint(worldPosition);
        position = new Vector2(position.x, Screen.height - position.y);

        GUIStyle st = new GUIStyle();
        st.normal.textColor = Color.red;
        st.normal.background = null;
        st.fontSize = (int)(Screen.height * GameOption._fontSizeScreeHeightRate);
        Vector2 nameSize = nameSize = GUI.skin.label.CalcSize(new GUIContent(_info.baseInfo.avatarName)) * st.fontSize / GUI.skin.font.fontSize;

        if (Facade._entityID == _info.entityInfo.eid)
        {
            st.normal.textColor = Color.yellow;
        }
        else if (_mainPlayer != null && _mainPlayer._info.entityInfo.camp != _info.entityInfo.camp)
        {
            st.normal.textColor = Color.red;
        }
        GUI.Label(new Rect(position.x - (nameSize.x / 2), position.y - nameSize.y, nameSize.x, nameSize.y), _info.baseInfo.avatarName, st);
    }
    void FixedUpdate()
    {
        //check main player 
        if (Facade._entityID != 0 && (_mainPlayer == null || _mainPlayer._info.entityInfo.eid != Facade._entityID))
        {
            _mainPlayer = Facade._sceneManager.GetEntity(Facade._entityID);
        }

        if (_info.entityMove.action == (ushort)Proto4z.MoveAction.MOVE_ACTION_IDLE
            && (_anim.IsPlaying(_runned.name) || !_anim.isPlaying))
        {
            _anim.CrossFade(_free.name, 0.2f);
        }
        Vector3 serverPosition = new Vector3((float)_info.entityMove.position.x, transform.position.y, (float)_info.entityMove.position.y);
        var dist = Vector3.Distance(transform.position, serverPosition);
        if (dist < 0.1f && _info.entityMove.action == (ushort)Proto4z.MoveAction.MOVE_ACTION_IDLE)
        {
            return;
        }
        var old = transform.position; 
        transform.position = Vector3.Lerp(_startMovePosition, serverPosition, (Time.realtimeSinceStartup - _startMoveTime)/_lastFrameTime * 0.6f );

        /*
        Debug.LogWarning("move[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "]eid=" + _info.entityMove.eid
+ ", action=" + _info.entityMove.action + ", addtime=" + (Time.realtimeSinceStartup - _startMoveTime) + ", lastframeTime=" + _lastFrameTime
+ ", local=" + old.x + ":" + old.z
+ ", new=" + transform.position.x + ":" + transform.position.z
+ ", remote=" + serverPosition.x + ":" + serverPosition.y);
        */

        //Debug.DrawLine(transform.position + transform.up * 0.3f, nextPos + transform.up * 0.3f, Color.red, 1.2f);
        //Debug.DrawLine(transform.position + transform.up * 0.3f, endPos + transform.up * 0.3f, Color.blue, 1.2f);
        //Debug.DrawLine(transform.position + transform.up * 0.3f, transform.forward * 10 + transform.position + transform.up * 0.3f, Color.yellow, 1.2f);

 
        if (_info.entityMove.action == (ushort)Proto4z.MoveAction.MOVE_ACTION_FOLLOW
            || _info.entityMove.action == (ushort)Proto4z.MoveAction.MOVE_ACTION_PATH)
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
