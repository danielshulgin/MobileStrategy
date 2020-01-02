using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ApplyBuildingButtons : MonoBehaviour
{
    [SerializeField]
    private BuildingController buildingController;
    [SerializeField]
    private GameObject buildingMenu;

    public Vector3 offset = new Vector3(0f, 5f, 0f);

    private CanvasGroup buildingMenuCanvasGroup;

    private void Start()
    {
        buildingMenuCanvasGroup = buildingMenu.GetComponent<CanvasGroup>();
        buildingController.onStartBuilding += Activate;
        buildingController.onEndBuilding += DisActivate;
    }

    private void Update()
    {
        if (buildingController.State == BuildingControllerState.InPrototype)
        {
            GameObject prototype = buildingController.prototypeGO;
            if (prototype != null)
            {
            buildingMenu.transform.position = prototype.transform.position + offset;
            }
        }
    }

    private void Activate()
    {
        offset.x = buildingController.prototypeGO.GetComponent<GridObject>().xLength / 2f;
        buildingMenuCanvasGroup.alpha = 1f;
        buildingMenuCanvasGroup.interactable = true;
    }

    private void DisActivate()
    {
        buildingMenuCanvasGroup.alpha = 0f;
        buildingMenuCanvasGroup.interactable = false;
    }
}
