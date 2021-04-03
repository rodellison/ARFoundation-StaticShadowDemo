﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.Experimental.XR;
using System;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TrackableType = UnityEngine.XR.ARSubsystems.TrackableType;


public class ARTapToPlaceObject : MonoBehaviour
{
    private Vector2 _screenCenterV2;
    public GameObject objectToPlace;
    public GameObject placementIndicator;

    public Camera ARCamera;

    private GameObject theGO;
    private ARRaycastManager _raycastManager;
    private Pose _placementPose;
    private bool _placementPoseIsValid;
    private bool _scaleSet;
    private bool _objectSet;
    private bool _readyToPlace;

    ARSessionOrigin m_SessionOrigin;

    void Awake()
    {
        m_SessionOrigin = GetComponent<ARSessionOrigin>();
    }

    void Start()
    {
        _screenCenterV2 = new Vector2(0.5f, 0.5f);
        _raycastManager = FindObjectOfType<ARRaycastManager>();
    }

    public void Reset()
    {
        placementIndicator.SetActive(false);
        if (_objectSet)
        {
            Destroy(theGO);
            _objectSet = false;
        }

        _readyToPlace = false;
        GameManager.Instance.SetLogText("");
    }

    public void Ready()
    {
        _readyToPlace = true;
        placementIndicator.SetActive(true);
        GameManager.Instance.SetLogText("Please, tap on your screen to set content in position");
    }


    void Update()
    {
        if (!_objectSet && _readyToPlace)
        {
            UpdatePlacementPose();

            // Check if there is a touch
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                // Check if finger is over a UI element e.g. one of the Sliders
                if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
                {
                }
                else
                {
                    PlaceObject();
                    _objectSet = true;
                    placementIndicator.SetActive(false);
                    GameManager.Instance.SetLogText("");
                }
            }
        }
    }

    public bool IsGOPlaced()
    {
        return (theGO != null);
    }

    public Transform GOTransform()
    {
        return theGO.transform;
    }

    public Pose GetPlacementPose()
    {
        return _placementPose;
    }

    private void PlaceObject()
    {
        theGO = Instantiate(objectToPlace, _placementPose.position, _placementPose.rotation);
        m_SessionOrigin.MakeContentAppearAt(theGO.transform, _placementPose.position, _placementPose.rotation);
    }


    private void UpdatePlacementPose()
    {
        var screenCenter = ARCamera.ViewportToScreenPoint(_screenCenterV2);
        var hits = new List<ARRaycastHit>();

#if UNITY_IOS
        _raycastManager.Raycast(screenCenter, hits, TrackableType.PlaneWithinPolygon | TrackableType.FeaturePoint);
#else
        _raycastManager.Raycast(screenCenter, hits, TrackableType.Planes);
#endif

        _placementPoseIsValid = hits.Count > 0;
        if (_placementPoseIsValid)
        {
            //This contains the default pose, which would point whichever way the camera was at launch.
            _placementPose = hits[0].pose;

            //instead, we want to recalculate the pose rotation based on where the camera is pointing real time
            var cameraForward = Camera.current.transform.forward;
            var cameraBearing = new Vector3(cameraForward.x, 0, cameraForward.z).normalized;
            _placementPose.rotation = Quaternion.LookRotation(cameraBearing);

            placementIndicator.SetActive(true);
            placementIndicator.transform.SetPositionAndRotation(_placementPose.position, _placementPose.rotation);
        }
        else
        {
            placementIndicator.SetActive(false);
        }
    }
}