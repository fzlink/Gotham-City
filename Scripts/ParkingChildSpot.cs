using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParkingChildSpot : MonoBehaviour
{
    [SerializeField] private ParkingSpot parkingSpot;
    [SerializeField] private bool isEnter;
    private Collider collider;
    private Renderer renderer;

    private void Awake()
    {
        collider = GetComponent<Collider>();
        renderer = GetComponent<Renderer>();
    }

    private void Start()
    {
        ToggleActive(false);
    }

    private void OnEnable()
    {
        parkingSpot.OnParkingOccupied += OnParkingOccupied;
        parkingSpot.OnParkingSpotReset += OnParkingSpotReset;
    }

    private void OnDisable()
    {
        parkingSpot.OnParkingOccupied -= OnParkingOccupied;
        parkingSpot.OnParkingSpotReset -= OnParkingSpotReset;
    }

    private void OnParkingOccupied()
    {
        ToggleActive(true);
    }

    private void OnParkingSpotReset()
    {
        ToggleActive(false);
    }

    private void ToggleActive(bool active)
    {
        collider.enabled = active;
        renderer.enabled = active;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent(out CarAgent carAgent))
        {
            if(parkingSpot.ParkingChildSpot_OnTriggerEnter(carAgent, isEnter))
            {
                ToggleActive(false);
            }
        }
    }
}
