using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpaceGame
{
    /// <summary>
    /// GameOptions - serialisable in Windows
    /// </summary>
    public class GameOptions
    {
        // The game name
        public const string GameName = "SpaceGame";

        // Initial screen width
        public int ScreenWidth = 1280;

        // Initial screen height
        public int ScreenHeight = 720;

        // Sync to vertical retrace
        public bool vsync = true;

        // FullScreen?
        public bool fullscreen = false;

        // FSAA
        public bool antialiasing = true;
    }
}
