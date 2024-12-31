using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavAgentController : MonoBehaviour
{
    [SerializeField] NavMeshAgent navAgent;
    [SerializeField] Transform target;

    [SerializeField] List<Transform> spawn_locations = new List<Transform>(3);

    Vector3[] target_spawn_locations = new Vector3[]
    { new Vector3(10f, 0f, -10f),
    new Vector3(0f, 0f, -10f),
    new Vector3(-10f, 0f, -10f)};

    private void Start()
    {
        navAgent = GetComponent<NavMeshAgent>();

        //spawn Agent
        spawnAgent();

        // Reset target position
        spawnTarget();

        navAgent.SetDestination(target.position);
    }

    private void spawnAgent()
    {
        int randomIndex = Random.Range(0, spawn_locations.Count);

        Transform randSpawn = spawn_locations[randomIndex]; 
        Vector3 random_spawn_location = randSpawn.transform.position + new Vector3(0f, 0f, 0f);

        transform.localPosition = random_spawn_location;
        navAgent.Warp(transform.localPosition);

    }

    private void spawnTarget()
    {
        int randomIndex = Random.Range(0, target_spawn_locations.Length);
        //Debug.Log("Random: "+ randomIndex);
        Vector3 randSpawn = target_spawn_locations[randomIndex];
        //Debug.Log("RandSpawn: " + randSpawn);

        target.position = randSpawn;

        //Debug.Log($"Target Position: {target.position}");
    }

    void Update()
    {
        

        // Calculate path to the target
        NavMeshPath path = new NavMeshPath();
        if (NavMesh.CalculatePath(navAgent.transform.position, target.position, NavMesh.AllAreas, path))
        {
            float pathDistance = GetPathDistance(path);
            Debug.Log($"Path Distance to Target: {pathDistance}");
        }
        else
        {
            Debug.LogWarning("Unable to calculate path to the target!");
        }
    }

    private float GetPathDistance(NavMeshPath path)
    {
        float distance = 0f;

        //Iterate through the path's corners
        for (int i = 1; i < path.corners.Length; i++)
        {
            distance += Vector3.Distance(path.corners[i - 1], path.corners[i]);
        }

        return distance;
    }

    private void OnDrawGizmos()
    {
        if (navAgent == null) return;

        NavMeshPath path = new NavMeshPath();
        if (NavMesh.CalculatePath(navAgent.transform.position, target.position, NavMesh.AllAreas, path))
        {
            Gizmos.color = Color.green;
            for (int i = 1; i < path.corners.Length; i++)
            {
                Gizmos.DrawLine(path.corners[i - 1], path.corners[i]);
            }
        }
    }
}

