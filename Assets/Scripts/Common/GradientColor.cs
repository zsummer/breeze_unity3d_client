using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


public class GradientColor
{

     float[] counter_step = new float[3] { 0.01f, 0.003f, 0.001f };
     float[] counter = new float[3] { 0, 0, 0 };

     float[] color_step = new float[3] { 255, 0, 255 };
     float[] color = new float[3] { 255, 255, 255 };
     Color cacheColor = new Color(0, 0, 0);
    public  Color fetchColor()
    {
        return cacheColor;
    }
    public  void refresh()
    {
        for (int i = 0; i < 3; i++)
        {
            counter[i] += counter_step[i];
        }
        for (int i = 0; i < 3; i++)
        {
            if (counter[i] >= 1)
            {
                counter[i] = 0;
                color[i] += color_step[i];

                if (color[i] > 255)
                {
                    color[i] = 255;
                    if (color_step[i] >0 )
                    {
                        color_step[i] *= -1;
                    }
                }
                if (color[i] < 0)
                {
                    color[i] = 0;
                    if (color_step[i] < 0)
                    {
                        color_step[i] *= -1;
                    }
                }
            }
        }

        cacheColor.r = color[0];
        cacheColor.g = color[1];
        cacheColor.b = color[2];
    }
}

