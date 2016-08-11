using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class Input : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    Transform _placeholder;
    Transform _inputText;
	// Use this for initialization
	void Start ()
    {
        var tss = GetComponentsInChildren<Transform>();
        foreach (var ts in tss)
        {
            if (ts.gameObject.name == "Placeholder")
            {
                _placeholder = ts;
            }
            else if (ts.gameObject.name == "Text")
            {
                _inputText = ts;
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
            _placeholder.gameObject.SetActive(false);
        }
    }
    public void OnDeselect(BaseEventData eventData)
    {
        if (_placeholder)
        {
            _placeholder.gameObject.SetActive(true);
        }
    }


}
