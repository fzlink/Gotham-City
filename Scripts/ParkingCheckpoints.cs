using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParkingCheckpoints : TrackCheckpoints
{
    /*public bool isOccupied { get; set; }
    [SerializeField] private Collider rewardCollider;
    [SerializeField] private Collider exitingCollider;
    private float colliderEnabledDuration = 3f;
    private CarAgent parkingCar;
    private bool carSended;
    private Coroutine sendCarBackCoroutine;

    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        rewardCollider.gameObject.SetActive(false);
    }

    public override void DeactivatedLastCheckpoint()
    {
        if(parkingCar != null)
        {
            carSended = false;
            //rewardCollider.gameObject.SetActive(true);
            //sendCarBackCoroutine = StartCoroutine(WaitSeconds(SendCarBack, colliderEnabledDuration));
            ResetCheckpoint(parkingCar.transform);
            parkingCar.UnSubscribeToPathEvents(this);
        }
    }

    private IEnumerator WaitSeconds(Action action, float duration)
    {
        yield return new WaitForSeconds(duration);
        action();
    }

    private void SendCarBack()
    {
        if (parkingCar != null && parkingCar.IsOnParkStop)
        {
            rewardCollider.gameObject.SetActive(false);
            parkingCar.SetPaths(linkedPaths[UnityEngine.Random.Range(0, linkedPaths.Count)]);
            parkingCar.RefreshCheckpointActivation();
            parkingCar.ChangeState(CarAgent.CarState.Traversing);
            parkingCar.IsOnParkStop = false;
        }
        carSended = true;
    }

    public void Occupy(CarAgent carAgent)
    {
        parkingCar = carAgent;
        parkingCar.OnPathFinished += CarAgent_OnPathFinished;
        isOccupied = true;
    }

    private void CarAgent_OnPathFinished(CarAgent carAgent)
    {
        CarExited();
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.TryGetComponent(out CarAgent carAgent))
        {
            if(carAgent == parkingCar)
            {
                parkingCar.IsOnParkStop = false;
                //if(carSended)
                    //CarExited();
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.TryGetComponent(out CarAgent carAgent))
        {
            if (carAgent == parkingCar)// && !carSended)
            {
                parkingCar.CheckStopParking(exitingCollider.transform.forward);
                //parkingCar.MoveToParkingSpot(exitingCollider.transform.position, exitingCollider.transform.forward);
            }
        }
    }

    private void CarExited()
    {
        isOccupied = false;
        parkingCar.OnPathFinished -= CarAgent_OnPathFinished;
        parkingCar = null;
        carSended = false;
    }

    public override void ResetPath(CarAgent carAgent)
    {
        base.ResetPath(carAgent);
        CarExited();
        StopAllCoroutines();
        rewardCollider.gameObject.SetActive(false);
    }
    */
}
