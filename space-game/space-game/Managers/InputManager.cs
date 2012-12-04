#region File Information
/*
 * Space Game (it's a working title)
 *  Copyright (C) 2012 Big Fat Soda Company
 *
 * InputManager.cs - Manages input.
 * Here is where all the input related magic happens. 'Nuff said.
 */
#endregion

#region Imports
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using SpaceGame.InputComponents;
#endregion

namespace SpaceGame.Managers
{
    public class InputManager
    {
        #region Fields & properties
        // The player to whom this IM corresponds
        public PlayerIndex PlayerIndex { get; private set; }

        // The Game pad and keyboard states
        public GamePadState GamePadState { get; private set; }
        public KeyboardState KeyboardState { get; private set; }

        /// <summary>
        /// The last 'real time' that new input was received. Slightly late
        /// button presses will not update this time; they are merged with the
        /// previous input.
        /// </summary>
        public TimeSpan LastInputTime { get; private set; }

        /// <summary>
        /// The current sequence of pressed buttons
        /// </summary>
        public List<Buttons> Buffer;

        /// <summary>
        /// How long to wait until the data expires.
        /// </summary>
        public readonly TimeSpan BufferTimeOut =
                TimeSpan.FromMilliseconds(500);

        /// <summary>
        /// The size of the merge window for combining presses that happen
        /// almost simultaneously.
        /// </summary>
        public readonly TimeSpan MergeWindow =
                TimeSpan.FromMilliseconds(100);

        internal static readonly Dictionary<Buttons, Keys>
                NonDirectionButtons = new Dictionary<Buttons, Keys>
                {
                    { Buttons.A, Keys.A },
                    { Buttons.B, Keys.B },
                    { Buttons.X, Keys.X },
                    { Buttons.Y, Keys.Y },
                    // Can also map other non-directional buttons
                };
        #endregion

        #region Initialisation
        public InputManager(PlayerIndex playerIndex, int bufferSize)
        {
            PlayerIndex = playerIndex;
            Buffer = new List<Buttons>(bufferSize);
        }
        #endregion

        #region Update
        public void Update(GameTime gameTime)
        {
            // Get latest peripheral state
            GamePadState lastGamePadState = GamePadState;
            KeyboardState lastKeyboardState = KeyboardState;
            GamePadState = GamePad.GetState(PlayerIndex);
#if WINDOWS
            if (PlayerIndex == PlayerIndex.One)
                KeyboardState = Keyboard.GetState(PlayerIndex);
#endif

            // Expire old input
            TimeSpan time = gameTime.TotalGameTime;
            TimeSpan timeSinceLast = time - LastInputTime;
            if (timeSinceLast > BufferTimeOut)
                Buffer.Clear();

            // Get all non-direction buttons
            Buttons buttons = 0;
            foreach (var buttonAndKey in NonDirectionButtons)
            {
                Buttons button = buttonAndKey.Key;
                Keys key = buttonAndKey.Value;

                // Check the gamepad and keyboard for presses
                if ((lastGamePadState.IsButtonUp(button) &&
                        GamePadState.IsButtonDown(button)) ||
                        (lastKeyboardState.IsKeyUp(key) &&
                        KeyboardState.IsKeyDown(key)))
                {
                    // Use a bitwise-or to accumulate button presses
                    buttons |= button;
                }
            }

            // For two buttons pressed close together, consider them pressed
            // simultaneously
            bool mergeInput = (Buffer.Count > 0 &&
                    timeSinceLast < MergeWindow);

            // If there is a new direction
            var direction = Direction.FromInput(GamePadState, KeyboardState);
            if (Direction.FromInput(lastGamePadState, lastKeyboardState)
                    != direction)
            {
                // combine the direction with the buttons
                buttons |= direction;

                // Don't merge two different directions
                mergeInput = false;
            }

            if (buttons != 0)
            {
                if (mergeInput)
                {
                    Buffer[Buffer.Count - 1] = Buffer[Buffer.Count - 1] |
                            buttons;
                }
                else
                {
                    // append this input to the buffer, expiring the old if
                    // necessary
                    if (Buffer.Count == Buffer.Capacity)
                        Buffer.RemoveAt(0);

                    Buffer.Add(buttons);

                    // Record the time of this input to begin the merge window
                    LastInputTime = time;
                }
            }
        }
        #endregion

        public bool Matches(Move move)
        {
            // If the move is longet than the buffer, it cannot match
            if (Buffer.Count < move.Sequence.Length)
                return false;

            // Loop backwards to match against the most recent input
            for (int i = 1; i <= move.Sequence.Length; ++i)
            {
                if (Buffer[Buffer.Count - i] !=
                        move.Sequence[move.Sequence.Length - i])
                    return false;
            }

            if (!move.IsSubMove)
                Buffer.Clear();

            return true;
        }
    }
}
