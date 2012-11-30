#region File Information
/*
 * Space Game (it's a working title)
 *  Copyright (C) 2012 Big Fat Soda Company
 *
 * FileManager.cs - Manages Configs, Options and Save Games
 * We hook into the Gamer SignIn event, which then determines which storage
 * device to use for that gamer. We also (de-)serialise configs, options,
 * saved games, etc. Reusability FTW!!
 */
#endregion

#region Imports
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Storage;
#endregion

namespace SpaceGame.Managers
{
    public class FileManager : GameComponent
    {
        #region Fields and properties
        // Storage Device per player
        private Dictionary<PlayerIndex, StorageDevice> _sd =
                new Dictionary<PlayerIndex, StorageDevice>();

        // GameOptions will be loaded from the first signed in player's
        // storage area
        private PlayerIndex firstSignedIn;
        private bool firstSignIn = true;

        // Container for the options and save game
        private string containerName = null;

        // Various file names
        private const string configFilename = "config.xml";
        private const string optionsFilename = "options.xml";

        // The system options (serialisable in Windows)
        private SystemConfig _sc = null;
        public SystemConfig Config
        {
            get { return _sc; }
        }

        // The Game Options
        private GameOptions _go = null;
        public GameOptions Options
        {
            get { return _go; }
        }
        #endregion

        #region Initialisation
        public FileManager(Game game)
            : base(game)
        {
            this.Enabled = false;
            containerName = GameOptions.GameName;
        }

        public override void Initialize()
        {
            base.Initialize();
            SignedInGamer.SignedIn += OpenStorageForPlayer;
            SignedInGamer.SignedOut += CloseStorageForPlayer;
        }
        #endregion

        #region Storage device initialisation/finalisation
        /// <summary>
        /// Event handler for initialising storage for a player
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="siea">The SignedInEventArgs</param>
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
        /// Callback for opening storage for a player.
        /// </summary>
        /// <param name="result"></param>
        private void StorageOpened(IAsyncResult result)
        {
            PlayerIndex player = (PlayerIndex)result.AsyncState;
            _sd[player] = StorageDevice.EndShowSelector(result);

            if (firstSignIn)
            {
                firstSignedIn = player;
                firstSignIn = false;
            }
        }

        /// <summary>
        /// Disposes of the StorageDevice
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="soea">The SignedOutEventArgs</param>
        public void CloseStorageForPlayer(object sender, SignedOutEventArgs soea)
        {
            PlayerIndex player = soea.Gamer.PlayerIndex;
            _sd[player] = null;
            _sd.Remove(player);
        }
        #endregion

        #region Load/Save config
        /// <summary>
        /// Load the options file from the disk. (XBox just instantates a
        /// SystemConfig class).
        /// </summary>
        public void LoadConfig()
        {
#if WINDOWS
            try
            {
                Stream stream = File.Open(configFilename, FileMode.Open);
                XmlReader reader = new XmlTextReader(stream);
                XmlSerializer serialiser = new XmlSerializer(
                        typeof(SystemConfig));
                _sc = (SystemConfig)serialiser.Deserialize(reader);
                _sc.ClearChanged();
                reader.Close();
                stream.Close();
            }
            catch (FileNotFoundException)
            {
                _sc = new SystemConfig();
            }
#else // XBOX
            _sc = new SystemConfig();
#endif
        }

        /// <summary>
        /// Save the system options (Windows only) to disk
        /// </summary>
        public void SaveConfig()
        {
#if WINDOWS
            // No options set (which should never happen), then there's
            // nothing to save. Similarly, if the options have't been
            // fiddled with, then don't resave them
            if (_sc == null || !_sc.HasChanged())
                return;

            Stream stream = File.Open(configFilename,
                    FileMode.OpenOrCreate, FileAccess.Write);
            XmlWriter writer = new XmlTextWriter(new StreamWriter(stream));
            XmlSerializer serialiser = new XmlSerializer(
                    typeof(SystemConfig));
            serialiser.Serialize(writer, _sc);
            writer.Close();
            stream.Close();
#endif
        }
        #endregion

        #region Load/save game options
        public void LoadGameOptions()
        {
            PlayerIndex p = firstSignedIn;

            // This should NEVER happen. EVER.
            // If this happens, whomever wrote the code to call this function
            // has downright royally fucked up...
            if (!(_sd.ContainsKey(p)) || _sd[p] == null)
            {
#if DEBUG
                // We may need to do something else here for release
                Debug.WriteLine("TURD BURGLARS!! The player's StorageDevice " +
                        "has been freed >_<");
                Debug.WriteLine("PlayerIndex: {0}", p);
#endif
                _go = new GameOptions();
                return;
            }

            // Options ALWAYS load from the first person who signed in
            IAsyncResult result = _sd[p].BeginOpenContainer(
                    containerName, null, null);

            // Loading options is done outside the context of the game (i.e.
            // after the options have been set) so we can make this a
            // synchronous operation
            result.AsyncWaitHandle.WaitOne();
            StorageContainer container = _sd[p].EndOpenContainer(result);
            result.AsyncWaitHandle.Close();

            // Use defaults if the file's not there...
            if (!container.FileExists(optionsFilename))
            {
                container.Dispose();
                _go = new GameOptions();
                return;
            }

            // Read the file from storage
            Stream stream = container.OpenFile(optionsFilename, FileMode.Open);
            XmlReader reader = new XmlTextReader(stream);
            XmlSerializer serialiser = new XmlSerializer(typeof(GameOptions));
            if (serialiser.CanDeserialize(reader))
            {
                _go = (GameOptions)serialiser.Deserialize(reader);
            }
            else
            {
                // There was a problem with deserialising the file
                _go = new GameOptions();
            }

            stream.Close();
            container.Dispose();
        }

        /// <summary>
        /// Save the game options
        /// </summary>
        public void SaveGameOptions()
        {
            PlayerIndex p = firstSignedIn;

            // No options, nowt to write;
            // No storage open for player, nowt to write.
            if (_go == null || !(_sd.ContainsKey(p)) || _sd[p] == null)
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
