using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

namespace icup1
{
    public class Level_OceanDumping : DrawableGameComponent
    {
        SpriteBatch spriteBatch;
        Rectangle viewportRect;
        Rectangle safeTitleAreaRect;
        Texture2D background;
        Texture2D gridTexture;
        Texture2D[] pipes;
        Texture2D carbonTexture;
        GamePadState previousGamePad;
        SpriteFont font1;

        //Grid
        int width;
        int height;
        Rectangle[,] grid;
        Rectangle gridSquare;
        Rectangle playArea;
        Rectangle displayArea;

        //Pipes
        List<PipeObject> activePipes;
        List<PipeObject> pipeList;
        PipeObject currentPipe;

        //Random pipe generator
        Random random;

        //Scoring
        int carbonSaved;
        int carbonLost;

        public Level_OceanDumping(Game game, Rectangle nViewportRect, Rectangle nSafeTitleAreaRect, int Width) 
            : base(game)
        {
            LoadContent();

            int Height = (int)Math.Floor(Width * (4 / 5f));

            viewportRect = nViewportRect;
            safeTitleAreaRect = nSafeTitleAreaRect;

            random = new Random();

            carbonSaved = 0;
            carbonLost = 0;
            spriteBatch = new SpriteBatch(game.GraphicsDevice);

            //Grid
            width = Width;
            height = Height;
            playArea = new Rectangle(safeTitleAreaRect.X + ((int)Math.Floor(safeTitleAreaRect.Width * 0.3)), safeTitleAreaRect.Y, safeTitleAreaRect.Width - ((int)Math.Floor(safeTitleAreaRect.Width * 0.3)), safeTitleAreaRect.Height);
            gridSquare = new Rectangle(playArea.X, playArea.Y, playArea.Width / Width, playArea.Height / Height);
            displayArea = new Rectangle(safeTitleAreaRect.X, safeTitleAreaRect.Y, gridSquare.Width * 2, safeTitleAreaRect.Height);
            grid = new Rectangle[width, height];
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    int rectWidth = gridSquare.Width;
                    int rectHeight = gridSquare.Height;
                    int rectX = playArea.X + (gridSquare.Width * i);
                    int rectY = playArea.Y + (gridSquare.Height * j);
                    grid[i, j] = new Rectangle(rectX, rectY, rectWidth, rectHeight);
                }
            }

            //Pipes
            activePipes = new List<PipeObject>();
            pipeList = new List<PipeObject>();
            for (int i = 0; i < 6; i++)
            {
                AddPipeToList();
            }
            currentPipe = new PipeObject(gridSquare.Width, gridSquare.Height, 1, GetPipeSprite(1));
            DropPipe();
            currentPipe.Position = new Vector2(playArea.X + playArea.Width / 2, playArea.Y + playArea.Height / 2);

            activePipes.Add(new PipeObject(gridSquare.Width, gridSquare.Height, 1, GetPipeSprite(1)));
            activePipes[0].Position = new Vector2(grid[0, 0].X, grid[0, 0].Y);
            activePipes[0].Rect = new Rectangle(grid[0, 0].X, grid[0, 0].Y, grid[0, 0].Width, grid[0, 0].Height);
            activePipes[0].Alive = false;
            activePipes.Add(new PipeObject(gridSquare.Width, gridSquare.Height, 1, GetPipeSprite(1)));
            activePipes[1].Position = new Vector2(grid[width - 1, height - 1].X, grid[width - 1, height - 1].Y);
            activePipes[1].Rect = new Rectangle(grid[width - 1, height - 1].X, grid[width - 1, height - 1].Y, grid[width - 1, height - 1].Width, grid[width - 1, height - 1].Height);
            activePipes[1].Alive = false;

            for (int i = 0; i < activePipes[0].Height; i++)
            {
                if (activePipes[0].Carbon[0, i] < 2)
                {
                    activePipes[0].Carbon[0, i] = 1;
                }
            }

            //Test
            activePipes[0].CarbonFlow = true;
        }

        protected override void LoadContent()
        {
            pipes = new Texture2D[6];
            pipes[0] = Game.Content.Load<Texture2D>("Sprites\\Pipes\\Corner_pipe_right");
            pipes[1] = Game.Content.Load<Texture2D>("Sprites\\Pipes\\Corner_pipe_left");
            pipes[2] = Game.Content.Load<Texture2D>("Sprites\\Pipes\\Straight_pipe_1");
            pipes[3] = Game.Content.Load<Texture2D>("Sprites\\Pipes\\Straight_pipe_2");
            pipes[4] = Game.Content.Load<Texture2D>("Sprites\\Pipes\\Straight_pipe_3");
            pipes[5] = Game.Content.Load<Texture2D>("Sprites\\Pipes\\Straight_pipe_4");

            background = Game.Content.Load<Texture2D>("Backgrounds\\waterBackground");
            gridTexture = Game.Content.Load<Texture2D>("Sprites\\Pipes\\grid");

            carbonTexture = Game.Content.Load<Texture2D>("Sprites\\Pipes\\square");

            font1 = Game.Content.Load<SpriteFont>("Sprites\\font1");

            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            ApplyCarbonRules();

            UpdateCurrentPipe();

            base.Update(gameTime);

            previousGamePad = GamePad.GetState(PlayerIndex.One);
        }

        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend);

            BackgroundDraw(spriteBatch);

            DrawGrid(spriteBatch);

            DrawCarbon(spriteBatch);
            DrawPipes(spriteBatch);

            DrawHUD(spriteBatch);

            base.Draw(gameTime);

            spriteBatch.End();
        }

        protected void BackgroundDraw(SpriteBatch spriteBatch)
        {
            //http://www.eyesontutorials.com/images/Effects/VladStudio/tut24_UnderwaterWallpaper/Underwater_1024x768.jpg
            spriteBatch.Draw(background, viewportRect, Color.White);
        }

        protected void DrawGrid(SpriteBatch spriteBatch)
        {
            foreach (Rectangle gridRect in grid)
            {
                Color drawColor = Color.White;
                Point checkLocation = new Point((int)Math.Floor(currentPipe.Position.X + currentPipe.Centre.X),
                                                (int)Math.Floor(currentPipe.Position.Y + currentPipe.Centre.Y));
                if (gridRect.Contains(checkLocation))
                {
                    drawColor = Color.Violet;
                }
                spriteBatch.Draw(gridTexture, gridRect, drawColor);
            }
        }

        protected void DrawCarbon(SpriteBatch spriteBatch)
        {
        }

        protected void DrawPipes(SpriteBatch spriteBatch)
        {
            currentPipe.Draw(spriteBatch, carbonTexture);

            Rectangle pipeListRect = new Rectangle(displayArea.X, displayArea.Y, displayArea.Width, displayArea.Width);
            foreach (PipeObject pipe in pipeList)
            {
                pipe.Draw(spriteBatch, pipeListRect);
                pipeListRect.Y += (int)(pipeListRect.Height * 1.1);
            }

            foreach (PipeObject pipe in activePipes)
            {
                pipe.Draw(spriteBatch, carbonTexture);
            }
        }

        protected void DrawHUD(SpriteBatch spriteBatch)
        {
            Vector2 stringDrawPosition = new Vector2(displayArea.X + gridSquare.Width * 2, displayArea.Y);
            spriteBatch.DrawString(font1, "Carbon Saved: " + carbonSaved.ToString(), stringDrawPosition, Color.DarkMagenta);
            stringDrawPosition.Y += 100;
            spriteBatch.DrawString(font1, "Carbon Lost: " + carbonLost.ToString(), stringDrawPosition, Color.DarkMagenta);
        }

        //Copies the current to previous
        //Expands carbon down the pipe
        protected void ApplyCarbonRules()
        {
            for (int i = 0; i < pipeList.Count - 1; i++)
            {
                if (pipeList[i].CarbonFlow)
                {
                    pipeList[i].Update();

                    if (pipeList[i].CarbonFull)
                    {
                        Point checkPoint = new Point(pipeList[i].TransferPoint.X, pipeList[i].TransferPoint.Y);
                        if (pipeList[i].FlowDirection == 1)
                        {
                            checkPoint.Y -= 1;
                        }
                        else if (pipeList[i].FlowDirection == 2)
                        {
                            checkPoint.X += 1;
                        }
                        else if (pipeList[i].FlowDirection == 3)
                        {
                            checkPoint.Y += 1;
                        }
                        else if (pipeList[i].FlowDirection == 4)
                        {
                            checkPoint.X -= 1;
                        }

                        bool loseMoreCarbon = true;
                        foreach (PipeObject pipe in pipeList)
                        {
                            if (pipe.Rect.Contains(new Point(checkPoint.X + (int)pipeList[i].Position.X, checkPoint.Y + (int)pipeList[i].Position.Y)))
                            {
                                if (pipe.Carbon[checkPoint.X, checkPoint.Y] < 2)
                                {
                                    if (pipeList[i].FlowDirection == 1)
                                    {
                                        for (int j = 0; j < pipe.Width; j++)
                                        {
                                            if (pipe.Carbon[j, 0] < 2)
                                            {
                                                pipe.Carbon[j, 0] = 1;
                                            }
                                        }
                                    }
                                    else if (pipeList[i].FlowDirection == 2)
                                    {
                                        for (int j = 0; j < pipe.Height; j++)
                                        {
                                            if (pipe.Carbon[pipe.Width - 1, j] < 2)
                                            {
                                                pipe.Carbon[pipe.Width - 1, j] = 1;
                                            }
                                        }
                                    }
                                    else if (pipeList[i].FlowDirection == 3)
                                    {
                                        for (int j = 0; j < pipe.Width; j++)
                                        {
                                            if (pipe.Carbon[j, pipe.Height - 1] < 2)
                                            {
                                                pipe.Carbon[j, pipe.Height - 1] = 1;
                                            }
                                        }
                                    }
                                    else if (pipeList[i].FlowDirection == 4)
                                    {
                                        for (int j = 0; j < pipe.Height; j++)
                                        {
                                            if (pipe.Carbon[0, j] < 2)
                                            {
                                                pipe.Carbon[0, j] = 1;
                                            }
                                        }
                                    }
                                    pipeList[i].CarbonFlow = false;
                                    pipe.CarbonFlow = true;
                                }
                                else
                                {
                                    carbonLost++;
                                    loseMoreCarbon = false;
                                }
                                break;
                            }
                        }
                        if (loseMoreCarbon)
                        {
                            carbonLost++;
                        }
                    }
                    break;
                }
            } 
        }

        //Drops a pipe on to the map
        //And removes that pipe from the pipeList
        //Adds the pipe to the activePipe list
        protected void DropPipe()
        {
            bool positionOK = true;

            foreach (PipeObject pipe in activePipes)
            {
                if (currentPipe.Rect == pipe.Rect && pipe != activePipes[0] && pipe != activePipes[1])
                {
                    if (!pipe.CarbonFull)
                    {
                        DeletePipe(pipe);
                        positionOK = true;
                        break;
                    }
                    else
                    {
                        positionOK = false;
                    }
                }
                else
                {
                    positionOK = false;
                }
            }

            if (positionOK)
            {
                currentPipe.Alive = false;
                activePipes.Add(currentPipe);
                currentPipe = pipeList[0];
                currentPipe.Position = new Vector2(activePipes[activePipes.Count - 1].Position.X, activePipes[activePipes.Count - 1].Position.Y);
                pipeList.Remove(currentPipe);
                AddPipeToList();
            }
        }

        //Destroyes pipe at location
        void DeletePipe(PipeObject pipe)
        {
            if (!pipe.CarbonFull && pipe != activePipes[0] && pipe != activePipes[1])
            {
                activePipes.Remove(pipe);
            }
        }

        //Updates the pipe currently being controller by the player
        protected void UpdateCurrentPipe()
        {
            currentPipe.Velocity = new Vector2(GamePad.GetState(PlayerIndex.One).ThumbSticks.Left.X * 5f,
                                               -GamePad.GetState(PlayerIndex.One).ThumbSticks.Left.Y * 5f);
            currentPipe.Update(playArea, grid);

            //Drops pipe when A is pressed
            if (GamePad.GetState(PlayerIndex.One).Buttons.A == ButtonState.Pressed &&
                previousGamePad.Buttons.A == ButtonState.Released)
            {
                DropPipe();
            }

            //Deletes pipe when B is pressed
            if (GamePad.GetState(PlayerIndex.One).Buttons.B == ButtonState.Pressed &&
                previousGamePad.Buttons.B == ButtonState.Released)
            {
                foreach (PipeObject pipe in activePipes)
                {
                    if (currentPipe.Rect == pipe.Rect)
                    {
                        DeletePipe(pipe);
                        break;
                    }
                }
            }
        }

        protected Texture2D GetPipeSprite(int randomPipe)
        {
            if (randomPipe == 1 || randomPipe == 2)
            {
                //Return straight pipe
                switch(random.Next(0, 4))
                {
                    case 0:
                        {
                            return pipes[2];
                        }
                    case 1:
                        {
                            return pipes[3];
                        }
                    case 2:
                        {
                            return pipes[4];
                        }
                    case 3:
                        {
                            return pipes[5];
                        }
                    default:
                        {
                            return pipes[2];
                        }
                }
            }
            else if (randomPipe == 3 || randomPipe == 5)
            {
                //L or upside down L
                return pipes[0];
            }
            else
            {
                //Reverse L or reverse upside down 
                return pipes[1];
            }
        }

        protected void AddPipeToList()
        {
            int randomPipe = random.Next(1, 7);
            pipeList.Add(new PipeObject(gridSquare.Width, gridSquare.Height, randomPipe, GetPipeSprite(randomPipe)));
        }
    }
}
