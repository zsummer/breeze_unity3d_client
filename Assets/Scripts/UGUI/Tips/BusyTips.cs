using UnityEngine;
using System.Collections;

public class BusyTips : MonoBehaviour {

    // Use this for initialization
    public float _speed = 7.0f;
	void Start ()
    {
        Facade.GetSingleton<NetController>().SetMainSessionDelegate(
    delegate () { gameObject.SetActive(false); },
    delegate () { gameObject.SetActive(true); },
    delegate (bool sus) { gameObject.SetActive(false); });
    }
	
	// Update is called once per frame
	void Update ()
    {
        transform.localEulerAngles += new Vector3(0, 0, _speed);
	}
}
