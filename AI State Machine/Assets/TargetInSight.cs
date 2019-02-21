using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TargetInSight : MonoBehaviour {

    NavMeshAgent agent;
    public Transform target;
    bool InSight;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        InSight = false;
    }
	
	// Update is called once per frame
	void Update () {
        if (!InSight)
            return;

        agent.SetDestination(target.position);
    }

    void SetInSight()
    {
        InSight = true;
    }
    void SetNotInsight()
    {
        InSight = false;
    }
}
