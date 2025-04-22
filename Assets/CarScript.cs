using System.Linq;
using UnityEngine;


public class CarScript : MonoBehaviour
{
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float maxSpeed = 10f;
    [SerializeField] private float turnSpeed = 10f;
    [SerializeField] private float maxTurnSpeed = 45f;

    [SerializeField] private Transform[] points;
    private int nextPointIndex;


    private void Start()
    {
        nextPointIndex = GetNearestPointIndex();
    }


    private int GetNearestPointIndex()
    {
        return points.Select((p, i) => (p.position - transform.position, i)).OrderBy(p => p.Item1.sqrMagnitude).First().Item2;
    }


    private void Update()
    {
        CheckDestination();
        Move();
    }


    private void CheckDestination()
    {
        if (Vector3.Distance(transform.position, points[nextPointIndex].position) < 2)
            NextPoint();
    }


    private void Move()
    {
        var direction = (points[nextPointIndex].position - transform.position).normalized;
        var angle = Vector3.SignedAngle(transform.forward, direction, Vector3.up);

        var turnAmount = Mathf.Clamp(angle * turnSpeed * Time.deltaTime, -maxTurnSpeed, maxTurnSpeed);
        transform.Rotate(Vector3.up * turnAmount);

        var currentSpeed = Vector3.Dot(transform.forward, direction) * acceleration * Time.deltaTime;
        currentSpeed = Mathf.Clamp(currentSpeed, 0, maxSpeed);
        transform.position += transform.forward * currentSpeed;
    }


    private void NextPoint()
    {
        nextPointIndex = (nextPointIndex + 1) % points.Length;
    }
}