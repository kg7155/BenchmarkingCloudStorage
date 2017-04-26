using System;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;
using Microsoft.OneDrive.Sdk;
using Microsoft.OneDrive.Sdk.Authentication;

namespace BenchmarkingCloudStorage
{
    class OneDrive : IClouds
    {
        private OneDriveClient _service;

        public async Task StartService()
        {
            var clientId = ConfigurationManager.AppSettings["OneDriveClientId"];
            var clientSecret = ConfigurationManager.AppSettings["OneDriveClientSecret"];
            string[] scopes = new[] {"onedrive.readwrite", "wl.signin", "wl.offline_access"};
            var msaAuthProvider = new MsaAuthenticationProvider(clientId, "", scopes);
            await msaAuthProvider.AuthenticateUserAsync();

            _service = new OneDriveClient(msaAuthProvider);
        }

        public void UploadFile(Stream stream, string filepath)
        {
            _service.Drive.Root.ItemWithPath(filepath).Content.Request().PutAsync<Item>(stream);

        }

        public void ListFiles()
        {
            throw new NotImplementedException();
        }

        public string GetName()
        {
            return "OneDrive";
        }
    }
}
