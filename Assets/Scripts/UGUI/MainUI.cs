using UnityEngine;
using System.Collections;

public class MainUI : MonoBehaviour
{
    UnityEngine.EventSystems.EventSystem _event;
    void Start()
    {
        _event = UnityEngine.EventSystems.EventSystem.current;
        Init();
    }
    void Init()
    {

    }

    // Update is called once per frame
    void Update()
    {


    }
}
