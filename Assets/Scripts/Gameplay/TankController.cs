using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Serves as the parent class of Player and Bot Controller to provide common functionality
/// </summary>
public abstract class TankController : MonoBehaviour
{
    public new Rigidbody rigidbody;
    public new Collider collider;
    public Transform turret;
    public GameObject destroyEffectPrefab;
    public Slider healthSlider;
    public Transform shootPoint;
    public Cannon cannonPrefab;

    [Header("Settings")]
    public float moveSpeed = 10;
    public int health = 100; // Represents the Max Health
    public float shootForce = 10;
    public float shootInterval = 1;
    public float cannonDamage = 25;

    public float Health { get; protected set; }
    public bool IsDead { get; protected set; }
    public Vector3 MoveVector { get; protected set; }
    public Vector3 ShootDirection { get; set; }

    protected float m_ShootTimer = 0.0f;


    protected virtual void Awake()
    {
        Health = health;
        OnHealthUpdated();

        ShootDirection = transform.forward;
    }

    protected void OnHealthUpdated()
    {
        healthSlider.value = Mathf.Lerp(healthSlider.minValue, healthSlider.maxValue, Mathf.Clamp01(Health / health));
    }

    /// <summary>
    /// Returns if the player or AI should run their Update Logic
    /// </summary>
    protected bool CanUpdate()
    {
        return GameManager.Instance.HasGameStarted && !GameManager.Instance.HasGameFinished;
    }

    protected virtual void FixedUpdate()
    {
        if (!CanUpdate())
            return;

        Vector3 moveVec = MoveVector;
        moveVec.y = 0; // prevent movement in Y axis (vertical)
        moveVec = Vector3.ClampMagnitude(moveVec, 1); // Move Vector should never be more than 1 unit in magnitude

        rigidbody.velocity = moveVec * moveSpeed;
        if (moveVec != Vector3.zero)
            rigidbody.rotation = Quaternion.LookRotation(moveVec);
    }

    protected virtual void Update()
    {
        if (!CanUpdate())
            return;

        turret.rotation = Quaternion.LookRotation(ShootDirection, Vector3.up);

        m_ShootTimer += Time.deltaTime;
    }

    public virtual void Shoot()
    {
        if (m_ShootTimer > shootInterval)
        {
            m_ShootTimer = 0;

            Vector3 shootDir = ShootDirection;
            shootDir.y = 0;

            // Spawn a cannon
            Cannon cannon = Instantiate(cannonPrefab.gameObject, shootPoint.position, Quaternion.LookRotation(shootDir, shootPoint.up)).GetComponent<Cannon>();
            cannon.Initialize(this, cannonDamage); // Set owner and damage of the cannon
            
            Physics.IgnoreCollision(collider, cannon.collider);

            cannon.rigidbody.AddForce(shootDir * shootForce, ForceMode.VelocityChange); // Apply Impulse force to the cannon
        }
    }

    /// <summary>
    /// Call this function to apply damage to the tank
    /// </summary>
    public virtual void ApplyDamage(float damage)
    {
        if (IsDead) return;

        Health -= damage;
        if (Health <= 0)
        {
            Health = 0;
            Kill();
        }

        OnHealthUpdated();
    }

    /// <summary>
    /// Kill the tank directly
    /// </summary>
    public virtual void Kill()
    {
        if (IsDead) return;

        IsDead = true;
        GameObject effects = Instantiate(destroyEffectPrefab, transform.position, Quaternion.identity);
        Destroy(effects, 3f);

        if (this is BotController) // if this is a bot, then let the Game Manager know about it's death
            GameManager.Instance.OnBotDead(this as BotController);
        Destroy(gameObject);
    }
}
