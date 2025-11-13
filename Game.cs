using System.Collections.Generic;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Graphics.OpenGL4;

namespace Breakout
{
    internal class Game : GameWindow
    {
        private GameObject _paddle;
        private BallObject _ball;
        private List<GameObject> _bricks;

        private float _paddleSpeed = 500f;

        private float _windowWidth => Size.X;
        private float _windowHeight => Size.Y;

        // --- Rendering stuff ---
        private int _vao;
        private int _vbo;
        private int _ebo;
        private Shader2D _shader;

        public Game()
            : base(GameWindowSettings.Default,
                   new NativeWindowSettings
                   {
                       Title = "Mini Breakout",
                       Size = new Vector2i(800, 600)
                   })
        {
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            GL.ClearColor(0.1f, 0.1f, 0.15f, 1f);

            // Paddle setup
            Vector2 paddleSize = new Vector2(100, 20);
            Vector2 paddlePos = new Vector2((_windowWidth - paddleSize.X) / 2f, _windowHeight - 40);
            _paddle = new GameObject(paddlePos, paddleSize, Vector2.Zero);

            // Ball setup
            float radius = 10f;
            Vector2 ballPos = paddlePos + new Vector2(paddleSize.X / 2f - radius, -radius * 2);
            _ball = new BallObject(ballPos, radius, new Vector2(250f, -250f));

            // Level bricks
            _bricks = new List<GameObject>();
            GenerateLevel();

            InitRenderer();
        }

        private void InitRenderer()
        {
            // Quad in [0,1]x[0,1] (2D)
            float[] vertices =
            {
                // x, y
                0f, 0f,
                1f, 0f,
                1f, 1f,
                0f, 1f
            };

            uint[] indices = { 0, 1, 2, 2, 3, 0 };

            _vao = GL.GenVertexArray();
            _vbo = GL.GenBuffer();
            _ebo = GL.GenBuffer();

            GL.BindVertexArray(_vao);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);

            GL.BindVertexArray(0);

            // Simple shader: converts screen coords to NDC
            string vertexSrc = @"
                #version 330 core
                layout (location = 0) in vec2 aPos;

                uniform vec2 uScreenSize;  // window size
                uniform vec2 uPos;         // top-left in pixels
                uniform vec2 uSize;        // size in pixels

                void main()
                {
                    // Convert unit quad to pixel space
                    vec2 world = aPos * uSize + uPos;

                    // Convert to NDC
                    float x = (world.x / uScreenSize.x) * 2.0 - 1.0;
                    float y = 1.0 - (world.y / uScreenSize.y) * 2.0;

                    gl_Position = vec4(x, y, 0.0, 1.0);
                }";

            string fragmentSrc = @"
                #version 330 core
                out vec4 FragColor;

                uniform vec3 uColor;

                void main()
                {
                    FragColor = vec4(uColor, 1.0);
                }";

            _shader = new Shader2D(vertexSrc, fragmentSrc);
        }

        private void GenerateLevel()
        {
            int rows = 5;
            int cols = 10;
            float brickWidth = _windowWidth / cols;
            float brickHeight = 25f;

            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < cols; x++)
                {
                    Vector2 pos = new Vector2(x * brickWidth, 50 + y * brickHeight);
                    Vector2 size = new Vector2(brickWidth - 4, brickHeight - 4); // small gap
                    _bricks.Add(new GameObject(pos, size, Vector2.Zero));
                }
            }
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, Size.X, Size.Y);
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);
            if (!IsFocused) return;

            var input = KeyboardState;

            if (input.IsKeyDown(Keys.Escape))
                Close();

            float dt = (float)args.Time;

            // Paddle movement
            if (input.IsKeyDown(Keys.Left))
            {
                _paddle.Position.X -= _paddleSpeed * dt;
                if (_paddle.Position.X < 0)
                    _paddle.Position.X = 0;
            }
            if (input.IsKeyDown(Keys.Right))
            {
                _paddle.Position.X += _paddleSpeed * dt;
                if (_paddle.Position.X + _paddle.Size.X > _windowWidth)
                    _paddle.Position.X = _windowWidth - _paddle.Size.X;
            }

            // Launch the ball with space
            if (_ball.Stuck && input.IsKeyDown(Keys.Space))
            {
                _ball.Stuck = false;
            }

            if (_ball.Stuck)
            {
                _ball.Position.X = _paddle.Position.X + _paddle.Size.X / 2f - _ball.Radius;
                _ball.Position.Y = _paddle.Position.Y - _ball.Radius * 2;
            }

            _ball.Move(dt, _windowWidth, _windowHeight);

            DoCollisions();
        }

        private void DoCollisions()
        {
            // Paddle vs Ball
            if (Collision.CheckAabbCircle(_paddle, _ball))
            {
                _ball.Velocity.Y = -System.MathF.Abs(_ball.Velocity.Y);

                float paddleCenter = _paddle.Center.X;
                float distance = _ball.Center.X - paddleCenter;
                float percentage = distance / (_paddle.Size.X / 2f);
                _ball.Velocity.X = percentage * 300f;
            }

            // Ball vs Bricks
            foreach (var brick in _bricks)
            {
                if (brick.Destroyed)
                    continue;

                if (Collision.CheckAabbCircle(brick, _ball))
                {
                    brick.Destroyed = true;
                    _ball.Velocity.Y *= -1f;
                    break;
                }
            }
        }

        private void DrawRect(Vector2 pos, Vector2 size, Vector3 color)
        {
            _shader.Use();
            _shader.SetVector2("uScreenSize", new Vector2(_windowWidth, _windowHeight));
            _shader.SetVector2("uPos", pos);
            _shader.SetVector2("uSize", size);
            _shader.SetVector3("uColor", color);

            GL.BindVertexArray(_vao);
            GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);
            GL.BindVertexArray(0);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            GL.Clear(ClearBufferMask.ColorBufferBit);

            // paddle: blue
            DrawRect(_paddle.Position, _paddle.Size, new Vector3(0.2f, 0.6f, 1.0f));

            // ball: draw as a square for now (you can switch to circle texture later)
            Vector2 ballSize = new Vector2(_ball.Radius * 2f, _ball.Radius * 2f);
            DrawRect(_ball.Position, ballSize, new Vector3(1.0f, 0.8f, 0.2f));

            // bricks: green (skip destroyed)
            foreach (var brick in _bricks)
            {
                if (!brick.Destroyed)
                    DrawRect(brick.Position, brick.Size, new Vector3(0.3f, 0.9f, 0.4f));
            }

            SwapBuffers();
        }

        protected override void OnUnload()
        {
            base.OnUnload();
            GL.DeleteVertexArray(_vao);
            GL.DeleteBuffer(_vbo);
            GL.DeleteBuffer(_ebo);
            _shader?.Dispose();
        }
    }
}
