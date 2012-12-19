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

#region Imports
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SpaceGame.Screens;
#endregion

namespace SpaceGame.Managers
{
    public class ScreenManager : IDisposable
    {
        enum TransitionType
        {
            TransitionDone = 0,
            FadeIn,
            FadeOut,
        };

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

        // Transition type
        private TransitionType transitionType = TransitionType.TransitionDone;

        // Fade in/out time
        private float fadeInTime = 0.0f;
        private float fadeOutTime = 0.0f;

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

        // Time for background animation used 7on menus
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
            _screens.Add(new Intro(this, game));
            _screens.Add(new Demo(this, game));

            // Fade to intro screen
            SetNextScreen(ScreenType.ScreenIntro, SystemConfig.fadeColour,
                    SystemConfig.fadeTime);
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
                else if (_im.IsKeyPressed(i, Keys.F1))
                    _fps = !_fps;
            }
        }

        public void Update(float elapsedTime)
        {
            // Fading out
            if (transitionType == TransitionType.FadeOut)
            {
                fade -= elapsedTime;

                // Have we finished that fade?
                if (_next != null && fade < 0)
                {
                    // Tell the next screen that it's gained focus
                    _next.SetFocus(_cm, true);

                    // tell the old screen it lost focus
                    if (_current != null)
                        _current.SetFocus(_cm, false);

                    // set next screen as current
                    _current = _next;
                    _next = null;

                    // begin the fadein
                    transitionType = TransitionType.FadeIn;
                    fade = fadeInTime;
                }
            }
            else if (transitionType == TransitionType.FadeIn)
            {
                fade -= elapsedTime;
                if (fade < 0)
                {
                    transitionType = TransitionType.TransitionDone;
                    fade = 0;
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

                // Draw 2D Scene
                _current.Draw2D(gd, _fm);

            }

            if (transitionType != TransitionType.TransitionDone)
            {
                // Compute transition fade intensity
                float step;

                if (transitionType == TransitionType.FadeOut)
                    step = (fadeOutTime - fade) / fadeOutTime;
                else
                    step = 1.0f - ((fadeInTime - fade) / fadeInTime);

                fadeColour.W = 1.25f * step;

                // Set alph blend and no depth test or write
                gd.DepthStencilState = DepthStencilState.None;
                gd.BlendState = BlendState.AlphaBlend;

                // Draw transition fade colour
                _bm.RenderScreenQuad(gd, BlurTechnique.Color, null,
                        fadeColour);
            }

            // Begin text mode
            _fm.BeginText();

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
            try
            {
                colourRT = new RenderTarget2D(gd, width, height, true,
                        SurfaceFormat.Color, DepthFormat.Depth24);
            }
            catch (NotSupportedException)
            {
                colourRT = new RenderTarget2D(gd, width, height, false,
                        SurfaceFormat.Color, DepthFormat.Depth24);
            }
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

                fadeInTime = _next.FadeIn;
                if (_current != null)
                    fadeOutTime = _current.FadeOut;
                else
                    fadeOutTime = 0.5f;

                if ((fadeInTime + fadeOutTime) > 0)
                {
                    float sum = time / (fadeInTime + fadeOutTime);
                    fadeInTime *= sum;
                    fadeOutTime *= sum;
                }

                fadeColour = colour;
                fade = fadeOutTime;
                transitionType = TransitionType.FadeOut;
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
        public Demo ScreenIntro
        {
            get { return (Demo)_screens[(int)ScreenType.ScreenIntro]; }
        }
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
