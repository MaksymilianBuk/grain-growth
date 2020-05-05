using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorHandler : MonoBehaviour
{
    public static List<Color32> colors = new List<Color32>();
    public static Color32 colorMax = new Color32(255, 0, 0, 255);
    public static Color32 colorMin = new Color32(0, 0, 255, 255);
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

    public static Color32 MapEnergyToColor(int energy)
    {
        if (energy == 0)
        {
            return new Color32(255, 255, 255, 255);
        }
        if (energy == 1)
        {
            return new Color(0, 255, 255, 255);
        }
        if (energy == 2)
        {
            return new Color(0, 255, 33, 255);
        }
        if (energy == 3)
        {
            return new Color(76, 255, 0, 255);
        }
        if (energy == 4)
        {
            return new Color(255, 216, 0, 255);
        }
        if (energy == 5)
        {
            return new Color(255, 106, 0, 255);
        }

        return new Color(255, 0, 0, 255); // if energy equals 6 or above
    }

    public static Color32 MapEnergyToColor(int energy, int maxEnergy)
    {
        int r, g, b;
        float N1, N2;

        // FEM 1D Shape Functions
        N1 = ((float)maxEnergy - energy);
        N1 = N1 / maxEnergy;
        N2 = ((float)energy / maxEnergy);

        r = (int)((N1 * colorMin.r) + (N2 * colorMax.r));
        g = (int)((N1 * colorMin.g) + (N2 * colorMax.g));
        b = (int)((N1 * colorMin.b) + (N2 * colorMax.b));

        //Debug.Log("Color is "+r + " " + g + " " + b+" Max energy: "+maxEnergy+" my energy: "+energy);
        return new Color32((byte)r, (byte)g, (byte)b, 255);
    }

    public static void ResetColors()
    {
        colors = new List<Color32>();
    }
}
