using UnityEngine;

public class Move : MonoBehaviour
{
    public float moveSpeed = 10f; // Vitesse de déplacement
    public float turnSpeed = 50f; // Vitesse de rotation
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        // Récupérer les inputs ZQSD
        float moveInput = Input.GetAxis("Vertical"); // Z (avancer) / S (reculer)
        float turnInput = Input.GetAxis("Horizontal"); // Q (gauche) / D (droite)

        // Calculer le déplacement
        Vector3 moveDirection = transform.forward * moveInput * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + moveDirection);

        // Calculer la rotation
        float turn = turnInput * turnSpeed * Time.fixedDeltaTime;
        Quaternion turnRotation = Quaternion.Euler(0, turn, 0);
        rb.MoveRotation(rb.rotation * turnRotation);
    }
}