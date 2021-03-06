﻿using System; // add to allow Windows message box
using System.Runtime.InteropServices; // add to allow Windows message box

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using System.Collections.Generic;

namespace Demo_MG_MazeGame
{
    public enum GameAction
    {
        None,
        PlayerRight,
        PlayerLeft,
        PlayerUp,
        PlayerDown,
        Quit
    }

    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class MazeGame : Game
    {
        // add code to allow Windows message boxes when running in a Windows environment
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern uint MessageBox(IntPtr hWnd, String text, String caption, uint type);

        // set the cell size in pixels
        public static int CELL_WIDTH = 64;
        public static int CELL_HEIGHT = 64;
        public static int CELL_WIDTH_SMALL_SPRITE_OFFSET = 8;
        public static int CELL_HEIGHT_SMALL_SPRITE_OFFSET = 8;

        // set the map size in cells
        public static int MAP_CELL_ROW_COUNT = 9;
        private const int MAP_CELL_COLUMN_COUNT = 9;

        // set game info display
        public static int GAME_INFO_DISPLAY_X_POSITION = 0;
        public static int GAME_INFO_DISPLAY_Y_POSITION = MAP_CELL_ROW_COUNT * CELL_HEIGHT;
        public static int GAME_INFO_DISPLAY_HEIGHT = 192;

        // set the window size
        public static int WINDOW_WIDTH = MAP_CELL_COLUMN_COUNT * CELL_WIDTH;
        public static int WINDOW_HEIGHT = MAP_CELL_ROW_COUNT * CELL_HEIGHT + GAME_INFO_DISPLAY_HEIGHT;

        // wall objects
        private List<Wall> walls;

        // jewel objects
        private List<Jewel> jewels;

        // death ball object
        private DeathBall deathBall;

        // score variable
        private int score;

        // lives variable
        private int lives;

        // score font 
        private SpriteFont scoreFont;

        // map array
        private int[,] map;

        // player object
        private Player player;

        // player starting position
        private Vector2 playerStartingPosition;

        // variable to hold the player's current game action
        GameAction playerGameAction;

        // keyboard state objects to track a single keyboard press
        KeyboardState newState;
        KeyboardState oldState;

        // declare a GraphicsDeviceManager object
        GraphicsDeviceManager graphics;

        // declare a SpriteBatch object
        SpriteBatch spriteBatch;

        public MazeGame()
        {
            graphics = new GraphicsDeviceManager(this);

            // set the window size 
            graphics.PreferredBackBufferWidth = WINDOW_WIDTH;
            graphics.PreferredBackBufferHeight = WINDOW_HEIGHT;

            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // add floors, walls, and ceilings
            walls = new List<Wall>();

            // add game objects
            jewels = new List<Jewel>();

            // initialize game info
            score = 0;
            lives = 3;

            BuildMap();

            // add the player
            playerStartingPosition = new Vector2(1 * CELL_WIDTH, 1 * CELL_HEIGHT);
            player = new Player(Content, playerStartingPosition);
            player.Active = true;

            // set the player's initial speed
            player.SpeedHorizontal = 5;
            player.SpeedVertical = 5;

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // load font for score
            scoreFont = Content.Load<SpriteFont>("score_font");

            // Note: wall and player sprites loaded when instantiated
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // get the player's current action based on a keyboard event
            playerGameAction = GetKeyboardEvents();

            ManageGameStatus();

            ManageGameActions(playerGameAction);

            ManageGameObjects();

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();

            DrawWalls(spriteBatch);

            DrawGameObjects(spriteBatch);

            DrawGameInfo();

            player.Draw(spriteBatch);

            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void ManageGameStatus()
        {
            //if (lives <= 0)
            //{
            //    MessageBox(new IntPtr(0), "     You have no more lives.\n              Game Over\n            Press OK to Exit.", "Game Status", 0);
            //    Exit();
            //}
        }

        /// <summary>
        /// manage all player game actions
        /// </summary>
        /// <param name="gameAction">player game action choice</param>
        private void ManageGameActions(GameAction gameAction)
        {
            switch (playerGameAction)
            {
                case GameAction.None:
                    break;

                // move player right
                case GameAction.PlayerRight:
                    player.PlayerDirection = Player.Direction.Right;

                    // only move player if allowed
                    if (CanMove())
                    {
                        player.Position = new Vector2(player.Position.X + player.SpeedHorizontal, player.Position.Y);
                    }
                    break;

                //move player left
                case GameAction.PlayerLeft:
                    player.PlayerDirection = Player.Direction.Left;

                    // only move player if allowed
                    if (CanMove())
                    {
                        player.Position = new Vector2(player.Position.X - player.SpeedHorizontal, player.Position.Y);
                    }

                    break;

                // move player up
                case GameAction.PlayerUp:
                    player.PlayerDirection = Player.Direction.Up;

                    // only move player if allowed
                    if (CanMove())
                    {
                        player.Position = new Vector2(player.Position.X, player.Position.Y - player.SpeedVertical);
                    }
                    break;

                case GameAction.PlayerDown:
                    player.PlayerDirection = Player.Direction.Down;

                    // only move player if allowed
                    if (CanMove())
                    {
                        player.Position = new Vector2(player.Position.X, player.Position.Y + player.SpeedVertical);
                    }
                    break;

                // quit game
                case GameAction.Quit:
                    Exit();
                    break;

                default:
                    break;
            }
        }

        private void DrawGameInfo()
        {
            spriteBatch.DrawString(scoreFont, String.Format("Score: {0}", score), new Vector2(GAME_INFO_DISPLAY_X_POSITION + 50, GAME_INFO_DISPLAY_Y_POSITION + 50), Color.Black);
            spriteBatch.DrawString(scoreFont, String.Format("Lives: {0}", lives), new Vector2(GAME_INFO_DISPLAY_X_POSITION + 50, GAME_INFO_DISPLAY_Y_POSITION + 80), Color.Black);
        }

        /// <summary>
        /// manage the interaction of all game objects
        /// </summary>
        private void ManageGameObjects()
        {

            ManageJewels();
            //ManageDeathBalls();

        }

        private void ManageDeathBalls()
        {
            //if (player.BoundingRectangle.Intersects(deathBall.BoundingRectangle))
            //{
            //    lives--;
            //    player.Position = playerStartingPosition;
            //}

            //deathBall.Update();
        }

        /// <summary>
        /// manage all jewel functions
        /// </summary>
        private void ManageJewels()
        {
            // set a default value to test against
            int indexOfJewel = -1;

            // cycle through the list of jewels to test for an collision with the player 
            // and record the index of a jewel in the list of jewels
            // use the continue statement to exit the foreach loop
            foreach (Jewel jewel in jewels)
            {
                if (player.BoundingRectangle.Intersects(jewel.BoundingRectangle))
                {
                    indexOfJewel = jewels.IndexOf(jewel);
                    continue;
                }
            }

            // if the player collided with a jewel, handle the event
            if (indexOfJewel != -1)
            {
                jewels.RemoveAt(indexOfJewel);
                score++;
            }

            //
            // example of using a lambda function to remove collected jewels
            // Note: not as efficient since it does not break out of loop once the jewel is found
            //
            //jewels.RemoveAll(x => player.BoundingRectangle.Intersects(x.BoundingRectangle));
        }

        /// <summary>
        /// get keyboard events
        /// </summary>
        /// <returns>GameAction</returns>
        private GameAction GetKeyboardEvents()
        {
            GameAction playerGameAction = GameAction.None;

            newState = Keyboard.GetState();

            if (CheckKey(Keys.Right) == true)
            {
                playerGameAction = GameAction.PlayerRight;
            }
            else if (CheckKey(Keys.Left) == true)
            {
                playerGameAction = GameAction.PlayerLeft;
            }
            else if (CheckKey(Keys.Up) == true)
            {
                playerGameAction = GameAction.PlayerUp;
            }
            else if (CheckKey(Keys.Down) == true)
            {
                playerGameAction = GameAction.PlayerDown;
            }
            else if (CheckKey(Keys.Escape) == true)
            {
                playerGameAction = GameAction.Quit;
            }

            oldState = newState;

            return playerGameAction;
        }

        /// <summary>
        /// check the current state of the keyboard against the previous state
        /// </summary>
        /// <param name="theKey">bool new key press</param>
        /// <returns></returns>
        private bool CheckKey(Keys theKey)
        {
            // allows the key to be held down
            return newState.IsKeyDown(theKey);

            // player must continue to tap the key
            //return oldState.IsKeyDown(theKey) && newState.IsKeyUp(theKey); 
        }

        /// <summary>
        /// check to confirm that player movement is allowed
        /// </summary>
        /// <returns></returns>
        private bool CanMove()
        {
            bool canMove = true;

            // do not allow movement into wall
            foreach (Wall wall in walls)
            {
                if (WallCollision(wall))
                {
                    canMove = false;
                    continue;
                }
            }

            return canMove;
        }

        /// <summary>
        /// test for player collision with a wall object
        /// </summary>
        /// <param name="wall">wall object to test</param>
        /// <returns>true if collision</returns>
        private bool WallCollision(Wall wall)
        {
            bool wallCollision = false;

            // create a Rectangle object for the new move's position
            Rectangle newPlayerPosition = player.BoundingRectangle;

            // test the new move's position for a collision with the wall
            switch (player.PlayerDirection)
            {
                case Player.Direction.Left:
                    // set the position of the new move's rectangle
                    newPlayerPosition.Offset(-player.SpeedHorizontal, 0);

                    // test for a collision with the new move and the wall
                    if (newPlayerPosition.Intersects(wall.BoundingRectangle))
                    {
                        wallCollision = true;

                        // move player next to wall
                        player.Position = new Vector2(wall.BoundingRectangle.Right, player.Position.Y);
                    }
                    break;

                case Player.Direction.Right:
                    // set the position of the new move's rectangle
                    newPlayerPosition.Offset(player.SpeedHorizontal, 0);

                    // test for a collision with the new move and the wall
                    if (newPlayerPosition.Intersects(wall.BoundingRectangle))
                    {
                        wallCollision = true;

                        // move player next to wall
                        player.Position = new Vector2(wall.BoundingRectangle.Left - player.BoundingRectangle.Width, player.Position.Y);
                    }
                    break;

                case Player.Direction.Up:
                    // set the position of the new move's rectangle
                    newPlayerPosition.Offset(0, -player.SpeedVertical);

                    // test for a collision with the new move and the wall
                    if (newPlayerPosition.Intersects(wall.BoundingRectangle))
                    {
                        wallCollision = true;

                        // move player next to wall
                        player.Position = new Vector2(player.Position.X, wall.BoundingRectangle.Bottom);
                    }
                    break;

                case Player.Direction.Down:
                    // set the position of the new move's rectangle
                    newPlayerPosition.Offset(0, player.SpeedVertical);

                    // test for a collision with the new move and the wall
                    if (newPlayerPosition.Intersects(wall.BoundingRectangle))
                    {
                        wallCollision = true;

                        // move player next to wall
                        player.Position = new Vector2(player.Position.X, wall.BoundingRectangle.Top - player.BoundingRectangle.Height);
                    }
                    break;

                default:
                    break;
            }

            return wallCollision;
        }

        private void BuildMap()
        {
            // Note: initialized array size must equal the MAP_CELL_COLUMN_COUNT and MAP_CELL_ROW_COUNT
            //
            // 1 = wall
            // 2 = green jewel
            //
            map = new int[,]
            {
                { 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                { 1, 0, 0, 0, 0, 0, 2, 0, 1 },
                { 1, 0, 1, 1, 0, 1, 1, 0, 1 },
                { 1, 0, 1, 1, 0, 1, 1, 0, 1 },
                { 1, 3, 0, 0, 0, 0, 0, 0, 1 },
                { 1, 0, 1, 1, 0, 1, 1, 0, 1 },
                { 1, 0, 1, 1, 2, 1, 1, 0, 1 },
                { 1, 0, 0, 0, 0, 0, 0, 0, 1 },
                { 1, 1, 1, 1, 1, 1, 1, 1, 1 }
            };

            for (int row = 0; row < MAP_CELL_ROW_COUNT; row++)
            {
                for (int column = 0; column < MAP_CELL_COLUMN_COUNT; column++)
                {
                    // add walls
                    if (map[row, column] == 1)
                    {
                        walls.Add(new Wall(Content, "wall", new Vector2(column * CELL_HEIGHT, row * CELL_WIDTH)));
                    }
                    // add jewels
                    if (map[row, column] == 2)
                    {
                        jewels.Add(new Jewel(Content, Jewel.TypeName.Green, new Vector2(column * CELL_HEIGHT + CELL_HEIGHT_SMALL_SPRITE_OFFSET, row * CELL_WIDTH + CELL_WIDTH_SMALL_SPRITE_OFFSET)));
                    }
                }
            }

            // add horizontal death ball
            //deathBall = new DeathBall(Content, "death_ball", new Vector2(1 * CELL_WIDTH, 4 * CELL_HEIGHT));
            //deathBall.Active = true;
            //deathBall.SpeedHorizontal = 5;
            //deathBall.SpeedVertical = 0;
            //deathBall.EndingPosition = new Vector2(7 * CELL_WIDTH, 4 * CELL_HEIGHT);
            //deathBall.Loop = true;
        }

        /// <summary>
        /// draw all walls on map
        /// </summary>
        /// <param name="spriteBatch">spriteBatch object</param>
        private void DrawWalls(SpriteBatch spriteBatch)
        {
            foreach (Wall wall in walls)
            {
                wall.Draw(spriteBatch);
            }
        }

        /// <summary>
        /// draw all game objects on map
        /// </summary>
        /// <param name="spriteBatch">spriteBatch object</param>
        private void DrawGameObjects(SpriteBatch spriteBatch)
        {
            foreach (Jewel jewel in jewels)
            {
                jewel.Draw(spriteBatch);
            }

            //deathBall.Draw(spriteBatch);
        }
    }
}
