using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static CarAgent;

public class CarParkingHandler : CarComponentBase
{

    public ParkingSpot parkingSpot { get; set; }

    public float distanceToParkingSpot;
    public float prevDistanceToParkingSpot = Mathf.Infinity;

    public bool IsOnParkStop { get; set; }

    private CarDriver carDriver;

    protected override void Awake()
    {
        base.Awake();
        carDriver = GetComponent<CarDriver>();
    }

    protected override void OnCarEpisodeBegin()
    {
        StopAllCoroutines();
        distanceToParkingSpot = 0f;
        prevDistanceToParkingSpot = Mathf.Infinity;
        IsOnParkStop = false;
    }

    public void InitParking(ParkingSpot parkingSpot)
    {
        if (parkingSpot == null)
        {
            carAgent.FinishEpisode(0f);
            return;
        }
        this.parkingSpot = parkingSpot;
        parkingSpot.Occupy(carAgent);
        carAgent.ChangeState(CarState.Parking);
        distanceToParkingSpot = 0f;
        prevDistanceToParkingSpot = Mathf.Infinity;
    }

    public bool CheckStopParking(Vector3 forward)
    {
        if (carAgent.state == CarState.Parking && !IsOnParkStop)
        {
            if (carDriver.IsStopping())
            {
                StartCoroutine(WaitForParking(forward));
                IsOnParkStop = true;
                return true;
            }
        }
        return false;
    }

    private IEnumerator WaitForParking(Vector3 forward)
    {
        //print("Waiting");
        yield return new WaitForSeconds(2.5f);
        if (carDriver.IsStopping() && IsOnParkStop)
        {
            float dot = Vector3.Dot(transform.forward, forward);
            if (dot < 0.1f) dot = 0.1f;
            //carDriver.StopCompletely();
            carAgent.AddReward(dot);
            parkingSpot.CarAgent_OnPathFinished(carAgent);
            StopAllCoroutines();
            IsOnParkStop = false;
            carAgent.FinishEpisode(0f);
            /*if (carAgent.IsTraining)
                carAgent.FinishEpisode(0f);
            else
            {
                carAgent.ChangeState(CarState.UnParking);
                prevDistanceToParkingSpot = Mathf.Infinity;
                distanceToParkingSpot = 0f;
            }*/
        }
        else
        {
            IsOnParkStop = false;
        }
    }

    public void SetPrevParkingDistance()
    {
        if (distanceToParkingSpot != 0f && distanceToParkingSpot < prevDistanceToParkingSpot)
            prevDistanceToParkingSpot = distanceToParkingSpot;
    }

    public bool IsCloserToTarget()
    {
        return distanceToParkingSpot < prevDistanceToParkingSpot;
    }
}
