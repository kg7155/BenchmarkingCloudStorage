using System;
using System.Linq;
using CG.Web.MegaApiClient;

namespace BenchmarkingCloudStorage
{
    class Mega : IClouds
    {
        private MegaApiClient _service;

        public void StartService()
        {
            _service = new MegaApiClient();
            _service.Login("project.zzrs@gmail.com", "mega2017zzrs");
        }

        public void UploadFile(string filePath)
        {
            var nodes = _service.GetNodes();
            INode root = nodes.Single(s => s.Type == NodeType.Root);

            _service.UploadFile(filePath, root);
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
    }
}