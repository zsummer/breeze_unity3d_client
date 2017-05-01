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

public class Deviation
{
    System.Collections.Generic.List<float> _bucket = new System.Collections.Generic.List<float>();
    public void pushValue(float val)
    {
        _bucket.Add(val);
    }
    public void cleanBucket()
    {
        _bucket.Clear();
    }
    public string OutDeviationData(ref float threshold)
    {
        float avg = 0; //基线
        float va = 0; //方差
        float sd = 0; //标准差
        float md = 0; //平均差
        float maxd = float.MinValue;
        float mind = float.MaxValue;

        foreach (var val in _bucket)
        {
            avg += val;
            if (val > maxd)
            {
                maxd = val;
            }
            if (val < mind)
            {
                mind = val;
            }
        }
        avg = avg / (float)_bucket.Count;

        foreach (var val in _bucket)
        {
            va += (float)Math.Pow(avg - val, 2.0);
        }
        va = va / (float)_bucket.Count;

        sd = (float)Math.Sqrt(va);

        foreach (var val in _bucket)
        {
            md += Math.Abs(avg - val);
        }
        md = md / (float)_bucket.Count;
        if (md / avg > threshold)
        {
            threshold = md / avg;
        }

        string ret = "[";
        ret += "count:" + _bucket.Count + ", ";
        ret += "avg:" + avg.ToString(".####") + ", ";
        ret += "mind:" + mind.ToString(".####") + ", ";
        ret += "maxd:" + maxd.ToString(".####") + ", ";
        //ret += "va:" + va.ToString(".####") + ":" + (va / avg).ToString("P") + ", ";
        //ret += "sd:" + sd.ToString(".####") + ":" + (sd / avg).ToString("P") + ", ";
        ret += "md:" + md.ToString(".####") + ":" + (md / avg).ToString("P");
        ret += "]";
        return ret;
    }
}

public class SyncCheck
{
    float _beginTime;


    float _sumFixedDeltaTime;
    float _sumSmoothTime;
    float _sumDeltaTime;
    float _sumSyncMoveCount;

    Deviation _devFixedDeltaTime = new Deviation();
    Deviation _devSmoothTime = new Deviation();
    Deviation _devDeltaTime = new Deviation();
    Deviation _devPassTime = new Deviation();
    Deviation _devSyncMoveTime = new Deviation();
    Deviation _devRealMoveTime = new Deviation();

    float _lastUpdateTime;
    float _lastSyncTime;
    float _lastShowTime;
    float _expectSpeed;
    public void Init()
    {
        _beginTime = Time.realtimeSinceStartup;

        _sumFixedDeltaTime = 0;
        _sumSmoothTime = 0;
        _sumDeltaTime = 0;
        _sumSyncMoveCount = 0;
        _expectSpeed = 0;
        _lastUpdateTime = _beginTime;
        _lastShowTime = _beginTime;
        _lastSyncTime = _beginTime;
    }
    public void whenSync(float realTime, float expectSpeed)
    {
        float now = Time.realtimeSinceStartup;
        _sumSyncMoveCount++;
        _devSyncMoveTime.pushValue(now - _lastSyncTime);
        _devRealMoveTime.pushValue(realTime);
        _expectSpeed = expectSpeed;
        _lastSyncTime = now;
    }
    public void FixedUpdate(ulong eid)
    {
        float now = Time.realtimeSinceStartup;
        _sumFixedDeltaTime += Time.fixedDeltaTime;
        _sumSmoothTime += Time.smoothDeltaTime;
        _sumDeltaTime += Time.deltaTime;

        _devFixedDeltaTime.pushValue(Time.fixedDeltaTime);
        _devSmoothTime.pushValue(Time.smoothDeltaTime);
        _devDeltaTime.pushValue(Time.deltaTime);
        _devPassTime.pushValue(now - _lastUpdateTime);
        

        _lastUpdateTime = now;
        if (now - _lastShowTime > 2)
        {
            _lastShowTime = now;

            float threshold = 0.0f;
            string showDev = "passTime=" + (now - _beginTime).ToString(".####");
            showDev += ", _sumFixedDeltaTime=" + _sumFixedDeltaTime.ToString(".####");
            showDev += ", _sumSmoothTime=" + _sumSmoothTime.ToString(".####");
            showDev += ", _sumDeltaTime=" + _sumDeltaTime.ToString(".####");
            showDev += ", _sumSyncTime=" + (_sumSyncMoveCount * 100.0f).ToString(".####");
//             showDev += ", _devFixedDeltaTime=" + _devFixedDeltaTime.OutDeviationData(ref threshold);
//             _devFixedDeltaTime.cleanBucket();
//             showDev += ", _devSmoothTime=" + _devSmoothTime.OutDeviationData(ref threshold);
//             _devSmoothTime.cleanBucket();
//             showDev += ", _devDeltaTime=" + _devDeltaTime.OutDeviationData(ref threshold);
//             _devDeltaTime.cleanBucket();
            showDev += ", _devPassTime=" + _devPassTime.OutDeviationData(ref threshold);
            _devPassTime.cleanBucket();
            showDev += ", _devSyncMoveTime=" + _devSyncMoveTime.OutDeviationData(ref threshold);
            _devSyncMoveTime.cleanBucket();
            showDev += ", _expectSpeed=" + _expectSpeed.ToString(".####");
            showDev += ", _devRealMoveTime=" + _devRealMoveTime.OutDeviationData(ref threshold);
            _devRealMoveTime.cleanBucket();

            if (threshold > 1.2)
            {
                Debug.LogError("eid:" + eid + " -> " + showDev);
            }
            else if (threshold > 0.6)
            {
                Debug.LogWarning("eid:" + eid + " -> " + showDev);
            }
//             else
//             {
//                 Debug.Log("eid:" + eid + " -> " + showDev);
//             }
//             
        }
    }
}

public class EntityModel : MonoBehaviour
{
    public Proto4z.EntityFullData _info;
    public float _modelHeight = 2;
    public Transform _model = null;
    private AnimationState _free;
    private AnimationState _runned;
    private AnimationState _attack;
    private AnimationState _death;
    private Animation _anim;



    private float _startMoveTime = 0;
    private Vector3 _startMovePosition;

    private Camera _mainCamera;

    private EntityModel _mainPlayer;


   private SyncCheck _check = new SyncCheck();
    void Start()
    {
        _anim = _model.GetComponent<Animation>();
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
        _check.Init();
    }
    public void RefreshMoveInfo(Proto4z.EntityMove mv)
    {
        _startMovePosition = transform.position;
        _startMoveTime = Time.realtimeSinceStartup;
        _info.mv = mv;
        if (Facade._entityID == _info.state.eid)
        {
            _check.whenSync((float)_info.mv.realSpeed, (float)_info.mv.expectSpeed);
        }
        
    }


    public IEnumerator CreateEffect(string path, Vector3 offset,  float delay, float keep)
    {
        yield return new WaitForSeconds(delay);

        var attack = Instantiate(Resources.Load<GameObject>(path));
        attack.transform.SetParent(gameObject.transform, false);
        attack.transform.localPosition += offset;
        var childs = attack.GetComponentsInChildren<Transform>();
        DestroyObject(attack.gameObject, keep);
    }

    public void PlayAttack()
    {
        _anim.CrossFade(_attack.name);
        StartCoroutine(CreateEffect("Effect/skill/attack", Vector3.forward, 0.1f, 2f));
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
        target.y += 0.2f;
        src.y += 0.2f;
        Debug.DrawLine(src, target, Color.yellow, 1.0f);

        Facade._serverProxy.SendToScene(new UseSkillReq(Facade._entityID, 1, new EPosition(target.x, target.z), 1));
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

            worldPosition = new Vector3((float)_info.mv.position.x, transform.position.y, (float)_info.mv.position.y);
            
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

        if (_info.state.etype == (ushort)Proto4z.ENTITY_TYPE.ENTITY_PLAYER)
        {
            if (Facade._entityID == _info.state.eid)
            {
                st.normal.textColor = Color.yellow;
            }
            else if (_mainPlayer != null && _mainPlayer._info.state.camp != _info.state.camp)
            {
                st.normal.textColor = Color.red;
            }
            text = _info.state.avatarName;
            textSize = GUI.skin.label.CalcSize(new GUIContent(text)) * st.fontSize / GUI.skin.font.fontSize;
            GUI.Label(new Rect(position.x - (textSize.x / 2), position.y - textSize.y, textSize.x, textSize.y), text, st);
        }

        int hpGridCount = 10;
        int curHpGrid = (int)(_info.state.curHP /_info.props.hp * hpGridCount);
        if (curHpGrid > hpGridCount)
        {
            curHpGrid = hpGridCount;
        }
        //text = _info.info.curHP.ToString();
        //text += ":";
        text = "hp:";
        for (int i = 0; i < curHpGrid; i++)
        {
            text += "■";
        }
        for (int i = 0; i < hpGridCount - curHpGrid; i++)
        {
            text += "□";
        }
        st.normal.textColor = Color.red;
        st.fontSize = st.fontSize * 7 /10;
        if (Facade._entityID == _info.state.eid)
        {
            st.normal.textColor = Color.yellow;
        }
        
        textSize = GUI.skin.label.CalcSize(new GUIContent(text)) * st.fontSize / GUI.skin.font.fontSize;
        GUI.Label(new Rect(position.x - (textSize.x / 2), position.y - textSize.y - textSize.y, textSize.x, textSize.y), text, st);
    }
    void FixedUpdate()
    {
        //check main player 
        if (Facade._entityID != 0 && (_mainPlayer == null || _mainPlayer._info.state.eid != Facade._entityID))
        {
            _mainPlayer = Facade._sceneManager.GetEntity(Facade._entityID);
        }

        if (Facade._entityID == _info.state.eid)
        {
            _check.FixedUpdate(_info.state.eid);
        }

        if (_info.state.state == (ushort) Proto4z.ENTITY_STATE.ENTITY_STATE_ACTIVE 
            &&_info.mv.action == (ushort)Proto4z.MOVE_ACTION.MOVE_ACTION_IDLE
            && (_anim.IsPlaying(_runned.name) || (_anim.clip.name != _death.name  &&!_anim.isPlaying)))
        {
            PlayFree();
        }

        Vector3 serverPosition = new Vector3((float)_info.mv.position.x, transform.position.y, (float)_info.mv.position.y);
        if (_info.mv.action == (ushort)Proto4z.MOVE_ACTION.MOVE_ACTION_IDLE)
        {
            if (Vector3.Distance(transform.position, serverPosition) < 0.1f)
            {
                return;
            }
            else
            {
                _info.mv.realSpeed = _info.mv.expectSpeed = 10.0f;
            }
        }


        Vector3 serverWaypointPosition = serverPosition;
        if (_info.mv.waypoints.Count > 0)
        {
            serverWaypointPosition = new Vector3((float)_info.mv.waypoints[0].x, transform.position.y, (float)_info.mv.waypoints[0].y);
        }
        
        if (_info.state.state != (ushort)Proto4z.ENTITY_STATE.ENTITY_STATE_ACTIVE)
        {
            transform.position = serverPosition;
            return;
        }


        if (Vector3.Distance(transform.position, serverPosition) < 0.2 * (float)_info.mv.expectSpeed && _info.mv.realSpeed / _info.mv.expectSpeed > 0.9)
        {
            transform.position = Vector3.MoveTowards(transform.position, serverWaypointPosition, (float)_info.mv.realSpeed * Time.fixedDeltaTime);
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, serverPosition, (float)_info.mv.realSpeed * Time.fixedDeltaTime);
        }



        if (_info.state.state == (ushort)Proto4z.ENTITY_STATE.ENTITY_STATE_ACTIVE
            && (_info.mv.action == (ushort)Proto4z.MOVE_ACTION.MOVE_ACTION_FOLLOW
            || _info.mv.action == (ushort)Proto4z.MOVE_ACTION.MOVE_ACTION_PATH))
        {
            if (_anim.IsPlaying(_free.name) || !_anim.isPlaying)
            {
                _anim.CrossFade(_runned.name, 0.2f);
            }
            if (_info.mv.waypoints.Count > 0)
            {
                var face = new Vector3((float)_info.mv.waypoints[0].x, 0, (float)_info.mv.waypoints[0].y) - transform.position;
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
