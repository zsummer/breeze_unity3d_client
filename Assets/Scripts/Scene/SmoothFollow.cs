using UnityEngine;
using System.Collections;

public class SmoothFollow : MonoBehaviour
{
    public Transform target;


	void Start ()
    {

	}

     void FixedUpdate()
     {
        if (target == null && Facade._entityID != 0)
        {
            target = Facade._gameScene.GetEntity(Facade._entityID).transform;
        }
        if (target == null)
        {
            return;
        }
        Vector3 targetPos = target.position - Vector3.forward * 30.0f + Vector3.up * 30.0f;
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.fixedDeltaTime * 3.0f);
        transform.LookAt(target);
    }
}
