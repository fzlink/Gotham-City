using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Barracuda;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CarAgent : Agent
{
    public enum CarState
    {
        Traversing,
        Parking,
        UnParking
    }

    public event Action<CarAgent> OnPathFinished;
    public event Action<CarAgent> OnNotRunning;
    public event Action OnCarEpisodeBegin;

    private Vector3 parkingRelativePosition;
    private float distanceToTargetCheckpoint;

    private bool isRunning;
    [HideInInspector] public bool isAlreadyFinished;

    private CarDriver carDriver;
    private Rigidbody rb;
    private CarPathHandler pathHandler;
    private CarParkingHandler parkingHandler;

    [SerializeField] private NNModel traversingModel;
    [SerializeField] private NNModel parkingModel;
    [SerializeField] private NNModel unparkingModel;

    public CarState state;
    public bool IsTraining { get; set; }

    private void Awake()
    {
        carDriver = GetComponent<CarDriver>();
        rb = GetComponent<Rigidbody>();
        pathHandler = GetComponent<CarPathHandler>();
        parkingHandler = GetComponent<CarParkingHandler>();
    }

    public void ChangeState(CarState state)
    {
        switch (state)
        {
            case CarState.Parking:
                ChangeNNModel(parkingModel);
                break;
            case CarState.Traversing:
                ChangeNNModel(traversingModel);
                break;
            case CarState.UnParking:
                ChangeNNModel(unparkingModel);
                break;
        }
        this.state = state;
    }

    private void ChangeNNModel(NNModel model)
    {
        SetModel("CarDriver", model);
    }

    public void SetIsRunning(bool isRunning)
    {
        this.isRunning = isRunning;
        if (!isRunning)
            OnNotRunning?.Invoke(this);
    }

    public override void OnEpisodeBegin()
    {
        if (SceneManager.GetActiveScene().name == "Only Parking")
            ChangeState(CarState.Parking);
        else
            ChangeState(CarState.Traversing);
        pathHandler.PathFinished();
        if (isRunning)
        {
            OnCarEpisodeBegin?.Invoke();
            carDriver.StopCompletely();
            //carDriver.ChangeMaxSpeed();
        }
        else
            isAlreadyFinished = true;
    }

    public void SetSpawnPoint(SpawnPoint randomSpawnPoint)
    {
        pathHandler.SetSpawnPoint(randomSpawnPoint);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (!isRunning)
            return;

        distanceToTargetCheckpoint = pathHandler.GetDistanceToCheckpoint();
        if (state == CarState.Traversing)
        {
            CollectTraversingObservations(sensor);
        }
        if(state == CarState.Parking || state == CarState.UnParking)
        {
            CollectParkingObservations(sensor);
        }
    }

    public void CheckStopParking(Vector3 forward)
    {
        parkingHandler.CheckStopParking(forward);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        if (!isRunning)
            return;

        if (state == CarState.Traversing)
        {
            var nextCheckpoint = pathHandler.currentPath.GetNextCheckpoint(transform);
            if (nextCheckpoint == null)
            {
                var parkingSpot = pathHandler.currentPath.GetParkingSpot();
                if (parkingSpot != null)
                {
                    parkingHandler.InitParking(parkingSpot);
                }
                bool isSuccessful = pathHandler.SwitchToNextPath();
                if(!isSuccessful)
                {
                    EndEpisode();
                    return;
                }
            }
        }

        if (CheckEndingConditions())
            return;

        SetBrainInputs(actions);
        AddContinousRewards();
    }

    private bool CheckEndingConditions()
    {
        if (transform.position.y < -1f)
        {
            FinishEpisode(-1f);
            return true;
        }
        if (state == CarState.Traversing && distanceToTargetCheckpoint > 50f)
        {
            FinishEpisode(-1f);
            return true;
        }
        else if (state == CarState.Parking && parkingRelativePosition.magnitude >= 50f)
        {
            FinishEpisode(-1f);
            return true;
        }
        else if (state == CarState.UnParking && parkingRelativePosition.magnitude <= 5f)
        {
            AddReward(0.66f);
            float dot = Vector3.Dot(transform.forward, pathHandler.checkpoints[pathHandler.currentTargetCheckpoint].transform.forward);
            if (dot < 0.1f)
                dot = 0.1f;
            if (IsTraining)
                FinishEpisode(dot/3f);
            else
                ChangeState(CarState.Traversing);
            return true;
        }
        return false;
    }

    private void AddContinousRewards()
    {
        if (state == CarState.Parking)//|| state == CarState.UnParking)
        {
            /*if(state == CarState.UnParking)
                AddReward(-1f/MaxStep);*/
            if (parkingHandler.IsCloserToTarget())
            {
                AddReward(0.005f);
            }
        }
    }

    private void SetBrainInputs(ActionBuffers actions)
    {
        float forwardAmount = 0f;
        float turnAmount = 0f;

        switch (actions.DiscreteActions[0])
        {
            case 0: forwardAmount = 0f; break;
            case 1: forwardAmount = 1f; break;
            case 2: forwardAmount = -1f; break;
        }
        switch (actions.DiscreteActions[1])
        {
            case 0: turnAmount = 0f; break;
            case 1: turnAmount = 1f; break;
            case 2: turnAmount = -1f; break;
        }

        carDriver.SetInputs(forwardAmount, turnAmount);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        int forwardAction = 0;
        if (Input.GetKey(KeyCode.UpArrow)) forwardAction = 1;
        if (Input.GetKey(KeyCode.DownArrow)) forwardAction = 2;

        int turnAction = 0;
        if (Input.GetKey(KeyCode.RightArrow)) turnAction = 1;
        if (Input.GetKey(KeyCode.LeftArrow)) turnAction = 2;

        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;
        discreteActions[0] = forwardAction;
        discreteActions[1] = turnAction;

        pathHandler.ChangeCheckpointColor();
    }

    private void CollectVelocityObservation(VectorSensor sensor)
    {
        Vector3 localVel = transform.InverseTransformVector(rb.velocity) / 10f;
        if (localVel.z >= 1f) localVel.z = 1f;
        sensor.AddObservation(localVel.z);
        //print(localVel.z);
    }

    private void CollectTraversingObservations(VectorSensor sensor)
    {
        List<float> deltas = MathExtensions.GetAngleDeltas(3, pathHandler.checkpoints, pathHandler.currentTargetCheckpoint, transform);
        for (int i = 0; i < deltas.Count; i++)
            sensor.AddObservation(deltas[i]);
        CollectVelocityObservation(sensor);
        sensor.AddObservation(distanceToTargetCheckpoint / 50f);
        sensor.AddObservation(rb.angularVelocity.y / (rb.maxAngularVelocity / 2f));
        sensor.AddObservation(0f);
        //sensor.AddObservation(Vector3.Dot(transform.forward, pathHandler.checkpoints[pathHandler.currentTargetCheckpoint].transform.forward));
    }

    private void CollectParkingObservations(VectorSensor sensor)
    {
        Transform targetTransform = null;
        if (state == CarState.Parking)
            targetTransform = parkingHandler.parkingSpot.spot.transform;
        else if (state == CarState.UnParking)
            targetTransform = pathHandler.checkpoints[pathHandler.currentTargetCheckpoint].transform;

        Vector3 pos = transform.InverseTransformPoint(targetTransform.position);
        sensor.AddObservation(pos.x);
        sensor.AddObservation(pos.z);

        parkingRelativePosition = (targetTransform.position - transform.position);
        Vector3 parkingRelativePositionNorm = transform.InverseTransformDirection(parkingRelativePosition.normalized);
        sensor.AddObservation(parkingRelativePositionNorm.x);
        sensor.AddObservation(parkingRelativePositionNorm.z);

        sensor.AddObservation(parkingRelativePosition.magnitude / 50f);
        /*if(state == CarState.UnParking)
        {
            float angle = Vector3.SignedAngle(transform.forward, (targetTransform.position - transform.position), Vector3.up) / 180f;
            sensor.AddObservation(angle);
        }
        else if(state == CarState.Parking)
        {*/
            float dot = Vector3.Dot(transform.forward, targetTransform.forward);
            sensor.AddObservation(dot);
        //}

        CollectVelocityObservation(sensor);

        parkingHandler.SetPrevParkingDistance();
        parkingHandler.distanceToParkingSpot = parkingRelativePosition.magnitude / 50f;
    }

    public void FinishEpisode(float reward)
    {
        AddReward(reward);
        EndEpisode();
    }
    public void ResetParkingSpotDistance()
    {
        parkingHandler.prevDistanceToParkingSpot = Mathf.Infinity;
    }

    public void NotifyPathFinished()
    {
        OnPathFinished?.Invoke(this);
    }
}

/*private IEnumerator WaitForNextCheckpoint(int prevCheckpoint)
{
    yield return new WaitForSeconds(20f);
    if (prevCheckpoint == currentTargetCheckpoint)
    {
        FinishEpisode(0f);
    }
}*/

/*private float GetDistanceToCheckpoint()
{
    float dist;
    if (currentTargetCheckpoint < checkpoints.Count)
        dist = Vector3.Distance(transform.position, checkpoints[currentTargetCheckpoint].transform.position);
    else
        dist = 0f;
    return dist;
}*/

/*public void SetSpawnPoint(SpawnPoint spawnPoint)
   {
       this.spawnPoint = spawnPoint;
       paths = new List<TrackCheckpoints>();
       checkpoints = new List<CheckpointSingle>();
       var parkingSpot = spawnPoint.GetParkingSpot();
       if(parkingSpot != null)
       {
           InitParking(parkingSpot);
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
       while(iteratorPath.HasLinkedPath)
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
       if(carTransform == transform && state == CarState.Traversing)
       {
           AddReward(0.5f);
           OffsetCheckpoint();
           //if (waitForNextCheckpointCoroutine != null)
           //    StopCoroutine(waitForNextCheckpointCoroutine);
           //waitForNextCheckpointCoroutine =  StartCoroutine(WaitForNextCheckpoint(currentTargetCheckpoint));
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
               if(index - subtractedTotal > -1)
               {
                   if(paths[i] != null)
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
   }*/

/*private void PathFinished()
{
    if (!isAlreadyFinished)
    {
        for (int i = 0; i < previousTargetAmount; i++)
        {
            ToggleCheckpoint(currentTargetCheckpoint - i, false);
        }

        if (paths != null)
        {
            for (int i = 0; i < paths.Count; i++)
            {
                if(paths[i] != null)
                {
                    UnSubscribeToPathEvents(paths[i]);
                    paths[i].RemoveCar(transform);
                    paths[i].ResetPath(this);
                }
            }
        }
    }
    else
        isAlreadyFinished = false;
    OnPathFinished?.Invoke(this);
}*/
//var nextCheckpoint = currentPath.GetNextCheckpoint(transform);
//if(nextCheckpoint != null)
//{
//Vector3 checkPointForward = nextCheckpoint.transform.forward;
//float directionDot = Vector3.Dot(transform.forward, checkPointForward);
//sensor.AddObservation(directionDot);
/*sensor.AddObservation(nextCheckpoint.transform.position.x/180);
sensor.AddObservation(nextCheckpoint.transform.position.z/150);
sensor.AddObservation(transform.position.x/180);
sensor.AddObservation(transform.position.z/150);
print(nextCheckpoint.transform.position);
print(transform.position);*/
//Vector3 checkpointPos = nextCheckpoint.transform.position;

/*if(currentTargetCheckpoint < checkpoints.Count)
{
    Vector3 checkpointPos = checkpoints[currentTargetCheckpoint].transform.position;
    Vector3 thisPos = transform.position;
    checkpointPos.y = thisPos.y = 0f;
    Vector3 dir = checkpointPos - thisPos;
    float signedAngle = Vector3.SignedAngle(transform.forward, dir, Vector3.up) / 180f;
    //print("Angle: "  + signedAngle);
    sensor.AddObservation(signedAngle);
}*/

/*private void TrackCheckpoints_OnCarWrongPath(Transform carTransform)
{
    if (carTransform == transform)
    {
        //AddReward(-1f);
    }
}*/

/*if (checkpoints == null || index >= checkpoints.Count || index < 0) return;
CheckpointSingle checkpoint = checkpoints[index];
if (activate)
{
    if (checkpoint.usedByCount <= 0)
        checkpoint.Activate();
    checkpoint.usedByCount++;
}
else
{
    checkpoint.usedByCount--;
    if (checkpoint.usedByCount <= 0)
        checkpoint.Deactivate();
    if (index == currentPath.GetCheckpointCount() - 1)
        currentPath.DeactivatedLastCheckpoint();
}*/

/*private void TrackCheckpoints_OnCarWrongCheckpoint(Transform carTransform)
{
    if (carTransform == transform && state == CarState.Traversing)
    {
        AddReward(-1f);
    }
}*/

/*private List<float> GetAngleDeltas(int amount)
{
    List<float> deltas = new List<float>();
    for (int i = 0; i < amount; i++)
    {
        CheckpointSingle firstCheckpoint = null, secondCheckpoint = null;
        int index = currentTargetCheckpoint + i;
        if (index < checkpoints.Count)
            firstCheckpoint = checkpoints[index];
        if (firstCheckpoint != null)
        {
            Vector3 fCP = firstCheckpoint.transform.position;
            //Vector3 sCP = secondCheckpoint.transform.position;
            Vector3 carPos = transform.position;
            fCP.y = carPos.y = 0f;
            Vector3 dir = fCP - carPos;
            deltas.Add(Vector3.SignedAngle(transform.forward, dir, Vector3.up) / 180f);
            //deltas.Add(Mathf.DeltaAngle(firstCheckpoint.transform.eulerAngles.y, secondCheckpoint.transform.eulerAngles.y));
        }
        else
        {
            if (i > 0)
                deltas.Add(deltas[i - 1]);
            else
                deltas.Add(0f);
        }
    }
    return deltas;
}
*/

/*public bool CheckStopParking(Vector3 forward)
{
    if (state == CarState.Parking && !IsOnParkStop)
    {
        if (carDriver.IsStopping())
        {
            //carDriver.StopCompletely();
            StartCoroutine(WaitForParking(forward));

            //AddReward(dot);
            //print("Stopped: " + Vector3.Dot(transform.forward, forward));
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
        AddReward(dot);
        parkingSpot.StopChecking();
        StopAllCoroutines();
        ChangeState(CarState.UnParking);
    }
    else
    {
        IsOnParkStop = false;
    }
}*/

/*private void InitParking(ParkingSpot parkingSpot)
    {
        if(parkingSpot == null)
        {
            FinishEpisode(0f);
            return;
        }
        this.parkingSpot = parkingSpot;
        parkingSpot.Occupy(this);
        ChangeState(CarState.Parking);
        distanceToParkingSpot = 0f;
        prevDistanceToParkingSpot = Mathf.Infinity;
    }*/


/*sensor.AddObservation(parkingRelativePosition.normalized.x);
sensor.AddObservation(parkingRelativePosition.normalized.z);*/
//print(parkingRelativePosition.normalized);

//parkingRelativePosition = parkingSpot.transform.InverseTransformPoint(transform.position);
//sensor.AddObservation(parkingRelativePosition.x / 15f);
//print("reltaviex : " + parkingRelativePosition.x / 15f);
//sensor.AddObservation(parkingRelativePosition.z /15f);
//print("relativez : " + parkingRelativePosition.z / 15f);
//print(relativePosition);

//print(parkingRelativePosition.magnitude/50f);
/*float magnitude = parkingRelativePosition.magnitude;
magnitude *= 10;
float[] magnitudeFragments = MathExtensions.GetDigitsNormalized((int)magnitude, 3);
for (int i = magnitudeFragments.Length-1; i >= 0; i--)
{
    sensor.AddObservation(magnitudeFragments[i]);
    //print(magnitudeFragments[i]);
}*/


//AddReward(-1f / MaxStep);

/*float dot = Vector3.Dot(transform.forward, checkpoints[currentTargetCheckpoint].transform.forward);
AddReward(dot / 100f);*/
/*public void RemoveModel()
{
    behaviorParameters.Model = null;
    isRunning = false;
}*/