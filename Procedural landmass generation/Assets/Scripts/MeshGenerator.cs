using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGenerator {

	public static MeshData GenerateTerrainMesh(float[,] heightMap,float heighMultiplier,AnimationCurve _heightCurve, int levelOfDetail)
    {
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);

        AnimationCurve heightCurve =new AnimationCurve( _heightCurve.keys);

        //the mesh should be pivotally centered on the screen
        //topLeftX should have a value of -1;
        float topLeftX = (width - 1) / -2f;
        float topLeftZ = (height - 1) / 2f;

       
        //gives us current position of 1d array(vertex []).
        int vertexIndex = 0;

        //Controls the amount of detail renderered (controls levelof detail)
        int meshSimplificationIncrement = levelOfDetail == 0 ? 1 : levelOfDetail * 2;
        //number of vertices per line of the mesh.
        int verticesPerLine = ((width - 1) / meshSimplificationIncrement) + 1;

        MeshData meshData = new MeshData(verticesPerLine, verticesPerLine);

        //cycles through each point in the hheight map.
        for (int y = 0; y < height; y+=meshSimplificationIncrement)
        {
            for(int x = 0; x < width; x+=meshSimplificationIncrement)
            {
                //lock (heightCurve) {
                    meshData.vertices[vertexIndex] = new Vector3(topLeftX + x, heightCurve.Evaluate(heightMap[x, y]) * heighMultiplier, topLeftZ - y);
                //}
                //the uv array will contain where each vertex of the terrain is with respect with the entire map.
                meshData.uv[vertexIndex] = new Vector2(x / (float)width, y / (float)height);

                //if the vertexes of the 1d array are not on the right or the bottom,
                if(x<(width-1) && y <( height - 1))
                {
                    // add the first triangle of the square
                    meshData.AddTriangles(vertexIndex, vertexIndex + verticesPerLine + 1, vertexIndex + verticesPerLine);
                    //Add the second triangle of the square
                    meshData.AddTriangles(vertexIndex + verticesPerLine + 1, vertexIndex, vertexIndex +1);
                }
                vertexIndex++;
            }
        }
            
        return meshData;
    }

}

public class MeshData
{
    public  Vector3[] vertices;
    public int[] triangles;

    int triangleIndex;
    //to generate uv map for the terrain
    public Vector2[] uv;
   

    public MeshData(int width,int height)
    {
        vertices = new Vector3[width * height];
        triangles = new int[(width - 1) * (height - 1) * 6];
        uv = new Vector2[width * height];
    }

    public void AddTriangles(int a, int b, int c)
    {
        triangles[triangleIndex] = a;
        triangles[triangleIndex + 1] = b;
        triangles[triangleIndex + 2] = c;
        triangleIndex += 3;
    }

    public Mesh CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;

        mesh.RecalculateNormals();

        return mesh;
    }
}

/* triangles are added in this manner:
 * 
 * 0 1 2
 * 3 4 5 
 * 6 7 8 
 * 4 triangles will be made(3-1)*(3-1) each triangle has 3 vertices.
 * first triangle: 0,4,3 
 * second triangle 4,0,1
 * third triangle 1,5,4
 * fourth triangle 5,1,2 and so on. Pattern: vertexes used: currentIndex, currentINdex+width+1,current index +width, 
 * & currentindex+width+1,current index,currentINdex +1
 * */