using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace gmtk_jam
{
    /// <summary>
    /// A simple InputHandler. Add this as a component to your game
    /// to have it update.
    /// </summary>
    public class Input : GameComponent
    {
        #region Actions

        /// <summary>
        /// List of actions in the game
        /// </summary>
        public enum Action
        {
            BreatheIn,
            BreatheOut,
            BreatheUp,
            BreatheDown,
        }

        #endregion

        #region Action Map

        /// <summary>
        /// A combination of gamepad and keyboard keys mapped to a particular action.
        /// </summary>
        public class ActionMap
        {
            /// <summary>
            /// List of GamePad controls to be mapped to a given action.
            /// </summary>
            public List<GamePadButtons> GamePadButtons = new List<GamePadButtons>();

            /// <summary>
            /// List of Keyboard controls to be mapped to a given action.
            /// </summary>
            public List<Keys> KeyboardKeys = new List<Keys>();

        }

        public static bool IsPressed(Action action)
        {
            return IsActionMapPressed(ActionMaps[(int) action]);
        }

        public static bool IsDown(Action action)
        {
            return IsActionMapDown(ActionMaps[(int) action]);
        }

        public static bool IsReleased(Action action)
        {
            return IsActionMapReleased(ActionMaps[(int) action]);
        }

        private static bool IsActionMapPressed(ActionMap map)
        {
            return map.KeyboardKeys.Any(KeyPressed);
        }

        private static bool IsActionMapDown(ActionMap map)
        {
            return map.KeyboardKeys.Any(KeyDown);
        }

        private static bool IsActionMapReleased(ActionMap map)
        {
            return map.KeyboardKeys.Any(KeyReleased);
        }

        private static ActionMap[] ActionMaps { get; set; }

        private static void ResetActionMaps()
        {
            ActionMaps = new ActionMap[Enum.GetValues(typeof(Action)).Length];

            ActionMaps[(int) Action.BreatheIn] = new ActionMap();
            ActionMaps[(int) Action.BreatheIn].KeyboardKeys.Add(Keys.Up);

            ActionMaps[(int) Action.BreatheOut] = new ActionMap();
            ActionMaps[(int) Action.BreatheOut].KeyboardKeys.Add(Keys.Down);

            ActionMaps[(int) Action.BreatheUp] = new ActionMap();
            ActionMaps[(int) Action.BreatheUp].KeyboardKeys.Add(Keys.Right);

            ActionMaps[(int) Action.BreatheDown] = new ActionMap();
            ActionMaps[(int) Action.BreatheDown].KeyboardKeys.Add(Keys.Left);
        }

        #endregion

        #region Constructor

        public Input(Game game) : base(game)
        {
            _keyboardState = Keyboard.GetState();
            _gamePadState = GamePad.GetState(0);
            ResetActionMaps();
        }

        #endregion

        #region Update

        /// <summary>
        /// Updates keyboard and gamepad states
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            _lastKeyboardState = _keyboardState;
            _keyboardState = Keyboard.GetState();

            _lastGamePadState = _gamePadState;
            _gamePadState = GamePad.GetState(PlayerIndex.One);

            base.Update(gameTime);
        }

        #endregion

        #region Keyboard

        private static KeyboardState _keyboardState;
        private static KeyboardState _lastKeyboardState;
        private static GamePadState _gamePadState;
        private static GamePadState _lastGamePadState;

        /// <summary>
        /// Check if the given key is released this frame.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool KeyReleased(Keys key)
        {
            return _keyboardState.IsKeyUp(key) &&
                   _lastKeyboardState.IsKeyDown(key);
        }

        /// <summary>
        /// Check if the given key is pressed this frame.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool KeyPressed(Keys key)
        {
            return _keyboardState.IsKeyDown(key) &&
                   _lastKeyboardState.IsKeyUp(key);
        }

        /// <summary>
        /// Check if the given key is down.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool KeyDown(Keys key)
        {
            return _keyboardState.IsKeyDown(key);
        }

        #endregion
    }
}