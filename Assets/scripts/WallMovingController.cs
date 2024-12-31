using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallMovingController : MonoBehaviour
{
    public float speed = 1.3f;
    public float move_distance = 5f;

    public Transform[] walls;

    private Vector3[] start_positions;

    private void Start()
    {
        start_positions = new Vector3[walls.Length];

        for (int i = 0; i < walls.Length; i++)
        {
            if (walls[i] != null)
                start_positions[i] = walls[i].position;
        }
    }

    void Update()
    {
        for (int i = 0; i < walls.Length; i++)
        {
            if (walls[i] != null)
            {
                float offset = Mathf.Sin(Time.time * speed + i) * move_distance; // adding i allows
                                                                                 // walls to not be
                                                                                 // synced
                walls[i].position = new Vector3(start_positions[i].x + offset, 
                                                start_positions[i].y, start_positions[i].z);
            }
        }
    }
}
