using UnityEngine;
using System.Collections;

public class FightFloatingText: MonoBehaviour
{
    public Color _textColor = Color.red;
    public int _initFontSize = (int)(Screen.height * GameOption._fontSizeScreeHeightRate);
    public float _initScale = 0.8f;
    public float _stepScale = 0.2f;
    public float _floatSpeed = 1.2f;
    public float _keepTime = 2.0f;
    public string _text = "";

    GUIStyle _guiStyle = new GUIStyle();
    float _createTime;
    Camera _mainCamera;
    // Use this for initialization
    void Start ()
    {
        _createTime = Time.realtimeSinceStartup;
        _guiStyle.normal.background = null;
        _guiStyle.normal.textColor = _textColor;

        foreach (Camera camera in Camera.allCameras)
        {
            if (camera.name == "SceneCamera")
            {
                _mainCamera = camera;
                break;
            }
        }
        if (_mainCamera == null)
        {
            _mainCamera = Camera.main;
        }
    }
	
	// Update is called once per frame
	void Update ()
    {
        transform.position = new Vector3(transform.position.x, transform.position.y + _floatSpeed * Time.deltaTime, transform.position.z);
        if (Time.realtimeSinceStartup - _createTime > _keepTime)
        {
            GameObject.Destroy(transform.gameObject);
        }
	}
    void OnGUI()
    {
        Vector3 gp = new Vector3(transform.position.x, transform.position.y + 4, transform.position.z);
        Vector2 position = _mainCamera.WorldToScreenPoint(gp);
        position = new Vector2(position.x, Screen.height - position.y);
        _guiStyle.fontSize = (int)(_initFontSize  * (_initScale + (Time.realtimeSinceStartup - _createTime) * _stepScale));
        Vector2 textSize = GUI.skin.label.CalcSize(new GUIContent(_text)) * _guiStyle.fontSize / GUI.skin.font.fontSize;
        GUI.Label(new Rect(position.x - (textSize.x / 2), position.y - textSize.y, textSize.x, textSize.y), _text, _guiStyle);
    }
}
