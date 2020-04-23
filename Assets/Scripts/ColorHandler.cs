using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorHandler : MonoBehaviour
{
    public static List<Color32> colors = new List<Color32>();

    public static Color32 GenerateColor()
    {
        bool duplicated=false;
        for (; ; )
        { 
            Color32 temp = new Color32((byte)Random.Range(1, 255), (byte)Random.Range(1, 255), (byte)Random.Range(1, 255), (byte)255);
            foreach (Color32 color in colors)
            {
                if (color.Equals(temp))
                {
                    duplicated = true;
                }
            }
            if (!duplicated)
            {
                colors.Add(temp);
                return temp;
            }
        }
    }

    public static void ResetColors()
    {
        colors = new List<Color32>();
    }
}
