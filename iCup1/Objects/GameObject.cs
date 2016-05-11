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
    class GameObject
    {
        //Location variables
        protected Vector2 position;
        public Vector2 Position
        {
            get { 
                return position;
                }
            set {
                position = value;
                }
        }
        protected Vector2 velocity;
        public Vector2 Velocity
        {
            get
            {
                return velocity;
            }
            set
            {
                velocity = value;
            }
        }
        protected float rotation;
        public float Rotation
        {
            get { return rotation; }
            set { rotation = value; }
        }
        protected Vector2 centre;
        public Vector2 Centre
        {
            get { return centre; }
            set { centre = value; }
        }

        //Drawing varibales
        protected Texture2D sprite;
        public Texture2D Sprite
        {
            get { return sprite; }
        }
        protected Rectangle rect;
        public Rectangle Rect
        {
            get { return rect; }
            set { rect = value; }
        }
        protected float scale;
        public float Scale
        {
            get { return scale; }
        }
        protected Color spriteColor;
        public Color SpriteColor
        {
            get { return spriteColor; }
        }

        //Health variables
        protected bool alive;
        public bool Alive
        {
            get { return alive; }
            set { alive = true; }
        }

        public GameObject()
        {
            position = Vector2.Zero;
            velocity = Vector2.Zero;
            rect = Rectangle.Empty;
            alive = false;
            rotation = 0f;
            scale = 1f;
            spriteColor = Color.White;
            centre = Vector2.Zero;
        }
        public GameObject(Texture2D sp, Vector2 pos)
        {
            position = pos;
            velocity = Vector2.Zero;
            rect = Rectangle.Empty;
            alive = false;
            rotation = 0f;
            scale = 1f;
            spriteColor = Color.White;
            centre = Vector2.Zero;
            sprite = sp;
            UpdateCentre();
        }

        public virtual void Load(Texture2D nSprite)
        {
            sprite = nSprite;
			UpdateCentre();
        }

        public virtual void Update(Rectangle boundary)
        {
            UpdatePosition(boundary);
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if (alive && Sprite != null)
            {
                spriteBatch.Draw(sprite, position, null, SpriteColor, rotation, Centre, Scale, SpriteEffects.None, 0f);
            }
        }

        protected virtual void UpdatePosition(Rectangle boundary)
        {
            if (position.X + Centre.X + velocity.X >= boundary.Width + boundary.X)
            {
                position.X = boundary.Width - Centre.X + boundary.X;
                velocity.X = 0;
            }
            else if (position.X - Centre.X + velocity.X <= boundary.X)
            {
                position.X = boundary.X + Centre.X;
                velocity.X = 0;
            }
            if (position.Y + Centre.Y + velocity.Y >= boundary.Height + boundary.Y)
            {
                position.Y = boundary.Height - Centre.Y + boundary.Y;
                velocity.Y = 0;
            }
            else if (position.Y - Centre.Y + velocity.Y <= boundary.Y)
            {
                position.Y = boundary.Y + Centre.Y;
                velocity.Y = 0;
            }

            position += velocity;
            UpdateRect();
        }

        protected int RelativeSize(int number)
        {
            return (int)Math.Floor(number * scale);
        }

        public virtual void UpdateRect()
        {
           
            rect = new Rectangle((int)Math.Floor(position.X), (int)Math.Floor(position.Y), RelativeSize(sprite.Width), RelativeSize(sprite.Height));
        }
		
		protected void UpdateCentre()
		{
			centre = new Vector2(RelativeSize(sprite.Width), RelativeSize(sprite.Height));
		}
    }
}
