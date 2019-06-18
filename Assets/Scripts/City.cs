using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class City : MonoBehaviour {
    
    // List of buildings
    public List<GameObject> inner_zone_buildings_prefabs;
    public List<GameObject> middle_zone_buildings_prefabs;
    public List<GameObject> outer_zone_buildings_prefabs;

    // Road prefab
    public GameObject road_prefab;

    public int map_dimensions = 10;
    public Vector2 inner_zone_dimensions;
    public Vector2 middle_zone_dimensions;
    public Vector2 outer_zone_dimensions;

    public GameObject roads_parent;

    // Padding between buildings
    public float padding = 0.15f;


    // Current position to add item
    Vector3 position;
    // Orientation
    Quaternion quaternion = new Quaternion();
    
    Dictionary<Vector3, Building> map;

    // Current zone
    ZONE zone;

    // Selected building index
    int building_index;

    // Gameobject for instanstiated building
    GameObject created_building;


    // Use this for initialization
    void Start () {
        // Setup variables and lists
        Setup();

        // Build city
        StartCoroutine("BuildCity");
    }

    // Update is called once per frame
    void Update () {
	}


    /// <summary>
    /// Build the city according to the dimenstions and the provided prefabs
    /// </summary>
    IEnumerator BuildCity()
    {
        int i = 0;
        int j = 0;
        bool addRoad = false;
        Building building;
        while(i < map_dimensions)
        {
            if (addRoad)
            {
                // Add road
                for (j=0; j<map_dimensions + 2; j++)
                    AddRoad(new Vector3(position.x - 0.5f, 0, j-1), 1);
                addRoad = false;
                i++;
            }
            else
            {
                j = 0;
                position.z = 0;
                // Add buildings
                while (j < map_dimensions)
                {
                    // Check current zone starting from inner zone then middle and then outer
                    if (InZoneDimensions(i, j, inner_zone_dimensions))
                    {
                        // Inner Zone
                        // Add building in inner zone
                        AddBuilding(position, inner_zone_buildings_prefabs, true);
                    }
                    else if (InZoneDimensions(i, j, middle_zone_dimensions))
                    {
                        // Middle Zone 
                        // Add building in middle zone
                        AddBuilding(position, middle_zone_buildings_prefabs, true);
                    }
                    else
                    {
                        // Outer Zone
                        // Add building in outer zone
                        AddBuilding(position, outer_zone_buildings_prefabs, true);
                    }
                    building = created_building.GetComponent<Building>();
                    j += building.size.x;

                    // Update position
                    position.z += building.GetSize().x;


                    yield return new WaitForSeconds(0.3f);
                }

                // Add road in the next iteration
                addRoad = true;
                // Increment i
                i+=2;
            }
            position.x = i;
        }


        StopCoroutine("BuildCity");
    }

    /// <summary>
    /// Instantiate a new random building from the provided list
    /// </summary>
    /// <param name="position">The position for the new building</param>
    /// <param name="buildings">The list with available building in this zone</param>
    /// <param name="randomRotation">Boolean flag on whether to give a random rotation</param>
    void AddBuilding(Vector3 position, List<GameObject> buildings, bool randomRotation)
    {
        building_index = Random.Range(0, buildings.Count);
        created_building = Instantiate(
            buildings[building_index], 
            position, 
            randomRotation ? GetRandomQuaternion() : quaternion, 
            transform);
        created_building.AddComponent<Building>();
    }


    void AddRoad(Vector3 position, int length)
    {
        GameObject road = Instantiate(road_prefab, position, quaternion, roads_parent.transform);
        road.transform.localScale = new Vector3(length, 0.0001f, 1);
    }


    /// <summary>
    /// Get a quaternion with a random rotation in y
    /// </summary>
    /// <returns>A quaternion with a random rotation in y from [0, 90, 180, 270]</returns>
    Quaternion GetRandomQuaternion()
    {
        int y_rotation = UnityEngine.Random.Range(0, 359);
        y_rotation = Mathf.FloorToInt(y_rotation / 90);
        y_rotation = y_rotation * 90;
        return new Quaternion(0, y_rotation, 0, 0);
    }


    /// <summary>
    /// Setup function, initializes variables for the city creation
    /// </summary>
    void Setup()
    {
        // Starting position
        position = Vector3.zero;
        // Starting zone
        zone = ZONE.inner_zone;
        //Initialize map
        map = new Dictionary<Vector3, Building>();
    }

    /// <summary>
    /// Return true if the i,j coords are in the zone's dimensions
    /// </summary>
    /// <param name="i"></param>
    /// <param name="j"></param>
    /// <param name="dimensions">Zone dimensions</param>
    /// <returns></returns>
    bool InZoneDimensions(int i, int j, Vector2 dimensions)
    {
        return (i >= dimensions.x && i <= dimensions.y) && (j >= dimensions.x && j <= dimensions.y);
    }
}
