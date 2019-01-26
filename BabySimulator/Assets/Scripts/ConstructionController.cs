﻿using System;
using System.Collections;
using System.Collections.Generic;
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
        Wall,
        Chair
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
                var rotation = Quaternion.identity;
                if (ghostObject != null)
                {
                    rotation = ghostObject.rotation;
                    _constructedLocationsList.Add(new ConstructedLocations{TileCoordinates = latestTilePosition, TileGameObject = ghostObject.gameObject});
                }
                ghostObject = GetNewObject(objectToBuild, worldPosition, rotation);
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
            Destroy(construct.TileGameObject);
            _constructedLocationsList.Remove(construct);
        }
    }
}