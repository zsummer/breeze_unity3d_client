using UnityEngine;
using System.Collections;

public class ModelMgr : MonoBehaviour
{

    void Awake()
    {
        Debug.Log("Awake NetController.");
        DontDestroyOnLoad(gameObject);
    }
    // Use this for initialization
    void Start ()
    {
	    
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
