#region File Information
/*
 * Space Game (it's a working title)
 *  Copyright (C) 2012 Big Fat Soda Company
 */
#endregion

#region Imports
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Storage;
using System.Xml;
using System.Xml.Serialization;
#endregion

namespace SpaceGame.Managers
{
    public class FileManager : GameComponent
    {
        #region Fields and attributes
        // Storage Device per player
        private Dictionary<PlayerIndex,StorageDevice> _sd =
                new Dictionary<PlayerIndex, StorageDevice>();

        // First signed in player, for options purposes
        private PlayerIndex firstSignedIn;
        private bool firstSignIn = true;

        // Container for the options and save game
        private string containerName = null;

        // The game options file name
        private const string optionsFilename = "options.xml";

        // The Game Options
        private GameOptions _go = null;
        public GameOptions Options
        {
            get
            {
                return _go;
            }
        }
        #endregion

        #region Initialisation
        public FileManager(Game game) : base(game)
        {
            this.Enabled = false;
            containerName = GameOptions.GameName;
        }

        public override void Initialize()
        {
            base.Initialize();
            SignedInGamer.SignedIn += OpenStorageForPlayer;
        }
        #endregion

        #region Storage device initialisation
        public void OpenStorageForPlayer(object sender, SignedInEventArgs siea)
        {
            PlayerIndex player = siea.Gamer.PlayerIndex;

            try
            {
                // Reset the device
                _sd[player] = null;

                StorageDevice.BeginShowSelector(player, this.StorageOpened,
                        (Object)player);
            }
            catch (GuideAlreadyVisibleException gave)
            {
                Debug.WriteLine("Guide was already visible", "FileManager");
                Debug.WriteLine(gave.Message, "FileManager");
                Debug.WriteLine(gave.StackTrace, "FileManager");
            }
        }

        /// <summary>
        /// Callback for storage opening thing
        /// </summary>
        /// <param name="result"></param>
        private void StorageOpened(IAsyncResult result)
        {
            PlayerIndex player = (PlayerIndex)result.AsyncState;
            _sd[player] = StorageDevice.EndShowSelector(result);

            // If we're the first 
            if (firstSignIn)
            {
                firstSignIn = false;
                LoadOptions(player);
            }
        }
        #endregion

        #region Load/Save options
        public void LoadOptions(PlayerIndex player)
        {
#if WINDOWS
            // Options ALWAYS load from the first person who signed in
            IAsyncResult result = _sd[player].BeginOpenContainer(
                    containerName, null, null);
            result.AsyncWaitHandle.WaitOne();

            StorageContainer container = _sd[player].EndOpenContainer(result);
            result.AsyncWaitHandle.Close();

            if (!container.FileExists(optionsFilename))
            {
                container.Dispose();
                _go = new GameOptions();
                return;
            }

            Stream stream = container.OpenFile(optionsFilename, FileMode.Open);
            XmlReader reader = new XmlTextReader(stream);
            XmlSerializer serialiser = new XmlSerializer(typeof(GameOptions));
            if (serialiser.CanDeserialize(reader))
            {
                _go = (GameOptions)serialiser.Deserialize(reader);
            }
            else
            {
                _go = new GameOptions();
            }

            stream.Close();
            container.Dispose();
#else // XBOX
            _go = new GameOptions();
#endif
        }

        public void SaveOptions()
        {
            // No options, nowt to write
            if (_go == null)
                return;

            // Options ALWAYS save to the first person who signed in
            IAsyncResult result = _sd[firstSignedIn].BeginOpenContainer(
                    containerName, null, null);
            result.AsyncWaitHandle.WaitOne();

            StorageContainer container = _sd[firstSignedIn].
                    EndOpenContainer(result);
            result.AsyncWaitHandle.Close();

            if (container.FileExists(optionsFilename))
            {
                container.DeleteFile(optionsFilename);
            }

            Stream stream = container.CreateFile(optionsFilename);
            XmlSerializer serialiser = new XmlSerializer(typeof(GameOptions));
            serialiser.Serialize(stream, _go);
            stream.Close();
            container.Dispose();
        }
        #endregion
    }
}
