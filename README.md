# Research Component – Collision Detection in a Breakout-Style Game

## 1. What is Collision Detection? Why Use Simple Shapes?

Collision detection is the process of finding out if two objects in a game world are touching or overlapping. It allows the game to respond. Eg: bouncing the ball, hitting a wall, or destroying bricks.  
Even though every object has a detailed mesh or sprite shape, checking collisions against those exact shapes would be slow and complex. Instead, we wrap objects inside simple bounding shapes, such as:

- **AABBs (Axis-Aligned Bounding Boxes)** – rectangles aligned with the x/y axes
- **Circles** – defined by center + radius

Simple shapes make collision detection fast, predictable, and efficient enough to run every frame, even when many objects are on screen.

## 2. AABB–AABB vs AABB–Circle Collision Detection

### AABB–AABB Collision

Two rectangles collide if they overlap on both the x-axis and y-axis. If there is a gap on either axis, then they cannot be colliding.

**Pseudocode:**
```
function AABBvsAABB(A, B):
    collisionX = (A.x + A.width  >= B.x) AND
                 (B.x + B.width  >= A.x)

    collisionY = (A.y + A.height >= B.y) AND
                 (B.y + B.height >= A.y)

    return collisionX AND collisionY
```

### AABB–Circle Collision

To detect if a circle touches a rectangle:

1. Compute the rectangle's center.
2. Compute the vector from rectangle center → circle center.
3. Clamp this vector to the rectangle's half-extents to find the closest point on the box.
4. If the distance from that closest point to the circle's center ≤ radius → collision.

**Pseudocode:**
```
function AABBvsCircle(box, circle):
    boxCenter = box.position + box.size / 2
    diff = circle.center - boxCenter
    halfExtents = box.size / 2

    clampedX = clamp(diff.x, -halfExtents.x, halfExtents.x)
    clampedY = clamp(diff.y, -halfExtents.y, halfExtents.y)

    closest = boxCenter + (clampedX, clampedY)

    distSq = lengthSquared(circle.center - closest)

    return distSq <= circle.radius * circle.radius
```

## 3. Required Diagrams

### Diagram 1 – AABB–AABB Overlapping Edges

<img width="787" height="555" alt="image" src="https://github.com/user-attachments/assets/b7ab5535-6cdd-4a00-ae0d-08ca95795ade" />    

> AABB–AABB collision occurs only when both x-range and y-range overlap.

### Diagram 2 – Closest Point Calculation (AABB–Circle)

<img width="805" height="702" alt="image" src="https://github.com/user-attachments/assets/0f9506dd-70b1-4f8b-bef8-3bd1c3fe09d7" />


> The clamped point is the closest point on the rectangle to the circle. If the distance ≤ radius, a collision occurs.

## 4. What Does `clamp` Do? Why Is It Important?

`clamp(value, min, max)` forces a value to stay within a specified range.
```
clamp( value, min, max ):
    if value < min: return min
    if value > max: return max
    return value
```

In AABB–Circle collision detection, clamping ensures that the "closest point on the box" is actually on or inside the rectangle:

- If the circle is above the box → closest point = top edge
- If it's near the corner → closest point = that corner
- If circle is inside → closest point = circle center

Without clamping, the distance calculation would use a point outside the box, leading to incorrect results.

## 5. Experiments with Ball Sizes & Brick Dimensions

### Ball Size Changes

| Ball Configuration | Observations |
|-------------------|--------------|
| **Large ball** | • Collisions occur earlier (circle overlaps sooner)<br>• Sometimes appears to "hit" bricks before visually touching |
| **Small ball** | • Misses bricks more often<br>• Can slip between thin bricks at high speeds |

### Brick Dimension Changes

| Brick Configuration | Observations |
|--------------------|--------------|
| **Thin bricks (small height)** | • Higher chance of tunneling (ball passes through) |
| **Wide bricks** | • Collisions look more stable and predictable |

## 6. False Collision Observations & Improvements

### Observed Issues

**Corner phantom hits:**
- Ball barely touches the corner visually, but AABB–Circle reports a hit.

**Ball passes through thin bricks:**
- Due to high speed: tunneling between frames.

**Multiple brick hits in one frame:**
- Happens when ball overlaps two bricks at the same time.

### How to Improve Accuracy

- Use smaller bounding shapes or multiple rectangles per brick
- Use continuous collision detection (swept-volume tests)
- Use polygon-level collision for more precise physics
- Use frame interpolation or sub-stepping to avoid tunneling

## 7. Evidence of Successful Collisions
![20251113-0402-14 5551255](https://github.com/user-attachments/assets/2689dffb-4dc3-47eb-b9cf-8401fafb7a11)

> The ball successfully detects collisions with bricks using AABB–Circle logic and removes the brick when hit.
