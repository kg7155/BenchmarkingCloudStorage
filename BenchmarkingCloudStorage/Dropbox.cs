using System;
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
            _service = new DropboxClient("YWe0VYAez9AAAAAAAAAACGy_E89l7MsgPy9vScQqb4rdkcsVpK9h-250Vbewzhmv");

            return null;
        }

        public void UploadFile(Stream stream, string filepath)
        {
            // this method can be used for files up to 150 MB!
            _service.Files.UploadAsync('/' + filepath, WriteMode.Overwrite.Instance, body: stream);
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
