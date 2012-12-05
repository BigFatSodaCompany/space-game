﻿#region File Information
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
    /// <summary>
    /// Holds the state of the input devices for all players
    /// </summary>
    private class InputState
    {
        #region Fields & properties
        public GamePadState[] GamePadState { get; private set; }
        public KeyboardState[] KeyboardState { get; private set; }
        public List<Buttons>[] Buffer { get; private set; }

        public GamePadState[] LastGamePadState { get; set; }
        public KeyboardState[] LastKeyboardState { get; set; }

        private int _maxPlayers = 0;

        /// <summary>
        /// The last 'real time' that new input was received. Slightly late
        /// button presses will not update this time; they are merged with the
        /// previous input.
        /// </summary>
        public TimeSpan[] LastInputTime { get; private set; }
        private TimeSpan[] TimeSinceLast { get; set; }

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

        #region Initialise
        public InputState(int players, int bufferSize)
        {
            _maxPlayers = players;
            GamePadState = new GamePadState[players];
            KeyboardState = new KeyboardState[players];
            LastGamePadState = new GamePadState[players];
            LastKeyboardState = new KeyboardState[players];

            LastInputTime = new TimeSpan[players];
            TimeSinceLast = new TimeSpan[players];

            // Key buffers
            Buffer = new List<Buttons>[players];
            for (int i = 0; i < players; i++) {
                Buffer[i] = new List<Buttons>(bufferSize);
            }

            GetInput();
        }
        #endregion

        #region Processing
        public void GetInput()
        {
            for (int i = 0; i < _maxPlayers; i++)
            {
                PlayerIndex p = (PlayerIndex)i;

                LastGamePadState[i] = GamePadState[i];
                LastKeyboardState[i] = KeyboardState[i];
                GamePadState[i] = GamePad.GetState(p);
                KeyboardState[i] = Keyboard.GetState(p);
            }
        }

        public void ExpireInput(GameTime gameTime)
        {
            TimeSpan time = gameTime.TotalGameTime;

            for (int i = 0; i < _maxPlayers; i++)
            {
                TimeSinceLast[i] = time - LastInputTime[i];
                if (TimeSinceLast[i] > BufferTimeOut)
                    Buffer[i].Clear();
            }
        }

        public void UpdateBuffer(GameTime gameTime)
        {
            for (int i = 0; i < _maxPlayers; i++)
            {
                Buttons buttons = 0;
                foreach (var buttonAndKey in NonDirectionButtons)
                {
                    Buttons button = buttonAndKey.Key;
                    Keys key = buttonAndKey.Value;

                    if ((LastGamePadState[i].IsButtonUp(button) &&
                            GamePadState[i].IsButtonDown(button)) ||
                            (LastKeyboardState[i].IsKeyUp(key) &&
                            KeyboardState[i].IsKeyDown(key)))
                    {
                        buttons |= button;
                    }
                }
                // For two buttons pressed close together, consider them
                // pressed simultaneously
                bool mergeInput = (Buffer[i].Count > 0 &&
                        TimeSinceLast[i] < MergeWindow);

                // If there is a new direction
                var direction = Direction.FromInput(GamePadState[i],
                        KeyboardState[i]);
                if (Direction.FromInput(LastGamePadState[i],
                        LastKeyboardState[i]) != direction)
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
                        Buffer[i][Buffer[i].Count - 1] =
                                Buffer[i][Buffer[i].Count - 1] | buttons;
                    }
                    else
                    {
                        // append this input to the buffer, expiring the old
                        // if necessary
                        if (Buffer[i].Count == Buffer[i].Capacity)
                            Buffer[i].RemoveAt(0);

                        Buffer[i].Add(buttons);

                        // Record the time of this input to begin the merge
                        // window
                        LastInputTime[i] = gameTime.TotalGameTime;
                    }
                }
            }
        }
        #endregion

        #region Utility
        public bool Matches(PlayerIndex playerIndex, Move move)
        {
            int p = (int)playerIndex;
            List<Buttons> buf = Buffer[p];

            // If the move is longer than the buffer, it cannot match
            if (buf.Count < move.Sequence.Length)
                return false;

            // Loop backwards to match against the most recent input
            for (int i = 1; i <= move.Sequence.Length; ++i)
            {
                if (buf[buf.Count - i] != move.Sequence[move.Sequence.Length - i])
                    return false;
            }

            if (!move.IsSubMove)
                Buffer[p].Clear();

            return true;
        }
        #endregion
    }

    public class InputManager
    {
        #region Fields & properties
        // The Game pad and keyboard states
        private InputState InputState { get; set; }
        #endregion

        #region Initialisation
        public InputManager(int players, int bufferSize)
        {
            InputState = new InputState(players, bufferSize);
        }
        #endregion

        #region Update
        public void GetInput()
        {
            InputState.GetInput();
        }

        public void Update(GameTime gameTime)
        {
            InputState.ExpireInput(gameTime);
            InputState.UpdateBuffer(gameTime);
        }
        #endregion

        #region Utility
        #region Generic key down/pressed
        public bool IsKeyDown(int player, Keys key)
        {
            return InputState.KeyboardState[player].IsKeyDown(key);
        }

        public bool IsKeyPressed(int player, Keys key)
        {
            return InputState.KeyboardState[player].IsKeyDown(key) &&
                InputState.LastKeyboardState[player].IsKeyUp(key);
        }
        #endregion

        #region Stick Vectors, Presses, and Cardinal directions
        public Vector2 LeftStick(int player)
        {
            return InputState.GamePadState[player].ThumbSticks.Left;
        }

        public bool IsLeftStickPressed(int player)
        {
            return InputState.GamePadState[player].Buttons.LeftStick ==
                ButtonState.Pressed &&
                InputState.LastGamePadState[player].Buttons.LeftStick ==
                ButtonState.Released;
        }

        public bool IsLeftStickUp(int player)
        {
            return InputState.GamePadState[player].ThumbSticks.Left.Y > 0.5f &&
                InputState.LastGamePadState[player].ThumbSticks.Left.Y <= 0.5f;
        }

        public bool IsLeftStickDown(int player)
        {
            return InputState.GamePadState[player].ThumbSticks.Left.Y > -0.5f &&
                InputState.LastGamePadState[player].ThumbSticks.Left.Y <= -0.5f;
        }

        public bool IsLeftStickLeft(int player)
        {
            return InputState.GamePadState[player].ThumbSticks.Left.X > -0.5f &&
                InputState.LastGamePadState[player].ThumbSticks.Left.X <= -0.5f;
        }

        public bool IsLeftStickRight(int player)
        {
            return InputState.GamePadState[player].ThumbSticks.Left.X > 0.5f &&
                InputState.LastGamePadState[player].ThumbSticks.Left.X <= 0.5f;
        }

        public Vector2 RightStick(int player)
        {
            return InputState.GamePadState[player].ThumbSticks.Right;
        }

        public bool IsRightStickPressed(int player)
        {
            return InputState.GamePadState[player].Buttons.RightStick ==
                ButtonState.Pressed &&
                InputState.LastGamePadState[player].Buttons.RightStick ==
                ButtonState.Released;
        }

        public bool IsRightStickUp(int player)
        {
            return InputState.GamePadState[player].ThumbSticks.Right.Y > 0.5f &&
                InputState.LastGamePadState[player].ThumbSticks.Right.Y <= 0.5f;
        }

        public bool IsRightStickDown(int player)
        {
            return InputState.GamePadState[player].ThumbSticks.Right.Y > -0.5f &&
                InputState.LastGamePadState[player].ThumbSticks.Right.Y <= -0.5f;
        }

        public bool IsRightStickRight(int player)
        {
            return InputState.GamePadState[player].ThumbSticks.Right.X > -0.5f &&
                InputState.LastGamePadState[player].ThumbSticks.Right.X <= -0.5f;
        }

        public bool IsRightStickRight(int player)
        {
            return InputState.GamePadState[player].ThumbSticks.Right.X > 0.5f &&
                InputState.LastGamePadState[player].ThumbSticks.Right.X <= 0.5f;
        }
        #endregion

        #region Trigger pressed/pressure
        public bool IsLeftTriggerPressed(int player)
        {
            return InputState.GamePadState[player].Triggers.Left > 0 &&
                InputState.LastGamePadState[player].Triggers.Left == 0;
        }

        public float LeftTrigger(int player)
        {
            return InputState.GamePadState[player].Triggers.Left;
        }

        public bool IsRightTriggerPressed(int player)
        {
            return InputState.GamePadState[player].Triggers.Right > 0 &&
                InputState.LastGamePadState[player].Triggers.Right == 0;
        }

        public float RightTrigger(int player)
        {
            return InputState.GamePadState[player].Triggers.Right;
        }
        #endregion

        #region Back & start buttons
        public bool IsBackButtonPressed(int player)
        {
            return InputState.GamePadState[player].Buttons.Back ==
                ButtonState.Pressed &&
                InputState.LastGamePadState[player].Buttons.Back ==
                ButtonState.Released;
        }

        public bool IsStartButtonPressed(int player)
        {
            return InputState.GamePadState[player].Buttons.Start ==
                ButtonState.Pressed &&
                InputState.LastGamePadState[player].Buttons.Start ==
                ButtonState.Released;
        }
        #endregion

        #region D-pad
        public bool IsDPadLeftPressed(int player)
        {
            return InputState.GamePadState[player].DPad.Left ==
                ButtonState.Pressed &&
                InputState.LastGamePadState[player].DPad.Left ==
                ButtonState.Released;
        }

        public bool IsDPadRightPressed(int player)
        {
            return InputState.GamePadState[player].DPad.Right ==
                ButtonState.Pressed &&
                InputState.LastGamePadState[player].DPad.Right ==
                ButtonState.Released;
        }

        public bool IsDPadUpPressed(int player)
        {
            return InputState.GamePadState[player].DPad.Up ==
                ButtonState.Pressed &&
                InputState.LastGamePadState[player].DPad.Up ==
                ButtonState.Released;
        }

        public bool IsDPadDownPressed(int player)
        {
            return InputState.GamePadState[player].DPad.Down ==
                ButtonState.Pressed &&
                InputState.LastGamePadState[player].DPad.Down ==
                ButtonState.Released;
        }
        #endregion

        #region Buttons
        public bool IsAButtonPressed(int player)
        {
            return InputState.GamePadState[player].Buttons.A ==
                ButtonState.Pressed &&
                InputState.LastGamePadState[player].Buttons.A ==
                ButtonState.Released;
        }

        public bool IsBButtonPressed(int player)
        {
            return InputState.GamePadState[player].Buttons.B ==
                ButtonState.Pressed &&
                InputState.LastGamePadState[player].Buttons.B ==
                ButtonState.Released;
        }

        public bool IsXButtonPressed(int player)
        {
            return InputState.GamePadState[player].Buttons.X ==
                ButtonState.Pressed &&
                InputState.LastGamePadState[player].Buttons.X ==
                ButtonState.Released;
        }

        public bool IsYButtonPressed(int player)
        {
            return InputState.GamePadState[player].Buttons.Y ==
                ButtonState.Pressed &&
                InputState.LastGamePadState[player].Buttons.Y ==
                ButtonState.Released;
        }
        #endregion

        #region Shoulder buttons
        public bool IsLeftShoulderPressed(int player)
        {
            return InputState.GamePadState[player].Buttons.LeftShoulder ==
                ButtonState.Pressed &&
                InputState.LastGamePadState[player].Buttons.LeftShoulder ==
                ButtonState.Released;
        }

        public bool IsRightShoulderPressed(int player)
        {
            return InputState.GamePadState[player].Buttons.RightShoulder ==
                ButtonState.Pressed &&
                InputState.LastGamePadState[player].Buttons.RightShoulder ==
                ButtonState.Released;
        }
        #endregion

        #endregion
    }
}
