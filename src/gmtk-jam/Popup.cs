using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace gmtk_jam
{
    public class Popup
    {
        private readonly string _text;
        private readonly Color _color;
        public Vector2 Pos;

        private float TimeLeft;
        public bool IsDone => TimeLeft <= 0;

        public Popup(string text, Color color, Vector2 pos, float fadeTime)
        {
            _text = text;
            _color = color;
            Pos = pos;
            TimeLeft = fadeTime;
        }

        public void Update(GameTime gameTime)
        {
            var elapsed = (float) gameTime.ElapsedGameTime.TotalSeconds;
            TimeLeft -= elapsed;
            Pos = new Vector2(Pos.X, Pos.Y + elapsed * 40);
        }

        public void Draw(SpriteBatch sb)
        {
            sb.DrawString(Assets.Font24, _text, Pos, _color);
        }
    }
}