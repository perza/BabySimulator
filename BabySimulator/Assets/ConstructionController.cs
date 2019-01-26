using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ConstructionController : Singleton<ConstructionController>
{
    public Tilemap MyTilemap;
    private Grid MyGrid;
    private Camera _myCamera;

    public GameObject WallPrefab;
    public GameObject ChairPrefab;

    // Start is called before the first frame update
    void Start()
    {
        MyGrid = MyTilemap.GetComponentInParent<Grid>();
        _myCamera = Camera.main;
    }

    // Update is called once per frame
    // void Update()
    // {
    //     
    // }

    private void StartBuilding(GameObject objectToBuild)
    {
        StartCoroutine(BuildingModeLoop(objectToBuild));
    }

    private IEnumerator BuildingModeLoop(GameObject objectToBuild)
    {
        Vector3Int latestTilePosition = new Vector3Int(9999, 9999, 0);
        Transform ghostWall = null;
        Vector3 worldPosition = Vector3.zero;
        
        yield return new WaitForSeconds(0.2f);
        
        while (true)
        {
            if (Input.GetMouseButtonUp(1) || Input.GetKeyUp(KeyCode.Escape))
            {
                if (ghostWall != null) Destroy(ghostWall.gameObject);
                break;
            }
            
            var ray = _myCamera.ScreenPointToRay(Input.mousePosition);
            var worldPoint = ray.GetPoint(-ray.origin.y / ray.direction.y);
        
            var tilePosition = MyGrid.WorldToCell(worldPoint);

            if (tilePosition != latestTilePosition)
            {
                latestTilePosition = tilePosition;
                worldPosition = MyTilemap.GetCellCenterWorld(tilePosition);
                if(ghostWall == null)
                    ghostWall = GetNewObject(objectToBuild, worldPosition);
                else
                    ghostWall.position = worldPosition;
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                if (ghostWall != null)
                {
                    ghostWall.Rotate(Vector3.up, 90);
                }
            }

            if (Input.GetKeyDown(KeyCode.Q))
            {
                if (ghostWall != null)
                {
                    ghostWall.Rotate(Vector3.up, -90);
                }
            }

            if (Input.GetMouseButtonUp(0))
            {
                ghostWall = GetNewObject(objectToBuild, worldPosition);
            }
            
            yield return null;
        }
        
        yield return null;
    }

    private Transform GetNewObject(GameObject objectToBuild, Vector3 pos) => Instantiate(objectToBuild, pos, Quaternion.identity).transform;
    
    public void StartBuildingWall()
    {
        StartBuilding(WallPrefab);
    }

    public void StartBuildingChair()
    {
        StartBuilding(ChairPrefab);
    }
    
    
}
