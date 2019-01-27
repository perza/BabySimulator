using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
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
    private class ConstructedLocations
    {
        public Vector3Int TileCoordinates;
        public GameObject TileGameObject;
    }
    
    public enum Constructables
    {
        env_door,
        obj_armchair,
        obj_bed_2x2,
        obj_bin,
        obj_care_table,
        obj_chair,
        obj_chair_antique,
        obj_chair_normal,
        obj_cradle,
        obj_dishwasher,
        obj_drawer,
        obj_fridge,
        obj_oven,
        obj_plant,
        obj_plant_cactus,
        obj_plant_flower,
        obj_pot,
        obj_sink,
        obj_stool,
        obj_table_livingroom,
        obj_toilet_seat,
        obj_toilet_sink,
        obj_tv_table,
        wall_block,
        wall_thin
    }
    
    public List<EditorConstructable> editorConstructables = new List<EditorConstructable>();

    private Dictionary<string, GameObject> _constructablesDictionary = new Dictionary<string, GameObject>();
    
    public Tilemap MyTilemap;
    private Grid MyGrid;
    private Camera _myCamera;

    private List<ConstructedLocations> _constructedLocationsList = new List<ConstructedLocations>();

    public GameObject EraserPrefab;

    

    private bool _breakBool = false;

    // Start is called before the first frame update
    void Start()
    {
        MyGrid = MyTilemap.GetComponentInParent<Grid>();
        _myCamera = Camera.main;
        InitialiseDictionary();
    }

    private void InitialiseDictionary()
    {
        foreach (var editorConstructable in editorConstructables)
        {
            if(editorConstructable.Prefab != null)
                _constructablesDictionary.Add(editorConstructable.Constructables.ToString(), editorConstructable.Prefab);
        }
    }

    public void StartBuilding(Constructables constructable)
    {
        if (_constructablesDictionary == null)
            return;
        if(!_constructablesDictionary.ContainsKey(constructable.ToString())) return;
        var objectToBuild = _constructablesDictionary[constructable.ToString()];

        _breakBool = true;
        StartCoroutine(BuildingModeLoop(objectToBuild));
    }

    public void StartBuilding(String constructableString)
    {
        if (_constructablesDictionary == null)
            return;
        if(!_constructablesDictionary.ContainsKey(constructableString)) return;
        var objectToBuild = _constructablesDictionary[constructableString];

        _breakBool = true;
        StartCoroutine(BuildingModeLoop(objectToBuild));
    }

    private IEnumerator BuildingModeLoop(GameObject objectToBuild)
    {
        
        Vector3Int latestTilePosition = new Vector3Int(9999, 9999, 0);
        Transform ghostObject = null;
        var worldPosition = Vector3.zero;
        
        yield return new WaitForSeconds(0.2f);
        
        _breakBool = false;
        
        while (true)
        {
            if (Input.GetMouseButtonUp(1) || Input.GetKeyUp(KeyCode.Escape) || _breakBool)
            {
                
                break;
            }
            
            var ray = _myCamera.ScreenPointToRay(Input.mousePosition);
            var worldPoint = ray.GetPoint(-ray.origin.y / ray.direction.y);
        
            var tilePosition = MyGrid.WorldToCell(worldPoint);

            if (tilePosition != latestTilePosition)
            {
                latestTilePosition = tilePosition;
                worldPosition = MyTilemap.GetCellCenterWorld(tilePosition);
                if(ghostObject == null)
                    ghostObject = GetNewObject(objectToBuild, worldPosition, Quaternion.identity);
                else
                    ghostObject.position = worldPosition;
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                if (ghostObject != null)
                {
                    ghostObject.Rotate(Vector3.up, 90);
                }
            }

            if (Input.GetKeyDown(KeyCode.Q))
            {
                if (ghostObject != null)
                {
                    ghostObject.Rotate(Vector3.up, -90);
                }
            }

            if (Input.GetMouseButtonUp(0))
            {
                var success =
                    ResourceHandler.Instance.Purchase(objectToBuild.GetComponentInChildren<Consumable>().prize);
                
                if(success)
                {
                    var rotation = Quaternion.identity;
                    if (ghostObject != null)
                    {
                        rotation = ghostObject.rotation;
                        _constructedLocationsList.Add(new ConstructedLocations
                        {
                            TileCoordinates = latestTilePosition, TileGameObject = ghostObject.gameObject
                        });
                    }

                    ghostObject = GetNewObject(objectToBuild, worldPosition, rotation);
                }
            }
            
            yield return null;
        }
        
        if (ghostObject != null) Destroy(ghostObject.gameObject);
        
        yield return null;
    }

    private Transform GetNewObject(GameObject objectToBuild, Vector3 pos, Quaternion rotation) => Instantiate(objectToBuild, pos, rotation).transform;

    public void StartDestructing()
    {
        _breakBool = true;
        StartCoroutine(DestructionModeLoop());
    }

    private IEnumerator DestructionModeLoop()
    {
        
        var latestTilePosition = new Vector3Int(9999, 9999, 0);
        Transform ghostObject = null;

        yield return new WaitForSeconds(0.2f);
        
        _breakBool = false;
        
        while (true)
        {
            if (Input.GetMouseButtonUp(1) || Input.GetKeyUp(KeyCode.Escape) || _breakBool)
            {
                break;
            }
            
            var ray = _myCamera.ScreenPointToRay(Input.mousePosition);
            var worldPoint = ray.GetPoint(-ray.origin.y / ray.direction.y);
        
            var tilePosition = MyGrid.WorldToCell(worldPoint);
            
            if (tilePosition != latestTilePosition)
            {
                latestTilePosition = tilePosition;
                var worldPosition = MyTilemap.GetCellCenterWorld(tilePosition);
                if(ghostObject == null)
                    ghostObject = GetNewObject(EraserPrefab, worldPosition, Quaternion.identity);
                else
                    ghostObject.position = worldPosition;
            }
            
            if (Input.GetMouseButtonUp(0))
            {
                Deconstruct(tilePosition);
            }
            
            yield return null;
        }
        
        if (ghostObject != null) Destroy(ghostObject.gameObject);
        
        yield return null;
    }

    private void Deconstruct(Vector3Int tileCoordinate)
    {
        var construct = _constructedLocationsList.Find(x => x.TileCoordinates == tileCoordinate);

        if (construct != null)
        {
            ResourceHandler.Instance.GainMoney(construct.TileGameObject.GetComponentInChildren<Consumable>().prize / 2);
            Destroy(construct.TileGameObject);
            _constructedLocationsList.Remove(construct);
        }
    }
}
