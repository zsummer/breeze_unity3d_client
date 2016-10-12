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
    public Vector3 targetPos { get { return _targetPos; } set { _targetPos = value; } }
    public MoveType moveType { get { return _moveType; } set { _moveType = value; } }
    MoveType _moveType = MoveType.MT_IDLE;
    Vector3 _targetPos;

    private AnimationState _free;
    private AnimationState _runned;
    private AnimationState _attack;
    private Animation anim;


	Camera _mainCamera;


	public float _speed = 12.0f;

	void Start ()
    {
        anim = GetComponent<Animation>();
        _free = anim["free"];
        _runned = anim["walk"];
        _attack = anim["attack"];
        _free.wrapMode = WrapMode.Loop;
        _runned.wrapMode = WrapMode.Loop;
        foreach(Camera camera in Camera.allCameras)
        {
            if (camera.name == "Camera")
            {
                _mainCamera = camera;
                break;
            }
        }
    }

    public void CrossAttack()
    {
        anim.CrossFade(_attack.name);
    }

    void FixedUpdate()
    {
		if (_moveType == MoveType.MT_IDLE && (anim.IsPlaying(_runned.name) || !anim.isPlaying)) 
		{
			anim.CrossFade(_free.name, 0.2f);
		}

		if (_moveType != MoveType.MT_IDLE) 
		{
			if (anim.IsPlaying(_free.name) || !anim.isPlaying)
			{
				anim.CrossFade(_runned.name, 0.2f);
			}
            Vector3 target = _targetPos;
            if (_moveType == MoveType.MT_HANDLE)
            {
                target = transform.position + target;
            }
            else
            {
                target.y = transform.position.y;
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
                var cur = transform.eulerAngles.y;
                var targetRotation = Quaternion.Euler(0, euler, 0);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5);


                var rat = mdis / cdis;
                transform.position += dir * rat;
               // transform.LookAt(target);
            }
            else
            {
                transform.position = target;
                _moveType = MoveType.MT_IDLE;
            }

        }


    }
	// Update is called once per frame
	void Update ()
    {
		
	}
}
