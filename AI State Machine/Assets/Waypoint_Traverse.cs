using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Waypoint_Traverse : MonoBehaviour {

    public int NoOFWaypoints;
    NavMeshAgent agent;
    public Transform[] targets;

    List<int> Destinations;
    int currentDestination=1;

    
    

	// Use this for initialization
	void Start () {

        agent = GetComponent<NavMeshAgent>();

        Destinations = new List<int>();
       for(int x = 0; x < NoOFWaypoints; x++)
        {
            Destinations.Add(x);
        }

        if (!TestUpdate())
            print("Targets available");
        else
            print("add  targets");
        
	}
	
    bool TestUpdate()
    {
        bool Flag = targets == null ? true : false;
        return Flag;
    }

	// Update is called once per frame
	void Update () {

     
        if (CheckReach())
        {
            UpdateDestination();
            print("reached");
        }
        
        agent.SetDestination(targets[currentDestination].position);
                      
	}

    bool CheckReach()
    {
        bool Reached = false;

        float deltaX = transform.position.x - targets[currentDestination].position.x;
        float deltaY = transform.position.y - targets[currentDestination].position.y;
        float deltaZ = transform.position.z - targets[currentDestination].position.z;
        
        if (Mathf.Abs(deltaX) < 0.5 && Mathf.Abs(deltaZ )< 0.5)
             Reached = true;
        return Reached;

    }

    void UpdateDestination()
    {
        if(currentDestination!=2)
            currentDestination++;
        else
        {
            currentDestination = 0;
        }
    }
}
