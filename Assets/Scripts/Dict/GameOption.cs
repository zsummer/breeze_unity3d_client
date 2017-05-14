using System;
using UnityEngine;
using System.Collections.Generic;

public class GameOption : MonoBehaviour
{
    public static bool _debugVersion = true; //开发模式 
    public static bool _specialEffect = true; //开启场景特效  
    public static float _fontSizeScreeHeightRate = 18.0f / 628.0f; //屏幕字体大小 
    public static float _ServerFrameInterval = 0.1f; //服务器一帧的间隔时间  
    public static float _CompensationSpeed = 7.0f; //同步补偿速度 
    public static float _TouchRedius = 75.0f / 600.0f;  //摇杆中心大小 
	public static bool _EnbaleClickMove = false; // 开启点击移动  
    public static float _AudioVolume = 0.1f; //默认音量 

    void Awake()
    {
        Application.targetFrameRate = 60;
    }
}

