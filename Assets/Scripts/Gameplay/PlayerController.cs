using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : TankController
{

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    protected override void Update()
    {
        base.Update();

        if (!CanUpdate())
            return;

        float vertical = Input.GetAxis("Vertical1");
        float horizontal = Input.GetAxis("Horizontal1");

        MoveVector = new Vector3(horizontal, 0, vertical);

        if (Input.GetMouseButtonDown(0))
        {
            Shoot();
        }
    }
}
