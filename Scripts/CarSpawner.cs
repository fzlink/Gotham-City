using System;
using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents.Demonstrations;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using UnityEngine;
using System.Linq;

public class CarSpawner : MonoBehaviour
{
    [SerializeField] private List<SpawnPoint> spawnPoints;
    [SerializeField] private List<CarAgent> carPrefabs;
    [SerializeField] private CarAgent heuristicCarPrefab;
    [SerializeField] private int startSpawnAmount = 10;
    [SerializeField] private Transform carHolder;
    [SerializeField] private bool isDemoRide;
    [SerializeField] private bool isTestRide;
    [SerializeField] private bool isSelf;
    [SerializeField] private bool isTraining;
    private List<SpawnPoint> freeSpawnPoints;
    private Queue<CarAgent> carSpawnQueue;

    private void Start()
    {
        if (isSelf) startSpawnAmount = 1;
        freeSpawnPoints = new List<SpawnPoint>(spawnPoints);
        carSpawnQueue = new Queue<CarAgent>();
        for (int i = 0; i < spawnPoints.Count; i++)
            spawnPoints[i].SetCarSpawner(this);
        SpawnCars(startSpawnAmount);
    }

    private void SpawnCars(int startSpawnAmount)
    {
        for (int i = 0; i < startSpawnAmount; i++)
        {
            CarAgent randomCarPrefab = carPrefabs[UnityEngine.Random.Range(0, carPrefabs.Count)];
            CarAgent car;
            if (i == 0 && (isDemoRide || isTestRide))
            {
                car = Instantiate(heuristicCarPrefab, carHolder);
                EnableHeuresticDriver(car);
            }
            else
                car = Instantiate(randomCarPrefab, carHolder);
            car.OnPathFinished += CarAgent_OnPathFinished;
            car.IsTraining = isTraining;
        }
    }

    private void EnableHeuresticDriver(CarAgent car)
    {
        //car.GetComponent<BehaviorParameters>().BehaviorType = BehaviorType.HeuristicOnly;
        if (isDemoRide)
        {
            var recorder = car.GetComponent<DemonstrationRecorder>();
            recorder.enabled = true;
        }
        //car.GetComponentInChildren<Camera>(true).gameObject.SetActive(true);
    }

    private void CarAgent_OnPathFinished(CarAgent carAgent)
    {
        SpawnPoint randomSpawnPoint = GetRandomSpawnPoint();
        if(randomSpawnPoint != null)
        {
            carAgent.SetSpawnPoint(randomSpawnPoint);
            carAgent.SetIsRunning(true);
            OnSpawnPointStateChange(randomSpawnPoint, false);
        }
        else
            ToggleCarAgent(carAgent, false);
    }

    private void ToggleCarAgent(CarAgent carAgent, bool activate)
    {
        /*if(!activate)
            carSpawnQueue.Enqueue(carAgent);
        carAgent.SetIsRunning(activate);
        if (!activate && carSpawnQueue.Contains(carAgent))
            carAgent.gameObject.SetActive(activate);
        else if (activate)
            carAgent.gameObject.SetActive(true);*/
        carAgent.SetIsRunning(activate);
        carAgent.gameObject.SetActive(activate);
        if (!activate)
            carSpawnQueue.Enqueue(carAgent);
    }


    public void OnSpawnPointStateChange(SpawnPoint spawnPoint, bool isFreed)
    {
        if (isFreed)
        {
            if (!freeSpawnPoints.Contains(spawnPoint))
                freeSpawnPoints.Add(spawnPoint);
            CheckCarsToSpawn();
        }
        else
            freeSpawnPoints.Remove(spawnPoint);
    }

    private SpawnPoint GetRandomSpawnPoint()
    {
        SpawnPoint spawnPoint = null;
        if(freeSpawnPoints.Count > 0)
            spawnPoint = freeSpawnPoints[UnityEngine.Random.Range(0, freeSpawnPoints.Count)];
        return spawnPoint;
    }

    private void CheckCarsToSpawn()
    {
        if(carSpawnQueue.Count > 0)
        {
            SpawnPoint randomSpawnPoint = GetRandomSpawnPoint();
            if(randomSpawnPoint != null)
            {
                CarAgent carAgent = carSpawnQueue.Dequeue();
                ToggleCarAgent(carAgent, true);
                //carAgent.SetSpawnPoint(randomSpawnPoint);
            }
        }
    }
}

/*var sensors = car.GetComponents<RayPerceptionSensorComponent3D>();
for (int i = 0; i < sensors.Length; i++)
{
    if (sensors[i].SensorName == "RayPerceptionSensor2")
        Destroy(sensors[i]);
}*/
//car.GetComponent<RayPerceptionSensorComponent3D>()