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

        if (Input.GetButtonDown("Fire1"))
        {
            if (_event.IsPointerOverGameObject())
            {
                Vector3 pos = Input.mousePosition;
                Debug.Log("UGUI down." + pos);
            }
            else
            {
                Vector3 pos = Input.mousePosition;
                Debug.Log("3d down." + pos);
            }

        }

    }
}
