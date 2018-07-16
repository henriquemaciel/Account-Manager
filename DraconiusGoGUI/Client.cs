#region using directives

using Newtonsoft.Json;
using DraconiusGoGUI.Enums;
using DraconiusGoGUI.Exceptions;
using DraconiusGoGUI.Extensions;
using DraconiusGoGUI.DracoManager;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using DracoLib.Core;
using DracoLib.Core.Utils;
using DracoProtos.Core.Base;

#endregion

namespace DraconiusGoGUI
{
    public class Client : IDisposable
    {
        public Version VersionStr;
        public uint AppVersion;
        public bool LoggedIn = false;
        public Manager ClientManager;
        private string RessourcesFolder;
        private DracoClient DracoClient;
        private CancellationTokenSource CancellationTokenSource;

        public Client()
        {
            VersionStr = new Version("0.91.2");
            AppVersion = 9100;
            RessourcesFolder = $"data/{VersionStr.ToString()}/";
        }

        public void Logout()
        {
            if (!LoggedIn)
                return;

            if (ClientManager.AccountState == AccountState.Conecting)
                ClientManager.AccountState = AccountState.Good;

            LoggedIn = false;
            Dispose();
        }

        public async Task<MethodResult<bool>> DoLogin(Manager manager)
        {
            SetSettings(manager);

            var msgStr = "Session couldn't start up.";
            LoggedIn = false;
            CancellationTokenSource = new CancellationTokenSource();
            string authType = AuthType.DEVICE.ToString();

            switch (ClientManager.UserSettings.AuthType)
            {
                case AuthType.GOOGLE:
                    authType = AuthType.GOOGLE.ToString().ToUpper();
                    break;
                case AuthType.DEVICE:
                    authType = AuthType.DEVICE.ToString().ToUpper();
                    break;
                default:
                    throw new ArgumentException("Login provider must be either \"google\" or \"device\".");
            }

            return await Task.Run(() =>
            {

                User config = new User()
                {
                    Username = ClientManager.UserSettings.Username,
                    Password = ClientManager.UserSettings.Password,
                    DeviceId = DracoUtils.GenerateDeviceId(),
                    Login = authType
                };

                Config options = new Config()
                {
                    CheckProtocol = true,
                    EventsCounter = new Dictionary<string, int>(),
                    Lang = "English",
                    TimeOut = 20 * 1000,
                    UtcOffset = (int)TimeZoneInfo.Utc.GetUtcOffset(DateTime.Now).TotalSeconds,
                    Delay = 1000
                };

                string proxy = ClientManager.Proxy;

                DracoClient = new DracoClient(proxy, options);

                ClientManager.LogCaller(new LoggerEventArgs("Ping...", LoggerTypes.Info));
                var ping = DracoClient.Ping();
                if (!ping) throw new Exception();

                ClientManager.LogCaller(new LoggerEventArgs("Boot...", LoggerTypes.Info));
                DracoClient.Boot(config);

                ClientManager.LogCaller(new LoggerEventArgs("Login...", LoggerTypes.Info));
                var login = DracoClient.Login().Result;
                if (login == null) throw new Exception("Unable to login");

                var newLicence = login.info.newLicense;

                if (login.info.sendClientLog)
                {
                    ClientManager.LogCaller(new LoggerEventArgs("Send client log is set to true! Please report.", LoggerTypes.Info));
                }

                DracoClient.Post("https://us.draconiusgo.com/client-error", new
                {
                    appVersion = DracoClient.ClientVersion,
                    deviceInfo = $"platform = iOS\"nos ={ DracoClient.ClientInfo.platformVersion }\"ndevice = iPhone 6S",
                    userId = DracoClient.User.Id,
                    message = "Material doesn\"t have a texture property \"_MainTex\"",
                    stackTrace = "",
                });

                if (newLicence > 0)
                {
                    DracoClient.AcceptLicence(newLicence);
                }

                ClientManager.LogCaller(new LoggerEventArgs("Init client...", LoggerTypes.Info));
                DracoClient.Load();


                ClientManager.LogCaller(new LoggerEventArgs("Succefully added all events to the client.", LoggerTypes.Debug));

                msgStr = "Conected to server...";
                LoggedIn = true;

                return new MethodResult<bool>
                {
                    Success = true,
                    Message = msgStr
                };
            });
        }
   
 
        private void SessionMapUpdate(object sender, EventArgs e)
        {
            //Map Update
        }

 
        private void SessionInventoryUpdate(object sender, EventArgs e)
        {
            //TODO: review needed here            
            //ClientManager.UpdateInventory(InventoryRefresh.All); // <- this line should be the unique line updating the inventory
        }

        public void SetSettings(Manager manager)
        {
            ClientManager = manager;

            /*Dictionary<string, string> Header = new Dictionary<string, string>()
            {
                {"11.1.0", "CFNetwork/889.3 Darwin/17.2.0"},
                {"11.2.0", "CFNetwork/893.10 Darwin/17.3.0"},
                {"11.2.5", "CFNetwork/893.14.2 Darwin/17.4.0"},
                {"11.3.0", "CFNetwork/897.1 Darwin/17.5.0"}
            };*/
        }


        #region IDisposable Support
        private bool disposedValue = false; // Pour détecter les appels redondants

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: supprimer l'état managé (objets managés).
                    if (CancellationTokenSource != null)
                    {
                        CancellationTokenSource.Dispose();
                        return;
                    }
                    ClientManager.LogCaller(new LoggerEventArgs("Session closed!", LoggerTypes.FatalError));
                }

                // TODO: libérer les ressources non managées (objets non managés) et remplacer un finaliseur ci-dessous.
                // TODO: définir les champs de grande taille avec la valeur Null.

                disposedValue = true;
            }
        }

        // TODO: remplacer un finaliseur seulement si la fonction Dispose(bool disposing) ci-dessus a du code pour libérer les ressources non managées.
        // ~Client() {
        //   // Ne modifiez pas ce code. Placez le code de nettoyage dans Dispose(bool disposing) ci-dessus.
        //   Dispose(false);
        // }

        // Ce code est ajouté pour implémenter correctement le modèle supprimable.
        public void Dispose()
        {
            // Ne modifiez pas ce code. Placez le code de nettoyage dans Dispose(bool disposing) ci-dessus.
            Dispose(true);
            // TODO: supprimer les marques de commentaire pour la ligne suivante si le finaliseur est remplacé ci-dessus.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
