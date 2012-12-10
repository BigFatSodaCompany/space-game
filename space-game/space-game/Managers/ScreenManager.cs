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
        private ContentManager _cm = null;
        private FontManager _fm = null;
        private BlurManager _bm = null;

        // A list of screens
        private List<Screen> _screens = null;

        // Current and next active screens
        private Screen _current = null;
        private Screen _next = null;

        // Total fade time when in a transition
        private float fadeTime = 1.0f;

        // Current fade time when in a transition
        private float fade = 0.0f;

        // Colour that is fading
        private Vector4 fadeColour = Vector4.One;

        // Render target
        RenderTarget2D colourRT = null;

        // FPS
        private bool _fps = false;
        private float frameRateTime = 0.0f;
        private int frameRate = 0;
        private int frameRateCount = 0;

        // Background texture used on menus
        private Texture2D textureBackground;

        // Time for background animation used on menus
        private float backgroundTime = 0.0f;
        #endregion

        #region Initialisation
        public ScreenManager(SpaceGame spaceGame, FontManager fm,
                GameManager game)
        {
            _sg = spaceGame;
            _gm = game;
            _fm = fm;

            _screens = new List<Screen>();
            _im = new InputManager(SystemConfig.MaxPlayers, 1);

            // Add Screens here

            // Fade to intro screen
            // SetNextScreen(ScreenType.ScreenIntro, GameOptions.FadeColor,
            //      GameOptions.FadeTime);
            fade = fadeTime * 0.5f;
        }
        #endregion

        #region Draw & update
        public void ProcessInput(float elapsedTime)
        {
            _im.GetInput();

            if (_current != null && _next == null)
                _current.ProcessInput(elapsedTime, _im);

            for (int i = 0; i < SystemConfig.MaxPlayers; i++)
            {
                if (_im.IsKeyPressed(i, Keys.F5))
                    _sg.ToggleFullScreen();
                if (_im.IsKeyPressed(i, Keys.F1))
                    _fps = !_fps;
            }
        }

        public void Update(float elapsedTime)
        {
            // if we're transitioning
            if (fade > 0)
            {
                // update transition time
                fade -= elapsedTime;

                // if fadeout has finished
                if (_next != null && fade < 0.5f * fadeTime)
                {
                    // Tell the next screen that it's 
                    _next.SetFocus(_cm, true);

                    // tell the old screen it lost focus
                    if (_current != null)
                        _current.SetFocus(_cm, false);

                    // set next screen as current
                    _current = _next;
                    _next = null;
                }
            }

            // if current screen is available, update it!
            if (_current != null)
                _current.Update(elapsedTime);

            // calculate framerate
            frameRateTime += elapsedTime;
            if (frameRateTime > 0.5f)
            {
                frameRate = (int)((float)frameRateCount / frameRateTime);
                frameRateCount = 0;
                frameRateTime = 0.0f;
            }

            // Accumulate time for background animation
            backgroundTime += elapsedTime;
        }

        private void DrawRenderTargetTexture(GraphicsDevice gd,
                RenderTarget2D renderTarget, float intensity, bool addBlend)
        {
            if (gd == null)
                throw new ArgumentNullException("gd");

            gd.DepthStencilState = DepthStencilState.None;
            if (addBlend)
                gd.BlendState = BlendState.Additive;

            _bm.RenderScreenQuad(gd, BlurTechnique.ColorTexture, renderTarget,
                    new Vector4(intensity));

            gd.DepthStencilState = DepthStencilState.Default;
        }

        public void DrawTexture(Texture2D texture, Rectangle rect, Color colour,
                BlendState blend)
        {
            _fm.DrawTexture(texture, rect, colour, blend);
        }

        public void DrawTexture(Texture2D texture, Rectangle dest, Rectangle src,
                Color colour, BlendState blend)
        {
            _fm.DrawTexture(texture, dest, src, colour, blend);
        }

        public void Draw(GraphicsDevice gd)
        {
            if (gd == null)
                throw new ArgumentNullException("gd");

            frameRateCount++;

            // if a valid screen is set
            if (_current != null)
            {
                gd.SetRenderTarget(colourRT);

                // Draw the 3D scene
                _current.Draw3D(gd);

                // Resolve the render target
                gd.SetRenderTarget(null);

                // Draw the 3D scene texture
                DrawRenderTargetTexture(gd, colourRT, 1.0f, false);

                // Begin text mode
                _fm.BeginText();

                // Draw 2D Scene
                _current.Draw2D(gd, _fm);

                // Draw FPS
                if (_fps)
                {
                    _fm.DrawText("Arial", "FPS: " + frameRate,
                            new Vector2(gd.Viewport.Width - 80, 0),
                            Color.White);
                }

                // end text mode
                _fm.EndText();
            }

            if (fade > 0)
            {
                // Compute transition fade intensity
                float size = fadeTime * 0.5f;
                fadeColour.W = 1.25f * (1.0f - Math.Abs(fade - size) / size);

                // Set alph blend and no depth test or write
                gd.DepthStencilState = DepthStencilState.None;
                gd.BlendState = BlendState.AlphaBlend;

                // Draw transition fade colour
                _bm.RenderScreenQuad(gd, BlurTechnique.Color, null,
                        fadeColour);
            }
        }
        #endregion

        #region Content
        public void LoadContent(GraphicsDevice gd, ContentManager cm)
        {
            if (gd == null)
                throw new ArgumentNullException("gd");

            // Store the content manager
            _cm = cm;
            _bm = new BlurManager(gd, cm.Load<Effect>("shaders/basic"),
                    512, 512);

            // Load background here
            // textureBackground = content.Load<Texture2D>("misc/background");

            int width = gd.Viewport.Width;
            int height = gd.Viewport.Height;

            // create render target
            colourRT = new RenderTarget2D(gd, width, height, true,
                    SurfaceFormat.Color, DepthFormat.Depth24);
        }

        public void UnloadContent()
        {
            textureBackground = null;

            if (_bm != null)
            {
                _bm.Dispose();
                _bm = null;
            }

            if (colourRT != null)
            {
                colourRT.Dispose();
                colourRT = null;
            }
        }
        #endregion

        #region Screen Manipulation
        public bool SetNextScreen(ScreenType st, Vector4 colour, float time)
        {
            if (_next == null)
            {
                _next = _screens[(int)st];
                fadeTime = time;
                fadeColour = colour;
                fade = fadeTime;
                return true;
            }
            return false;
        }

        public bool SetNextScreen(ScreenType st, Vector4 colour)
        {
            return SetNextScreen(st, colour, 1.0f);
        }

        public bool SetNextScreen(ScreenType st)
        {
            return SetNextScreen(st, Vector4.Zero, 1.0f);
        }

        public Screen GetScreen(ScreenType st)
        {
            return _screens[(int)st];
        }

        public void Exit()
        {
            _sg.Exit();
        }

        #region Screen retrieval properties
        // Each type of screen that is used will be accessed from here.
        #endregion
        #endregion

        #region IDisposable Implementation
        private bool isDisposed = false;
        public bool IsDisposed
        {
            get { return isDisposed; }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing && !IsDisposed)
            {
                UnloadContent();
            }
        }
        #endregion
    }
}
