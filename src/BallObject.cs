using OpenTK.Mathematics;

namespace Breakout
{
    internal class BallObject
    {
        public Vector2 Position;
        public float Radius;
        public Vector2 Velocity;
        public bool Stuck;   // stuck to paddle until you launch (optional)

        public BallObject(Vector2 position, float radius, Vector2 velocity)
        {
            Position = position;
            Radius = radius;
            Velocity = velocity;
            Stuck = true;   // start attached to paddle
        }

        // Move ball and handle screen-edge collisions
        public void Move(float dt, float windowWidth, float windowHeight)
        {
            if (Stuck) return;

            Position += Velocity * dt;

            // Left / right walls
            if (Position.X <= 0.0f)
            {
                Position.X = 0.0f;
                Velocity.X *= -1.0f;
            }
            if (Position.X + Radius * 2 >= windowWidth)
            {
                Position.X = windowWidth - Radius * 2;
                Velocity.X *= -1.0f;
            }

            // Top wall
            if (Position.Y <= 0.0f)
            {
                Position.Y = 0.0f;
                Velocity.Y *= -1.0f;
            }

            // Bottom: you could treat this as “lose life”
            // if (Position.Y + Radius * 2 >= windowHeight) { ... }
        }

        public Vector2 Center => Position + new Vector2(Radius, Radius);

        public void Reset(Vector2 newPosition, Vector2 newVelocity)
        {
            Position = newPosition;
            Velocity = newVelocity;
            Stuck = true;
        }

        public void Draw()
        {
            // TODO: draw circle (or quad with circle texture)
        }
    }
}