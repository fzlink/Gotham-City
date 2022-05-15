using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarDriver : MonoBehaviour {

    #region Fields
    private float speed;
    private float speedMax = 10f;
    private float speedMin = -10f;
    private float acceleration = 20f;
    private float brakeSpeed = 100f;
    private float reverseSpeed = 10f;
    private float idleSlowdown = 10f;

    private float turnSpeed;
    private float turnSpeedMax = 200f;
    private float turnSpeedAcceleration = 150f;
    private float turnIdleSlowdown = 500f;

    private float forwardAmount;
    private float turnAmount;

    private Rigidbody carRigidbody;
    private bool inputSet;
    #endregion

    private void Awake() {
        carRigidbody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate() {
        if (forwardAmount > 0) {
            // Accelerating
            if(speed > 0)
            {
                speed += forwardAmount * acceleration * Time.fixedDeltaTime;
            }
            else
            {
                speed += forwardAmount * brakeSpeed * Time.fixedDeltaTime;
            }
        }
        if (forwardAmount < 0) {
            if (speed > 0) {
                // Braking
                speed += forwardAmount * brakeSpeed * Time.fixedDeltaTime;
            } else {
                // Reversing
                speed += forwardAmount * reverseSpeed * Time.fixedDeltaTime;
            }
        }

        if (forwardAmount == 0) {
            // Not accelerating or braking
            if (speed > 0) {
                speed -= idleSlowdown * Time.fixedDeltaTime;
            }
            if (speed < 0) {
                speed += idleSlowdown * Time.fixedDeltaTime;
            }
        }

        speed = Mathf.Clamp(speed, speedMin, speedMax);

        carRigidbody.velocity = transform.forward * speed;

        if (speed < 0 && inputSet && forwardAmount == -1) {
            // Going backwards, invert wheels
            turnAmount = turnAmount * -1f;
            inputSet = false;
        }

        if (turnAmount > 0 || turnAmount < 0) {
            // Turning
            if ((turnSpeed > 0 && turnAmount < 0) || (turnSpeed < 0 && turnAmount > 0)) {
                // Changing turn direction
                float minTurnAmount = 20f;
                turnSpeed = turnAmount * minTurnAmount;
            }
            turnSpeed += turnAmount * turnSpeedAcceleration * Time.fixedDeltaTime;
        } else {
            // Not turning
            if (turnSpeed > 0) {
                turnSpeed -= turnIdleSlowdown * Time.fixedDeltaTime;
            }
            if (turnSpeed < 0) {
                turnSpeed += turnIdleSlowdown * Time.fixedDeltaTime;
            }
            if (turnSpeed > -10f && turnSpeed < +10f) {
                // Stop rotating
                turnSpeed = 0f;
            }
        }

        float speedNormalized = speed / speedMax;
        float invertSpeedNormalized = Mathf.Clamp(1 - speedNormalized, .75f, 1f);

        turnSpeed = Mathf.Clamp(turnSpeed, -turnSpeedMax, turnSpeedMax);

        carRigidbody.angularVelocity = new Vector3(0, turnSpeed * (invertSpeedNormalized * 1f) * Mathf.Deg2Rad, 0);

        if (transform.eulerAngles.x > 2 || transform.eulerAngles.x < -2 || transform.eulerAngles.z > 2 || transform.eulerAngles.z < -2) {
            transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
        }
    }

    private void OnCollisionEnter(Collision collision) {
        if (collision.gameObject.layer == 6) {
            speed = Mathf.Clamp(speed, 0f, 20f);
        }
    }

    public void SetInputs(float forwardAmount, float turnAmount) {
        this.forwardAmount = forwardAmount;
        this.turnAmount = turnAmount;
        inputSet = true;
    }

    public void ClearTurnSpeed() {
        turnSpeed = 0f;
    }

    public float GetSpeed() {
        return speed;
    }


    public void SetSpeedMax(float speedMax) {
        this.speedMax = speedMax;
    }

    public void SetTurnSpeedMax(float turnSpeedMax) {
        this.turnSpeedMax = turnSpeedMax;
    }

    public void SetTurnSpeedAcceleration(float turnSpeedAcceleration) {
        this.turnSpeedAcceleration = turnSpeedAcceleration;
    }

    public void StopCompletely() {
        speed = 0f;
        turnSpeed = 0f;
    }

    public bool IsStopping()
    {
        float speed = carRigidbody.velocity.magnitude;
        return speed < 0.75f;
    }

    public void ChangeMaxSpeed()
    {
        speedMax = 10f + UnityEngine.Random.Range(0f, 10f);
    }

}
