using UnityEngine;
using System.Collections;
using System;


enum MoveType
{
	MT_IDLE,
	MT_HANDLE,
	MT_TARGET,
}
public class AvatarController : MonoBehaviour
{

    private AnimationState idle;
    private AnimationState runned;
    private Animation anim;


	RaycastHit _hit3D = new RaycastHit();
	Camera _mainCamera;

	private Vector3 _targetPos;
	private MoveType _moving = MoveType.MT_IDLE;
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

	void MoveControl()
	{
		if (true) 
		{
			float h = Input.GetAxis("Horizontal");
			float v = Input.GetAxis("Vertical");
			if (Math.Abs(h)> 0.1 || Math.Abs(v) > 0.1) 
			{
				_targetPos = transform.position + new Vector3 (h, 0, v) * 10;
				_moving = MoveType.MT_HANDLE;
			}

		}
		if (_moving == MoveType.MT_HANDLE) 
		{
			float h = Input.GetAxis("Horizontal");
			float v = Input.GetAxis("Vertical");
			if (Math.Abs(h)< 0.1 && Math.Abs(v) < 0.1) 
			{
				_moving = MoveType.MT_IDLE;
			}
		}
		if (_moving == MoveType.MT_IDLE || _moving == MoveType.MT_TARGET)
		{
			if (Input.GetButtonDown("Fire1")) 
			{
				Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
				Physics.Raycast(ray, out _hit3D, 100);
				if (_hit3D.transform != null && _hit3D.transform.name == "Terrain")
				{
					Debug.Log(_hit3D.transform.gameObject.name);
					_targetPos = _hit3D.point;
					_moving = MoveType.MT_TARGET;
				}
			}
		}
		if (_moving == MoveType.MT_TARGET)
		{
			if (Vector3.Distance(_targetPos, transform.position) < 0.1) 
			{
				_moving = MoveType.MT_IDLE;
			}
		}
	}

    void FixedUpdate()
    {
        if (Facade._avatarID == 0)
        {
            return;
        }
		MoveControl ();
		if (_moving == MoveType.MT_IDLE && anim.IsPlaying(runned.name)) 
		{
			anim.CrossFade(idle.name, 0.2f);
		}
		if (_moving != MoveType.MT_IDLE) 
		{
			if (!anim.IsPlaying(runned.name))
			{
				anim.CrossFade(runned.name, 0.2f);
			}

			var mdis = _speed * Time.fixedDeltaTime;
			var cdis = Vector3.Distance (transform.position, _targetPos);
			if (mdis < cdis) 
			{
				var rat = mdis / cdis;
				transform.localPosition += (_targetPos - transform.position) * rat;
				transform.LookAt(_targetPos);
			} 
			else 
			{
				transform.localPosition = _targetPos;
			}
		}


    }
	// Update is called once per frame
	void Update ()
    {
		
	}
}
