using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BenchmarkingCloudStorage
{
    public interface IClouds
    {
        Task StartService();
        Task DeleteFiles();
        void UploadFile(Stream stream, string filepath);
        Task ListFiles();
        string GetName();
    }

    class Program
    {
        enum Type { KB = 10, MB = 20 }

        static void Main(string[] args)
        {
            GoogleDrive gd = new GoogleDrive();
            Mega m = new Mega();
            Dropbox db = new Dropbox();

            IClouds[] clouds = {gd, m, db};

            foreach (var cloud in clouds)
            {
                cloud.StartService();
            }

            // Test One: consecutive upload of 10 files of sizes 1 KB, 100 KB, 1 MB, 5 MB
            int[] sizes = { 1, 100, 1, 5 };
            Type[] types = { Type.KB, Type.KB, Type.MB, Type.MB };

            for (var i = 0; i < 4; i++)
            {
                GenerateLoad(10, sizes[i], types[i]);
                foreach (var cloud in clouds)
                {
                    Upload(cloud, 10, sizes[i], types[i]);
                }
            }

            // Test Two: consecutive upload of different number of files with same size (1 KB)
            int[] numFiles = { 5, 10, 20, 50, 100 };

            for (var i = 0; i < 5; i++)
            {
                GenerateLoad(numFiles[i], 1, Type.KB);
                foreach (var cloud in clouds)
                {
                    Upload(cloud, numFiles[i], 1, Type.KB);
                }
            }

            //var task = Task.Run((Func<Task>) db.ListFiles);
            //task.Wait();
            Console.ReadLine();
        }
        
        // Run upload tests on the cloud
        private static void Upload(IClouds cloud, int n, int k, Type type)
        {
            List<Stream> streams = new List<Stream>();

            for (var i = 0; i < n; i++)
            {
                byte[] byteArray = File.ReadAllBytes($"out{i}.bin");
                streams.Add(new MemoryStream(byteArray));
            }
            
            List<TimeSpan> times = new List<TimeSpan>();

            Console.WriteLine("{0} - {1} files each of size {2} {3}", cloud.GetName(), n, k, type);
            for (var i = 0; i < 3; i++)
            {
                cloud.DeleteFiles();
                
                DateTime t1 = DateTime.Now;

                for (var j = 0; j < n; j++)
                {
                    cloud.UploadFile(streams.ElementAt(j), $"out{j}.bin");
                }

                TimeSpan t = DateTime.Now - t1;
                times.Add(t);
                Console.WriteLine("t{0}: {1} s", i+1, t.TotalSeconds);
            }

            double avg = times.Count > 0 ? times.Average(ts => ts.TotalSeconds) : 0.0;

            Console.WriteLine("avg t: {0} s", avg);
            Console.WriteLine();
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