﻿#region File Information
/*
 * Space Game (it's a working title)
 *  Copyright (C) 2012 Big Fat Soda Company
 *
 * SpaceGame.cs - Main entry point and Game subclass.
 * Here we initialise all the essential subsystems, and delegate all the hard
 * work to the GameManager component.
 */
#endregion

#region Imports
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
using SpaceGame.Managers;
#endregion

namespace SpaceGame
{
    public class SpaceGame : Microsoft.Xna.Framework.Game
    {
        #region Static Members
        // Singleton instance and accessor
        static SpaceGame _instance;
        static public SpaceGame Instance
        {
            get { return _instance; }
        }
        #endregion

        #region Fields and attributes
        private GraphicsDeviceManager _gdm = null;
        private GamerServicesComponent _gsc = null;
        private FileManager _fm = null;
        private GameManager _gm = null;
        private FontManager _font = null;
        #endregion

        #region Initialisation
        public SpaceGame()
        {
            // Initialise the graphics manager and content root directory
            _gdm = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            // Initialise Gamer services
            _gsc = new GamerServicesComponent(this);
            this.Components.Add(_gsc);

            // Initialise the filemanager and load graphics config
            _fm = new FileManager(this);
            _fm.LoadConfig();
            this.Components.Add(_fm);

            _gm = new GameManager(this);

            _gdm.PreferredBackBufferWidth = _fm.Config.ScreenWidth;
            _gdm.PreferredBackBufferHeight = _fm.Config.ScreenHeight;
            _gdm.PreferMultiSampling = _fm.Config.antialiasing;
            _gdm.IsFullScreen = _fm.Config.fullscreen;
            _gdm.SynchronizeWithVerticalRetrace = _fm.Config.vsync;
            IsFixedTimeStep = _fm.Config.vsync;
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void EndRun()
        {
            _fm.SaveConfig();
            base.EndRun();
        }
        #endregion

        #region Content
        protected override void LoadContent()
        {
            // Content managers load here
            _font = new FontManager(_gdm.GraphicsDevice);

            _font.LoadContent(Content);
        }

        protected override void UnloadContent()
        {
            // Content managers unload here
        }
        #endregion

        #region Update & draw
        protected override void Update(GameTime gameTime)
        {
            float ElapsedTimeFloat =
                    (float)gameTime.ElapsedGameTime.TotalSeconds;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            _gdm.GraphicsDevice.Clear(Color.Black);
            base.Draw(gameTime);
        }

        public void ToggleFullScreen()
        {
            if (!_gdm.IsFullScreen)
            {
                _gdm.PreferredBackBufferWidth = 3840;
                _gdm.PreferredBackBufferHeight = 2160;
                _gdm.IsFullScreen = true;
                _fm.Config.fullscreen = true;
            }
            else
            {
                _gdm.PreferredBackBufferWidth = _fm.Config.ScreenWidth;
                _gdm.PreferredBackBufferHeight = _fm.Config.ScreenHeight;
                _gdm.IsFullScreen = false;
                _fm.Config.fullscreen = false;
            }

            _gdm.ApplyChanges();
        }
        #endregion

        #region Entry point
        static void Main()
        {
            using (SpaceGame game = new SpaceGame())
            {
                _instance = game;
                game.Run();
            }
        }
        #endregion
    }
}
