using UnityEngine;
using System.Collections;
using System;
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
        if (Facade._avatarID == 0)
        {
            return;
        }

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        if (System.Math.Abs(h) < 0.1 && System.Math.Abs(v) < 0.1)
        {
            if (anim.IsPlaying(runned.name))
            {
                anim.CrossFade(idle.name, 0.2f);
            }
            return;
        }
        Vector3 velocity = new Vector3(h, 0, v);

        if (!anim.IsPlaying(runned.name))
        {
            anim.CrossFade(runned.name, 0.2f);
        }
        velocity *= 7.0f;

        transform.LookAt(transform.position + velocity * Time.fixedDeltaTime);
        transform.localPosition += velocity * Time.fixedDeltaTime;

    }
	// Update is called once per frame
	void Update ()
    {
	
	}
}
