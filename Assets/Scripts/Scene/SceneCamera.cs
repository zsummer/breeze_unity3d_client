using UnityEngine;
using System.Collections;

public class SceneCamera : MonoBehaviour
{

    public EntityModel _target;
    public Vector3 _follow;
    public Vector3 _lastLerp;
    public Vector3 _lastLastLerp;

    void Start ()
    {

	}

     void Update()
     {
        if (_target == null && Facade._entityID != 0)
        {
            _target = Facade._sceneManager.GetEntity(Facade._entityID);
            _follow = _target.transform.position;
            _lastLerp = new Vector3(0,0,0);
            _lastLastLerp = _lastLerp;
        }
        if (_target == null)
        {
            return;
        }
        if (false)
        {
            transform.position = Vector3.Lerp(transform.position, _target.transform.position - Vector3.forward * 30.0f + Vector3.up * 30.0f, Time.deltaTime * 3.0f);
            transform.LookAt(_target.transform.position);
            return;
        }

        Vector3 curTarget = _target.transform.position;
        if (_target._info.entityMove.action != (ushort) Proto4z.MoveAction.MOVE_ACTION_IDLE && _target._info.entityMove.waypoints.Count > 0)
        {
            curTarget.x = (float)_target._info.entityMove.waypoints[0].x;
            curTarget.z = (float)_target._info.entityMove.waypoints[0].y;
        }
        else
        {
            curTarget.x = (float)_target._info.entityMove.pos.x;
            curTarget.z = (float)_target._info.entityMove.pos.y;
        }
        if (Vector3.Distance(curTarget, _follow) < 1.0 && Vector3.Distance(_follow, transform.position) < 1.0)
        {
            return;
        }
        if (Vector3.Distance(curTarget, _follow) > 1.0)
        {
            curTarget.x = (float)_target._info.entityMove.pos.x;
            curTarget.z = (float)_target._info.entityMove.pos.y;

            Vector3 curLerp = curTarget - _follow;
            curLerp = curLerp / GameOption._ServerFrameInterval * Time.deltaTime;
            curLerp = (curLerp + _lastLerp + _lastLastLerp) / 3.0f;
            _lastLastLerp = _lastLerp;
            _lastLerp = curLerp;

            _follow = _follow + curLerp;
        }

        transform.position = Vector3.Lerp(transform.position, _follow - Vector3.forward * 30.0f + Vector3.up * 30.0f, Time.deltaTime * 3.0f);
        transform.LookAt(_follow);
    }
}
