using UnityEngine;

public interface IInputProvider
{
    Vector2 MoveInput { get; }
    bool DashPressed { get; }
}
