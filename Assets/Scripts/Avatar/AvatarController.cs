using UnityEngine;
using System.Collections;
using System;



public class AvatarController : MonoBehaviour
{

    private AnimationState idle;
    private AnimationState runned;
    private Animation anim;


	Camera _mainCamera;


	public float _speed = 7.0f;

	void Start ()
    {
        anim = GetComponent<Animation>();
        idle = anim["idle"];
        runned = anim["walk"];
        idle.wrapMode = WrapMode.Loop;
        runned.wrapMode = WrapMode.Loop;
		_mainCamera = Camera.main;
    }



    void FixedUpdate()
    {
        if (Facade.AvatarMode == null || ControlStick.instance == null)
        {
            return;
        }

		if (ControlStick.instance.moveType == MoveType.MT_IDLE && anim.IsPlaying(runned.name)) 
		{
			anim.CrossFade(idle.name, 0.2f);
		}
		if (ControlStick.instance.moveType != MoveType.MT_IDLE) 
		{
			if (!anim.IsPlaying(runned.name))
			{
				anim.CrossFade(runned.name, 0.2f);
			}
            Vector3 target = ControlStick.instance.targetPos;
            if (ControlStick.instance.moveType == MoveType.MT_HANDLE)
            {
                target = transform.position + target;
            }
            var mdis = _speed * Time.fixedDeltaTime;
            var cdis = Vector3.Distance(transform.position, target);
            if (mdis < cdis)
            {
                
                Debug.DrawLine(transform.position + transform.up * 0.3f, target+transform.up* 0.3f, Color.red, 1.2f);
                Debug.DrawLine(transform.position + transform.up * 0.3f, transform.forward*10+ transform.position + transform.up * 0.3f, Color.yellow, 1.2f);
                var dir = target - transform.position;
                var euler = Vector3.Angle(dir, Vector3.forward);
                if (dir.x < 0f)
                {
                    euler = 360f - euler;
                }
                var targetRotation = Quaternion.Euler(0, euler, 0);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 3);

                var rat = mdis / cdis;
                transform.localPosition += (target - transform.position) * rat;
               // transform.LookAt(target);
            }
            else
            {
                transform.localPosition = target;
                ControlStick.instance.moveType = MoveType.MT_IDLE;
            }

        }


    }
	// Update is called once per frame
	void Update ()
    {
		
	}
}
