using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;


public class MapGenerator : MonoBehaviour
{

    public enum DrawMode { drawNoiseMap,drawColorMap,drawMesh}
    public DrawMode drawMode;

    public Noise.NormalizeMode normalizeMode;

    public float MeshHeightMultiplier;
    public AnimationCurve heightCurve;
    public const int mapChunkSize = 241;

    Queue<ThreadRequirements<MapData>> mapThreadQueue = new Queue<ThreadRequirements<MapData>>();
    Queue<ThreadRequirements<MeshData>> meshThreadQueue= new Queue<ThreadRequirements<MeshData>>();
   
    public float scale;
    public int octaves; 
    [Range(0,1)]
    public float persistance;
    public float lacunarity;
    [Range(0,6)]
    public int EditorPreviewLOD;
    /// <summary>
    /// The seed value determines the value of the random number generated.
    /// </summary>
    public int seed;
    public Vector2 offset;

    public bool AutoUpdate;

    public TerrainType[] regions; 
    
    // Action<Mapdata> callback encapsulates the method which is only going to require mapData.(as a passed variable.)
    //this method will be called by the endless terrain script.
    public void RequestMapData(Vector2 center,Action<MapData> callBack)
    {
        //when the thread is started, it should call the mapData thread function with the callback passed to it.
        ThreadStart threadStart = delegate
        {
            mapDataThread(center,callBack);
        };

        //create the new thread and call the threadStart delegate.
        new Thread(threadStart).Start();
    }

    public void mapDataThread(Vector2 center,Action<MapData> callback)
    {
        //generate the mapData
        MapData mapData = GenerateMapData(center);

        // add the callback and the mapData generated into a queue to execute on main thread.
        // unity can only process main actions on its main thread.
        // the mapDataThreadQueue must not be accesible while this is being done.
        lock (mapThreadQueue){
            //Add the mapData created and the callBack method.
            mapThreadQueue.Enqueue(new ThreadRequirements<MapData>(callback, mapData));
        }
               
    }

    public void RequestMeshData(MapData mapdata,int LOD, Action<MeshData> callBack)
    {
        ThreadStart threadStart = delegate
         {
             MeshDataThread(mapdata,LOD, callBack);
         };

        new Thread(threadStart).Start();
    }

    void MeshDataThread(MapData mapData,int Lod, Action<MeshData> callBack)
    {
        //generate the meshData
        MeshData meshData = MeshGenerator.GenerateTerrainMesh(mapData.heightMap, MeshHeightMultiplier, heightCurve, Lod);

        lock (meshThreadQueue)
        {
            meshThreadQueue.Enqueue(new ThreadRequirements<MeshData>(callBack,meshData));
        }

    }

    void Update()
    {
        if (mapThreadQueue.Count > 0)
        {
            int queueLength = mapThreadQueue.Count;
            //loop through all the elements in the queue
            for (int i = 0; i <queueLength; i++)
            {
                //obtain the element in the queue and remove it.
                ThreadRequirements<MapData> threadInfo = mapThreadQueue.Dequeue();
                //call the method that was passed to the thread initially (as a parameter) in the main thread. 
                //this method (which is on the endless terrain script will now have the mapdata).
                threadInfo.callBack(threadInfo.parameter);
            }
        }

        if (meshThreadQueue.Count > 0)
        {
            for(int i = 0; i < meshThreadQueue.Count; i++)
            {
                ThreadRequirements<MeshData> threadInfo = meshThreadQueue.Dequeue();
                threadInfo.callBack(threadInfo.parameter);
            }
        }
    }

  public void mapDisplayCaller()
    {

        MapData mapData = GenerateMapData(Vector2.zero);
        // creating an object of type mapDisplay
        MapDisplay display = FindObjectOfType<MapDisplay>();


        if (drawMode == DrawMode.drawNoiseMap)
        {
            display.DrawTexture(TextureCreator.TextureFromHeightMap(mapData.heightMap));
        }
        else if (drawMode == DrawMode.drawColorMap)
        {
            display.DrawTexture(TextureCreator.TextureFromColorMap(mapData.colorMap, mapChunkSize, mapChunkSize));
        }
        else if (drawMode == DrawMode.drawMesh)
        {
            display.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapData.heightMap, MeshHeightMultiplier, heightCurve, EditorPreviewLOD), TextureCreator.TextureFromColorMap(mapData.colorMap, mapChunkSize, mapChunkSize));
        }



    }

    public MapData GenerateMapData(Vector2 center)
    {
        //generating the noisemap
        float[,] noiseMap = Noise.generateNoiceMap(mapChunkSize, mapChunkSize, scale,seed, octaves, lacunarity, persistance,center+  offset,normalizeMode);

        // creating a color map
        Color[] colorMap = new Color[mapChunkSize * mapChunkSize];

        // cycling through each point and assigning it a color based on its height.
        for (int y = 0; y < mapChunkSize; y++)
        {
            for(int x = 0; x < mapChunkSize; x++)
            {
                float currentHeight = noiseMap[x, y];

                for(int i = 0; i < regions.Length; i++)
                {
                    if (currentHeight >= regions[i].height)
                    {
                        colorMap[y * mapChunkSize + x] = regions[i].color;

                    }
                    else
                    {
                        break;
                    }

                }
            }
        }

        return new MapData(noiseMap, colorMap);

    }

    void OnValidate()
    {
      
        if(lacunarity < 1)
        {
            lacunarity = 1;
        }
        if (octaves < 0)
        {
            octaves = 0;
        }


    }

    struct  ThreadRequirements<T>{
        // variable containg the call that has to be added to a queue
        public Action<T> callBack;
        // variable that contains either the meshData or the MapData required by the endless terrain script
        public T parameter;

        //constructor.
        public ThreadRequirements(Action<T> callBack, T parameter)
        {
            this.callBack = callBack;
            this.parameter = parameter;
        }

    }
   
    }
//to make sure that it shows up in the inspector.
[System.Serializable]
public struct TerrainType
{
    public string name;
    public float height;
    public Color color;
}

public struct MapData
{
    public float[,] heightMap;
    public Color[] colorMap;

    public MapData(float[,] heightMap, Color[] colorMap)
    {
        this.heightMap = heightMap;
        this.colorMap = colorMap;
    }

}

