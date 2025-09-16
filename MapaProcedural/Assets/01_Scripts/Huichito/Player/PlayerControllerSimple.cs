using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerControllerSimple : MonoBehaviour
{
    [Header("Movimiento")]
    public float moveSpeed = 6f;
    public float gravity = -18f;
    public float jumpSpeed = 7.5f;

    [Header("Esquiva (Ctrl)")]
    public float dodgeDistance = 5f;     // cu�nto avanza
    public float dodgeDuration = 0.25f;  // tiempo del dash
    public float dodgeCooldown = 0.8f;   // cd entre dashes
    public float iframeDuration = 0.3f;  // invulnerable durante el dash

    CharacterController cc;
    Health health;
    float yVel;
    bool isDodging;
    float dodgeCdTimer;
    Vector3 dodgeVel;   // velocidad del dash (horizontal)
    float dodgeTime;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        health = GetComponent<Health>();
    }

    void Update()
    {
        // Rotaci�n con mouse (Yaw)
        float mouseX = Input.GetAxis("Mouse X") * 4f;
        transform.Rotate(0, mouseX, 0);

        // Input de movimiento (XZ)
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 wishDir = (transform.right * h + transform.forward * v).normalized;

        // Salto
        if (cc.isGrounded && yVel < 0) yVel = -2f;
        if (cc.isGrounded && Input.GetKeyDown(KeyCode.Space))
            yVel = jumpSpeed;

        // Esquiva (Ctrl) � en direcci�n del input; si no hay input, hacia adelante
        dodgeCdTimer -= Time.deltaTime;
        if (!isDodging && dodgeCdTimer <= 0f && Input.GetKeyDown(KeyCode.LeftControl))
        {
            Vector3 dashDir = wishDir.sqrMagnitude > 0.01f ? wishDir : transform.forward;
            float dashSpeed = dodgeDistance / Mathf.Max(0.05f, dodgeDuration);
            dodgeVel = dashDir * dashSpeed;

            isDodging = true;
            dodgeTime = 0f;
            dodgeCdTimer = dodgeCooldown;

            if (health) health.SetInvulnerableFor(iframeDuration);
        }

        Vector3 horizontalVel;

        if (isDodging)
        {
            dodgeTime += Time.deltaTime;
            horizontalVel = dodgeVel; // mantener velocidad constante durante el dash
            if (dodgeTime >= dodgeDuration) isDodging = false;
        }
        else
        {
            horizontalVel = wishDir * moveSpeed;
        }

        // Gravedad
        yVel += gravity * Time.deltaTime;

        // Mover
        Vector3 vel = horizontalVel + Vector3.up * yVel;
        cc.Move(vel * Time.deltaTime);
    }
}
