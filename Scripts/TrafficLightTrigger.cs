using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrafficLightTrigger : MonoBehaviour
{
    private Collider collider;
    private Renderer renderer;
    [SerializeField] private Collider rewardCollider;

    private void Awake()
    {
        collider = GetComponent<Collider>();
        renderer = GetComponent<Renderer>();
    }

    public void Toggle(bool activate)
    {
        collider.enabled = activate;
        renderer.enabled = activate;
        rewardCollider.enabled = activate;
    }

}
