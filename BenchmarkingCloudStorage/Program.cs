using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BenchmarkingCloudStorage
{
    public interface IClouds
    {
        void StartService();
        void UploadFile(Stream stream, string filepath);
        void ListFiles();
        string GetName();
    }

    class Program
    {
        enum Type { KB = 10, MB = 20 }

        static void Main(string[] args)
        {
            // Number of files.
            int n = 3;

            // Size of each file.
            int k = 1;

            // Type of file's size.
            var type = Type.KB;
            
            GenerateLoad(n, k, type);

            GoogleDrive gd = new GoogleDrive();
            Mega m = new Mega();

            Upload(gd, n, k, type);
            //Upload(m, n, k, type);
            Console.ReadLine();
        }

        private static void Upload(IClouds cloud, int n, int k, Type type)
        {
            cloud.StartService();

            List<Stream> streams = new List<Stream>();

            for (var i = 0; i < n; i++)
            {
                byte[] byteArray = File.ReadAllBytes($"out{i}.bin");
                streams.Add(new MemoryStream(byteArray));
            }

            DateTime t1 = DateTime.Now;

            for (var j = 0; j < n; j++)
            {
                cloud.UploadFile(streams.ElementAt(j), $"out{j}.bin");
            }
            
            TimeSpan t = DateTime.Now - t1;

            Console.WriteLine("{0} - Upload time of {1} files each of size {2} {3}: {4} s", cloud.GetName(), n, k, type, t.TotalSeconds);
        }
        
        // Generate n files, each file of size k
        private static void GenerateLoad(int n, int k, Type type)
        {
            var fileSize = (int)(Math.Pow(2, (int)type) / 4) * k;
            
            var rand = new Random();

            for (var i = 0; i < n; i++)
            {
                File.Delete($"out{i}.bin");
                using (var writer = new BinaryWriter(File.Open($"out{i}.bin", FileMode.CreateNew)))
                    for (var j = 0; j < fileSize; j++)
                        writer.Write(rand.Next());
            }
        }  
    }
}