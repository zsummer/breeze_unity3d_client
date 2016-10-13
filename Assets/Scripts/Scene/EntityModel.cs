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
	public ulong _eid = 0;
	public float _speed = 12.0f;
    public string _name = "unkown";
    float _npcHeight;


    public MoveType _moveType = MoveType.MT_IDLE;
    public Vector3 _targetPos;

    private AnimationState _free;
    private AnimationState _runned;
    private AnimationState _attack;
    private Animation anim;


	Camera _mainCamera;




	void Start ()
    {
        anim = GetComponent<Animation>();
        _free = anim["free"];
        _runned = anim["walk"];
        _attack = anim["attack"];
        _free.wrapMode = WrapMode.Loop;
        _runned.wrapMode = WrapMode.Loop;
        _npcHeight = GetComponent<CapsuleCollider>().bounds.size.y * transform.localScale.y;
    }

    public void CrossAttack()
    {
        anim.CrossFade(_attack.name);
    }
    void OnGUI()
    {
        Vector3 worldPosition = new Vector3 (transform.position.x , transform.position.y + _npcHeight,transform.position.z);
        Vector2 position = Camera.main.WorldToScreenPoint (worldPosition);
        position = new Vector2 (position.x, Screen.height - position.y);

        //Vector2 bloodSize = GUI.skin.label.CalcSize (new GUIContent(blood_red));
        //int blood_width = blood_red.width * HP/100;
        //GUI.DrawTexture(new Rect(position.x - (bloodSize.x/2),position.y - bloodSize.y ,bloodSize.x,bloodSize.y),blood_black);
        //GUI.DrawTexture(new Rect(position.x - (bloodSize.x/2),position.y - bloodSize.y ,blood_width,bloodSize.y),blood_red);

        Vector2 nameSize = GUI.skin.label.CalcSize (new GUIContent(_name));
        if (_eid == Facade._entityID && _eid != 0)
        {
            GUI.color = Color.yellow;
        }
        else
        {
            GUI.color = Color.red;
        }
        GUI.Label(new Rect(position.x - (nameSize.x/2),position.y - nameSize.y, nameSize.x,nameSize.y), name);

    }
    void FixedUpdate()
    {
		if (_mainCamera == null) 
		{
			foreach(Camera camera in Camera.allCameras)
			{
				if (camera.name == "SceneCamera")
				{
					_mainCamera = camera;
					break;
				}
            }
		}
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
