using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TextureCreator  {
    public static Texture2D TextureFromColorMap(Color [] colorMap,int Width,int Height)
    {
        Texture2D texture = new Texture2D(Width, Height);
        //making the edges sharp
        texture.filterMode = FilterMode.Point;
        //making texture not show part of the next section
        texture.wrapMode = TextureWrapMode.Clamp;
        //setting the color
        texture.SetPixels(colorMap);
        //applying all the changes made.
        texture.Apply();
        return texture;
    }

    public static Texture2D TextureFromHeightMap(float[,] noiseMap)
    {
        int width = noiseMap.GetLength(0);
        int height = noiseMap.GetLength(1);

    

        Color[] colorMap = new Color[width * height];


        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                //assigning colors to colorMap
                //width*y+x gives position of point on noice map 
                //Color.Lerp gives a color in between black and white which is defined by the random noice value given by map.
                colorMap[width * y + x] = Color.Lerp(Color.black, Color.white, noiseMap[x, y]);

            }
        }
        // only the colorMap is made using this method and the other method is used to apply the texture.
        return TextureFromColorMap(colorMap, width, height);
    }

}
