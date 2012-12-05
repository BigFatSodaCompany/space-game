#region File Information
/*
 * Space Game (it's a working title)
 *  Copyright (C) 2012 Big Fat Soda Company
 *
 * ScreenManager.cs - The, umm, screen manager.
 * This where the logic for levels, menus, general displays of cool, etc.,
 * are managed. We basically dish out updates, do funky transitions, and
 * other awesomeness.
 */
#endregion

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Storage;

namespace SpaceGame.Managers
{
    public class ScreenManager : IDisposable
    {
        #region Fields & properties
        private SpaceGame _sg = null;
        private GameManager _gm = null;
        private InputManager _im = null;

        // A list of screens
        private List<Screen> _screens = null;

        // Current and next active screens
        private Screen _current = null;
        private Screen _next = null;

        // Total fade time when in a transition
        private float fadeTime = 1.0f;

        // Current fade time when in a transition
        private float fade = 0.0f;
        #endregion

        public ScreenManager(SpaceGame spaceGame, GameManager game)
        {
            _sg = spaceGame;
            _gm = game;

            _screens = new List<Screen>();
            _im = new InputManager(SystemConfig.MaxPlayers, 1);

            // Add Screens here

            // Fade to intro screen
            // SetNextScreen(ScreenType.ScreenIntro, GameOptions.FadeColor,
            //      GameOptions.FadeTime);
            fade = fadeTime * 0.5f;
        }

        public void ProcessInput(float elapsedTime)
        {
            _im.GetInput();

            if (_current != null && _next == null)
                _current.ProcessInput(elapsedTime, _im);

            for (int i = 0; i < SystemConfig.MaxPlayers; i++)
            {
                if (_im.IsKeyPressed(i, Keys.F5))
                    _sg.ToggleFullScreen();
            }
        }

        #region IDisposable Members
        public void Dispose()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
