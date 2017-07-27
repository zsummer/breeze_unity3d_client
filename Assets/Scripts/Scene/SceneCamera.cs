using UnityEngine;
using System.Collections;

public class SceneCamera : MonoBehaviour
{
    
    private Transform _target;
    private float _targetInitHeight = 0.0f;
    void Start ()
    {

	}

     void Update()
     {
        if (_target == null && Facade.myShell != 0)
        {
            _target = Facade.sceneManager.GetShell(Facade.myShell).gameObject.transform;
			_targetInitHeight = _target.position.y;
			//var target = new Vector3(_target.position.x, _targetInitHeight, _target.position.z);
			//transform.position = target - Vector3.forward * 30.0f + Vector3.up * 40.0f;
			//transform.LookAt(target);

        }
        if (_target == null)
        {
            return;
        }
		var target = new Vector3(_target.position.x, _targetInitHeight, _target.position.z);
        transform.position = target - Vector3.forward * 30.0f + Vector3.up * 40.0f;
		transform.LookAt(target);
    }
}
