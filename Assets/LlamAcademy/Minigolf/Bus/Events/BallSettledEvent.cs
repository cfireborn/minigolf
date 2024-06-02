using UnityEngine;

namespace LlamAcademy.Minigolf.Bus.Events
{
    public struct BallSettledEvent
    {
        public Vector3 Position;

        public BallSettledEvent(Vector3 position)
        {
            Position = position;
        }
    }
}
