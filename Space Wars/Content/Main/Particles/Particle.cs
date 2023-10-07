using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space_Wars.Content.Main.Particles
{
    public class Particle
    {
        public bool isExpired = false;
        public bool fadesOut;
        Vector2 position;
        Vector2 velocity;
        Color color;
        Color fadeToColor;
        Color renderColor;
        public float timeLeft;
        private float originalTimeLeft;
        float angle;
        float angularVelocity;
        float transparency;
        public Texture2D texture;
        public Vector2 Size
        {
            get { return texture == null ? Vector2.Zero : new Vector2(texture.Width, texture.Height); }
        }
        public Particle(Texture2D _texture, float _timeLeft, Vector2 _position, Vector2 _velocity, float _angle, float _angularVelocity, float _transparency, bool _fadesOut, Color _color, Color _fadeToColor)
        {
            texture = _texture;
            timeLeft = _timeLeft;
            position = _position;
            velocity = _velocity;
            angle = _angle;
            angularVelocity = _angularVelocity;
            fadesOut = _fadesOut;
            color = _color;
            fadeToColor = _fadeToColor;
            if (fadesOut == true)
            {
                transparency = 1;
                originalTimeLeft = _timeLeft;
            }
            else
            {
                transparency = _transparency;
                originalTimeLeft = _timeLeft;
            }
        }
        public Particle(Texture2D _texture, Vector2 _position, float _angle, float _transparency, Color _color)
        {
            texture = _texture;
            timeLeft = Engine.deltaSeconds;
            position = _position;
            velocity = Vector2.Zero;
            angle = _angle;
            angularVelocity = 0;
            fadesOut = false;
            color = _color;
            fadeToColor = Color.White;
            transparency = _transparency;
            originalTimeLeft = timeLeft;
        }
        public void Update()
        {
            float timeScalar = timeLeft / originalTimeLeft;
            if(timeScalar > 1 || -1 > timeScalar)
            {
                timeScalar = 1;
            }
            renderColor.R = (byte)Engine.Lerp(color.R, fadeToColor.R, 1-timeScalar);
            renderColor.G = (byte)Engine.Lerp(color.G, fadeToColor.G, 1-timeScalar);
            renderColor.B = (byte)Engine.Lerp(color.B, fadeToColor.B, 1-timeScalar);
            if (timeLeft <= 0)
            {
                isExpired = true;
            }
            if (fadesOut == true)
            {
                transparency = timeScalar;
            }
            timeLeft -= Engine.deltaSeconds;
            position += velocity * Engine.deltaSeconds * 60;
            angle += angularVelocity * Engine.deltaSeconds * 60;
        }
        public void Draw(SpriteBatch _spriteBatch)
        {
            //Draws itself on the given spritebatch, position is offset by the screen position offset
            _spriteBatch.Draw(texture, position - Engine.mousePositionOffset, null, renderColor * transparency, angle, Size/2, 1f, 0, 0.2f);
        }
    }
}
