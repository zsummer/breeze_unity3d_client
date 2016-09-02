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
        if (Facade._avatarID == 0 || ControlStick.instance == null)
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
                Debug.DrawLine(transform.position, target, Color.red);
                Debug.DrawRay(transform.position, target, Color.red);
                var rat = mdis / cdis;
                transform.localPosition += (target - transform.position) * rat;
                transform.LookAt(target);
                //var targetRotation = Quaternion.FromToRotation(transform.position, target);
                //transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime*3);
                
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
