#region File Information
/*
 * Space Game (it's a working title)
 *  Copyright (C) 2012 Big Fat Soda Company
 *
 * GameOptions.cs - Game related options
 * This is for things like difficulty, and, umm..., high scores.
 */
#endregion

#region Imports
using System;
using Microsoft.Xna.Framework.GamerServices;
#endregion

namespace SpaceGame
{
    /// <summary>
    /// GameOptions.
    /// Things like difficulty, default view, key mapping, etc
    /// </summary>
    public class GameOptions
    {
        // Have the options changed?
        private bool _changed = false;
        public bool HasChanged() { return _changed; }
        public void ClearChanged() { _changed = false; }

        // The default difficulty
        private GameDifficulty _difficulty = GameDifficulty.Normal;
        public GameDifficulty Difficulty
        {
            get { return _difficulty; }
            set { _difficulty = value; _changed = true; }
        }
    }
}
