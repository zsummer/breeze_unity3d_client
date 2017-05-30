using UnityEngine;
using System.Collections;

public class BusyTips : MonoBehaviour {

    // Use this for initialization
    public float speed = 7.0f;
    public bool busy = false;
    void Start ()
    {
        gameObject.SetActive(false);
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (busy)
        {
            transform.localEulerAngles += new Vector3(0, 0, speed);
        }
    }
}
