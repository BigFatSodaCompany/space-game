#region File Information
/*
 * Space Game (it's a working title)
 *  Copyright (C) 2012 Big Fat Soda Company
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
        #endregion

        #region Initialisation
        public SpaceGame()
        {
            // Initialise the graphics manager and content root directory
            _gdm = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            _gsc = new GamerServicesComponent(this);
            this.Components.Add(_gsc);

            _gm = new GameManager(this);
            _fm = new FileManager(this);
            this.Components.Add(_fm);
        }

        protected override void Initialize()
        {
            base.Initialize();
#if WINDOWS
            Guide.ShowSignIn(1, false);
#endif
        }

        protected override void EndRun()
        {
            _fm.SaveOptions();
            base.EndRun();
        }
        #endregion

        #region Content
        protected override void LoadContent()
        {
            // Content managers load here
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
