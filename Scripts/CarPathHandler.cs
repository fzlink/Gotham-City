using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static CarAgent;

public class CarPathHandler : CarComponentBase
{

    private List<TrackCheckpoints> paths;
    private List<int> pathCumulativeLengths;
    [HideInInspector] public TrackCheckpoints currentPath;
    private int currentPathIndex;
    private SpawnPoint spawnPoint;

    [HideInInspector] public int previousTargetAmount = 1;
    [HideInInspector] public int currentTargetCheckpoint = -1;
    [HideInInspector] public List<CheckpointSingle> checkpoints;

    private Coroutine waitForNextCheckpointCoroutine;
    private CarParkingHandler parkingHandler;
    protected override void Awake()
    {
        base.Awake();
        parkingHandler = GetComponent<CarParkingHandler>();
    }

    protected override void OnCarEpisodeBegin()
    {
        StopAllCoroutines();
        transform.position = spawnPoint.transform.position;
        transform.forward = spawnPoint.transform.forward;
        ResetCheckpoints();
    }

    public void SetSpawnPoint(SpawnPoint spawnPoint)
    {
        this.spawnPoint = spawnPoint;
        paths = new List<TrackCheckpoints>();
        checkpoints = new List<CheckpointSingle>();
        var parkingSpot = spawnPoint.GetParkingSpot();
        if (parkingSpot != null)
        {
            parkingHandler.InitParking(parkingSpot);
        }
        SetPaths(spawnPoint.GetStartPath());
    }

    public void SetPaths(TrackCheckpoints startPath)
    {
        AddPath(startPath);
        SubscribeToPathEvents(startPath);
        currentPath = startPath;
        currentPathIndex = paths.IndexOf(currentPath);
        var iteratorPath = startPath;
        while (iteratorPath.HasLinkedPath)
        {
            var randomLinkedPath = iteratorPath.GetRandomLinkedPath();
            AddPath(randomLinkedPath);
            iteratorPath = randomLinkedPath;
        }
    }


    private void AddPath(TrackCheckpoints trackCheckpoints)
    {
        paths.Add(trackCheckpoints);
        trackCheckpoints.AddCar(transform);
        checkpoints.AddRange(trackCheckpoints.GetCheckpointSingles());
    }

    private void SubscribeToPathEvents(TrackCheckpoints trackCheckpoints)
    {
        trackCheckpoints.OnCarCorrectCheckpoint += TrackCheckpoints_OnCarCorrectCheckpoint;
    }

    public void UnSubscribeToPathEvents(TrackCheckpoints trackCheckpoints)
    {
        trackCheckpoints.OnCarCorrectCheckpoint -= TrackCheckpoints_OnCarCorrectCheckpoint;
    }

    private void TrackCheckpoints_OnCarCorrectCheckpoint(Transform carTransform)
    {
        if (carTransform == transform && carAgent.state == CarState.Traversing)
        {
            carAgent.AddReward(0.5f);
            OffsetCheckpoint();
            if (waitForNextCheckpointCoroutine != null)
                StopCoroutine(waitForNextCheckpointCoroutine);
            waitForNextCheckpointCoroutine =  StartCoroutine(WaitForNextCheckpoint(currentTargetCheckpoint));
        }
    }

    private IEnumerator WaitForNextCheckpoint(int prevCheckpoint)
    {
        yield return new WaitForSeconds(25f);
        if (prevCheckpoint == currentTargetCheckpoint && carAgent.state == CarState.Traversing)
        {
            carAgent.FinishEpisode(0f);
        }
    }

    private void OffsetCheckpoint()
    {
        currentTargetCheckpoint++;
        ToggleCheckpoint(currentTargetCheckpoint, true);
        ToggleCheckpoint(currentTargetCheckpoint - previousTargetAmount, false);
    }

    private void ToggleCheckpoint(int index, bool activate)
    {
        if (pathCumulativeLengths == null) return;
        for (int i = 0; i < pathCumulativeLengths.Count; i++)
        {
            if (index < pathCumulativeLengths[i] && index > -1)
            {
                int subtractedTotal = (i == 0) ? 0 : pathCumulativeLengths[i - 1];
                if (index - subtractedTotal > -1)
                {
                    if (paths[i] != null)
                    {
                        if (activate)
                            paths[i].ActivateCheckpoint(index - subtractedTotal);
                        else
                            paths[i].DeactivateCheckpoint(index - subtractedTotal);
                        break;
                    }
                }
            }
        }
    }

    private void ResetCheckpoints()
    {
        for (int i = 0; i < paths.Count; i++)
        {
            paths[i].ResetCheckpoint(transform);
        }
        currentPath = paths[0];
        currentPathIndex = 0;
        currentTargetCheckpoint = -1;
        DetermineTotals();
        OffsetCheckpoint();
    }

    public void RefreshCheckpointActivation()
    {
        ToggleCheckpoint(currentTargetCheckpoint, true);
    }

    private void DetermineTotals()
    {
        pathCumulativeLengths = new List<int>();
        int totalCheckpoints = 0;
        for (int i = 0; i < paths.Count; i++)
        {
            totalCheckpoints += paths[i].GetCheckpointCount();
            pathCumulativeLengths.Add(totalCheckpoints);
        }
    }
    public void PathFinished()
    {
        if (!carAgent.isAlreadyFinished)
        {
            for (int i = 0; i < previousTargetAmount; i++)
            {
                ToggleCheckpoint(currentTargetCheckpoint - i, false);
            }

            if (paths != null)
            {
                for (int i = 0; i < paths.Count; i++)
                {
                    if (paths[i] != null)
                    {
                        UnSubscribeToPathEvents(paths[i]);
                        paths[i].RemoveCar(transform);
                        paths[i].ResetPath(carAgent);
                    }
                }
            }
        }
        else
            carAgent.isAlreadyFinished = false;
        carAgent.NotifyPathFinished();
    }

    public float GetDistanceToCheckpoint()
    {
        float dist;
        if (currentTargetCheckpoint < checkpoints.Count)
            dist = Vector3.Distance(transform.position, checkpoints[currentTargetCheckpoint].transform.position);
        else
            dist = 0f;
        return dist;
    }

    public void ChangeCheckpointColor()
    {
        if (currentTargetCheckpoint < checkpoints.Count)
            checkpoints[currentTargetCheckpoint].ChangeColor(Color.blue);
    }

    public bool SwitchToNextPath()
    {
        int nextPathIndex = currentPathIndex + 1;
        if (nextPathIndex < paths.Count)
        {
            UnSubscribeToPathEvents(currentPath);
            currentPath.ResetCheckpoint(transform);
            currentPath = paths[nextPathIndex];
            currentPathIndex++;
            SubscribeToPathEvents(currentPath);
            return true;
        }
        return false;
    }
}
