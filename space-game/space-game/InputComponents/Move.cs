#region File Information
/*
 * Space Game (it's a working title)
 *  Copyright (C) 2012 Big Fat Soda Company
 *
 * Move.cs - Sequence of buttons description.
 * This class describes a pre-determined sequence of buttons, which are used
 * to match against the buffer of buttons that the InputManager has
 */
#endregion

using Microsoft.Xna.Framework.Input;

namespace SpaceGame.InputComponents
{
    /// <summary>
    /// Describes a sequences of buttons which must be pressed to active the move.
    /// A real game might add a virtual PerformMove() method to this class.
    /// </summary>
    public class Move
    {
        #region Fields & properties
        public string Name;

        // The sequence of button presses required to activate this move.
        public Buttons[] Sequence;

        // Set this to true if the input used to activate this move may
        // be reused as a component of longer moves.
        public bool IsSubMove;
        #endregion

        #region Initialisation
        public Move(string name, params Buttons[] sequence)
        {
            Name = name;
            Sequence = sequence;
        }
        #endregion
    }
}
