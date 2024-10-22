using System;
using System.Collections.Generic;
using UnityEngine;


public interface IMovement
{
    Vector2 CalculateMovement(Vector2 currentPOsition, Vector2 targetPosition);
}

