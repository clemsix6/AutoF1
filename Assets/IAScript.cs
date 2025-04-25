using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;


public class CarAgent : Agent
{
    [Header("Car Settings")] public Rigidbody carRigidbody;
    public float maxMotorTorque = 1500f;
    public float maxSteeringAngle = 30f;

    [Header("Raycast Settings")] public int numRays = 7;
    public float rayLength = 20f;
    public float rayAngleRange = 90f;

    [Header("Episode Settings")] public Transform[] resetPoints;
    private Vector3 startPosition;
    private Quaternion startRotation;


    public override void Initialize()
    {
        if (carRigidbody == null) carRigidbody = GetComponent<Rigidbody>();
        startPosition = transform.position;
        startRotation = transform.rotation;
    }


    public override void OnEpisodeBegin()
    {
        var idx = Random.Range(0, resetPoints.Length);
        transform.position = resetPoints.Length > 0
            ? resetPoints[idx].position
            : startPosition;
        transform.rotation = resetPoints.Length > 0
            ? resetPoints[idx].rotation
            : startRotation;

        carRigidbody.linearVelocity = Vector3.zero;
        carRigidbody.angularVelocity = Vector3.zero;
    }


    public override void CollectObservations(VectorSensor sensor)
    {
        var halfSpan = rayAngleRange / 2f;
        for (var i = 0; i < numRays; i++) {
            var angle = -halfSpan + (rayAngleRange / (numRays - 1)) * i;
            var dir = Quaternion.Euler(0, angle, 0) * transform.forward;
            var ray = new Ray(transform.position + Vector3.up * 0.5f, dir);
            if (Physics.Raycast(ray, out var hit, rayLength)) {
                sensor.AddObservation(hit.distance / rayLength);
                Debug.DrawRay(ray.origin, dir * hit.distance, Color.red);
            } else {
                sensor.AddObservation(1f);
                Debug.DrawRay(ray.origin, dir * rayLength, Color.green);
            }
        }

        var forwardSpeed = Vector3.Dot(carRigidbody.linearVelocity, transform.forward);
        sensor.AddObservation(forwardSpeed / 20f); // normalize by max speed ~20 m/s
    }


    public override void OnActionReceived(ActionBuffers actions)
    {
        var continuous = actions.ContinuousActions;
        var steer = Mathf.Clamp(continuous[0], -1f, 1f) * maxSteeringAngle;
        var accel = Mathf.Clamp(continuous[1], -1f, 1f);

        ApplySteering(steer);
        ApplyMotor(accel);

        var forwardReward = Vector3.Dot(carRigidbody.linearVelocity, transform.forward) * 0.01f;
        AddReward(forwardReward);

        var lateralVel = transform.InverseTransformDirection(carRigidbody.linearVelocity);
        AddReward(-Mathf.Abs(lateralVel.x) * 0.001f);
    }


    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var cont = actionsOut.ContinuousActions;
        cont[0] = Input.GetAxis("Horizontal");
        cont[1] = Input.GetAxis("Vertical");
    }


    private void ApplySteering(float angle)
    {
        foreach (Transform wheel in transform.Find("Wheels/Front"))
            wheel.localRotation = Quaternion.Euler(0, angle, 0);
    }


    private void ApplyMotor(float accel)
    {
        var force = transform.forward * accel * maxMotorTorque * Time.fixedDeltaTime;
        carRigidbody.AddForce(force);
    }


    private void OnCollisionEnter()
    {
        AddReward(-0.5f);
    }
}