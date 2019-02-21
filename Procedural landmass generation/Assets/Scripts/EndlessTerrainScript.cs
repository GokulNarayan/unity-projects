using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndlessTerrainScript : MonoBehaviour
{
    public Material material;

    const float viewerMoveThresholdForChunkUpdate = 25f;

    const float SquareViewerMoveThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;

    public LODinfo[] detailLevels;

    /// <summary>
    /// The maximum view distance.
    /// </summary>
    public static float maxViewDistance ;

    /// <summary>
    /// Contains reference to the viewer.
    /// </summary>
    public Transform viewer;

    static MapGenerator mapGenScript;

    public Transform parent;
  
    /// <summary>
    /// reference of viewers position.
    /// </summary>
    public static Vector2 viewerPosition;

    Vector2 oldViewerPosition;

    /// <summary>
    /// Reference to the chunk size of esach terrain.
    /// </summary>
    int chunkSize;

    /// <summary>
    /// The number of chunks in range.
    /// </summary>
    int chunksVisibleInViewDistance;

    /// <summary>
    /// A dictionary of the terrain chunks and its coordinates will be used to avoid instantiating duplicates.
    /// </summary>
    Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    
    /// <summary>
    /// the list will contain terrain chunks that need to be deactivated as it is no more visible in the current frame.
    /// </summary>
   static List<TerrainChunk> visibleLastUpdate = new List<TerrainChunk>();

    void Start()
    {
        
        //The number of visible chunks is simply the max view distance/ the size of each chunk. eg: 100 view distnace / 10 chunk size. 10 chunks in view.
      
        mapGenScript = FindObjectOfType<MapGenerator>();
        chunkSize = MapGenerator.mapChunkSize - 1;
        maxViewDistance = detailLevels[detailLevels.Length - 1].visibleDistanceThreshold;
        chunksVisibleInViewDistance = Mathf.RoundToInt(maxViewDistance / chunkSize);

        updateVisibleChunks();
      
    }
    
    //the viewers position must be updated
    void Update()
    {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);

        if ((oldViewerPosition-viewerPosition).sqrMagnitude> SquareViewerMoveThresholdForChunkUpdate) {

            oldViewerPosition = viewerPosition;
            updateVisibleChunks();
        }
           
      
    }

    void updateVisibleChunks()
    {
      
        // disable the terrainChunks not visible now.
        for (int i=0; i < visibleLastUpdate.Count; i++){
            visibleLastUpdate[i].SetVisible(false);
            
        }

        visibleLastUpdate.Clear();

        //the coordinate gives the position with respect to the chunk size. eg: if viewer is at (240,0), coordinate= 1,0)
        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunkSize);

        for(int yOffset = -chunksVisibleInViewDistance; yOffset <= chunksVisibleInViewDistance; yOffset++)
        {
            for (int xOffset = -chunksVisibleInViewDistance; xOffset <= chunksVisibleInViewDistance; xOffset++)
            {
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

                //if there is already a terrainchunk having the particular coordinate:
                if (terrainChunkDictionary.ContainsKey(viewedChunkCoord))
                {
                    terrainChunkDictionary[viewedChunkCoord].updateTerrainChunk();
                    
                }
                else
                {
                    terrainChunkDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize, parent, material, detailLevels));
                }

            }
        }
    }


    public class TerrainChunk{

        GameObject meshObject;
        Vector2 position;
        //the smallest distance between the viewing object an the terrain chunk needs to be obtained: using bounds
        // bounds struct will provide a pseudo box around the terrin chunk and the smallest distnace can be obtained.

        Bounds terrainBound;

        MeshRenderer terrainRenderer;
        MeshFilter terrainMeshFilter;
        LODinfo [] levelOfDetailInfo;
        LODMesh[] lodMeshes;
        MapData mapData;
        bool MapDataRecieved;
        int prevLevelOfDetailINdex = -1;

        public TerrainChunk(Vector2 coordinate, int size, Transform parent, Material mapMaterial,LODinfo[] levelOfDetailInfo)
        {
            this.levelOfDetailInfo = levelOfDetailInfo;
            position = coordinate * size;
            terrainBound = new Bounds(position, Vector2.one * size);
            Vector3 positionV3 = new Vector3(position.x, 0, position.y);
           
            meshObject = new GameObject("Terrain Chunk");
            terrainRenderer = meshObject.AddComponent<MeshRenderer>();
            terrainMeshFilter = meshObject.AddComponent<MeshFilter>();
            terrainRenderer.material = mapMaterial;
            meshObject.transform.position = positionV3;

            meshObject.transform.parent = parent;
            //by default, the terrain chunk should be deactivated.
            SetVisible(false);

            lodMeshes = new LODMesh[levelOfDetailInfo.Length];
            for (int i = 0; i < lodMeshes.Length; i++)
            {
                lodMeshes[i] = new LODMesh(levelOfDetailInfo[i].lod,updateTerrainChunk);
              
            }
           

            

            //request the mapData : the method that will be called back later is the ON mapData recieved.
            mapGenScript.RequestMapData(position,OnMapDataRecieved);

        }

        //this method will be called by the map generator script when the map Data has been generated.
        void OnMapDataRecieved(MapData mapData)
        {
            
            //do something with the Mapdata

            //now call the request mesh data to obtain the meshdata to create the mesh.
            // mapGenScript.RequestMeshData(mapData, OnMeshDataRecieved);
            this.mapData = mapData;
            MapDataRecieved = true;
            Texture2D texture = TextureCreator.TextureFromColorMap(mapData.colorMap, MapGenerator.mapChunkSize, MapGenerator.mapChunkSize);
            terrainRenderer.material.mainTexture = texture;
           
           
            updateTerrainChunk();
        }

     

        public void updateTerrainChunk()
        {
            if (MapDataRecieved)
            {

               
                float viewerDistanceFromNearestEdge = Mathf.Sqrt(terrainBound.SqrDistance(viewerPosition));

                bool visible = viewerDistanceFromNearestEdge <= maxViewDistance;

               
                if (visible)
                {
                    int lodIndex = 0;
                    for (int i = 0; i < levelOfDetailInfo.Length - 1; i++)
                    {
                        if (viewerDistanceFromNearestEdge > levelOfDetailInfo[i].visibleDistanceThreshold)
                        {
                            lodIndex = i + 1;
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (lodIndex != prevLevelOfDetailINdex)
                    {
                        //update which meshData to use.
                        LODMesh _LODMesh = lodMeshes[lodIndex];
                        if (_LODMesh.hasMesh)
                        {
                            terrainMeshFilter.mesh = _LODMesh.meshObject;
                            prevLevelOfDetailINdex = lodIndex;
                        }
                        else if (!_LODMesh.hasRequestedmesh)
                        {
                            _LODMesh.RequestMesh(mapData);
                        }
                    }
                    visibleLastUpdate.Add(this);
                }



                SetVisible(visible);
               
            }
        }

        public void SetVisible(bool _visible)
        {
           
            meshObject.SetActive(_visible);
           
            
        }
        
        public bool isVisible()
        {
            return meshObject.activeSelf;
        }
        

    }

    class LODMesh{

        public Mesh meshObject;
        public bool hasRequestedmesh;
        public bool hasMesh;
        int LODofCurrentMesh;

        System.Action updateCallBack;
        public LODMesh(int lod,System.Action updateCallBack)
        {
            LODofCurrentMesh = lod;
            this.updateCallBack = updateCallBack;
           
        }

        void OnMeshDataRecieved(MeshData meshData)
        {
           
            meshObject = meshData.CreateMesh();

        
            
            hasMesh = true;
            updateCallBack();
        }

        public void RequestMesh(MapData mapdata)
        {
            hasRequestedmesh = true;
            mapGenScript.RequestMeshData(mapdata,LODofCurrentMesh, OnMeshDataRecieved);
        }

    }

    [System.Serializable]
    public struct LODinfo
    {
        public int lod;
        public float visibleDistanceThreshold;
    }
}


/* chunk coordinate explanation: 
 * grid: (-1,1) | (0,1)| (1,1)
 *       (-1,0) | (0,0)| (1,0)
 *       (-1,-1)|(0,-1)| (1,-1)
 *       if my view distance is exactly 240, and my current coord is (0,0), the loop starts with y being -1 and x loop starts with -1.
 *       the Vector2 location of the chunk in that coordinate is (m,y x=0+ -1, my y=0+-1).
 *       */