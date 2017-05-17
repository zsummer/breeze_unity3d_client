using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public static class MainScreenLabel
{
    static GradientColor gradient = new GradientColor();
    static GUIStyle guiStyle = new GUIStyle();
    static Vector2 labelSize = new Vector2();
    public static void Preprocess()
    {
        gradient.refresh();
        guiStyle.normal.textColor = gradient.fetchColor();
        guiStyle.richText = true;
        guiStyle.normal.background = null;
        guiStyle.fontSize = (int)(Screen.height * GameOption._fontSizeScreeHeightRate);
        labelSize.x = guiStyle.fontSize;
        labelSize.y = guiStyle.fontSize;
    }

    public static void Label(ref string label)
    {
        Vector2 nameSize = GUI.skin.label.CalcSize(new GUIContent(label)) * guiStyle.fontSize / GUI.skin.font.fontSize;
        labelSize.y += nameSize.y;
        GUI.Label(new Rect(labelSize.x, labelSize.y, nameSize.x, nameSize.y), label, guiStyle);
    }
}


