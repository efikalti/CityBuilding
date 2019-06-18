using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using MLAgents;

[RequireComponent(typeof(CityDL))]
public class CityBuildingAgent : Agent {

    public float timeBetweenDecisions = 0.5f;

    private CityDL city;

    private int[] neighbors;
    private int[] diagonal_neighbors;

    private ZONE currentZone;

    public int inner_zone_range = 2;
    public int middle_zone_range = 5;
    public int outer_zone_range = 8;

    private int x;
    private int z;
    Vector3Int position;

    int map_dimensions;
    
    public static readonly ReadOnlyCollection<int[]> possible_areas = new ReadOnlyCollection<int[]>(
      new[] {
        new int [] {0, 1, 2}, // Possible area in zone 1
        new int [] {5, 3, 4}, // Possible area in zone 2
        new int [] {7, 8, 6}, // Possible area in zone 3
      }
    );


    public override void InitializeAgent()
    {
        // Get city variable
        city = GetComponent<CityDL>();
        map_dimensions = city.map_dimensions;


        // Initialize neighbor array
        neighbors = new int[4];
        diagonal_neighbors = new int[4];

        // Start corouting for desicion
        StartCoroutine("Decision");
    }

    public override void CollectObservations()
    {
        // Add current zone observation
        currentZone = city.GetZone();
        AddVectorObs((int) currentZone);

        // Add neighbor observations
        neighbors = city.GetNeighbors(position);
        foreach (int neighbor in neighbors) AddVectorObs(neighbor);

        // Add diagonal neighbor observations
        diagonal_neighbors = city.GetDiagonals(position);
        foreach (int neighbor in diagonal_neighbors) AddVectorObs(neighbor);
        
    }

    public override void AgentAction(float[] vectorAction, string textAction)
    {
        // Get building index
        int index = Mathf.FloorToInt(vectorAction[0]);
        //print(index);

        city.AddBuilding(index, position);

        SetReward(CalculateReward((BUILDING) index));

        if (x + 1 >= map_dimensions && z + 1 >= map_dimensions)
        {
            StopCoroutine("Decision");
            Done();
        }
    }


    public override void AgentReset()
    {
        city.ResetCity();
        x = 0;
        z = 0;
        StartCoroutine("Decision");
    }

    IEnumerator Decision()
    {
        x = 0;
        z = 0;
        int map_dimensions = city.map_dimensions;
        while (x < map_dimensions)
        {
            z = 0;
            while (z < map_dimensions)
            {
                position = new Vector3Int(x, 0, z);
                RequestDecision();
                yield return new WaitForSeconds(0.5f);
                z++;
            }
            x++;
        }
        StopCoroutine("Decision");
    }

    private float CalculateReward(BUILDING building)
    {
        float reward = 0.1f;

        if (building == BUILDING.road) // The selected index is a road
        {
            // Check horizontal and vertical
            foreach(int neighbor in neighbors)
            {
                if (neighbor == (int)building) reward += 0.25f;
            }
            // Check diagonals
            foreach (int neighbor in diagonal_neighbors)
            {
                if (neighbor == (int)building) reward -= 0.5f;
            }
        }
        else { // The selected index is a building
            int index = (int) building;

            // Check if the building is right for this zone
            bool wrong_zone_building;

            if (currentZone == ZONE.inner_zone) // inner zone
                wrong_zone_building = (index > inner_zone_range);
            else if (currentZone == ZONE.middle_zone) // middle zone
                wrong_zone_building = (index > middle_zone_range || index <= inner_zone_range);
            else // Outer zone
                wrong_zone_building = (index <= middle_zone_range);

            if (wrong_zone_building) reward -= 1; // Wrong building for this zone 
            else reward += 0.5f; // Right building for this zone

            // Check building next of this one
            if (neighbors[0] == index) reward -= 0.25f; // Building on the left 
            if (neighbors[2] == index) reward -= 0.25f; // Building on the right

            // Check if there is at least one road adjacent
            bool found = false;
            int i = 0;
            while(!found && i < diagonal_neighbors.Length)
            {
                if (diagonal_neighbors[i] == (int)BUILDING.road) found = true;
                i++;
            }
            i = 0;
            while (!found && i < diagonal_neighbors.Length)
            {
                if (diagonal_neighbors[i] == (int)BUILDING.road) found = true;
                i++;
            }

            if (found) reward += 0.5f;

            // Check buildings above
            int buildings_found = 0;
            if (neighbors[1] != (int)BUILDING.road && neighbors[1] >= 0) buildings_found++;
            if (diagonal_neighbors[0] != (int)BUILDING.road && neighbors[1] >= 0) buildings_found++;
            if (diagonal_neighbors[1] != (int)BUILDING.road && neighbors[1] >= 0) buildings_found++;
            // All items above this one are buildings
            if (buildings_found == 3) reward -= 0.5f;
        }

        return Mathf.Clamp(reward, -1, 1);
    }

}
