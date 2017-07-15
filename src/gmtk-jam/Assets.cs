using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace gmtk_jam
{
    public static class Assets
    {
        public static SpriteFont Font12 { get; private set; }
        public static SpriteFont Font16 { get; private set; }
        public static SpriteFont Font20 { get; private set; }
        public static SpriteFont Font24 { get; private set; }
        public static Texture2D BlankTexture { get; private set; }

        public static SpriteSheet TommySheet { get; private set; }

        public static void Load(ContentManager cm)
        {
            var gd = ((IGraphicsDeviceService) cm.ServiceProvider.GetService(typeof(IGraphicsDeviceService))).GraphicsDevice;
            if (gd == null)
                throw new Exception("GraphicsDevice must be loaded before loading content");

            Font12 = cm.Load<SpriteFont>("font12");
            Font16 = cm.Load<SpriteFont>("font16");
            Font20 = cm.Load<SpriteFont>("font20");
            Font24 = cm.Load<SpriteFont>("font24");
            BlankTexture = new Texture2D(gd, 1, 1);
            BlankTexture.SetData(new[] {Color.White.PackedValue});

            TommySheet = new SpriteSheet(cm.Load<Texture2D>("tommy"), 1, 2);
        }
    }
}
