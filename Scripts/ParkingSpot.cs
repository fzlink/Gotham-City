using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ParkingSpot : MonoBehaviour
{
    public bool IsOccupied { get; set; }

    private CarAgent parkingCar;
    private CarAgent prevParkingCar;
    private Renderer renderer;
    private Collider collider;

    [SerializeField] private Transform enterSpot;
    [SerializeField] private Transform exitSpot;
    [HideInInspector] public Transform spot;

    public event Action OnParkingOccupied;
    public event Action OnParkingSpotReset;

    [SerializeField] private CarAgent fillCar;
    private CarAgent instantiatedFillCar;

    private void Awake()
    {
        renderer = GetComponent<Renderer>();
        collider = GetComponent<Collider>();
    }

    private void Start()
    {
        ToggleActive(false);
        spot = enterSpot;

        if(SceneManager.GetActiveScene().name == "Only Parking")
            MakeFillCar();

    }

    private void MakeFillCar()
    {
        instantiatedFillCar = Instantiate(fillCar);
        instantiatedFillCar.transform.forward = transform.forward;
        instantiatedFillCar.transform.position = transform.position + transform.forward * 3f + Vector3.down;
        //instantiatedFillCar.RemoveModel();
        instantiatedFillCar.gameObject.SetActive(false);
        instantiatedFillCar.enabled = false;
    }

    private void ToggleActive(bool active)
    {
        collider.enabled = active;
        renderer.enabled = active;
    }

    public void ToggleFill(bool isFill)
    {
        IsOccupied = isFill;
        instantiatedFillCar.gameObject.SetActive(isFill);
        if (isFill)
        {
            instantiatedFillCar.transform.forward = transform.forward;
            instantiatedFillCar.transform.position = transform.position + transform.forward * 3f + Vector3.down;
        }
    }

    public void Occupy(CarAgent carAgent)
    {
        IsOccupied = true;
        prevParkingCar = parkingCar;
        parkingCar = carAgent; 
        spot = enterSpot;
        //parkingCar.OnPathFinished += CarAgent_OnPathFinished;
        OnParkingOccupied?.Invoke();
        //ToggleActive(true);
    }

    public void CarAgent_OnPathFinished(CarAgent carAgent)
    {
        IsOccupied = false;
        //parkingCar.OnPathFinished -= CarAgent_OnPathFinished;
        ToggleActive(false);
        spot = enterSpot;
        OnParkingSpotReset?.Invoke();
    }

    private void OnTriggerStay(Collider other)
    {
        if(other.TryGetComponent(out CarAgent carAgent))
        {
            if(carAgent == parkingCar)
            {
                parkingCar.CheckStopParking(transform.forward);
            }
        }
    }

    public bool ParkingChildSpot_OnTriggerEnter(CarAgent carAgent, bool isEnter)
    {
        if(carAgent == parkingCar)
        {
            if (isEnter)
            {
                spot = exitSpot;
                parkingCar.ResetParkingSpotDistance();
            }
            else
            {
                ToggleActive(true);
            }
            carAgent.AddReward(0.25f);
            return true;
        }
        return false;
    }
}
