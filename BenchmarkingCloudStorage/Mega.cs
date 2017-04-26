using System;
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
            _service.Login("project.zzrs@gmail.com", "mega2017zzrs");

            return null;
        }

        public void UploadFile(Stream stream, string filepath)
        {
            var nodes = _service.GetNodes();
            INode root = nodes.Single(s => s.Type == NodeType.Root);

            _service.Upload(stream, filepath, root);
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