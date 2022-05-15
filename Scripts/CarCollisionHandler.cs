using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static CarAgent;

public class CarCollisionHandler : CarComponentBase
{

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == Layers.sideWalk)
        {
            if (carAgent.state == CarState.Traversing)
                carAgent.AddReward(-0.4f);
            else if (carAgent.state == CarState.Parking)
                carAgent.AddReward(-0.05f);
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == Layers.redLight)
            carAgent.AddReward(-1f);
        else if (other.gameObject.layer == Layers.car)
        {
            if (carAgent.state == CarState.Traversing)
                carAgent.AddReward(-1f);
            else if (carAgent.state == CarState.Parking)
                carAgent.AddReward(-0.1f);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer == Layers.redLightReward)
            carAgent.AddReward(0.01f);
        else if (other.gameObject.layer == Layers.car && carAgent.state == CarState.Traversing)
            carAgent.AddReward(-0.05f);
        else if (other.gameObject.layer == Layers.redLight)
            carAgent.AddReward(-0.01f);
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.layer == Layers.sideWalk)
            carAgent.AddReward(-0.005f);
    }

    protected override void OnCarEpisodeBegin()
    {
    }
}
