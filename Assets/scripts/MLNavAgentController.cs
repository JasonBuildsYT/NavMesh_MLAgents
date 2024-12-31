using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine.AI;

public class MLNavAgentController : Agent
{
    [SerializeField] NavMeshAgent navAgent;
    [SerializeField] Transform target;
    private NavMeshPath path;
    private float previousDistanceToTarget;
    private float currentDistanceToTarget;

    [SerializeField] List<Transform> spawn_locations = new List<Transform>(3);

    Vector3[] target_spawn_locations = new Vector3[]
    { new Vector3(10f, 0f, -5f),
    new Vector3(0f, 0f, 10f),
    new Vector3(-10f, 0f, 10f)};

    private int step_count = 0;
    private int max_steps = 15000;
    private bool notbeenwithin8, notbeenwithin4, notbeenwithin2 = true;
    Vector3 last_position;

    public override void Initialize()
    {
        navAgent = GetComponent<NavMeshAgent>();  
    }

    public override void OnEpisodeBegin()
    {
        notbeenwithin8 = true; 
        notbeenwithin4 = true; 
        notbeenwithin2 = true;

        spawnAgent();

        spawnTarget();

        path = new NavMeshPath();
        NavMesh.CalculatePath(navAgent.transform.position, target.position, NavMesh.AllAreas, path);
        previousDistanceToTarget = GetPathDistance(path);

        step_count = 0;
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
        Vector3 randSpawn = target_spawn_locations[randomIndex];

        target.localPosition = randSpawn;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Agent's current position
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(navAgent.velocity);
        sensor.AddObservation(transform.forward);

        // Agent's destination
        sensor.AddObservation(target.localPosition);
        //Target's relative position
        sensor.AddObservation(target.localPosition - transform.localPosition);
        //Agent's angular distance between forward and target
        Vector3 to_target = (target.localPosition - transform.localPosition).normalized;
        float angle_to_target = Vector3.Angle(transform.forward, to_target) / 180f; // Normalize
        sensor.AddObservation(angle_to_target);

        // Distance to destination
        NavMesh.CalculatePath(navAgent.transform.position, target.position, NavMesh.AllAreas, path);
        sensor.AddObservation(GetPathDistance(path));
        //sensor.AddObservation(NavMesh.CalculatePath(navAgent.transform.position, target.position, 
                              //  NavMesh.AllAreas, path));


    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        step_count++;
        float move_rotate = actions.ContinuousActions[0];
        float move_forward = actions.ContinuousActions[1];


        transform.Rotate(Vector3.up, move_rotate * navAgent.angularSpeed * Time.deltaTime);

        Vector3 move = transform.forward * move_forward * navAgent.speed * Time.deltaTime;
        transform.position += move;

        // Recalculate path
        path = new NavMeshPath();
        NavMesh.CalculatePath(navAgent.transform.position, target.position, NavMesh.AllAreas, path);
        currentDistanceToTarget = GetPathDistance(path);

        

        if (currentDistanceToTarget < previousDistanceToTarget)
        {
            //Positive reward for reducing distance
            AddReward((previousDistanceToTarget - currentDistanceToTarget) * 1f);

            //Update previous distance
            previousDistanceToTarget = currentDistanceToTarget;
        }
        else
        {
            //Small penalty for increasing distance
            //AddReward(-0.01f);
        }

        //GetPathDistance(path);

        //Penalize for taking too long
        if (step_count >= max_steps)
        {
            //AddReward(-0.5f);
            EndEpisode();
        }


        if (currentDistanceToTarget < 8f && notbeenwithin8)
        {
            //AddReward(1.0f); // Encourage proximity
            notbeenwithin8 = false;
        }

        if (currentDistanceToTarget < 4f && notbeenwithin4)
        {
            //AddReward(2.0f); // Encourage proximity
            notbeenwithin4 = false;
        }

        if (currentDistanceToTarget < 2f && notbeenwithin2)
        {
            //AddReward(3.0f); // Encourage proximity
            notbeenwithin2 = false;
        }

        if (Vector3.Distance(transform.position, last_position) < 0.1f)
        {
            //AddReward(-0.1f); // Penalize for no movement
        }
        last_position = transform.localPosition;

    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxis("Horizontal");
        continuousActions[1] = Input.GetAxis("Vertical");
    }

    private void OnTriggerEnter(Collider other)
    {
        //Check if the agent touches the target
        if (other.gameObject.tag == "Target")
        {
            AddReward(50.0f);
            EndEpisode(); 
        }
    }
    void Update()
    {
        //Calculate path to the target
        NavMeshPath path = new NavMeshPath();
        if (NavMesh.CalculatePath(navAgent.transform.position, target.position, NavMesh.AllAreas, path))
        {
            float pathDistance = GetPathDistance(path);
            //Debug.Log($"Path Distance to Target: {pathDistance}");
        }
        else
        {
            //Debug.LogWarning("Unable to calculate path to the target!");
        }
    }

    private float GetPathDistance(NavMeshPath path)
    {
        float distance = 0f;

        // Iterate through the path's corners
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
