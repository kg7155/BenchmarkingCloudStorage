using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Microsoft.Win32;
using File = Google.Apis.Drive.v3.Data.File;

namespace BenchmarkingCloudStorage
{
    class GoogleDrive : IClouds
    {
        private DriveService _service;
        
        // If you modify these scopes, delete your previously saved credentials at ~/.credentials/drive-dotnet-quickstart.json
        // Or it won't work :)
        static string[] Scopes = { DriveService.Scope.Drive,
                           DriveService.Scope.DriveAppdata,
                           DriveService.Scope.DriveFile,
                           DriveService.Scope.DrivePhotosReadonly,
                           DriveService.Scope.DriveMetadataReadonly,
                           DriveService.Scope.DriveReadonly,
                           DriveService.Scope.DriveScripts };
        static string ApplicationName = "Benchmarking Cloud Storage";

        public void StartService()
        {
            // Create user credentials.
            UserCredential credential;

            using (var stream = new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                credPath = Path.Combine(credPath, ".credentials/drive-dotnet-quickstart.json");

                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                Console.WriteLine("Credential file saved to: " + credPath);
            }

            // Create Drive API service.
            _service = new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName
            });
        }
        
        public void UploadFile(Stream stream, string filepath)
        {
            File body = new File
            {
                Name = Path.GetFileName(filepath),
                MimeType = "application/unknown"
            };
            
            try
            {
                FilesResource.CreateMediaUpload request = _service.Files.Create(body, stream, body.MimeType);
                request.Upload();
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: " + e.Message);
            }
        }

        public void ListFiles()
        {
            FilesResource.ListRequest listRequest = _service.Files.List();
            listRequest.PageSize = 10;
            listRequest.Fields = "nextPageToken, files(id, name)";

            IList<File> files = listRequest.Execute().Files;

            Console.WriteLine("All files:");

            if (files != null && files.Count > 0)
            {
                foreach (var f in files)
                {
                    Console.WriteLine("{0} ({1})", f.Name, f.Id);
                }
            }
            else
            {
                Console.WriteLine("No files found.");
            }
            Console.ReadLine();
        }

        public string GetName()
        {
            return "Google Drive";
        }
    }
}