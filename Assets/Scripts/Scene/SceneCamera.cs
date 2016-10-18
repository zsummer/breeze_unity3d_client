using UnityEngine;
using System.Collections;

public class SceneCamera : MonoBehaviour
{

    public Transform _target;
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
            _target = Facade._sceneManager.GetEntity(Facade._entityID).gameObject.transform;
            _follow = _target.position;
            _lastLerp = new Vector3(0,0,0);
            _lastLastLerp = _lastLerp;
        }
        if (_target == null)
        {
            return;
        }
        transform.position = _target.position - Vector3.forward * 30.0f + Vector3.up * 40.0f;
        transform.LookAt(_target.position);
    }
}
