using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental;
using UnityEngine;
using UnityEngine.UI;

public class PhysicsMovement : MonoBehaviour
{
    public float MinGroundNormalY = .3f;

    public float GravityModifier = 1f;

    public Vector2 Velocity;

    public LayerMask LayerMask;

    protected Vector2 targetVelocity;

    protected bool grounded;

    protected Vector2 groundNormal;

    protected Rigidbody2D rb2d;

    protected ContactFilter2D contactFilter;

    protected RaycastHit2D[] hitBuffer = new RaycastHit2D[16];

    protected List<RaycastHit2D> hitBufferList = new List<RaycastHit2D>(16);

    protected const float minMoveDistance = 0.001f;

    protected const float shellRadius = 0.01f;

    private void OnEnable()
    {
        rb2d = GetComponent<Rigidbody2D>();
    }

    // Start is called before the first frame update
    void Start()
    {
        contactFilter.useTriggers = false;
        contactFilter.SetLayerMask(LayerMask);
        contactFilter.useLayerMask = true;
    }

    // Update is called once per frame
    void Update()
    {
        targetVelocity = new Vector2(Input.GetAxis("Horizontal"), 0);
        if (Input.GetKey(KeyCode.Space) && grounded)
            Velocity.y = 5;
    }

    private void FixedUpdate()
    {
        Velocity += Physics2D.gravity * GravityModifier * Time.deltaTime;
        //Velocity += Physics2D.gravity * (GravityModifier * Time.deltaTime);
        Velocity.x = targetVelocity.x;
        grounded = false;
        Vector2 deltaPosition = Velocity * Time.deltaTime;
        Vector2 moveAlongGround = new Vector2(groundNormal.y, -groundNormal.x);
        Vector2 move = moveAlongGround * deltaPosition.x;

        Movement(move, false);
        move = Vector2.up * deltaPosition.y;
        Movement(move, true);
    }

    void Movement(Vector2 move, bool yMovement)
    {
        float distance = move.magnitude;
        if (distance > minMoveDistance)
        {
            int count = rb2d.Cast(move, contactFilter, hitBuffer, distance + shellRadius);
            hitBufferList.Clear();
            for (int i = 0; i < count; i++)
            {
                hitBufferList.Add(hitBuffer[i]);
            }

            for (int i = 0; i < hitBufferList.Count; i++)
            {
                Vector2 currentNormal = hitBufferList[i].normal;
                if (currentNormal.y > MinGroundNormalY)
                {
                    grounded = true;
                    if (yMovement)
                    {
                        groundNormal = currentNormal;
                        currentNormal.x = 0;
                    }
                }

                float projection = Vector2.Dot(Velocity, currentNormal);
                if (projection < 0)
                    Velocity = Velocity - projection * currentNormal;
                float modifiedDistance = hitBufferList[i].distance - shellRadius;
                distance = modifiedDistance < distance ? modifiedDistance : distance;
            }
        }

        rb2d.position = rb2d.position + move.normalized* distance;

}

}
