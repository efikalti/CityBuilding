using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CityDL : MonoBehaviour
{

    // List of buildings
    public List<GameObject> building_prefabs;

    public int map_dimensions = 10;
    public Vector2Int inner_zone_dimensions;
    public Vector2Int middle_zone_dimensions;
    public Vector2Int outer_zone_dimensions;

    public GameObject roads_parent;
    public GameObject inner_zone_parent;
    public GameObject middle_zone_parent;
    public GameObject outer_zone_parent;

    // Padding between buildings
    public float padding = 0.15f;

    private ZONE zone = ZONE.outer_zone;

    // Orientation
    Quaternion quaternion = new Quaternion();

    Dictionary<Vector3, Building> map;

    // Gameobject for instanstiated building
    GameObject created_building;
    Building created_script;
    
    // Use this for initialization
    void Start()
    {
        //Initialize map
        map = new Dictionary<Vector3, Building>();
    }

    // Update is called once per frame
    void Update()
    {
    }

    public ZONE GetZone()
    {
        return zone;
    }

    /// <summary>
    /// Instantiate a new random building from the provided list
    /// </summary>
    /// <param name="position">The position for the new building</param>
    /// <param name="buildings">The list with available building in this zone</param>
    /// <param name="randomRotation">Boolean flag on whether to give a random rotation</param>
    public void AddBuilding(int index, Vector3Int position)
    {
        if (InZoneLimits(position.x, position.z, inner_zone_dimensions)) zone = ZONE.inner_zone;
        else if (InZoneLimits(position.x, position.z, middle_zone_dimensions)) zone = ZONE.middle_zone;
        else zone = ZONE.outer_zone;


        Transform parent;
        switch (zone)
        {
            case ZONE.inner_zone:
                parent = inner_zone_parent.transform;
                break;
            case ZONE.middle_zone:
                parent = middle_zone_parent.transform;
                break;
            case ZONE.outer_zone:
                parent = outer_zone_parent.transform;
                break;
            default:
                parent = transform;
                break;
        }
        created_building = Instantiate(
            building_prefabs[index],
            position,
            quaternion,
            parent);
        created_script = created_building.AddComponent<Building>();
        created_script.SetBuildingType(index);
        map.Add(position, created_script);
    }

    public int[] GetNeighbors(Vector3 position)
    {
        int[] neighbors = new int[4];

        // Neighbor left
        neighbors[0] = GetNeighborIndex(new Vector3(position.x - 1, position.y, position.z));
        // Neighbor up
        neighbors[1] = GetNeighborIndex(new Vector3(position.x, position.y, position.z + 1));
        // Neighbor right
        neighbors[2] = GetNeighborIndex(new Vector3(position.x + 1, position.y, position.z));
        // Neighbor down
        neighbors[3] = GetNeighborIndex(new Vector3(position.x, position.y, position.z - 1));

        return neighbors;
    }

    public int[] GetDiagonals(Vector3 position)
    {
        int[] diagonals = new int[4];

        // Neighbor up and left
        diagonals[0] = GetNeighborIndex(new Vector3(position.x - 1, position.y, position.z + 1));
        // Neighbor up and right
        diagonals[1] = GetNeighborIndex(new Vector3(position.x + 1, position.y, position.z + 1));
        // Neighbor down and right
        diagonals[2] = GetNeighborIndex(new Vector3(position.x + 1, position.y, position.z - 1));
        // Neighbor down and left
        diagonals[3] = GetNeighborIndex(new Vector3(position.x - 1, position.y, position.z - 1));

        return diagonals;
    }

    int GetNeighborIndex(Vector3 position)
    {
        if (map.ContainsKey(position))
            return (int) map[position].GetBuildingType();
        return -1;
    }

    /// <summary>
    /// Return true if the i,j coords are in the zone's dimensions
    /// </summary>
    /// <param name="i"></param>
    /// <param name="j"></param>
    /// <param name="dimensions">Zone dimensions</param>
    /// <returns></returns>
    bool InZoneLimits(int i, int j, Vector2 dimensions)
    {
        return (i >= dimensions.x && i <= dimensions.y) && (j >= dimensions.x && j <= dimensions.y);
    }

    public void ResetCity()
    {
        map.Clear();
        foreach (Transform child in roads_parent.transform) Destroy(child.gameObject);
        foreach (Transform child in inner_zone_parent.transform) Destroy(child.gameObject);
        foreach (Transform child in middle_zone_parent.transform) Destroy(child.gameObject);
        foreach (Transform child in outer_zone_parent.transform) Destroy(child.gameObject);
    }
}
