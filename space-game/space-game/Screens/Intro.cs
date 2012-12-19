using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SpaceGame.Managers;

namespace SpaceGame.Screens
{
    public class Intro : Screen
    {
        #region Fields & properties
        private ScreenManager _sm = null;
        private GameManager _gm = null;
        #endregion

        public Intro(ScreenManager sm, GameManager gm)
        {
            _sm = sm;
            _gm = gm;

            FadeOut = 0.0f;
        }

        public override void SetFocus(ContentManager content, bool focus)
        {
        }

        public override void ProcessInput(float elapsedTime, InputManager input)
        {
            for (int i = 0; i < SystemConfig.MaxPlayers; i++)
            {
                if (input.IsKeyPressed(i, Keys.Enter))
                    _sm.SetNextScreen(ScreenType.ScreenDemo);
            }
        }

        public override void Update(float elapsedTime)
        {
        }

        public override void Draw3D(GraphicsDevice gd)
        {
            if (gd == null)
                throw new ArgumentNullException("gd");

            gd.Clear(Color.Black);
        }

        public override void Draw2D(GraphicsDevice gd, FontManager font)
        {
        }
    }
}