#region File Information
/*
 * Space Game (it's a working title)
 *  Copyright (C) 2012 Big Fat Soda Company
 *
 * FontManager.cs - Manages fonts and the ilk.
 * Here we locate all the fonts in the 'fonts/' directory, and stick them
 * in a dictionary with their names.
 */
#endregion

#region Imports
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
#endregion

namespace SpaceGame.Managers
{
    public class FontManager : IDisposable
    {
        #region Fields & properties
        private GraphicsDevice _gd = null;
        private SpriteBatch _sb = null;
        private Dictionary<string, SpriteFont> _fonts = null;
        private bool _textMode = false;

        public Rectangle ScreenRectangle
        {
            get
            {
                return new Rectangle(_gd.Viewport.TitleSafeArea.X,
                        _gd.Viewport.TitleSafeArea.Y,
                        _gd.Viewport.TitleSafeArea.Width,
                        _gd.Viewport.TitleSafeArea.Height);
            }
        }
        #endregion

        #region Initialisation
        public FontManager(GraphicsDevice gd)
        {
            if (gd == null)
                throw new ArgumentNullException("gd");

            _gd = gd;
            _sb = new SpriteBatch(gd);
            _fonts = new Dictionary<string, SpriteFont>();
        }
        #endregion

        #region Content
        public void LoadContent(ContentManager cm)
        {
            string fontDir = Directory.GetCurrentDirectory() + "\\" +
                    cm.RootDirectory + "\\fonts";

            if (!Directory.Exists(fontDir))
                throw new DirectoryNotFoundException(fontDir +
                        " does not exist!");

            string[] fonts = Directory.GetFiles(fontDir);
            if (fonts.Length == 0)
                throw new FileNotFoundException("No files were found in " +
                        fontDir);

            foreach (string font in fonts)
            {
                FileInfo fi = new FileInfo(font);
                string name = fi.Name.Remove(fi.Name.LastIndexOf("."));

                _fonts[name] = cm.Load<SpriteFont>("fonts/" + name);
            }
        }

        public void UnloadContent()
        {
            _fonts.Clear();
        }
        #endregion

        #region Utility
        #region Text
        public void BeginText()
        {
            _sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            _textMode = true;
        }

        public void DrawText(string font, string text, Vector2 pos,
                Color colour)
        {
            if (_textMode)
                _sb.DrawString(_fonts[font], text, pos, colour);
        }

        public void EndText()
        {
            _sb.End();
            _textMode = false;
        }
        #endregion

        #region Textures
        public void DrawTexture(Texture2D texture, Rectangle rect,
                Color colour, BlendState blend)
        {
            if (_textMode)
                _sb.End();

            _sb.Begin(SpriteSortMode.Immediate, blend);
            _sb.Draw(texture, rect, colour);
            _sb.End();

            if (_textMode)
                _sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
        }

        public void DrawTexture(Texture2D texture, Rectangle rect, float rot,
                Color colour, BlendState blend)
        {
            if (_textMode)
                _sb.End();

            rect.X += rect.Width / 2;
            rect.Y += rect.Height / 2;

            _sb.Begin(SpriteSortMode.Immediate, blend);
            _sb.Draw(texture, rect, null, colour, rot,
                    new Vector2(rect.Width / 2, rect.Height / 2),
                    SpriteEffects.None, 0);
            _sb.End();

            if (_textMode)
                _sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
        }

        public void DrawTexture(Texture2D texture, Rectangle destRect,
                Rectangle srcRect, Color colour, BlendState blend)
        {
            if (_textMode)
                _sb.End();

            _sb.Begin(SpriteSortMode.Immediate, blend);
            _sb.Draw(texture, destRect, srcRect, colour);
            _sb.End();

            if (_textMode)
                _sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
        }

        #endregion
        #endregion

        #region IDisposable Members

        bool isDisposed = false;
        public bool IsDisposed
        {
            get { return isDisposed; }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing)
        {
            if (disposing && !isDisposed)
            {
                if (_sb != null)
                {
                    _sb.Dispose();
                    _sb = null;
                }
            }
        }
        #endregion
    }
}
