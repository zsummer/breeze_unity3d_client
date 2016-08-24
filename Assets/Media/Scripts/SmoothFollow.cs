using UnityEngine;
using System.Collections;

public class SmoothFollow : MonoBehaviour
{
    public Transform target;
    private Camera mainCamera = null;

	void Start ()
    {
        mainCamera = Camera.main;
	}

     void FixedUpdate()
     {
        FixedUpdatePosition();
     }







    // Update is called once per frame
    public void FixedUpdatePosition()
    {
        // Early out if we don't have a target
        if (target == null && Facade._avatar != null)
        {
            target = Facade._avatar;
        }
        Vector3 targetPos = target.position + target.forward * 20.0f  + target.up*30.0f;
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.fixedDeltaTime * 3.0f);
        transform.LookAt(target);
    }
}
