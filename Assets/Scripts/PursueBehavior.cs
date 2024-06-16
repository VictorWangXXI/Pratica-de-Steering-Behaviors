using UnityEngine.UI;
using UnityEngine;

enum Behavior2 { Idle, Pursue }
enum State2 { Idle, Arrive, Pursue }

[RequireComponent(typeof(Rigidbody2D))]
public class PursueBehavior : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] Behavior2 behavior = Behavior2.Pursue;
    [SerializeField] Transform target = null;
    [SerializeField] float maxSpeed = 4f;
    [SerializeField, Range(0.1f, 0.99f)] float decelerationFactor = 0.75f;
    [SerializeField] float arriveRadius = 1.2f;
    [SerializeField] float stopRadius = 0.5f;

    Text behaviorDisplay = null;
    Rigidbody2D physics;

    public float maxPrediction = 2f;
    Rigidbody2D physicsTarget;

    State2 state = State2.Idle;


    void FixedUpdate()
    {
        if (target != null)
        {
            switch (behavior)
            {
                case Behavior2.Idle: IdleBehavior(); break;
                case Behavior2.Pursue: PursueBeh(); break;
            }
        }

        physics.velocity = Vector2.ClampMagnitude(physics.velocity, maxSpeed);

        behaviorDisplay.text = state.ToString().ToUpper();
    }

    void IdleBehavior()
    {
        physics.velocity = physics.velocity * decelerationFactor;
    }

    void PursueBeh()
    {
        Vector2 direction = target.transform.position - transform.position;
        float originalDistance = direction.magnitude;
        float speed = physics.velocity.magnitude;
        float prediction;
        if (speed <= (originalDistance / maxPrediction))
        {
            prediction = maxPrediction;
        }
        else
        {
            prediction = originalDistance / speed;
        }

        
        Vector2 predictedTarget = new Vector2 (target.transform.position.x + (physicsTarget.velocity.x * prediction), target.transform.position.y + (physicsTarget.velocity.y * prediction));

        Vector2 delta = new Vector2 (predictedTarget.x - transform.position.x, predictedTarget.y - transform.position.y);
        Vector2 steering = delta.normalized * maxSpeed - physics.velocity;
        float distance = delta.magnitude;

        if (distance < stopRadius)
        {
            state = State2.Idle;
        }
        else if (distance < arriveRadius)
        {
            state = State2.Arrive;
        }
        else
        {
            state = State2.Pursue;
        }

        switch (state)
        {
            case State2.Idle:
                IdleBehavior();
                break;
            case State2.Arrive:
                var arriveFactor = 0.01f + (distance - stopRadius) / (arriveRadius - stopRadius);
                physics.velocity += arriveFactor * steering * Time.fixedDeltaTime;
                break;
            case State2.Pursue:
                physics.velocity += steering * Time.fixedDeltaTime;
                break;
        }
    }


    void Awake()
    {
        physics = GetComponent<Rigidbody2D>();
        physics.isKinematic = true;
        behaviorDisplay = GetComponentInChildren<Text>();
        physicsTarget = target.GetComponent<Rigidbody2D>();

    }

    void OnDrawGizmos()
    {
        if (target == null)
        {
            return;
        }

        switch (behavior)
        {
            case Behavior2.Idle:
                break;
            case Behavior2.Pursue:
                Gizmos.color = Color.white;
                Gizmos.DrawWireSphere(transform.position, arriveRadius);
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(transform.position, stopRadius);
                break;
        }

        Gizmos.color = Color.gray;
        Gizmos.DrawLine(transform.position, target.position);
    }
}
