using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class Input : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    GameObject _placeholder;
    GameObject _inputText;
	// Use this for initialization
	void Start ()
    {
        var tss = GetComponentsInChildren<Transform>();
        foreach (var ts in tss)
        {
            if (ts.gameObject.name == "Placeholder")
            {
                _placeholder = ts.gameObject;
            }
            else if (ts.gameObject.name == "Text")
            {
                _inputText = ts.gameObject;
            }
        }
    }
	
	// Update is called once per frame
	void Update ()
    {
	
	}
    public void OnSelect(BaseEventData eventData)
    {
        if (_placeholder)
        {
            _placeholder.SetActive(false);
        }
    }
    public void OnDeselect(BaseEventData eventData)
    {
        if (_placeholder)
        {
            _placeholder.SetActive(true);
        }
    }


}
