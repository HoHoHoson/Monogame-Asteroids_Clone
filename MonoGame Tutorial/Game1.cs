using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections; 

namespace MonoGame_Tutorial
{
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        const int STATE_SPLASH = 0;
        const int STATE_GAME = 1;
        const int STATE_GAMEOVER = 2;

        int gameState = STATE_SPLASH;

        float timer = 0.0f;

        float fireRate = 0.3f;

        int currentFPS = 0;
        float fpsTimer = 0;
        int fpsCounter = 0;

        SpriteFont arialFont;
        Texture2D shipTexture;
        Texture2D asteroidTexture;
        Texture2D bulletTexture;
        Texture2D background;

        float playerSpeed = 150;
        float playerStrafe = 120;
        float playerRotateSpeed = 3;
        Vector2 playerPosition = new Vector2(0, 0);
        Vector2 playerOffset = new Vector2(0, 0);
        float playerAngle = 0;
        bool playerActive = false;

        float asteroidSpeed = 80;
        Vector2 asteroidOffset = new Vector2(0, 0);
        ArrayList asteroidPositions = new ArrayList();
        ArrayList asteroidVelocities = new ArrayList();

        float bulletSpeed = 700;
        ArrayList bulletPositions = new ArrayList();
        ArrayList bulletVelocities = new ArrayList();



        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            this.IsFixedTimeStep = true;
            this.graphics.SynchronizeWithVerticalRetrace = true;
        }

       
        protected override void Initialize()
        {
            playerPosition = new Vector2(
                graphics.GraphicsDevice.Viewport.Width / 2,
                graphics.GraphicsDevice.Viewport.Height / 2);
            playerActive = true;

            base.Initialize();
        }

        

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            arialFont = Content.Load<SpriteFont>("Arial");
            shipTexture = Content.Load<Texture2D>("ship");
            asteroidTexture = Content.Load<Texture2D>("rock_large");
            bulletTexture = Content.Load<Texture2D>("bullet");
            background = Content.Load<Texture2D>("grass");

            playerOffset = new Vector2(shipTexture.Width / 2, shipTexture.Height / 2);
            asteroidOffset = new Vector2(asteroidTexture.Width / 2, asteroidTexture.Height / 2);

            Random random = new Random();
            for (int i = 0; i < 10; i++)
            {
                Vector2 randDirection = new Vector2(random.Next(-100, 100), random.Next(-100, 100));

                randDirection.Normalize();

                Vector2 asteroidPosition = randDirection * graphics.GraphicsDevice.Viewport.Height;

                asteroidPositions.Add(asteroidPosition);

                Vector2 velocity = (playerPosition - asteroidPosition);
                velocity.Normalize();
                velocity *= asteroidSpeed;

                asteroidVelocities.Add(velocity);
            }
        }



        protected override void UnloadContent()
        {
        }



        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            fpsTimer += deltaTime;
            if (fpsTimer > 1.0f)
            {
                fpsTimer = 0f;
                currentFPS = fpsCounter;
                fpsCounter = 0;
            }
            fpsCounter++;

            //int counter = 0;
            //for (int i = 0; i < 10000000; i++)
            //{
            //    counter += i;
            //}

            switch (gameState)
            {
                case STATE_SPLASH:
                    UpdateSplashState(deltaTime);
                    break;
                case STATE_GAME:
                    UpdateGameState(deltaTime);
                    break;
                case STATE_GAMEOVER:
                    UpdateGameOverState(deltaTime);
                    break;
            }

                base.Update(gameTime);
        }



        private void UpdateSplashState(float deltaTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Enter) == true)
            {
                gameState = STATE_GAME;
            }
        }



        private void DrawSplashState(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(arialFont, "Forward (W)", new Vector2(300, 150), Color.White);
            spriteBatch.DrawString(arialFont, "Reverse (S)", new Vector2(300, 170), Color.White);
            spriteBatch.DrawString(arialFont, "Rotate Right (D)", new Vector2(300, 190), Color.White);
            spriteBatch.DrawString(arialFont, "Rotate Left (A)", new Vector2(300, 210), Color.White);
            spriteBatch.DrawString(arialFont, "Strafe Right (->)", new Vector2(300, 230), Color.White);
            spriteBatch.DrawString(arialFont, "Strafe Left (<-)", new Vector2(300, 250), Color.White);
            spriteBatch.DrawString(arialFont, "Quit Game (Esc)", new Vector2(300, 270), Color.White);
            spriteBatch.DrawString(arialFont, "Ready?", new Vector2(300, 310), Color.White);
            spriteBatch.DrawString(arialFont, "Press Enter to Play", new Vector2(300, 330), Color.White);
        }



        private void UpdateGameState(float deltaTime)
        {
            UpdatePlayer(deltaTime);
            UpdateAsteroids(deltaTime);
            UpdateBullets(deltaTime);

            if (gameState == STATE_GAME)
            {
                timer += deltaTime;
            }
            
            // Teacher example for generating circular collision boundries
            //double shipRadius = Math.Sqrt(((shipTexture.Width / 2) * (shipTexture.Width / 2)) + ((shipTexture.Height / 2) * (shipTexture.Height / 2)));
            //double asteroidRadius = Math.Sqrt(((asteroidTexture.Width / 2) * (asteroidTexture.Width / 2)) + ((asteroidTexture.Height / 2) * (asteroidTexture.Height / 2)));

            Rectangle shipRect = new Rectangle((int)(playerPosition.X - playerOffset.X), (int)(playerPosition.Y - playerOffset.Y), shipTexture.Bounds.Width, shipTexture.Bounds.Height);


            //Rectangle on rectangle collsion
            for (int asteroidIdx = 0; asteroidIdx < asteroidPositions.Count; asteroidIdx++)
            {
                Vector2 position = (Vector2)asteroidPositions[asteroidIdx];

                Rectangle asteroid = new Rectangle((int)(position.X - asteroidOffset.X),(int)(position.Y - asteroidOffset.Y), asteroidTexture.Width, asteroidTexture.Height);

                    for (int bulletIdx = 0; bulletIdx < bulletPositions.Count; bulletIdx++)
                    {
                        Vector2 bullposition = (Vector2)bulletPositions[bulletIdx];

                        Rectangle bulletRect = new Rectangle((int)bullposition.X, (int)bullposition.Y, bulletTexture.Bounds.Width, bulletTexture.Bounds.Height);

                        if (IsColliding(asteroid, bulletRect) == true)
                        {
                            asteroidPositions.RemoveAt(asteroidIdx);
                            asteroidVelocities.RemoveAt(asteroidIdx);
                            bulletPositions.RemoveAt(bulletIdx);
                            bulletVelocities.RemoveAt(bulletIdx);
                        break;
                        }
                    }
            }


            if (asteroidPositions.Count == 0)
            {
                gameState = STATE_GAMEOVER;
            }
        }



        private void DrawGameState(SpriteBatch spriteBatch)
        {
            if (playerActive == true)
            {
                spriteBatch.Draw(shipTexture, playerPosition, null, null, playerOffset, playerAngle, null, Color.White, SpriteEffects.FlipVertically, 0);
            }

            for (int asteroidIdx = 0; asteroidIdx < asteroidPositions.Count; asteroidIdx++)
            {
                Vector2 position = (Vector2)asteroidPositions[asteroidIdx];
                spriteBatch.Draw(asteroidTexture, position, null, null, asteroidOffset, 0, null, Color.White);
            }

            for (int bulletIdx = 0; bulletIdx < bulletPositions.Count; bulletIdx++)
            {
                Vector2 position = (Vector2)bulletPositions[bulletIdx];
                spriteBatch.Draw(bulletTexture, position, null, null, null, 0, null, Color.White);
            }

            spriteBatch.DrawString(arialFont, timer.ToString(), new Vector2(350, 20), Color.White);
        }


        private void UpdateGameOverState(float deltaTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Enter) == true)
            {
                gameState = STATE_GAME;

                playerPosition = new Vector2(
                graphics.GraphicsDevice.Viewport.Width / 2,
                graphics.GraphicsDevice.Viewport.Height / 2);
                playerActive = true;

                playerAngle = 0;

                timer = 0f;

                Random random = new Random();
                for (int i = 0; i < 10; i++)
                {
                    Vector2 randDirection = new Vector2(random.Next(-100, 100), random.Next(-100, 100));

                    randDirection.Normalize();

                    Vector2 asteroidPosition = randDirection * graphics.GraphicsDevice.Viewport.Height;

                    asteroidPositions.Add(asteroidPosition);

                    Vector2 velocity = (playerPosition - asteroidPosition);
                    velocity.Normalize();
                    velocity *= asteroidSpeed;

                    asteroidVelocities.Add(velocity);
                }
            }
        }



        private void DrawGameOverState(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(arialFont, "Mission Complete.", new Vector2(300, 150), Color.White);
            spriteBatch.DrawString(arialFont, "Retry (Enter)", new Vector2(300, 170), Color.White);
            spriteBatch.DrawString(arialFont, "Quit (Esc)", new Vector2(300, 190), Color.White);
            spriteBatch.DrawString(arialFont, timer.ToString(), new Vector2(350, 20), Color.White);
        }



        private void UpdatePlayer(float deltaTime)
        {
            float currentSpeed = 0;

            fireRate += deltaTime;


            if (Keyboard.GetState().IsKeyDown(Keys.W) == true)
            {
                currentSpeed = -playerSpeed * deltaTime;
            }
            else if (Keyboard.GetState().IsKeyDown(Keys.S) == true)
            {
                currentSpeed = (playerSpeed - 50) * deltaTime;
            }
            
            if (Keyboard.GetState().IsKeyDown(Keys.A) == true)
            {
                playerAngle -= playerRotateSpeed * deltaTime;
            }
            else if (Keyboard.GetState().IsKeyDown(Keys.D) == true)
            {
                playerAngle += playerRotateSpeed * deltaTime;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Right) == true)
            {
                float s = (float)Math.Sin(playerAngle);
                float c = (float)Math.Cos(playerAngle);
                float xSpeed = playerStrafe * deltaTime;
                float ySpeed = 0;
                playerPosition.X += (xSpeed * c) - (ySpeed * s);
                playerPosition.Y += (xSpeed * s) + (ySpeed * c);
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Left) == true)
            {
                float s = (float)Math.Sin(playerAngle);
                float c = (float)Math.Cos(playerAngle);
                float xSpeed = -playerStrafe * deltaTime;
                float ySpeed = 0;
                playerPosition.X += (xSpeed * c) - (ySpeed * s);
                playerPosition.Y += (xSpeed * s) + (ySpeed * c);
            }

            Vector2 playerDirection = new Vector2(-(float)Math.Sin(playerAngle), (float)Math.Cos(playerAngle));
            playerDirection.Normalize();

            if (Keyboard.GetState().IsKeyDown(Keys.Space) == true && fireRate > 0.3f)
            {
                for(int bulletIdx = 0; bulletIdx < 1; bulletIdx++)
                {
                    Vector2 bulletPos = playerPosition;
                    bulletPositions.Add(bulletPos);

                    Vector2 bulletVel = -playerDirection * bulletSpeed;
                    bulletVel.Normalize();
                    bulletVel *= bulletSpeed;
                    bulletVelocities.Add(bulletVel);
                }
                fireRate = 0f;
            }

            Vector2 direction = new Vector2(40, 30);
            direction.Normalize();

            Vector2 playerVelocity = playerDirection * currentSpeed;
            playerPosition += playerVelocity;
        }



        private void UpdateAsteroids(float deltaTime)
        {
            for (int asteroidIdx = 0; asteroidIdx < asteroidPositions.Count; asteroidIdx++)
            {
                Vector2 position = (Vector2)asteroidPositions[asteroidIdx];
                Vector2 velocity = (Vector2)asteroidVelocities[asteroidIdx];

                position += velocity * deltaTime;
                asteroidPositions[asteroidIdx] = position;
                if (position.X < 0 && velocity.X < 0 ||
                    position.X > graphics.GraphicsDevice.Viewport.Width && velocity.X > 0)
                {
                    velocity.X = -velocity.X;
                    asteroidVelocities[asteroidIdx] = velocity;
                }
                if (position.Y < 0 && velocity.Y < 0 ||
                    position.Y > graphics.GraphicsDevice.Viewport.Height && velocity.Y > 0)
                {
                    velocity.Y = -velocity.Y;
                    asteroidVelocities[asteroidIdx] = velocity;
                }
            }

        }



        private void UpdateBullets(float deltaTime)
        {
            for (int bulletIdx = 0; bulletIdx < bulletPositions.Count; bulletIdx++)
            {
                Vector2 position = (Vector2)bulletPositions[bulletIdx];
                Vector2 velocity = (Vector2)bulletVelocities[bulletIdx];

                position += velocity * deltaTime;
                bulletPositions[bulletIdx] = position;

                if (position.X < 0 ||
                position.X > graphics.GraphicsDevice.Viewport.Width ||
                position.Y < 0 ||
                position.Y > graphics.GraphicsDevice.Viewport.Height)
                {
                    bulletPositions.RemoveAt(bulletIdx);
                    bulletVelocities.RemoveAt(bulletIdx);
                }
            }
        }



        //Circle on Circle collision function
        private bool IsColliding(Vector2 position1, float radius1, Vector2 position2, float radius2)
        {
            Vector2 distance = position2 - position1;

            if (distance.Length() < radius1 + radius2)
            {
                return true;
            }
            return false;
        }


        //Rectangle on rectangle collision function
        private bool IsColliding(Rectangle rect1, Rectangle rect2)
        {
            if (rect1.X + rect1.Width < rect2.X ||
                rect1.X > rect2.X + rect2.Width ||
                rect1.Y + rect1.Height < rect2.Y ||
                rect1.Y > rect2.Y + rect2.Height)
            {
                return false;
            }
            return true;
        }



        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.MidnightBlue);

            spriteBatch.Begin();

            spriteBatch.DrawString(arialFont, currentFPS.ToString(), new Vector2(20, 20), Color.Red);

            spriteBatch.DrawString(arialFont, "Cert II - Asteroids Alpha",
            new Vector2(630, 460), Color.White);

            switch (gameState)
            {
                case STATE_SPLASH:
                    DrawSplashState(spriteBatch);
                    break;
                case STATE_GAME:
                    DrawGameState(spriteBatch);
                    break;
                case STATE_GAMEOVER:
                    DrawGameOverState(spriteBatch);
                    break;
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
