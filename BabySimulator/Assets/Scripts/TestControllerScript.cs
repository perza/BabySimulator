using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TestControllerScript : MonoBehaviour
{
    public Tile wallTile;
    public Tilemap Tilemap1;
    public GameObject WallPrefab;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonUp(0))
            RayTest();
    }

    private void RayTest()
    {
        
        // Ray ray = GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
        //
        // var hit = Physics2D.Raycast(ray.origin, ray.direction);
        //
        //
        //
        // var currentHighlighted = Tilemap1.WorldToCell(hit.transform.position);

        // Vector3 pz = GetComponent<Camera>().ScreenToWorldPoint(Input.mousePosition);
        // pz.y = 0;

        // convert mouse click's position to Grid position
        // GridLayout gridLayout = Tilemap1.transform.parent.GetComponentInParent<GridLayout>();
        // Vector3Int cellPosition = Tilemap1.WorldToCell(pz);

        // set selectedUnit to clicked location on grid
        // selectedUnit.setLocation(cellPosition);
        
        // Debug.Log(currentHighlighted);
        //
        // Tilemap1.SetTile(currentHighlighted, wallTile);
        // Tilemap1.CellToWorld()
        // Tilemap1.
        
        
        Grid grid = Tilemap1.GetComponentInParent<Grid>();
        // save the camera as public field if you using not the main camera
        Ray ray = GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
        // get the collision point of the ray with the z = 0 plane
        Vector3 worldPoint = ray.GetPoint(-ray.origin.y / ray.direction.y);
        
        Vector3Int position = grid.WorldToCell(worldPoint);
        
        var wall = Instantiate(WallPrefab, Tilemap1.GetCellCenterWorld(position), Quaternion.identity);
    }
}
