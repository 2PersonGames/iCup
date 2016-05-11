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
    class PipeObject : GameObject
    {
        int[,] carbon;
        public int[,] Carbon
        {
            get { return carbon; }
            set
            {
                for (int i = 0; i < width; i++)
                {
                    for (int j = 0; j < height; j++)
                    {
                        carbon[i, j] = value[i, j];
                    }
                }
            }
        }
        int[,] previousCarbon;
        public int[,] PreviousCarbon
        {
            get { return previousCarbon; }
            set
            {
                for (int i = 0; i < width; i++)
                {
                    for (int j = 0; j < height; j++)
                    {
                        previousCarbon[i, j] = value[i, j];
                    }
                }
            }
        }
        int width;
        public int Width
        {
            get { return width; }
        }
        int height;
        public int Height
        {
            get { return height; }
        }
        bool carbonFlow;
        public bool CarbonFlow
        {
            get { return carbonFlow; }
            set { carbonFlow = value; }
        }

        //Transfer variables
        int flowDirection;
        public int FlowDirection
        {
            get { return flowDirection; }
        }
        Point transferPoint;
        public Point TransferPoint
        {
            get { return transferPoint; }
        }
        bool carbonFull;
        public bool CarbonFull
        {
            get { return carbonFull; }
        }

        const float PipeWidth = 0.33984375f;

        SpriteEffects spriteEffect;

        public PipeObject(int nWidth, int nHeight, int option, Texture2D nSprite)
            : base()
        {
            spriteEffect = SpriteEffects.None;
            width = nWidth;
            height = nHeight;
            sprite = nSprite;

            carbon = new int[width, height];
            previousCarbon = new int[width, height];
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    carbon[i, j] = 0;
                    previousCarbon[i, j] = 0;
                }
            }

            carbonFlow = false;
            alive = true;
            TileLoad(option);

            flowDirection = 0;
            transferPoint = new Point(0, 0);
            carbonFull = false;
        }

        public override void Load(Texture2D nSprite)
        {
            base.Load(nSprite);
        }

        public void Update(Rectangle boundary, Rectangle[,] grid)
        {
            base.Update(boundary);

            UpdateRect(grid);
        }

        public void Update()
        {
            UpdateCarbonFlow();
        }

        private void UpdateRect(Rectangle[,] grid)
        {
            foreach (Rectangle gridRect in grid)
            {
                Point checkPoint = new Point((int)position.X, (int)position.Y);
                if (gridRect.Contains(checkPoint))
                {
                    rect = new Rectangle((int)gridRect.X, (int)gridRect.Y, gridRect.Width, gridRect.Height);
                    break;
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D carbonSprite)
        {
            Rectangle drawRect;
            if (rotation == 0)
            {
                drawRect = rect;
            }
            else
            {
                drawRect = new Rectangle(rect.X + rect.Width, rect.Y, rect.Width, rect.Height);
            }

            if (!alive)
            {
                spriteBatch.Draw(sprite, drawRect, null, Color.Gray, rotation, centre, spriteEffect, 0f);

                Rectangle carbonRect = new Rectangle((int)position.X, (int)position.Y, (int)1, (int)1);
                for (int i = 0; i < width; i++)
                {
                    for (int j = 0; j < height; j++)
                    {
                        if (carbon[i, j] == 1)
                        {
                            carbonRect.X = (int)position.X + i;
                            carbonRect.Y = (int)position.Y + j;
                            spriteBatch.Draw(carbonSprite, carbonRect, Color.Green);
                        }
                    }
                }
            }
            else
            {
                spriteBatch.Draw(sprite, drawRect, null, Color.White, rotation, centre, spriteEffect, 0f);
            }
        }

        public void Draw(SpriteBatch spriteBatch, Rectangle drawRect)
        {
            if (rotation != 0)
            {
                drawRect.X += drawRect.Width;
            }

            spriteBatch.Draw(sprite, drawRect, null, Color.White, rotation, centre, spriteEffect, 0f);
        }

        private void TileLoad(int option)
        {
            switch (option)
            {
                case 1:
                    {
                        //Vertical line piece
                        LinePiece(PipeWidth, 1f);
                        break;
                    }
                case 2:
                    {
                        //Horizontal line piece
                        LinePiece(1f, PipeWidth);
                        rotation = MathHelper.PiOver2;
                        break;
                    }
                case 3:
                    {
                        //L corner
                        Corner(PipeWidth, 1f);
                        break;
                    }
                case 4:
                    {
                        //Reverse L corner
                        ReverseCorner(PipeWidth, 1f);
                        break;
                    }
                case 5:
                    {
                        //Upside down L corner
                        UpsideDownCorner(PipeWidth, 1f);
                        spriteEffect = SpriteEffects.FlipVertically;
                        break;
                    }
                case 6:
                    {
                        //Reverse upside down L corner
                        UpsideDownReverseCorner(PipeWidth, 1f);
                        spriteEffect = SpriteEffects.FlipVertically;
                        break;
                    }
            }
        }

        private void LinePiece(float nPipeWidth, float nPipeHeight)
        {
            int pipeWidth = (int)Math.Floor(width * nPipeWidth);
            int pipeHeight = (int)Math.Floor(height * nPipeHeight);

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    if (i < pipeWidth || i > width - pipeWidth)
                    {
                        if (j < pipeHeight || j > height - pipeHeight)
                        {
                            carbon[i, j] = 2;
                        }
                    }
                }
            }
        }

        private void Corner(float nPipeWidth, float nPipeHeight)
        {
            LinePiece(nPipeWidth, nPipeHeight);
            LinePiece(nPipeHeight, nPipeWidth);

            for (int i = 0; i < width * PipeWidth; i++)
            {
                for (int j = (int)Math.Floor(height - (height * PipeWidth)); j < height; j++)
                {
                    carbon[i, j] = 2;
                }
            }
        }

        private void ReverseCorner(float nPipeWidth, float nPipeHeight)
        {
            LinePiece(nPipeWidth, nPipeHeight);
            LinePiece(nPipeHeight, nPipeWidth);

            for (int i = (int)Math.Floor(width - (width * PipeWidth)); i < width; i++)
            {
                for (int j = (int)Math.Floor(height - (height * PipeWidth)); j < height; j++)
                {
                    carbon[i, j] = 2;
                }
            }
        }

        private void UpsideDownCorner(float nPipeWidth, float nPipeHeight)
        {
            LinePiece(nPipeWidth, nPipeHeight);
            LinePiece(nPipeHeight, nPipeWidth);

            for (int i = 0; i < width * PipeWidth; i++)
            {
                for (int j = 0; j < height * PipeWidth; j++)
                {
                    carbon[i, j] = 2;
                }
            }
        }

        private void UpsideDownReverseCorner(float nPipeWidth, float nPipeHeight)
        {
            LinePiece(nPipeWidth, nPipeHeight);
            LinePiece(nPipeHeight, nPipeWidth);

            for (int i = 0; i < width * PipeWidth; i++)
            {
                for (int j = (int)Math.Floor(height - (height * PipeWidth)); j < height; j++)
                {
                    carbon[i, j] = 2;
                }
            }
        }

        protected override void UpdatePosition(Rectangle boundary)
        {
            boundary.Width -= 25;
            boundary.Height -= 25;
            base.UpdatePosition(boundary);
        }

        private void UpdateCarbonFlow()
        {
            PreviousCarbon = Carbon;

            if (carbonFlow)
            {
                for (int i = 0; i < width; i++)
                {
                    for (int j = 0; j < height; j++)
                    {
                        if (previousCarbon[i, j] == 1)
                        {
                            if (j < height - 1 && carbon[i, j + 1] < 2)
                            {
                                carbon[i, j + 1] = 1;
                            }
                            if (j > 0 && carbon[i, j - 1] < 2)
                            {
                                carbon[i, j - 1] = 1;
                            }

                            if (i < width - 1)
                            {
                                if (carbon[i + 1, j] < 2)
                                {
                                    carbon[i + 1, j] = 1;
                                }
                                if (j < height - 1 && carbon[i + 1, j + 1] < 2)
                                {
                                    carbon[i + 1, j + 1] = 1;
                                }
                                if (j > 0 && carbon[i + 1, j - 1] < 2)
                                {
                                    carbon[i + 1, j - 1] = 1;
                                }
                            }

                            if (i > 0)
                            {
                                if (carbon[i - 1, j] < 2)
                                {
                                    carbon[i - 1, j] = 1;
                                }
                                if (j < height - 1 && carbon[i - 1, j + 1] < 2)
                                {
                                    carbon[i - 1, j + 1] = 1;
                                }
                                if (j > 0 && carbon[i - 1, j - 1] < 2)
                                {
                                    carbon[i - 1, j - 1] = 1;
                                }
                            }
                        }
                    }
                }
            }

            CheckIfFull();
        }

        private void CheckIfFull()
        {
            bool full = true;

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    if (carbon[i, j] == 0)
                    {
                        full = false;
                        break;
                    }
                }
            }

            if (full)
            {
                carbonFull = true;
                CalculateFlowDirection();
            }
        }

        public void CalculateFlowDirection()
        {
            Point flowPoint = new Point(0, 0);
            int direction = 0;

            //Calculates the position of the last empty space
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    if (previousCarbon[i, j] == 0)
                    {
                        flowPoint = new Point(i, j);
                        break;
                    }
                }
            }

            if (flowPoint.X == 0 && flowPoint.Y > 0)
            {
                direction = 4;
            }
            else if (flowPoint.X > 0 && flowPoint.Y == 0)
            {
                direction = 1;
            }
            else if (flowPoint.X > 0 && flowPoint.Y > 0)
            {
                if (flowPoint.X == rect.Width - 1)
                {
                    direction = 2;
                }
                else if (flowPoint.Y == rect.Height - 1)
                {
                    direction = 3;
                }
            }

            flowDirection = direction;
            transferPoint = flowPoint;
        }
    }
}
