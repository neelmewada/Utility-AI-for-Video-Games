using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The class that controls the AI tanks
/// </summary>
public class BotController : TankController
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


    }
}
