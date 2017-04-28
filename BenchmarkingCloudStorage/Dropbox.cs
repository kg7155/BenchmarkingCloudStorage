using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dropbox.Api;
using Dropbox.Api.Files;

namespace BenchmarkingCloudStorage
{
    class Dropbox : IClouds
    {
        private DropboxClient _service;

        public Task StartService()
        {
            var accessToken = ConfigurationManager.AppSettings["DropboxAccessToken"];
            _service = new DropboxClient(accessToken);

            return null;
        }

        public async Task DeleteFiles()
        {
            var list = await _service.Files.ListFolderAsync(string.Empty);

            foreach (var item in list.Entries.Where(i => i.IsFile))
            {
                await _service.Files.DeleteAsync("/" + item.Name);
            }
        }

        public void UploadFile(Stream stream, string filepath)
        {
            // this method can be used for files up to 150 MB only!
            _service.Files.UploadAsync('/' + filepath, WriteMode.Add.Instance, body: stream);
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
