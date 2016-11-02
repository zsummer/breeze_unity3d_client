using UnityEngine;
using System.Collections;

public class SceneCamera : MonoBehaviour
{
    
    private Transform _target;
    private float _height = 60.0f;
    void Start ()
    {

	}

     void Update()
     {
        if (_target == null && Facade._entityID != 0)
        {
            _target = Facade._sceneManager.GetEntity(Facade._entityID).gameObject.transform;
            _height = _target.position.y;
        }
        if (_target == null)
        {
            return;
        }
        var target = new Vector3(_target.position.x, _height, _target.position.z);
        transform.position = target - Vector3.forward * 30.0f + Vector3.up * 40.0f;
        transform.LookAt(_target.position);
    }
}
