using System;
using System.Configuration;
using System.IO;
using System.Linq;
using Dropbox.Api;
using Dropbox.Api.Files;
using System.Threading.Tasks;

namespace BenchmarkingCloudStorage
{
    class Dropbox : IClouds
    {
        private DropboxClient _service;

        public void StartService()
        {
            var accessToken = ConfigurationManager.AppSettings["DropboxAccessToken"];
            _service = new DropboxClient(accessToken);
        }

        public async Task DeleteFiles()
        {
            var list = await _service.Files.ListFolderAsync(string.Empty);

            foreach (var item in list.Entries.Where(i => i.IsFile))
            {
                await _service.Files.DeleteAsync(item.PathLower);
            }
        }

        public async Task UploadFile(Stream stream, string filename)
        {
            // this method can be used for files up to 150 MB only!
            await _service.Files.UploadAsync('/' + filename, WriteMode.Add.Instance, body: stream);
            
            Console.WriteLine("Done uploading");
        }

        public async Task DownloadFiles()
        {
            var list = await _service.Files.ListFolderAsync(string.Empty);

            foreach (var item in list.Entries.Where(i => i.IsFile))
            {
                var download = await _service.Files.DownloadAsync('/' + item.Name);
                var stream = await download.GetContentAsByteArrayAsync();

                File.WriteAllBytes(item.Name, stream);
                Console.WriteLine("Done downloading");
            }
        }

        public async Task ListFiles()
        {
            var list = await _service.Files.ListFolderAsync(string.Empty);

            // show folders then files
            foreach (var item in list.Entries.Where(i => i.IsFolder))
            {
                Console.WriteLine("D  {0}/", item.Name);
            }

            foreach (var item in list.Entries.Where(i => i.IsFile))
            {
                Console.WriteLine("F{0,8} {1}", item.AsFile.Size, item.Name);
            }
        }

        public string GetName()
        {
            return "Dropbox";
        }
    }
}
