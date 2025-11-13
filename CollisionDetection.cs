using OpenTK.Mathematics;

namespace Breakout
{
    internal static class Collision
    {
        // AABB - AABB
        public static bool CheckAabbAabb(GameObject a, GameObject b)
        {
            bool collisionX = a.Position.X + a.Size.X >= b.Position.X &&
                              b.Position.X + b.Size.X >= a.Position.X;

            bool collisionY = a.Position.Y + a.Size.Y >= b.Position.Y &&
                              b.Position.Y + b.Size.Y >= a.Position.Y;

            return collisionX && collisionY;
        }

        // AABB - Circle
        public static bool CheckAabbCircle(GameObject box, BallObject ball)
        {
            Vector2 boxCenter = box.Position + box.Size / 2f;
            Vector2 ballCenter = ball.Center;

            Vector2 halfExtents = box.Size / 2f;
            Vector2 difference = ballCenter - boxCenter;

            // clamp difference to box extents
            float clampedX = MathHelper.Clamp(difference.X, -halfExtents.X, halfExtents.X);
            float clampedY = MathHelper.Clamp(difference.Y, -halfExtents.Y, halfExtents.Y);

            Vector2 closestPoint = boxCenter + new Vector2(clampedX, clampedY);

            Vector2 dist = ballCenter - closestPoint;
            return dist.LengthSquared <= ball.Radius * ball.Radius;
        }
    }
}