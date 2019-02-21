using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Not going to be assigned to any gameObject,no instances going to be made(hence static)
public static class Noise {

    public enum NormalizeMode { local, global };


    public static float[,] generateNoiceMap(int mapWidth,int mapHeight,float scale, int seed, int octaves, float lacunarity, float persistance, Vector2 offset, NormalizeMode mode)
    {
        float[,] noiseMap = new float[mapWidth, mapHeight];

        float maxPossibleHeight = 0;
        float amplitude = 1;
        float frequency = 1;
        float noiseHeight = 0;

        // when scale is increased, the map should zoom in towards the center:
        float halfWidth = mapWidth / 2f;
        float halfHeight = mapHeight / 2f;

        //seed controls the output of the random object. if the seed is constant, the random generator will always have the same output.
        System.Random prng = new System.Random(seed);
        //Each octave should be sampled from a different location. 
        Vector2[] Octaveoffsets = new Vector2[octaves];
        //determining the values for the offset.
        for(int i = 0; i < octaves; i++)
        {
            // user offset value is also added. enables user to scroll through various noise maps.
            float offsetX = prng.Next(-100000, 100000)+ offset.x;
            float offsetY = prng.Next(-100000, 100000)-offset.y;
            Octaveoffsets[i] = new Vector2(offsetX, offsetY);

            maxPossibleHeight += amplitude;
            amplitude *= persistance;
        }
      

        //clamping the scale if scale is zero.(Can't do division by zero)
        if (scale <= 0)
        {
            scale = 0.001f ;
        }

        //initializing the max and min.
        float maxNoiseValue = float.MinValue;
        float minNoiseValue = float.MaxValue;


        // cycling through all the points of the map.
        for (int y = 0; y < mapHeight; y++){
            for(int x = 0; x < mapWidth; x++)
            {
                 amplitude = 1;
                 frequency = 1;
                 noiseHeight = 0;
                for (int z = 0; z < octaves; z++)
                {
                    // higher the frequency, further apart the sample points will be. change in heights will be more rapid.
                    //applying the offset to get a different sample location
                    float SampleX =( (x-halfWidth)+ Octaveoffsets[z].x) / scale*frequency;
                    float SampleY = ((y-halfHeight) + Octaveoffsets[z].y )/ scale*frequency;

                    // PerlinNoise method returns only 0 to 1. to have negative values, *2-1
                    float PerlinValue = Mathf.PerlinNoise(SampleX, SampleY) * 2 - 1;
                   

                    noiseHeight += PerlinValue * amplitude;

                    amplitude *= persistance;
                    frequency *= lacunarity;
                }
                //update the min and max noiseHeight
                //determines the rfange of noiseHeight values.
                if (noiseHeight > maxNoiseValue)
                {
                    maxNoiseValue = noiseHeight;
                }else if (noiseHeight < minNoiseValue)
                {
                    minNoiseValue = noiseHeight;
                }
                noiseMap[x, y] = noiseHeight;
            }
        }

        //normalize the noiseMap values (make it range between 0 and 1)
        //process through all values and clamp it between 0 and 1.
        for(int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                //InverseLerp returns value between 0 & 1. if noiseMap[x,y] = min, returns 0, if it is equal to max, returns 1

                if (mode == NormalizeMode.local)
                {
                    noiseMap[x, y] = Mathf.InverseLerp(minNoiseValue, maxNoiseValue, noiseMap[x, y]);

                }
                else
                {
                    float normalizedHeight = (noiseMap[x, y] + 1) / (2f * (maxPossibleHeight / 1.75f));
                    noiseMap[x, y] = Mathf.Clamp(normalizedHeight, 0, int.MaxValue);

                }

            }
        }

        return noiseMap;
    }


}

//Lacunarity: Controls the frequency of the object.
//Persistance: controls amplitude of the object
// in case of mountains, 3 octaves: main outline, boulders and small rocks.. frequency of small rocks should be greates, and so on. however, the amplitude of small rocks shoould be lowest.
