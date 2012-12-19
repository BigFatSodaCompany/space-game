using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SpaceGame.Managers;
using Microsoft.Xna.Framework.Input;

namespace SpaceGame.Screens
{
    public class Demo : Screen
    {
        #region Fields & properties
        private ScreenManager _sm = null;
        private GameManager _gm = null;
        #endregion

        public Demo(ScreenManager sm, GameManager gm)
        {
            _sm = sm;
            _gm = gm;

            FadeIn = 0.0f;
            FadeOut = 3.0f;
        }

        public override void SetFocus(ContentManager content, bool focus)
        {
        }

        public override void ProcessInput(float elapsedTime, InputManager input)
        {
            for (int i = 0; i < SystemConfig.MaxPlayers; i++)
            {
                if (input.IsKeyPressed(i, Keys.Escape))
                    _sm.SetNextScreen(ScreenType.ScreenIntro);
            }
        }

        public override void Update(float elapsedTime)
        {
        }

        public override void Draw3D(GraphicsDevice gd)
        {
            if (gd == null)
                throw new ArgumentNullException("gd");

            gd.Clear(Color.SlateBlue);
        }

        public override void Draw2D(GraphicsDevice gd, FontManager font)
        {
        }
    }
}
