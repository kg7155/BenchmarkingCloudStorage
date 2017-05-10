using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
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

        public Task DeleteFiles()
        {
            List<File> result = new List<File>();
            FilesResource.ListRequest listRequest = _service.Files.List();
            listRequest.Fields = "nextPageToken, files(id, name)";
            listRequest.PageSize = 1000;

            do
            {
                try
                {
                    FileList files = listRequest.Execute();
                    result.AddRange(files.Files);
                    listRequest.PageToken = files.NextPageToken;
                }
                catch (Exception e)
                {
                    Console.WriteLine("An error occurred: " + e.Message);
                    listRequest.PageToken = null;
                }
            } while (!String.IsNullOrEmpty(listRequest.PageToken));
            
            foreach (var file in result)
                _service.Files.Delete(file.Id).Execute();
            
            return Task.FromResult(0);
        }

        public Task UploadFile(Stream stream, string filename)
        {
            
            File body = new File
            {
                Name = Path.GetFileName(filename),
                MimeType = "application/unknown"
            };
            
            try
            {
                FilesResource.CreateMediaUpload request = _service.Files.Create(body, stream, body.MimeType);
                // 5 MB
                request.ChunkSize = 20 * Google.Apis.Upload.ResumableUpload.MinimumChunkSize;
                request.Upload();
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: " + e.Message);
            }

            Console.WriteLine("Done uploading");
            return Task.FromResult(0);
        }

        public Task DownloadFiles()
        {
            FilesResource.ListRequest listRequest = _service.Files.List();
            listRequest.PageSize = 10;
            listRequest.Fields = "nextPageToken, files(id, name)";

            IList<File> files = listRequest.Execute().Files;
            
            if (files != null && files.Count > 0)
            {
                foreach (var f in files)
                {
                    var request = _service.Files.Get(f.Id);
                    var stream = new System.IO.MemoryStream();
                    request.Download(stream);
                }
            }
            else
            {
                Console.WriteLine("No files found.");
            }

            Console.WriteLine("Done downloading");
            return Task.FromResult(0);
        }

        public Task ListFiles()
        {
            FilesResource.ListRequest listRequest = _service.Files.List();
            listRequest.PageSize = 10;
            listRequest.Fields = "nextPageToken, files(id, name)";

            IList<File> files = listRequest.Execute().Files;
            
            if (files != null && files.Count > 0)
            {
                foreach (var f in files)
                {
                    Console.WriteLine("{0}", f.Name);
                }
            }
            else
            {
                Console.WriteLine("No files found.");
            }

            return Task.FromResult(0);
        }

        public string GetName()
        {
            return "Google Drive";
        }
    }
}