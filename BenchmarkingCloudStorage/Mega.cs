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

        public Task StartService()
        {
            _service = new MegaApiClient();
            var username = ConfigurationManager.AppSettings["MegaUsername"];
            var password = ConfigurationManager.AppSettings["MegaPassword"];
            _service.Login(username, password);

            return null;
        }

        public void UploadFile(Stream stream, string filepath)
        {
            var nodes = _service.GetNodes();
            INode root = nodes.Single(s => s.Type == NodeType.Root);

            try
            {
                _service.Upload(stream, filepath, root);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public Task ListFiles()
        {
            var nodes = _service.GetNodes();
            foreach (var node in nodes)
            {
                Console.WriteLine("{0}, {1}", node.Name, node.Size);
            }

            if (!nodes.Any())
                Console.WriteLine("No files found.");

            Console.ReadLine();
            return null;
        }

        public string GetName()
        {
            return "Mega";
        }
    }
}