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
            return null;
        }

        public Task UploadFile(Stream stream, string filename)
        {
            var nodes = _service.GetNodes();
            INode root = nodes.Single(s => s.Type == NodeType.Root);

            try
            {
                _service.Upload(stream, filename, root);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            return null;
        }

        public void DownloadFiles()
        {
            var nodes = _service.GetNodes();
            
            foreach (var node in nodes)
            {
                if (node.Type == NodeType.File)
                    _service.DownloadFile(node, node.Name);
            }
        }

        public void ListFiles()
        {
            var nodes = _service.GetNodes();
            foreach (var node in nodes)
            {
                Console.WriteLine("{0}, {1}", node.Name, node.Size);
            }

            if (!nodes.Any())
                Console.WriteLine("No files found.");

            Console.ReadLine();
        }

        public string GetName()
        {
            return "Mega";
        }
    }
}