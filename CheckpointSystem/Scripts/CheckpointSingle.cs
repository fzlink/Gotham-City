using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointSingle : MonoBehaviour {

    private TrackCheckpoints trackCheckpoints;
    private MeshRenderer meshRenderer;
    private Collider collider;
    public int usedByCount { get; set; }

    private void Awake() {
        meshRenderer = GetComponent<MeshRenderer>();
        collider = GetComponent<Collider>();
    }

    private void Start() {
        Deactivate();
    }

    private void OnTriggerEnter(Collider other) {
        if (other.TryGetComponent<CarAgent>(out CarAgent player)) {
            trackCheckpoints.CarThroughCheckpoint(this, other.transform);
        }
    }

    /*private void OnTriggerEnter2D(Collider2D collider) {
        if (collider.TryGetComponent<Player_RollSpeed>(out Player_RollSpeed player)) {
            trackCheckpoints.CarThroughCheckpoint(this, collider.transform);
        }
    }*/

    public void SetTrackCheckpoints(TrackCheckpoints trackCheckpoints) {
        this.trackCheckpoints = trackCheckpoints;
    }

    public void Show() {
        meshRenderer.enabled = true;
        //ChangeColor(Color.green);
    }

    public void Hide() {
        meshRenderer.enabled = false;
    }

    public void Activate()
    {
        collider.enabled = true;
        Show();
    }

    public void Deactivate()
    {
        collider.enabled = false;
        Hide();
    }

    public void ChangeColor(Color color)
    {
        color.a = 0.5f;
        meshRenderer.material.color = color;
    }

    public bool GetIsLastBeforeParking()
    {
        return (trackCheckpoints.HasParkingSpot()) && trackCheckpoints.IsLastCheckpoint(this);
    }

}
