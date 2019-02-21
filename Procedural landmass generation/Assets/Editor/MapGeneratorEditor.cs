using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

//tel unity that this is a custom editor for object.
[CustomEditor (typeof (MapGenerator))]
public class MapGeneratorEditor : Editor {

    public override void OnInspectorGUI()
    {
        //Getting refernece to object whose inspector we are editing
        MapGenerator mapGen = (MapGenerator)target;

        //draw default inspector
        //if any change is made:
        if (DrawDefaultInspector())
        {
            if (mapGen.AutoUpdate)
            {
                mapGen.mapDisplayCaller();
            }
        }
       // DrawDefaultInspector();

        //if the new created button is pressed, generate the map.
        if (GUILayout.Button("Generate"))
        {
            mapGen.mapDisplayCaller();
        }
      
    }
}
