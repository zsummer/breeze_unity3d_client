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



    private float _npcHeight;
    private AnimationState _free;
    private AnimationState _runned;
    private AnimationState _attack;
    private Animation _anim;

    
    private float _lastLastStep = 0.0f;
    private float _lastStep = 0.0f;

    private EntityModel _mainPlayer;

    void Start ()
    {
        _anim = GetComponent<Animation>();
        _free = _anim["free"];
        _runned = _anim["walk"];
        _attack = _anim["attack"];
        _free.wrapMode = WrapMode.Loop;
        _runned.wrapMode = WrapMode.Loop;
        _npcHeight = GetComponent<CapsuleCollider>().bounds.size.y * transform.localScale.y;
    }

    public void CrossAttack()
    {
        _anim.CrossFade(_attack.name);
    }
    void OnGUI()
    {
        Vector3 worldPosition = new Vector3 (transform.position.x , transform.position.y + _npcHeight,transform.position.z);
        Vector2 position = Camera.main.WorldToScreenPoint (worldPosition);
        position = new Vector2 (position.x, Screen.height - position.y);

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
        GUI.Label(new Rect(position.x - (nameSize.x/2),position.y - nameSize.y, nameSize.x,nameSize.y), _info.baseInfo.avatarName, st);

    }
    void FixedUpdate()
    {
        //check main player 
        if (Facade._entityID != 0 && (_mainPlayer == null || _mainPlayer._info.entityInfo.eid != Facade._entityID ))
        {
            _mainPlayer = Facade._gameScene.GetEntity(Facade._entityID);
        }


        if (_info.entityMove.action == (ushort)Proto4z.MoveAction.MOVE_ACTION_IDLE 
            && (_anim.IsPlaying(_runned.name) || !_anim.isPlaying)) 
		{
            _anim.CrossFade(_free.name, 0.2f);
		}

        var nextPos = new Vector3((float)_info.entityMove.pos.x, transform.position.y, (float)_info.entityMove.pos.y);
        var endPos = nextPos;
        if (_info.entityMove.action != (ushort)Proto4z.MoveAction.MOVE_ACTION_IDLE)
        {
            if (_info.entityMove.waypoints.Count > 0)
            {
                endPos.x = (float)_info.entityMove.waypoints[0].x;
                endPos.z = (float)_info.entityMove.waypoints[0].y;
            }
        }

        

        var nextDist = Vector3.Distance(transform.position, nextPos);
        var endDist = Vector3.Distance(transform.position, endPos);
        if (endDist < 0.1f) 
        {
            _lastStep = 0.0f;
            _lastLastStep  = 0.0f;
            return;
        }
        //fix speed. 0.1 is server frame interval 
        var curStep = nextDist / GameOption._ServerFrameInterval * Time.fixedDeltaTime;
        curStep = (curStep + _lastStep + _lastLastStep) / 3.0f;
        var expectStep = (float)_info.entityMove.speed * Time.fixedDeltaTime;
        if (curStep < expectStep *0.8f)
        {
            curStep = expectStep * 0.8f;
        }
        else if (curStep > expectStep * 1.2f)
        {
            curStep = expectStep * 1.2f;
        }
        _lastLastStep = _lastStep;
        _lastStep = curStep;


        Debug.DrawLine(transform.position + transform.up * 0.3f, nextPos+transform.up* 0.3f, Color.red, 1.2f);
        Debug.DrawLine(transform.position + transform.up * 0.3f, endPos+transform.up* 0.3f, Color.blue, 1.2f);
        Debug.DrawLine(transform.position + transform.up * 0.3f, transform.forward*10+ transform.position + transform.up * 0.3f, Color.yellow, 1.2f);

        var dir = endPos - transform.position;

        if (_info.entityMove.action == (ushort)Proto4z.MoveAction.MOVE_ACTION_FOLLOW
            ||_info.entityMove.action == (ushort)Proto4z.MoveAction.MOVE_ACTION_PATH)
        {
            if (_anim.IsPlaying(_free.name) || !_anim.isPlaying)
            {
                _anim.CrossFade(_runned.name, 0.2f);
            }
            var euler = Vector3.Angle(dir, Vector3.forward);
            if (dir.x < 0f)
            {
                euler = 360f - euler;
            }
            var targetRotation = Quaternion.Euler(0, euler, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5);
        }


        dir = nextPos - transform.position;
        if (curStep < 0.001 || nextDist < 0.001 )
        {
            return;
        }
 //       Debug.Log("local x=" + transform.position.x + ", z=" + transform.position.z + ", nextPos x=" + nextPos.x + ", z=" + nextPos.z + ", mdist=" + curStep + ", nextDist=" + nextDist);
        transform.position += dir * (curStep / nextDist);
    }
	// Update is called once per frame
	void Update ()
    {
		
	}
}
