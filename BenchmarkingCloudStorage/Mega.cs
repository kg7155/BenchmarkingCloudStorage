using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CG.Web.MegaApiClient;

namespace BenchmarkingCloudStorage
{
    class Mega : IClouds
    {
        private MegaApiClient _service;

        public void StartService()
        {
            _service = new MegaApiClient();
            var username = ConfigurationManager.AppSettings["MegaUsername"];
            var password = ConfigurationManager.AppSettings["MegaPassword"];
            _service.Login(username, password);
        }

        public Task DeleteFiles()
        {
            var nodes = _service.GetNodes();
            
            foreach (var node in nodes)
            {
                if (node.Type == NodeType.File)
                    _service.Delete(node, moveToTrash: false);
            }
            return Task.FromResult(0);
        }

        public Task UploadFile(Stream stream, string filename)
        {
            var nodes = _service.GetNodes();
            INode root = nodes.Single(s => s.Type == NodeType.Root);

            try
            {
                _service.ChunksPackSize = 384;
                _service.Upload(stream, filename, root);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            Console.WriteLine("Done uploading");
            return Task.FromResult(0);
        }

        public Task DownloadFiles()
        {
            var nodes = _service.GetNodes();
            
            foreach (var node in nodes)
            {
                if (node.Type == NodeType.File)
                    _service.DownloadFile(node, node.Name);
            }

            Console.WriteLine("Done downloading");
            return Task.FromResult(0);
        }

        public Task ListFiles()
        {
            var nodes = _service.GetNodes();
            foreach (var node in nodes)
            {
                if (node.Type == NodeType.File)
                    Console.WriteLine("{0}, {1}", node.Name, node.Size);
            }

            if (!nodes.Any())
                Console.WriteLine("No files found.");

            return Task.FromResult(0);
        }

        public string GetName()
        {
            return "Mega";
        }
    }
}