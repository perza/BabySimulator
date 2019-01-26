using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ConstructionController : Singleton<ConstructionController>
{
    [Serializable]
    public class EditorConstructable
    {
        public Constructables Constructables;
        public GameObject Prefab;
    }
    
    public enum Constructables
    {
        Wall,
        Chair
    }
    
    public List<EditorConstructable> EditorConstructables = new List<EditorConstructable>();

    private Dictionary<string, GameObject> _constructablesDictionary = new Dictionary<string, GameObject>();
    
    public Tilemap MyTilemap;
    private Grid MyGrid;
    private Camera _myCamera;

    // Start is called before the first frame update
    void Start()
    {
        MyGrid = MyTilemap.GetComponentInParent<Grid>();
        _myCamera = Camera.main;
        InitialiseDictionary();
    }

    private void InitialiseDictionary()
    {
        foreach (var editorConstructables in EditorConstructables)
        {
            _constructablesDictionary.Add(editorConstructables.Constructables.ToString(), editorConstructables.Prefab);
        }
    }

    public void StartBuilding(String constructableString)
    {
        if (_constructablesDictionary != null)
        {
            if(!_constructablesDictionary.ContainsKey(constructableString)) return;
            var objectToBuild = _constructablesDictionary[constructableString];

            StartCoroutine(BuildingModeLoop(objectToBuild));
        }
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
                    ghostWall = GetNewObject(objectToBuild, worldPosition, Quaternion.identity);
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
                var rotation = Quaternion.identity;
                if(ghostWall != null)
                    rotation = ghostWall.rotation;
                ghostWall = GetNewObject(objectToBuild, worldPosition, rotation);
            }
            
            yield return null;
        }
        
        yield return null;
    }

    private Transform GetNewObject(GameObject objectToBuild, Vector3 pos, Quaternion rotation) => Instantiate(objectToBuild, pos, rotation).transform;
    
    
}
