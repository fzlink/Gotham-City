using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TrackCheckpoints : MonoBehaviour {

    public event Action<Transform> OnCarCorrectCheckpoint;
    public event Action<Transform> OnCarWrongCheckpoint;
    public event Action<Transform> OnCarWrongPath;

    private List<Transform> carTransformList;

    [SerializeField] protected List<TrackCheckpoints> linkedPaths;

    private List<CheckpointSingle> checkpointSingleList;
    private List<int> nextCheckpointSingleIndexList;
    private Dictionary<Transform, int> nextCheckpointSingleDict;

    public bool HasLinkedPath { get { return linkedPaths.Count > 0; } }
    [SerializeField] private ParkingSpotManager parkingSpotManager;

    protected virtual void Awake() {
        Transform checkpointsTransform = transform;

        checkpointSingleList = new List<CheckpointSingle>();
        foreach (Transform checkpointSingleTransform in checkpointsTransform) {
            CheckpointSingle checkpointSingle = checkpointSingleTransform.GetComponent<CheckpointSingle>();
            if (checkpointSingle)
            {
                checkpointSingle.SetTrackCheckpoints(this);
                checkpointSingleList.Add(checkpointSingle);
            }
        }

        //nextCheckpointSingleIndexList = new List<int>();
        nextCheckpointSingleDict = new Dictionary<Transform, int>();
        carTransformList = new List<Transform>();
        /*foreach (Transform carTransform in carTransformList) {
            nextCheckpointSingleIndexList.Add(0);
        }*/
    }

    public void AddCar(Transform carTransform)
    {
        if (!carTransformList.Contains(carTransform))
        {
            carTransformList.Add(carTransform);
            nextCheckpointSingleDict.Add(carTransform, 0);
            //nextCheckpointSingleIndexList.Add(0);
        }
    }

    public void RemoveCar(Transform carTransform)
    {
        int carIndex = carTransformList.IndexOf(carTransform);
        carTransformList.Remove(carTransform);
        nextCheckpointSingleDict.Remove(carTransform);
        //nextCheckpointSingleIndexList.RemoveAt(carIndex);
    }

    public void CarThroughCheckpoint(CheckpointSingle checkpointSingle, Transform carTransform) {
        int carIndex = carTransformList.IndexOf(carTransform);
        if(carIndex == -1)
        {
            OnCarWrongPath?.Invoke(carTransform);
            return;
        }
        int nextCheckpointSingleIndex = nextCheckpointSingleDict[carTransform];//nextCheckpointSingleIndexList[carIndex];
        if (nextCheckpointSingleIndex >= checkpointSingleList.Count) {
            return;
        } 
        else if (checkpointSingleList.IndexOf(checkpointSingle) == nextCheckpointSingleIndex) {
            // Correct checkpoint
            //Debug.Log("Correct");
            CheckpointSingle correctCheckpointSingle = checkpointSingleList[nextCheckpointSingleIndex];
            //correctCheckpointSingle.Hide();

            nextCheckpointSingleDict[carTransform]++;
            /*nextCheckpointSingleIndexList[carTransformList.IndexOf(carTransform)]
                = (nextCheckpointSingleIndex + 1) % checkpointSingleList.Count;*/
            OnCarCorrectCheckpoint?.Invoke( carTransform);
        } else {
            // Wrong checkpoint
            //Debug.Log("Wrong");
            OnCarWrongCheckpoint?.Invoke(carTransform);

            //CheckpointSingle correctCheckpointSingle = checkpointSingleList[nextCheckpointSingleIndex];
            //correctCheckpointSingle.Show();
        }
        if (nextCheckpointSingleDict[carTransform] >= checkpointSingleList.Count)
        {
            DeactivatedLastCheckpoint();
        }
    }

    public virtual void ResetPath(CarAgent carAgent)
    {

    }

    public void ResetCheckpoint(Transform carTransform)
    {
        //nextCheckpointSingleIndexList[carTransformList.IndexOf(carTransform)] = 0;
        nextCheckpointSingleDict[carTransform] = 0;
    }

    public CheckpointSingle GetNextCheckpoint(Transform carTransform)
    {
        if (!nextCheckpointSingleDict.ContainsKey(carTransform))
        {
            Debug.Log("asd");
        }
        if (nextCheckpointSingleDict[carTransform] >= checkpointSingleList.Count)
            return null;
        return checkpointSingleList[nextCheckpointSingleDict[carTransform]];
    }

    public List<float> GetAngleDeltas(Transform carTransform, int amount)
    {
        List<float> deltas = new List<float>();
        for (int i = 0; i < amount; i++)
        {
            CheckpointSingle firstCheckpoint = null, secondCheckpoint = null;
            int index = nextCheckpointSingleDict[carTransform] + i;
            if (index < checkpointSingleList.Count)
                firstCheckpoint = checkpointSingleList[index];
            if(index + 1 < checkpointSingleList.Count)
                secondCheckpoint = checkpointSingleList[index + 1];
            if(firstCheckpoint != null && secondCheckpoint != null)
            {
                deltas.Add(Mathf.DeltaAngle(firstCheckpoint.transform.localEulerAngles.y, secondCheckpoint.transform.localEulerAngles.y));
            }
            else
            {
                deltas.Add(0f);
            }
        }
        return deltas;
    }

    public virtual TrackCheckpoints GetRandomLinkedPath()
    {
        if (linkedPaths.Count <= 0)
            return null;
        TrackCheckpoints linkedPath = null;
        //if (!IsParkingEnter)
            linkedPath = linkedPaths[UnityEngine.Random.Range(0, linkedPaths.Count)];
        /*else
            linkedPath = linkedPaths.Where(x => !((ParkingCheckpoints)x).isOccupied ).FirstOrDefault();*/
        return linkedPath;
    }

    [ContextMenu("Rotate 180 Locally")]
    public void RotateChildsLocally180()
    {
        foreach (Transform item in transform)
        {
            item.localRotation *= Quaternion.Euler(0, 180, 0);
        }
    }

    public int GetCheckpointCount()
    {
        return checkpointSingleList.Count;
    }

    public void ActivateCheckpoint(int index)
    {
        if(index < checkpointSingleList.Count)
        {
            CheckpointSingle checkpoint = checkpointSingleList[index];
            if (checkpoint.usedByCount <= 0)
                checkpoint.Activate();
            checkpoint.usedByCount++;
        }
    }

    public void DeactivateCheckpoint(int index)
    {
        if (index < checkpointSingleList.Count)
        {
            CheckpointSingle checkpoint = checkpointSingleList[index];
            checkpoint.usedByCount--;
            if (checkpoint.usedByCount <= 0)
                checkpoint.Deactivate();
            /*if (index == checkpointSingleList.Count - 1)
                DeactivatedLastCheckpoint();*/
        }
    }

    public virtual void DeactivatedLastCheckpoint()
    {

    }

    public void DeactivateAllCheckpoints()
    {
        for (int i = 0; i < checkpointSingleList.Count; i++)
        {
            checkpointSingleList[i].Deactivate();
        }
    }

    public List<CheckpointSingle> GetCheckpointSingles()
    {
        return checkpointSingleList;
    }

    public ParkingSpot GetParkingSpot()
    {
        if(parkingSpotManager != null)
        {
            return parkingSpotManager.GetRandomFreeParkingSpot();
        }
        return null;
    }

    public bool IsLastCheckpoint(CheckpointSingle checkpoint)
    {
        return checkpointSingleList[checkpointSingleList.Count - 1] == checkpoint;
    }

    public bool HasParkingSpot()
    {
        return parkingSpotManager != null;
    }
}
