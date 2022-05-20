using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blob : MonoBehaviour
{
    public SpriteRenderer body;

    [Space]

    public float moveForce;

    [Space]

    public Color color;
    public Vector2 finalPosition;
    public bool goToFinalPosition = false;

    private Vector2? randomTargetPosition = null;
    private float waitBeforeNextRandomMove;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Start is called before the first frame update
    void Start()
    {
        body.color = color;

        waitBeforeNextRandomMove = Random.Range(1f, 3f);
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 localScale = transform.localScale;
        localScale.x = rb.velocity.sqrMagnitude / 20 + 1;
        localScale.x = Mathf.Clamp(localScale.x, 1, 2);
        localScale.y = 1 / localScale.x;
        transform.localScale = localScale;

        if (rb.velocity.sqrMagnitude > 0.1f)
        {
            Vector2 direction = rb.velocity;
            transform.rotation = Quaternion.AngleAxis(Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg, Vector3.forward);
        }
    }

    private void FixedUpdate()
    {
        if (goToFinalPosition)
            MoveToPosition(finalPosition);
        else
            RandomMove();
    }

    private void RandomMove()
    {
        if (randomTargetPosition.HasValue)
        {
            float sqrDistanceFromRandomTargetPosition = (randomTargetPosition.Value - (Vector2)transform.position).sqrMagnitude;

            if (sqrDistanceFromRandomTargetPosition > 0.1f)
            {
                MoveToPosition(randomTargetPosition.Value);
                return;
            }
        }

        if (waitBeforeNextRandomMove > 0)
        {
            waitBeforeNextRandomMove -= Time.fixedDeltaTime;
            randomTargetPosition = null;
            return;
        }

        float angle = Random.Range(0f, 360f);
        float distance = Random.Range(1f, 3f);
        Vector2 offset = new Vector2(Mathf.Cos(angle) * distance, Mathf.Sin(angle) * distance);

        randomTargetPosition = (Vector2)transform.position + offset;
        waitBeforeNextRandomMove = Random.Range(1f, 3f);
    }

    private void MoveToPosition(Vector2 targetPosition)
    {
        Vector2 direction = targetPosition - (Vector2)transform.position;

        if (direction.sqrMagnitude > 1)
            direction.Normalize();

        rb.AddForce(direction * moveForce);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (GameManager.instance.disableCollision == true)
            return;

        Blob otherBlob = collision.GetComponent<Blob>();

        if (otherBlob == null)
            return;

        Vector2 myDirection = transform.position - otherBlob.transform.position;
        Vector2 otherBlobDirection = -myDirection;
        float sqrDistance = Vector2.SqrMagnitude(myDirection);

        if (sqrDistance == 0)
            return;

        float force = 2 / sqrDistance;

        rb.AddForce(myDirection * force); // TODO normalized ?
        otherBlob.rb.AddForce(otherBlobDirection * force); // TODO normalized ?
    }
}
