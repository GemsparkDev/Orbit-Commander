using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Space_Wars.Content.Main.Particles
{
    public class Particle
    {
        public bool isExpired = false;
        public bool experienceGravity = false;
        public Vector2 Size => texture == null ? Vector2.Zero : new Vector2(texture.Width, texture.Height);
        public String drawText;
        private Texture2D texture;
        private Vector2 position;
        public Vector2 Position { get { return position; } set { velocity = value; } }
        private Vector2 velocity;
        public Vector2 Velocity { get { return velocity; } set { velocity = value; } }
        private Color color;
        private Color renderColor;
        private Color fadeToColor;
        private float timeLeft;
        private float originalTimeLeft;
        private float angle;
        private float angularVelocity;
        public Particle(Texture2D _texture, float _timeLeft, Vector2 _position, Vector2 _velocity, float _angle, float _angularVelocity, Color _color, Color _fadeToColor)
        {
            texture = _texture;
            timeLeft = _timeLeft;
            position = _position;
            velocity = _velocity;
            angle = _angle;
            angularVelocity = _angularVelocity;
            color = _color;
            fadeToColor = _fadeToColor;
            originalTimeLeft = _timeLeft;
        }
        public Particle(Texture2D _texture, Vector2 _position, float _angle, Color _color)
        {
            texture = _texture;
            timeLeft = float.Epsilon;
            position = _position;
            velocity = Vector2.Zero;
            angle = _angle;
            angularVelocity = 0;
            color = _color;
            fadeToColor = Color.White;
            originalTimeLeft = timeLeft;
        }
        public void Update()
        {
            float timeScalar = Math.Clamp(timeLeft / originalTimeLeft, 0, 1);
            renderColor = new Color(
            (byte)Util.Lerp(color.R, fadeToColor.R, 1 - timeScalar),
            (byte)Util.Lerp(color.G, fadeToColor.G, 1 - timeScalar),
            (byte)Util.Lerp(color.B, fadeToColor.B, 1 - timeScalar),
            (byte)Util.Lerp(color.A, fadeToColor.A, 1 - timeScalar)
            );
            if (timeLeft <= 0)
            {
                isExpired = true;
            }
            timeLeft -= Engine.DeltaSeconds;
            position += velocity * Engine.DeltaSeconds * 60;
            angle += angularVelocity * Engine.DeltaSeconds * 60;
        }
        public void Draw(SpriteBatch _spriteBatch)
        {
            if(drawText != null)
            {
                _spriteBatch.DrawString(Assets.TextFont, drawText, position, renderColor * ((float)renderColor.A/255f), angle, Assets.TextFont.MeasureString(drawText) / 2, 0.75f, 0, 0.2f);
                return;
            }
            _spriteBatch.Draw(texture, position, null, renderColor * ((float)renderColor.A / 255f), angle, Size/2, 1f, 0, 0.2f);
        }
    }
}
