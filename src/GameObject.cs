using OpenTK.Mathematics;

namespace Breakout
{
    internal class GameObject
    {
        public Vector2 Position;
        public Vector2 Size;
        public Vector2 Velocity;
        public bool IsSolid;     // e.g., walls or unbreakable bricks (optional)
        public bool Destroyed;   // set to true when hit

        public GameObject(Vector2 position, Vector2 size, Vector2 velocity, bool isSolid = false)
        {
            Position = position;
            Size = size;
            Velocity = velocity;
            IsSolid = isSolid;
            Destroyed = false;
        }

        // Use your own rendering here (VAO/VBO/shader)
        public virtual void Draw()
        {
            // TODO: draw rectangle using OpenGL
        }

        public Vector2 Center => Position + Size / 2f;
    }
}