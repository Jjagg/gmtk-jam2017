using System;
using Microsoft.Win32.SafeHandles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using OpenGL;

namespace gmtk_jam
{
    public static class Assets
    {
        public static SpriteFont Font12 { get; private set; }
        public static SpriteFont Font16 { get; private set; }
        public static SpriteFont Font20 { get; private set; }
        public static SpriteFont Font24 { get; private set; }
        public static Texture2D BlankTexture { get; private set; }

        public static int TommySheetSize = 5;
        public static int EyesSheetSize = 5;
        public static int GrassSheetSize = 5;
        public static int CloudsSheetSize = 3;

        public static SpriteSheet TommySheet { get; private set; }
        public static SpriteSheet EyesSheet { get; private set; }
        public static SpriteSheet Eyes2Sheet { get; private set; }
        public static SpriteSheet GrassSheet { get; private set; }
        public static SpriteSheet CloudsSheet { get; private set; }
        public static SpriteSheet ObstacleSheet { get; private set; }

        public static Texture2D Scenery1 { get; private set; }
        public static Texture2D Scenery2 { get; private set; }
        public static Texture2D Scenery3 { get; private set; }

        public static Song Bgm { get; private set; }

        public static SoundEffect BreatheInSfx { get; private set; }
        public static SoundEffect BreatheOutSfx { get; private set; }

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

            TommySheet = new SpriteSheet(cm.Load<Texture2D>("tommy"), 1, TommySheetSize);
            EyesSheet = new SpriteSheet(cm.Load<Texture2D>("eyes"), 1, EyesSheetSize);
            Eyes2Sheet = new SpriteSheet(cm.Load<Texture2D>("eyes2"), 1, 2);
            GrassSheet = new SpriteSheet(cm.Load<Texture2D>("grass"), 1, GrassSheetSize);
            CloudsSheet = new SpriteSheet(cm.Load<Texture2D>("clouds"), CloudsSheetSize, 1);
            ObstacleSheet = new SpriteSheet(cm.Load<Texture2D>("obstacles"), 1, 2);

            Scenery1 = cm.Load<Texture2D>("scenery1");
            Scenery2 = cm.Load<Texture2D>("scenery2");
            Scenery3 = cm.Load<Texture2D>("scenery3");

            Bgm = cm.Load<Song>("bgm");

            BreatheInSfx = cm.Load<SoundEffect>("in");
            BreatheOutSfx = cm.Load<SoundEffect>("out");
        }
    }
}
