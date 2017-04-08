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
    class Program
    {
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

        static void Main(string[] args)
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
            var service = new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName
            });

            // File path to upload.
            string filePath = "../../files/Konzultacije.jpg";

            DateTime t1 = DateTime.Now;
            UploadFile(service, filePath);
            TimeSpan t = DateTime.Now - t1;

            Console.WriteLine(t);
            ListFiles(service);
        }

        // List all files on Drive.
        private static void ListFiles(DriveService _service)
        {
            FilesResource.ListRequest listRequest = _service.Files.List();
            listRequest.PageSize = 10;
            listRequest.Fields = "nextPageToken, files(id, name)";
            
            IList<File> files = listRequest.Execute().Files;

            Console.WriteLine("Files:");

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
            Console.Read();
        }

        // Get the mime type of the file. 
        private static string GetMimeType(string fileName)
        {
            string mimeType = "application/unknown";
            string ext = Path.GetExtension(fileName).ToLower();
            RegistryKey regKey = Registry.ClassesRoot.OpenSubKey(ext);

            if (regKey != null && regKey.GetValue("Content Type") != null)
                mimeType = regKey.GetValue("Content Type").ToString();

            return mimeType;
        }
        
        // Upload file to Drive.
        public static File UploadFile(DriveService _service, string _uploadFile)
        {
            if (System.IO.File.Exists(_uploadFile))
            {
                File body = new File();
                body.Name = Path.GetFileName(_uploadFile);
                body.Description = "Test upload";
                body.MimeType = GetMimeType(_uploadFile);
                
                // File's content. 
                byte[] byteArray = System.IO.File.ReadAllBytes(_uploadFile);
                MemoryStream stream = new MemoryStream(byteArray);

                try
                {
                    FilesResource.CreateMediaUpload request = _service.Files.Create(body, stream, GetMimeType(_uploadFile));
                    request.Upload();
                    return request.ResponseBody;
                }
                catch (Exception e)
                {
                    Console.WriteLine("An error occurred: " + e.Message);
                    return null;
                }
            }
            Console.WriteLine("File does not exist: " + _uploadFile);
            return null;
        }     
    }
}