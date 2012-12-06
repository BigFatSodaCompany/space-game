#region File Information
/*
 * Space Game (it's a working title)
 *  Copyright (C) 2012 Big Fat Soda Company
 * 
 * SystemConfig.cs - Graphics configuration.
 * So we can start off in the correct graphics mode, we serialise and
 * deserialise this class at startup. Unless we're running on an XBox, in
 * which case this class isn't serialised nor deserialised.
 */
#endregion
using System;

namespace SpaceGame
{
    /// <summary>
    /// SystemOptions - serialisable in Windows
    /// </summary>
    public class SystemConfig
    {
        // The game name
        public const string GameName = "SpaceGame";
        public const int MaxPlayers = 1;

        // Have any of our options changed?
        private bool _changed = false;
        public bool HasChanged()
        {
            return _changed;
        }

        public void ClearChanged()
        {
            _changed = false;
        }

        // Initial screen width
        private int _width = 1280;
        public int ScreenWidth
        {
            get { return _width; }
            set { _width = value; _changed = true; }
        }

        // Initial screen height
        private int _height = 720;
        public int ScreenHeight
        {
            get { return _height; }
            set { _height = value; _changed = true; }
        }

        // Sync to vertical retrace
        private bool _vsync = true;
        public bool vsync
        {
            get { return _vsync; }
            set { _vsync = value; _changed = true; }
        }

        // FullScreen?
        private bool _fullscreen = false;
        public bool fullscreen
        {
            get { return _fullscreen; }
            set { _fullscreen = value; _changed = true; }
        }

        // FSAA
        private bool _antialiasing = true;
        public bool antialiasing
        {
            get { return _antialiasing; }
            set { _antialiasing = value; _changed = true; }
        }
    }
}
