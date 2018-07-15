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

            return new MethodResult<bool>();
            /*return await Task.Run(async () =>
            {
                switch (ClientManager.UserSettings.AuthType)
                {
                    case AuthType.GOOGLE:
                        //LoginProvider = new GoogleLoginProvider(ClientManager.UserSettings.Username, ClientManager.UserSettings.Password);
                        break;
                    case AuthType.DEVICE:
                        //LoginProvider = new PtcLoginProvider(ClientManager.UserSettings.Username, ClientManager.UserSettings.Password, ClientManager.UserSettings.Proxy.AsWebProxy());
                        break;
                    default:
                        throw new ArgumentException("Login provider must be either \"google\" or \"ptc\".");
                }

                ClientManager.LogCaller(new LoggerEventArgs("Succefully added all events to the client.", LoggerTypes.Debug));

            });*/
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

            Dictionary<string, string> Header = new Dictionary<string, string>()
            {
                {"11.1.0", "CFNetwork/889.3 Darwin/17.2.0"},
                {"11.2.0", "CFNetwork/893.10 Darwin/17.3.0"},
                {"11.2.5", "CFNetwork/893.14.2 Darwin/17.4.0"},
                {"11.3.0", "CFNetwork/897.1 Darwin/17.5.0"}
            };

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
