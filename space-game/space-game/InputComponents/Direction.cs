#region File Information
/*
 * Space Game (it's a working title)
 *  Copyright (C) 2012 Big Fat Soda Company
 *
 * Direction.cs - Static input direction worker class
 * A helper class to help work with 8-way direction stored in a Button enum.
 */
#endregion

using Microsoft.Xna.Framework.Input;

namespace SpaceGame.InputComponents
{
    static class Direction
    {
        #region Fields
        // Helper bit masks for directions defined with the Buttons flags enum.
        public const Buttons None = 0;
        public const Buttons Up = Buttons.DPadUp | Buttons.LeftThumbstickUp;
        public const Buttons Down = Buttons.DPadDown | Buttons.LeftThumbstickDown;
        public const Buttons Left = Buttons.DPadLeft | Buttons.LeftThumbstickLeft;
        public const Buttons Right = Buttons.DPadRight | Buttons.LeftThumbstickRight;
        public const Buttons UpLeft = Up | Left;
        public const Buttons UpRight = Up | Right;
        public const Buttons DownLeft = Down | Left;
        public const Buttons DownRight = Down | Right;
        public const Buttons Any = Up | Down | Left | Right;
        #endregion

        /// <summary>
        /// Gets the current direction from a game pad and keyboard.
        /// </summary>
        public static Buttons FromInput(GamePadState gamePad, KeyboardState keyboard)
        {
            Buttons direction = None;

            // Get vertical direction.
            if (gamePad.IsButtonDown(Buttons.DPadUp) ||
                gamePad.IsButtonDown(Buttons.LeftThumbstickUp) ||
                keyboard.IsKeyDown(Keys.Up))
            {
                direction |= Up;
            }
            else if (gamePad.IsButtonDown(Buttons.DPadDown) ||
                gamePad.IsButtonDown(Buttons.LeftThumbstickDown) ||
                keyboard.IsKeyDown(Keys.Down))
            {
                direction |= Down;
            }

            // Comebine with horizontal direction.
            if (gamePad.IsButtonDown(Buttons.DPadLeft) ||
                gamePad.IsButtonDown(Buttons.LeftThumbstickLeft) ||
                keyboard.IsKeyDown(Keys.Left))
            {
                direction |= Left;
            }
            else if (gamePad.IsButtonDown(Buttons.DPadRight) ||
                gamePad.IsButtonDown(Buttons.LeftThumbstickRight) ||
                keyboard.IsKeyDown(Keys.Right))
            {
                direction |= Right;
            }

            return direction;
        }

        /// <summary>
        /// Gets the direction without non-direction buttons from a set of Buttons flags.
        /// </summary>
        public static Buttons FromButtons(Buttons buttons)
        {
            // Extract the direction from a full set of buttons using a bit mask.
            return buttons & Any;
        }
    }
}
