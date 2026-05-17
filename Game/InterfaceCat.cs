using System;
using System.Numerics;
using Raylib_cs;

public static class InterfaceCat
{
    private enum GameScreen
    {
        Menu,
        Playing,
        Credits,
        Win,
        GameOver
    }

    private struct Checkpoint
    {
        public Rectangle Rect;
        public bool Active;

        public Checkpoint(Rectangle rect)
        {
            Rect = rect;
            Active = false;
        }
    }

    public static void Run()
    {
        const int screenWidth = 1280;
        const int screenHeight = 720;

        Raylib.InitWindow(screenWidth, screenHeight, "Gatinho Voltando para Casa");
        Raylib.SetTargetFPS(60);

        GameScreen currentScreen = GameScreen.Menu;

        const float worldWidth = 2600f;
        const float groundY = 620f;

        Rectangle player = new Rectangle(120, 300, 42, 42);
        Vector2 velocity = Vector2.Zero;

        float walkSpeed = 280f;
        float runMultiplier = 1.6f;
        float gravity = 1800f;
        float jumpForce = 720f;

        int maxJumps = 2;
        int jumpsRemaining = 2;

        bool onGround = false;
        bool facingRight = true;

        bool isDashing = false;
        float dashSpeed = 850f;
        float dashTime = 0.12f;
        float dashTimer = 0f;
        float dashCooldown = 0.45f;
        float dashCooldownTimer = 0f;

        int maxLives = 3;
        int lives = 3;
        float damageCooldown = 1.0f;
        float damageCooldownTimer = 0f;

        Camera2D camera = new Camera2D
        {
            Offset = new Vector2(screenWidth / 2f, screenHeight / 2f),
            Target = new Vector2(player.X + player.Width / 2f, player.Y + player.Height / 2f),
            Rotation = 0f,
            Zoom = 1.0f
        };

        Vector2 initialSpawn = new Vector2(120, 300);
        Vector2 currentSpawn = initialSpawn;

        Rectangle[] platforms =
        {
            new Rectangle(0, groundY, worldWidth, 100),
            new Rectangle(250, 540, 180, 20),
            new Rectangle(520, 470, 170, 20),
            new Rectangle(760, 400, 180, 20),
            new Rectangle(1040, 520, 210, 20),
            new Rectangle(1350, 450, 180, 20),
            new Rectangle(1640, 380, 180, 20),
            new Rectangle(1920, 500, 200, 20),
            new Rectangle(2230, 430, 180, 20)
        };

        Rectangle[] fish =
        {
            new Rectangle(310, 500, 22, 22),
            new Rectangle(570, 430, 22, 22),
            new Rectangle(810, 360, 22, 22),
            new Rectangle(1090, 480, 22, 22),
            new Rectangle(1390, 410, 22, 22),
            new Rectangle(1670, 340, 22, 22),
            new Rectangle(1970, 460, 22, 22),
            new Rectangle(2280, 390, 22, 22)
        };
        bool[] fishCollected = new bool[fish.Length];
        int score = 0;

        Checkpoint[] checkpoints =
        {
            new Checkpoint(new Rectangle(700, 340, 28, 60)),
            new Checkpoint(new Rectangle(1550, 390, 28, 60))
        };

        Rectangle[] hazards =
        {
            new Rectangle(430, 605, 80, 15),
            new Rectangle(1250, 605, 90, 15)
        };

        Rectangle[] movingHazards =
        {
            new Rectangle(900, 605, 90, 15),
            new Rectangle(2080, 605, 90, 15)
        };

        float[] movingHazardMinX = { 860f, 2020f };
        float[] movingHazardMaxX = { 1120f, 2280f };
        float[] movingHazardSpeed = { 220f, 260f };
        int[] movingHazardDirection = { 1, -1 };

        Rectangle home = new Rectangle(2450, 350, 90, 90);

        while (!Raylib.WindowShouldClose())
        {
            float dt = Raylib.GetFrameTime();

            switch (currentScreen)
            {
                case GameScreen.Menu:
                    UpdateMenu(
                        ref currentScreen,
                        ref player,
                        ref velocity,
                        ref currentSpawn,
                        initialSpawn,
                        ref lives,
                        maxLives,
                        ref score,
                        fishCollected,
                        ref jumpsRemaining,
                        checkpoints,
                        movingHazards,
                        movingHazardDirection
                    );
                    break;

                case GameScreen.Credits:
                    if (Raylib.IsKeyPressed(KeyboardKey.Escape))
                        currentScreen = GameScreen.Menu;
                    break;

                case GameScreen.Playing:
                    UpdateGame(
                        dt,
                        ref currentScreen,
                        ref player,
                        ref velocity,
                        ref onGround,
                        ref facingRight,
                        ref jumpsRemaining,
                        maxJumps,
                        gravity,
                        jumpForce,
                        walkSpeed,
                        runMultiplier,
                        ref isDashing,
                        ref dashTimer,
                        dashTime,
                        dashSpeed,
                        ref dashCooldownTimer,
                        dashCooldown,
                        platforms,
                        ref camera,
                        screenWidth,
                        worldWidth,
                        fish,
                        fishCollected,
                        ref score,
                        ref lives,
                        ref damageCooldownTimer,
                        damageCooldown,
                        hazards,
                        movingHazards,
                        movingHazardMinX,
                        movingHazardMaxX,
                        movingHazardSpeed,
                        movingHazardDirection,
                        checkpoints,
                        ref currentSpawn,
                        home
                    );
                    break;

                case GameScreen.Win:
                case GameScreen.GameOver:
                    if (Raylib.IsKeyPressed(KeyboardKey.R))
                    {
                        ResetGame(
                            ref currentScreen,
                            ref player,
                            ref velocity,
                            initialSpawn,
                            ref currentSpawn,
                            ref lives,
                            maxLives,
                            ref score,
                            fishCollected,
                            ref jumpsRemaining,
                            maxJumps,
                            checkpoints,
                            movingHazards,
                            movingHazardDirection
                        );
                    }

                    if (Raylib.IsKeyPressed(KeyboardKey.C))
                        currentScreen = GameScreen.Credits;
                    break;
            }

            DrawGame(
                currentScreen,
                camera,
                platforms,
                hazards,
                movingHazards,
                fish,
                fishCollected,
                checkpoints,
                player,
                home,
                score,
                lives,
                maxLives
            );
        }

        Raylib.CloseWindow();
    }

    private static void UpdateMenu(
        ref GameScreen currentScreen,
        ref Rectangle player,
        ref Vector2 velocity,
        ref Vector2 currentSpawn,
        Vector2 initialSpawn,
        ref int lives,
        int maxLives,
        ref int score,
        bool[] fishCollected,
        ref int jumpsRemaining,
        Checkpoint[] checkpoints,
        Rectangle[] movingHazards,
        int[] movingHazardDirection
    )
    {
        if (Raylib.IsKeyPressed(KeyboardKey.Enter))
        {
            player.X = initialSpawn.X;
            player.Y = initialSpawn.Y;
            velocity = Vector2.Zero;
            currentSpawn = initialSpawn;
            lives = maxLives;
            score = 0;
            jumpsRemaining = 2;

            for (int i = 0; i < fishCollected.Length; i++)
                fishCollected[i] = false;

            for (int i = 0; i < checkpoints.Length; i++)
                checkpoints[i].Active = false;

            movingHazards[0].X = 900;
            movingHazards[1].X = 2080;
            movingHazardDirection[0] = 1;
            movingHazardDirection[1] = -1;

            currentScreen = GameScreen.Playing;
        }

        if (Raylib.IsKeyPressed(KeyboardKey.C))
            currentScreen = GameScreen.Credits;
    }

    private static void UpdateGame(
        float dt,
        ref GameScreen currentScreen,
        ref Rectangle player,
        ref Vector2 velocity,
        ref bool onGround,
        ref bool facingRight,
        ref int jumpsRemaining,
        int maxJumps,
        float gravity,
        float jumpForce,
        float walkSpeed,
        float runMultiplier,
        ref bool isDashing,
        ref float dashTimer,
        float dashTime,
        float dashSpeed,
        ref float dashCooldownTimer,
        float dashCooldown,
        Rectangle[] platforms,
        ref Camera2D camera,
        int screenWidth,
        float worldWidth,
        Rectangle[] fish,
        bool[] fishCollected,
        ref int score,
        ref int lives,
        ref float damageCooldownTimer,
        float damageCooldown,
        Rectangle[] hazards,
        Rectangle[] movingHazards,
        float[] movingHazardMinX,
        float[] movingHazardMaxX,
        float[] movingHazardSpeed,
        int[] movingHazardDirection,
        Checkpoint[] checkpoints,
        ref Vector2 currentSpawn,
        Rectangle home
    )
    {
        if (Raylib.IsKeyPressed(KeyboardKey.C))
        {
            currentScreen = GameScreen.Credits;
            return;
        }

        if (damageCooldownTimer > 0f)
            damageCooldownTimer -= dt;

        if (dashCooldownTimer > 0f)
            dashCooldownTimer -= dt;

        UpdateMovingHazards(dt, movingHazards, movingHazardMinX, movingHazardMaxX, movingHazardSpeed, movingHazardDirection);

        float moveInput = 0f;
        if (Raylib.IsKeyDown(KeyboardKey.A) || Raylib.IsKeyDown(KeyboardKey.Left)) moveInput -= 1f;
        if (Raylib.IsKeyDown(KeyboardKey.D) || Raylib.IsKeyDown(KeyboardKey.Right)) moveInput += 1f;

        if (moveInput > 0) facingRight = true;
        if (moveInput < 0) facingRight = false;

        float currentSpeed = walkSpeed;
        if (Raylib.IsKeyDown(KeyboardKey.LeftControl) || Raylib.IsKeyDown(KeyboardKey.RightControl))
            currentSpeed *= runMultiplier;

        if (!isDashing)
            velocity.X = moveInput * currentSpeed;

        if ((Raylib.IsKeyPressed(KeyboardKey.Space) || Raylib.IsKeyPressed(KeyboardKey.Up)) && jumpsRemaining > 0)
        {
            velocity.Y = -jumpForce;
            jumpsRemaining--;
            onGround = false;
        }

        if ((Raylib.IsKeyPressed(KeyboardKey.LeftShift) || Raylib.IsKeyPressed(KeyboardKey.X)) &&
            dashCooldownTimer <= 0f && !isDashing)
        {
            isDashing = true;
            dashTimer = dashTime;
            dashCooldownTimer = dashCooldown;
            velocity.Y = 0f;
            velocity.X = (facingRight ? 1f : -1f) * dashSpeed;
        }

        if (isDashing)
        {
            dashTimer -= dt;
            if (dashTimer <= 0f)
                isDashing = false;
        }
        else
        {
            velocity.Y += gravity * dt;
        }

        MoveAndCollide(ref player, ref velocity, ref onGround, ref jumpsRemaining, maxJumps, platforms, dt);

        for (int i = 0; i < checkpoints.Length; i++)
        {
            if (Raylib.CheckCollisionRecs(player, checkpoints[i].Rect))
            {
                checkpoints[i].Active = true;
                currentSpawn = new Vector2(checkpoints[i].Rect.X, checkpoints[i].Rect.Y - 50);
            }
        }

        for (int i = 0; i < fish.Length; i++)
        {
            if (!fishCollected[i] && Raylib.CheckCollisionRecs(player, fish[i]))
            {
                fishCollected[i] = true;
                score++;
            }
        }

        bool tookDamage = false;

        for (int i = 0; i < hazards.Length; i++)
        {
            if (Raylib.CheckCollisionRecs(player, hazards[i]))
            {
                tookDamage = true;
                break;
            }
        }

        if (!tookDamage)
        {
            for (int i = 0; i < movingHazards.Length; i++)
            {
                if (Raylib.CheckCollisionRecs(player, movingHazards[i]))
                {
                    tookDamage = true;
                    break;
                }
            }
        }

        if (tookDamage && damageCooldownTimer <= 0f)
        {
            lives--;
            damageCooldownTimer = damageCooldown;

            if (lives <= 0)
            {
                currentScreen = GameScreen.GameOver;
            }
            else
            {
                player.X = currentSpawn.X;
                player.Y = currentSpawn.Y;
                velocity = Vector2.Zero;
            }
        }

        if (Raylib.CheckCollisionRecs(player, home))
            currentScreen = GameScreen.Win;

        camera.Target = new Vector2(player.X + player.Width / 2f, player.Y + player.Height / 2f);

        float halfViewWidth = screenWidth / 2f;
        if (camera.Target.X < halfViewWidth) camera.Target.X = halfViewWidth;
        if (camera.Target.X > worldWidth - halfViewWidth) camera.Target.X = worldWidth - halfViewWidth;
        if (camera.Target.Y < 250f) camera.Target.Y = 250f;
    }

    private static void MoveAndCollide(
        ref Rectangle player,
        ref Vector2 velocity,
        ref bool onGround,
        ref int jumpsRemaining,
        int maxJumps,
        Rectangle[] platforms,
        float dt
    )
    {
        onGround = false;

        player.X += velocity.X * dt;
        for (int i = 0; i < platforms.Length; i++)
        {
            if (Raylib.CheckCollisionRecs(player, platforms[i]))
            {
                if (velocity.X > 0)
                    player.X = platforms[i].X - player.Width;
                else if (velocity.X < 0)
                    player.X = platforms[i].X + platforms[i].Width;

                velocity.X = 0;
            }
        }

        player.Y += velocity.Y * dt;
        for (int i = 0; i < platforms.Length; i++)
        {
            if (Raylib.CheckCollisionRecs(player, platforms[i]))
            {
                if (velocity.Y > 0)
                {
                    player.Y = platforms[i].Y - player.Height;
                    velocity.Y = 0;
                    onGround = true;
                    jumpsRemaining = maxJumps;
                }
                else if (velocity.Y < 0)
                {
                    player.Y = platforms[i].Y + platforms[i].Height;
                    velocity.Y = 0;
                }
            }
        }
    }

    private static void UpdateMovingHazards(
        float dt,
        Rectangle[] movingHazards,
        float[] minX,
        float[] maxX,
        float[] speed,
        int[] direction
    )
    {
        for (int i = 0; i < movingHazards.Length; i++)
        {
            movingHazards[i].X += speed[i] * direction[i] * dt;

            if (movingHazards[i].X <= minX[i])
            {
                movingHazards[i].X = minX[i];
                direction[i] = 1;
            }
            else if (movingHazards[i].X >= maxX[i])
            {
                movingHazards[i].X = maxX[i];
                direction[i] = -1;
            }
        }
    }

    private static void ResetGame(
        ref GameScreen currentScreen,
        ref Rectangle player,
        ref Vector2 velocity,
        Vector2 initialSpawn,
        ref Vector2 currentSpawn,
        ref int lives,
        int maxLives,
        ref int score,
        bool[] fishCollected,
        ref int jumpsRemaining,
        int maxJumps,
        Checkpoint[] checkpoints,
        Rectangle[] movingHazards,
        int[] movingHazardDirection
    )
    {
        currentScreen = GameScreen.Menu;
        player.X = initialSpawn.X;
        player.Y = initialSpawn.Y;
        velocity = Vector2.Zero;
        currentSpawn = initialSpawn;
        lives = maxLives;
        score = 0;
        jumpsRemaining = maxJumps;

        for (int i = 0; i < fishCollected.Length; i++)
            fishCollected[i] = false;

        for (int i = 0; i < checkpoints.Length; i++)
            checkpoints[i].Active = false;

        movingHazards[0].X = 900;
        movingHazards[1].X = 2080;
        movingHazardDirection[0] = 1;
        movingHazardDirection[1] = -1;
    }

    private static void DrawGame(
        GameScreen currentScreen,
        Camera2D camera,
        Rectangle[] platforms,
        Rectangle[] hazards,
        Rectangle[] movingHazards,
        Rectangle[] fish,
        bool[] fishCollected,
        Checkpoint[] checkpoints,
        Rectangle player,
        Rectangle home,
        int score,
        int lives,
        int maxLives
    )
    {
        Raylib.BeginDrawing();
        Raylib.ClearBackground(new Color(200, 235, 255, 255));

        if (currentScreen == GameScreen.Menu)
        {
            Raylib.DrawText("GATINHO VOLTANDO PARA CASA", 380, 150, 46, Color.DarkBlue);
            Raylib.DrawText("Plataforma 2D do gatinho", 450, 215, 28, Color.DarkGray);

            Raylib.DrawText("ENTER - Jogar", 520, 310, 30, Color.Black);
            Raylib.DrawText("C - Creditos", 530, 355, 30, Color.Black);

            Raylib.DrawText("Controles", 110, 500, 28, Color.Maroon);
            Raylib.DrawText("A/D ou setas - mover", 110, 540, 24, Color.Black);
            Raylib.DrawText("CTRL - correr", 110, 575, 24, Color.Black);
            Raylib.DrawText("SPACE / UP - pulo e pulo duplo", 110, 610, 24, Color.Black);
            Raylib.DrawText("SHIFT ou X - dash horizontal", 110, 645, 24, Color.Black);

            Raylib.EndDrawing();
            return;
        }

        if (currentScreen == GameScreen.Credits)
        {
            Raylib.DrawText("CRÉDITOS", 520, 140, 42, Color.DarkBlue);
            Raylib.DrawText("Jogo: Gatinho Voltando para Casa", 440, 250, 30, Color.Black);
            Raylib.DrawText("Tema: Jogo 2D do Gatinho", 360, 310, 30, Color.Black);
            Raylib.DrawText("Implementacao: C# + Raylib-cs", 390, 370, 30, Color.Black);
            Raylib.DrawText("ESC - voltar", 525, 500, 28, Color.Maroon);
            Raylib.EndDrawing();
            return;
        }

        Raylib.BeginMode2D(camera);

        Raylib.DrawRectangle(-200, -200, 3200, 1000, new Color(210, 240, 255, 255));

        for (int i = 0; i < platforms.Length; i++)
            Raylib.DrawRectangleRec(platforms[i], new Color(110, 110, 110, 255));

        for (int i = 0; i < checkpoints.Length; i++)
        {
            Color c = checkpoints[i].Active ? Color.Green : Color.Orange;
            Raylib.DrawRectangleRec(checkpoints[i].Rect, c);
            Raylib.DrawText("CP", (int)checkpoints[i].Rect.X - 5, (int)checkpoints[i].Rect.Y - 25, 20, c);
        }

        for (int i = 0; i < hazards.Length; i++)
            Raylib.DrawRectangleRec(hazards[i], Color.Red);

        for (int i = 0; i < movingHazards.Length; i++)
            Raylib.DrawRectangleRec(movingHazards[i], Color.Red);

        for (int i = 0; i < fish.Length; i++)
        {
            if (!fishCollected[i])
            {
                Raylib.DrawCircle((int)(fish[i].X + 11), (int)(fish[i].Y + 11), 11, Color.Gold);
                Raylib.DrawText("F", (int)fish[i].X + 5, (int)fish[i].Y + 2, 16, Color.Brown);
            }
        }

        Raylib.DrawRectangleRec(home, new Color(175, 110, 60, 255));
        Raylib.DrawText("HOME", (int)home.X + 10, (int)home.Y + 34, 24, Color.White);

        DrawCat(player);

        Raylib.EndMode2D();

        Raylib.DrawRectangle(15, 15, 310, 108, new Color(255, 255, 255, 210));
        Raylib.DrawText($"Peixes: {score}", 30, 30, 28, Color.DarkBlue);
        Raylib.DrawText($"Vidas: {lives}/{maxLives}", 30, 62, 28, Color.Maroon);
        Raylib.DrawText("C - creditos", 30, 94, 18, Color.DarkGray);

        if (currentScreen == GameScreen.Win)
        {
            Raylib.DrawRectangle(270, 250, 740, 150, new Color(255, 255, 255, 225));
            Raylib.DrawText("O gatinho chegou em casa!", 380, 285, 36, Color.DarkGreen);
            Raylib.DrawText("R - voltar ao menu | C - creditos", 405, 335, 24, Color.DarkGray);
        }

        if (currentScreen == GameScreen.GameOver)
        {
            Raylib.DrawRectangle(320, 250, 650, 150, new Color(255, 255, 255, 225));
            Raylib.DrawText("Game Over", 535, 285, 42, Color.Red);
            Raylib.DrawText("R - voltar ao menu | C - creditos", 398, 340, 24, Color.DarkGray);
        }

        Raylib.EndDrawing();
    }

    private static void DrawCat(Rectangle player)
    {
        Raylib.DrawRectangleRec(player, Color.Black);

        Raylib.DrawTriangle(
            new Vector2(player.X + 4, player.Y),
            new Vector2(player.X + 12, player.Y - 12),
            new Vector2(player.X + 18, player.Y),
            Color.Black
        );

        Raylib.DrawTriangle(
            new Vector2(player.X + 24, player.Y),
            new Vector2(player.X + 31, player.Y - 12),
            new Vector2(player.X + 38, player.Y),
            Color.Black
        );

        Raylib.DrawCircle((int)player.X + 12, (int)player.Y + 15, 3, Color.White);
        Raylib.DrawCircle((int)player.X + 29, (int)player.Y + 15, 3, Color.White);
    }
}