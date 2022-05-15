using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [SerializeField] private TrackCheckpoints startPath;
    public bool isOccupied { get; set; }
    private CarSpawner carSpawner;
    private int carsInsideAmount;
    private List<CarAgent> carsInside;
    [SerializeField] private ParkingSpotManager parkingSpotManager;
    [SerializeField] private bool onParkingSpot;

    private void Start()
    {
        carsInside = new List<CarAgent>();
    }
    public ParkingSpot GetParkingSpot()
    {
        if (onParkingSpot)
        {
            SetToParkingSpot();
        }
        else if(parkingSpotManager != null)
        {
            parkingSpotManager.FillRandomly();
            return parkingSpotManager.GetRandomFreeParkingSpot();
            //return parkingSpotManager.GetRandomFreeParkingSpot();
        }
        return null;
    }

    private void SetToParkingSpot()
    {
        parkingSpotManager.FillRandomly();
        var spot = parkingSpotManager.GetRandomFreeParkingSpot();
        transform.position = spot.transform.position + spot.transform.forward - transform.up;
        transform.forward = spot.transform.forward;
    }

    public TrackCheckpoints GetStartPath()
    {
        return startPath;
    }

    public void SetCarSpawner(CarSpawner carSpawner)
    {
        this.carSpawner = carSpawner;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out CarAgent carAgent))
        {
            CarEntered(carAgent);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.TryGetComponent(out CarAgent carAgent))
        {
            CarExited(carAgent);
        }
    }

    private void CarEntered(CarAgent carAgent)
    {
        if (!carsInside.Contains(carAgent))
        {
            carsInside.Add(carAgent);
            carAgent.OnNotRunning += OnCarNotRunning;
        }

        carsInsideAmount++;
        isOccupied = true;
        carSpawner.OnSpawnPointStateChange(this, false);
    }

    private void CarExited(CarAgent carAgent)
    {
        carsInside.Remove(carAgent);
        
        carsInsideAmount--;
        if (carsInsideAmount <= 0)
        {
            carsInsideAmount = 0;
            isOccupied = false;
            carSpawner.OnSpawnPointStateChange(this, true);
        }
    }

    private void OnCarNotRunning(CarAgent carAgent)
    {
        carAgent.OnNotRunning -= OnCarNotRunning;
        CarExited(carAgent);
    }
}
