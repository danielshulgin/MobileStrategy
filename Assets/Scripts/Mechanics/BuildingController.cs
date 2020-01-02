using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public enum BuildingControllerState
{
    InPrototype, Disabled
}

public class BuildingController : MonoBehaviour
{
    public Grid grid;
    public PanAndZoom panAndZoom;
    public GameObject buildingParent;
    public CameraMovementController cameraMovementController;

    public List<GameObject> buildingPrefabs = new List<GameObject>();
    public Material highlightedMaterial;

    public float buildingMoveSpeed = 1f;
    public float prototypeScaleMultiplyer = 1.01f;
    public int MaxRayCastDistance = 100;

    [HideInInspector]
    public BuildingControllerState State { get; private set; }
    [HideInInspector]
    public GameObject prototypeGO;
    [HideInInspector]
    public Vector3 buildingPosition = Vector3.zero;

    private Material prototypeMaterial;
    private bool intersect = false;
    private bool touchOnPrototype = false;
    private int currentId;

    public Action onStartBuilding = () => { };
    public Action onEndBuilding = () => { };

    private void Start()
    {
        State = BuildingControllerState.Disabled;
    }

    public void StartBuilding(int id)
    {
        GameObject selctedBuildingPref = buildingPrefabs.Find(
        prefab => prefab.GetComponent<GridObject>().id == id);
        if (selctedBuildingPref != null)
        {
            currentId = id;
            State = BuildingControllerState.InPrototype;
            buildingPosition = grid.FromGridToWorldCoordinates((grid.center));
            panAndZoom.onSwipe += MoveBuilding;
            panAndZoom.onStartTouch += StartMoveBuilding;
            panAndZoom.onEndTouch += EndMoveBuilding;
            CreatePrototype(selctedBuildingPref, grid.center);
        }
        else
        {
            Debug.Log("Wrong id!!!");
        }
    }

    public void ApplyBuilding()
    {
        if (grid.PutObjectOnGrid(prototypeGO.GetComponent<GridObject>()))
        {
            prototypeGO.layer = 0;
            prototypeGO.transform.localScale = prototypeGO.transform.localScale / prototypeScaleMultiplyer;
            prototypeGO = null;
            EndBuilding();
        }
    }

    public void EndBuilding()
    {
        onEndBuilding();
        State = BuildingControllerState.Disabled;
        panAndZoom.onSwipe -= MoveBuilding;
        panAndZoom.onStartTouch -= StartMoveBuilding;
        panAndZoom.onEndTouch -= EndMoveBuilding;
        if (prototypeGO != null)
        {
            Destroy(prototypeGO);
        }
    }

    private void CreatePrototype(GameObject selctedBuildingPref, GridCoordinates position)
    {
        prototypeGO = CreateBuilding(selctedBuildingPref, position, false);
        prototypeGO.transform.localScale = prototypeGO.transform.localScale * prototypeScaleMultiplyer;
        prototypeGO.layer = 8;
        onStartBuilding();
        if (!grid.CanPutObject(prototypeGO.GetComponent<GridObject>(), position, true))
        {
            if (!intersect)
            {
                HighlightPrototype();
            }
        }
    }

    public GameObject CreateBuilding(GameObject prefab, GridCoordinates position, bool checkPosition = true)
    {
        GridObject gridObject = prefab.GetComponent<GridObject>();
        gridObject.gridPosition = position;
        if (grid.CanPutObject(gridObject) || !checkPosition)
        {
            GameObject building = Instantiate(prefab);
            building.transform.position = grid.FromGridToWorldCoordinates(position);
            building.transform.SetParent(buildingParent.transform);
            return building;
        }
        //TODO
        //Debug.Log("wrong coordinates or grid object size");
        return null;
    }

    public void MoveBuilding(Vector2 diff)
    {
        if (touchOnPrototype)
        {
            Vector3 updatedBuildingPosition = buildingPosition
                + new Vector3(diff.x, 0f, diff.y) * buildingMoveSpeed * Time.deltaTime;
            GridCoordinates updatedCoordinates = updatedBuildingPosition;
            GridObject prototypeGridObject = prototypeGO.GetComponent<GridObject>();
            if (grid.CanPutObject(prototypeGridObject, updatedCoordinates, false))
            {
                if (updatedCoordinates.x != prototypeGridObject.gridPosition.x ||
                updatedCoordinates.y != prototypeGridObject.gridPosition.y)
                {
                    if (!grid.CanPutObject(prototypeGridObject, updatedCoordinates, true))
                    {
                        if (!intersect)
                        {
                            HighlightPrototype();
                        }
                    }
                    else if (intersect)
                    {
                        intersect = false;
                        prototypeGO.transform.GetChild(0).
                            gameObject.GetComponent<MeshRenderer>().material = prototypeMaterial;
                    }
                    prototypeGridObject.gridPosition = updatedCoordinates;
                    prototypeGO.transform.position = grid.FromGridToWorldCoordinates(updatedCoordinates);
                }
                buildingPosition = updatedBuildingPosition;
            }
        }
    }

    private void HighlightPrototype()
    {
        prototypeMaterial = prototypeGO.gameObject.transform.GetChild(0).GetComponent<MeshRenderer>().material;
        intersect = true;
        prototypeGO.transform.GetChild(0).
            gameObject.GetComponent<MeshRenderer>().material = highlightedMaterial;
    }

    public void StartMoveBuilding(Vector2 screenPosition)
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(screenPosition);
        if (Physics.Raycast(ray, out hit, MaxRayCastDistance, LayerMask.GetMask("Prototype")))
        {
            if (hit.transform.gameObject == prototypeGO)
            {
                cameraMovementController.blockMovement = true;
                touchOnPrototype = true;
            }
            else
            {
                Debug.Log(hit.transform.gameObject.name);
            }
        }
    }

    public void EndMoveBuilding(Vector2 screenPosition)
    {
        cameraMovementController.blockMovement = false;
        touchOnPrototype = false;
    }
}
