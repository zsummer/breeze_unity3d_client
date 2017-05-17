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

    static Dictionary<int, string> bulletins = new Dictionary<int, string>();


    public static void Preprocess()
    {
        gradient.refresh();
        guiStyle.normal.textColor = gradient.fetchColor();
        guiStyle.richText = true;
        guiStyle.normal.background = null;
        guiStyle.fontSize = (int)(Screen.height * GameOption._fontSizeScreeHeightRate);
        labelSize.x = guiStyle.fontSize;
        labelSize.y = guiStyle.fontSize;

        for (int i = 0; i < bulletins.Count; i++)
        {
            Label(bulletins.ElementAt(i).Value);
        }
    }

    public static void Bulletin(int id, string bt)
    {
        if (bt.Length == 0)
        {
            bulletins.Remove(id);
        }
        else
        {
            bulletins[id] = bt;
        }
    }

    public static void Label(string label)
    {
        Vector2 nameSize = GUI.skin.label.CalcSize(new GUIContent(label)) * guiStyle.fontSize / GUI.skin.font.fontSize;
        labelSize.y += nameSize.y;
        GUI.Label(new Rect(labelSize.x, labelSize.y, nameSize.x, nameSize.y), label, guiStyle);
    }
}


