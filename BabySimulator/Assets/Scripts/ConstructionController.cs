using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ConstructionController : Singleton<ConstructionController>
{
    // Log object positions and rotations
    // String log = "";

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
        InitHouse();
    }

    private void InitialiseDictionary()
    {
        foreach (var editorConstructable in editorConstructables)
        {
            if(editorConstructable.Prefab != null)
                _constructablesDictionary.Add(editorConstructable.Constructables.ToString(), editorConstructable.Prefab);
        }
    }

    private void InitHouse()
    {
        GameObject wall = _constructablesDictionary[Constructables.wall_thin.ToString()];

        GetNewObject(wall, new Vector3(-7.0f, 0.0f, -1.0f), new Quaternion(0.0f, 0.0f, 0.0f, 1.0f));
        GetNewObject(wall, new Vector3(-7.0f, 0.0f, 1.0f), new Quaternion(0.0f, 0.0f, 0.0f, 1.0f));
        GetNewObject(wall, new Vector3(-7.0f, 0.0f, 3.0f), new Quaternion(0.0f, 0.0f, 0.0f, 1.0f));
        GetNewObject(wall, new Vector3(-7.0f, 0.0f, 5.0f), new Quaternion(0.0f, 0.0f, 0.0f, 1.0f));
        GetNewObject(wall, new Vector3(-5.0f, 0.0f, 5.0f), new Quaternion(0.0f, -0.7f, 0.0f, 0.7f));
        GetNewObject(wall, new Vector3(-3.0f, 0.0f, 5.0f), new Quaternion(0.0f, -0.7f, 0.0f, 0.7f));
        GetNewObject(wall, new Vector3(-1.0f, 0.0f, 5.0f), new Quaternion(0.0f, -0.7f, 0.0f, 0.7f));
        GetNewObject(wall, new Vector3(1.0f, 0.0f, 5.0f), new Quaternion(0.0f, -0.7f, 0.0f, 0.7f));
        GetNewObject(wall, new Vector3(3.0f, 0.0f, 5.0f), new Quaternion(0.0f, -0.7f, 0.0f, 0.7f));
        GetNewObject(wall, new Vector3(5.0f, 0.0f, 5.0f), new Quaternion(0.0f, -1.0f, 0.0f, 0.0f));
        GetNewObject(wall, new Vector3(5.0f, 0.0f, 3.0f), new Quaternion(0.0f, -1.0f, 0.0f, 0.0f));
        GetNewObject(wall, new Vector3(5.0f, 0.0f, 1.0f), new Quaternion(0.0f, -1.0f, 0.0f, 0.0f));
        GetNewObject(wall, new Vector3(5.0f, 0.0f, -1.0f), new Quaternion(0.0f, -1.0f, 0.0f, 0.0f));
        GetNewObject(wall, new Vector3(5.0f, 0.0f, -5.0f), new Quaternion(0.0f, -1.0f, 0.0f, 0.0f));
        GetNewObject(wall, new Vector3(3.0f, 0.0f, -5.0f), new Quaternion(0.0f, -0.7f, 0.0f, -0.7f));
        GetNewObject(wall, new Vector3(1.0f, 0.0f, -5.0f), new Quaternion(0.0f, -0.7f, 0.0f, -0.7f));
        GetNewObject(wall, new Vector3(-1.0f, 0.0f, -5.0f), new Quaternion(0.0f, -0.7f, 0.0f, -0.7f));
        GetNewObject(wall, new Vector3(-3.0f, 0.0f, -5.0f), new Quaternion(0.0f, -0.7f, 0.0f, -0.7f));
        GetNewObject(wall, new Vector3(-5.0f, 0.0f, -5.0f), new Quaternion(0.0f, -0.7f, 0.0f, -0.7f));
        GetNewObject(wall, new Vector3(-7.0f, 0.0f, -5.0f), new Quaternion(0.0f, 0.0f, 0.0f, -1.0f));
        GetNewObject(wall, new Vector3(-7.0f, 0.0f, -3.0f), new Quaternion(0.0f, 0.0f, 0.0f, -1.0f));
        GetNewObject(wall, new Vector3(5.0f, 0.0f, -5.0f), new Quaternion(0.0f, 0.7f, 0.0f, 0.7f));
        GetNewObject(wall, new Vector3(7.0f, 0.0f, -5.0f), new Quaternion(0.0f, 0.7f, 0.0f, 0.7f));
        GetNewObject(wall, new Vector3(9.0f, 0.0f, -5.0f), new Quaternion(0.0f, 0.7f, 0.0f, 0.7f));
        GetNewObject(wall, new Vector3(9.0f, 0.0f, -5.0f), new Quaternion(0.0f, 0.0f, 0.0f, 1.0f));
        GetNewObject(wall, new Vector3(9.0f, 0.0f, -3.0f), new Quaternion(0.0f, 0.0f, 0.0f, 1.0f));
        GetNewObject(wall, new Vector3(9.0f, 0.0f, -1.0f), new Quaternion(0.0f, 0.0f, 0.0f, 1.0f));
        GetNewObject(wall, new Vector3(9.0f, 0.0f, 1.0f), new Quaternion(0.0f, 0.0f, 0.0f, 1.0f));
        GetNewObject(wall, new Vector3(9.0f, 0.0f, 3.0f), new Quaternion(0.0f, 0.0f, 0.0f, 1.0f));
        GetNewObject(wall, new Vector3(9.0f, 0.0f, 5.0f), new Quaternion(0.0f, 0.0f, 0.0f, 1.0f));
        GetNewObject(wall, new Vector3(5.0f, 0.0f, 5.0f), new Quaternion(0.0f, -0.7f, 0.0f, 0.7f));
        GetNewObject(wall, new Vector3(9.0f, 0.0f, 5.0f), new Quaternion(0.0f, -0.7f, 0.0f, 0.7f));
        GetNewObject(wall, new Vector3(9.0f, 0.0f, 7.0f), new Quaternion(0.0f, 0.0f, 0.0f, -1.0f));
        GetNewObject(wall, new Vector3(9.0f, 0.0f, 9.0f), new Quaternion(0.0f, 0.0f, 0.0f, -1.0f));
        GetNewObject(wall, new Vector3(9.0f, 0.0f, 13.0f), new Quaternion(0.0f, 0.0f, 0.0f, -1.0f));
        GetNewObject(wall, new Vector3(9.0f, 0.0f, 13.0f), new Quaternion(0.0f, 0.7f, 0.0f, -0.7f));
        GetNewObject(wall, new Vector3(7.0f, 0.0f, 13.0f), new Quaternion(0.0f, 0.7f, 0.0f, -0.7f));
        GetNewObject(wall, new Vector3(5.0f, 0.0f, 13.0f), new Quaternion(0.0f, 0.7f, 0.0f, -0.7f));
        GetNewObject(wall, new Vector3(3.0f, 0.0f, 13.0f), new Quaternion(0.0f, 0.7f, 0.0f, -0.7f));
        GetNewObject(wall, new Vector3(3.0f, 0.0f, 13.0f), new Quaternion(0.0f, 1.0f, 0.0f, 0.0f));
        GetNewObject(wall, new Vector3(3.0f, 0.0f, 7.0f), new Quaternion(0.0f, 1.0f, 0.0f, 0.0f));

        GetNewObject(wall, new Vector3(-7.0f, 0.0f, 11.0f), new Quaternion(0.0f, 0.0f, 0.0f, 1.0f)); 
        GetNewObject(wall, new Vector3(9.0f, 0.0f, 11.0f), new Quaternion(0.0f, 0.0f, 0.0f, 1.0f));

        GetNewObject(wall, new Vector3(1.0f, 0.0f, 13.0f), new Quaternion(0.0f, 0.7f, 0.0f, -0.7f));
        GetNewObject(wall, new Vector3(-1.0f, 0.0f, 13.0f), new Quaternion(0.0f, 0.7f, 0.0f, -0.7f));
        GetNewObject(wall, new Vector3(-3.0f, 0.0f, 13.0f), new Quaternion(0.0f, 0.7f, 0.0f, -0.7f));
        GetNewObject(wall, new Vector3(-5.0f, 0.0f, 13.0f), new Quaternion(0.0f, 0.7f, 0.0f, -0.7f));
        GetNewObject(wall, new Vector3(-7.0f, 0.0f, 7.0f), new Quaternion(0.0f, 0.0f, 0.0f, 1.0f));
        GetNewObject(wall, new Vector3(-7.0f, 0.0f, 9.0f), new Quaternion(0.0f, 0.0f, 0.0f, 1.0f));
        GetNewObject(wall, new Vector3(-7.0f, 0.0f, 13.0f), new Quaternion(0.0f, 0.0f, 0.0f, 1.0f));
        GetNewObject(wall, new Vector3(-7.0f, 0.0f, 1.0f), new Quaternion(0.0f, 0.0f, 0.0f, 1.0f));

        GameObject oven = _constructablesDictionary[Constructables.obj_oven.ToString()];
        GameObject dishwasher = _constructablesDictionary[Constructables.obj_dishwasher.ToString()];
        GameObject fridge = _constructablesDictionary[Constructables.obj_fridge.ToString()];
        GameObject chair = _constructablesDictionary[Constructables.obj_chair.ToString()];
        GameObject chairNormal = _constructablesDictionary[Constructables.obj_chair_normal.ToString()];
        GameObject cradle = _constructablesDictionary[Constructables.obj_cradle.ToString()];
        GameObject bed = _constructablesDictionary[Constructables.obj_bed_2x2.ToString()];

        GetNewObject(oven, new Vector3(-3.0f, 0.0f, 5.0f), new Quaternion(0.0f, -1.0f, 0.0f, 0.0f));
        GetNewObject(dishwasher, new Vector3(-5.0f, 0.0f, 5.0f), new Quaternion(0.0f, -1.0f, 0.0f, 0.0f));
        GetNewObject(dishwasher, new Vector3(-1.0f, 0.0f, 5.0f), new Quaternion(0.0f, -1.0f, 0.0f, 0.0f));
        GetNewObject(dishwasher, new Vector3(1.0f, 0.0f, 5.0f), new Quaternion(0.0f, -1.0f, 0.0f, 0.0f));
        GetNewObject(fridge, new Vector3(-5.0f, 0.0f, 1.0f), new Quaternion(0.0f, -0.7f, 0.0f, -0.7f));
        GetNewObject(chair, new Vector3(-3.0f, 0.0f, -3.0f), new Quaternion(0.0f, 0.7f, 0.0f, 0.7f));
        GetNewObject(chair, new Vector3(-3.0f, 0.0f, -5.0f), new Quaternion(0.0f, -0.7f, 0.0f, -0.7f));
        GetNewObject(chairNormal, new Vector3(1.0f, 0.0f, -3.0f), new Quaternion(0.0f, -0.7f, 0.0f, 0.7f));
        GetNewObject(chairNormal, new Vector3(1.0f, 0.0f, -5.0f), new Quaternion(0.0f, -0.7f, 0.0f, 0.7f));
        GetNewObject(dishwasher, new Vector3(-1.0f, 0.0f, -3.0f), new Quaternion(0.0f, 0.0f, 0.0f, 1.0f));
        GetNewObject(dishwasher, new Vector3(-1.0f, 0.0f, -5.0f), new Quaternion(0.0f, -1.0f, 0.0f, 0.0f));
        GetNewObject(cradle, new Vector3(5.0f, 0.0f, -1.0f), new Quaternion(0.0f, -0.7f, 0.0f, -0.7f));
        GetNewObject(cradle, new Vector3(5.0f, 0.0f, 1.0f), new Quaternion(0.0f, -0.7f, 0.0f, -0.7f));
        GetNewObject(bed, new Vector3(7.0f, 0.0f, 11.0f), new Quaternion(0.0f, -1.0f, 0.0f, 0.0f));

        GameObject toiletSeat = _constructablesDictionary[Constructables.obj_toilet_seat.ToString()];
        GameObject toiletSink = _constructablesDictionary[Constructables.obj_toilet_sink.ToString()];
        GameObject pot = _constructablesDictionary[Constructables.obj_pot.ToString()];

        GetNewObject(toiletSeat, new Vector3(-5.0f, 0.0f, 7.0f), new Quaternion(0.0f, -0.7f, 0.0f, -0.7f));
        GetNewObject(toiletSink, new Vector3(-1.0f, 0.0f, 7.0f), new Quaternion(0.0f, 0.0f, 0.0f, -1.0f));
        GetNewObject(pot, new Vector3(-5.0f, 0.0f, 13.0f), new Quaternion(0.0f, 1.0f, 0.0f, 0.0f));
        GetNewObject(pot, new Vector3(-3.0f, 0.0f, 13.0f), new Quaternion(0.0f, 1.0f, 0.0f, 0.0f));
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
                // Log element position to help initializing objects
                //
                // String logEntry = String.Format(("{0}, {1}, {2} \n"), objectToBuild.name, worldPosition, rotation);
                // log += logEntry;
                // Debug.Log(log);
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
