using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TouchController : MonoBehaviour
{
    public Image image1;
    public Image image2;
    bool isPress = false;
    UnityEngine.EventSystems.EventSystem _event;
    void Start ()
    {
        _event = UnityEngine.EventSystems.EventSystem.current;
        Init();
	}
    void Init()
    {
        image2.transform.position = image1.transform.position;
        var color = image1.color;
        color.a = 10;
        image1.color = color;
        image2.color = image1.color;
    }
	
	// Update is called once per frame
	void Update ()
    {

//         if (!isPress && Facade._avatarID != 0 && Input.GetButtonDown("Fire1") && _event.IsPointerOverGameObject())
//         {
//             Vector3 pos = Input.mousePosition;
//             isPress = true;
//         }
//        if (isPress)
//         {
//             image2.transform.position = Input.mousePosition;
//         }
//         if (isPress && Input.GetButtonUp("Fire1"))
//         {
//             isPress = false;
//             Init();
//         }
    }
}
