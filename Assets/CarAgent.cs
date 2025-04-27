using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))] // Ensure the agent has a Rigidbody

public class CarAgent : Agent
{
    [SerializeField] private Transform[] checkpoints; // List of checkpoints defining the path
    [SerializeField] private float accelerationMultiplier = 1000f; // Force applied for acceleration
    [SerializeField] private float turnTorque = 300f; // Torque applied for turning
    [SerializeField] private float maxVelocity = 20f; // Maximum speed of the car
    [SerializeField] private LayerMask trackLayer; // Layer mask to detect the track
    [SerializeField] private float offTrackDistanceThreshold = 1.0f; // How far below the car to check for the track

    private Rigidbody carRigidbody;
    private int nextCheckpointIndex = 0;
    private Vector3 startPosition;
    private Quaternion startRotation;

    // Called once when the agent initializes
    public override void Initialize()
    {
        carRigidbody = GetComponent<Rigidbody>();
        startPosition = transform.position;
        startRotation = transform.rotation;

        // Ensure physics settings are suitable for a car
        carRigidbody.centerOfMass = Vector3.down; // Lower center of mass for stability
    }

    // Called at the beginning of each training episode
    public override void OnEpisodeBegin()
    {
        // Reset car position, rotation and velocity
        transform.position = startPosition;
        transform.rotation = startRotation;
        carRigidbody.linearVelocity = Vector3.zero;
        carRigidbody.angularVelocity = Vector3.zero;

        // Find the closest checkpoint to start with (optional, could just start from 0)
        // nextCheckpointIndex = GetNearestCheckpointIndex();
        nextCheckpointIndex = 0; // Let's keep it simple and always start from the first checkpoint in the array
    }

    // Find the index of the checkpoint closest to the agent's current position
    // Useful if the agent starts at a random point or if checkpoints aren't perfectly ordered
    private int GetNearestCheckpointIndex()
    {
        var closestIndex = 0;
        var minDistance = float.MaxValue;

        for (var i = 0; i < checkpoints.Length; i++)
        {
            var distance = Vector3.Distance(transform.position, checkpoints[i].position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestIndex = i;
            }
        }
        return closestIndex;
    }


    // Collect observations for the agent's decision making
    public override void CollectObservations(VectorSensor sensor)
    {
        // Observe current velocity (normalized)
        sensor.AddObservation(carRigidbody.linearVelocity / maxVelocity); // 3 observations

        // Observe direction and distance to the next checkpoint
        if (checkpoints.Length > 0)
        {
            var nextCheckpoint = checkpoints[nextCheckpointIndex];
            var directionToCheckpoint = (nextCheckpoint.position - transform.position).normalized;
            var distanceToCheckpoint = Vector3.Distance(transform.position, nextCheckpoint.position);

            // Observe direction relative to car's forward direction
            sensor.AddObservation(transform.InverseTransformDirection(directionToCheckpoint)); // 3 observations
            sensor.AddObservation(distanceToCheckpoint / 100f); // Normalize distance (adjust 100f based on track size) // 1 observation
        }
        else
        {
            // Add placeholder observations if no checkpoints are assigned
            sensor.AddObservation(Vector3.zero); // 3 observations
            sensor.AddObservation(0f);           // 1 observation
        }

        // Total observations: 3 (velocity) + 3 (direction) + 1 (distance) = 7
    }

    // Process actions received from the neural network or heuristic control
    public override void OnActionReceived(ActionBuffers actions)
    {
        // Read continuous actions for steering and throttle
        var steerAction = actions.ContinuousActions[0]; // Value between -1 (left) and 1 (right)
        var throttleAction = actions.ContinuousActions[1]; // Value between -1 (brake/reverse) and 1 (accelerate)

        // Apply steering torque
        carRigidbody.AddTorque(transform.up * turnTorque * steerAction);

        // Apply acceleration/braking force
        var forwardForce = transform.forward * accelerationMultiplier * throttleAction;
        carRigidbody.AddForce(forwardForce);

        // Clamp velocity to max speed
        if (carRigidbody.linearVelocity.sqrMagnitude > maxVelocity * maxVelocity)
            carRigidbody.linearVelocity = carRigidbody.linearVelocity.normalized * maxVelocity;


        // --- Rewards ---

        // Small negative reward per step to encourage finishing faster
        AddReward(-0.001f);

        // Reward for moving towards the next checkpoint
        if (checkpoints.Length > 0)
        {
            var nextCheckpoint = checkpoints[nextCheckpointIndex];
            var directionToCheckpoint = (nextCheckpoint.position - transform.position).normalized;
            var forwardAlignment = Vector3.Dot(transform.forward, directionToCheckpoint);
            // Add reward based on how much the car is facing and moving towards the checkpoint
            AddReward(forwardAlignment * 0.01f);
        }

        // Check if the car has reached the next checkpoint
        CheckCheckpointReached();

        // Check if the car is on the track
        CheckOffTrack();
    }

    // Check if the agent has reached the current target checkpoint
    private void CheckCheckpointReached()
    {
        if (checkpoints.Length == 0) return;

        var distanceToCheckpoint = Vector3.Distance(transform.position, checkpoints[nextCheckpointIndex].position);

        // Check if close enough to the checkpoint (adjust distance threshold as needed)
        if (distanceToCheckpoint < 5f) // Threshold distance to consider checkpoint 'reached'
        {
            AddReward(1.0f); // Significant reward for reaching a checkpoint
            Debug.Log($"Reached checkpoint {nextCheckpointIndex}");

            // Move to the next checkpoint in the list
            nextCheckpointIndex = (nextCheckpointIndex + 1) % checkpoints.Length;

            // Optional: End episode after a full lap
            // if (nextCheckpointIndex == 0) {
            //     AddReward(5.0f); // Bonus for completing a lap
            //     Debug.Log("Lap Completed!");
            //     EndEpisode();
            // }
        }
    }

    // Check if the car has gone off the track
    private void CheckOffTrack()
    {
        // Raycast downwards to see what the car is driving on
        if (Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, out var hit, offTrackDistanceThreshold, trackLayer))
        {
            // If the ray hit something AND it's tagged "Track", we are on track
            if (hit.collider.CompareTag("Track"))
            {
                // On track - potentially add a small positive reward or do nothing
                 AddReward(0.002f); // Small reward for staying on track
            }
            else
            {
                // Hit something, but it's not tagged "Track" (e.g., grass, gravel, maybe even a wall if layers are not set correctly)
                AddReward(-0.05f); // Penalty for being off-track
                // Consider ending the episode if severely off-track
                 // EndEpisode(); // Optional: uncomment to end episode immediately when off track
            }
        }
        else
        {
            // Raycast didn't hit anything within the threshold distance (likely airborne or completely off)
            AddReward(-0.1f); // Larger penalty
            EndEpisode(); // End the episode if the car is lost
        }
    }


    // Handle collisions (e.g., hitting walls)
    private void OnCollisionEnter(Collision collision)
    {
        // Check if the car collided with a wall
        if (collision.collider.CompareTag("Wall"))
        {
            AddReward(-1.0f); // Strong penalty for hitting a wall
            Debug.Log("Hit a wall!");
            EndEpisode(); // End the episode on collision
        }
    }

    // Provide manual control for testing purposes (optional)
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Horizontal"); // Steering
        continuousActionsOut[1] = Input.GetAxis("Vertical");   // Throttle
    }
}
