using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapDisplay : MonoBehaviour {

    public Renderer textureRenderer;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;

    public void DrawTexture(Texture2D texture)
    {
       
        //setting the texture so that  it can be seen without gameMode on.
        textureRenderer.sharedMaterial.mainTexture = texture;

        //set size of plane to width and height
        textureRenderer.transform.localScale = new Vector3(texture.width, 1, texture.height);


    }

    public void DrawMesh(MeshData meshData, Texture2D texture)
    {
        meshFilter.sharedMesh = meshData.CreateMesh();
        
        meshRenderer.sharedMaterial.mainTexture = texture;
    }

}
