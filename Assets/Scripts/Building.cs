using UnityEngine;
using System.Collections;

public class Building : MonoBehaviour
{
    public BUILDING type;
    public Vector2Int size;

    void Awake()
    {
        // Calculate and save size
        MeshRenderer mesh = GetComponent<MeshRenderer>();
        size = new Vector2Int(Mathf.CeilToInt(mesh.bounds.size.x), Mathf.CeilToInt(mesh.bounds.size.z));
    }

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetBuildingType(int type_index)
    {
        type = (BUILDING) type_index;
    }

    public Vector2Int GetSize()
    {
        return size;
    }

    public BUILDING GetBuildingType()
    {
        return type;
    }
}
