using UnityEngine;
using System.Collections;

public class AvatarController : MonoBehaviour {

    private AnimationState idle;
    private AnimationState runned;
    private Animation anim;
	void Start ()
    {
        anim = GetComponent<Animation>();
        idle = anim["idle"];
        runned = anim["walk"];
        idle.wrapMode = WrapMode.Loop;
        runned.wrapMode = WrapMode.Loop;

    }

    void FixedUpdate()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 velocity = new Vector3(0, 0, v);
        velocity = transform.TransformDirection(velocity);
        if (v > 0.1 || v < -0.1)
        {
            velocity *= 7.0f;
            transform.localPosition += velocity * Time.fixedDeltaTime;
            transform.Rotate(0, h * 2.0f, 0);
            if (!anim.IsPlaying(runned.name))
            {
                anim.CrossFade(runned.name, 0.2f);
            }
        }
        else
        {
            if (!anim.IsPlaying(idle.name))
            {
                anim.CrossFade(idle.name, 0.2f);
            }
        }

    }
	// Update is called once per frame
	void Update ()
    {
	
	}
}
