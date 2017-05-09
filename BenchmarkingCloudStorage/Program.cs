using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BenchmarkingCloudStorage
{
    public interface IClouds
    {
        void StartService();
        Task DeleteFiles();
        Task UploadFile(Stream stream, string filename);
        void DownloadFiles();
        void ListFiles();
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

            //foreach (var cloud in clouds)
            //{
            //    cloud.StartService();
            //}

            gd.StartService();
            PingTest(gd);

            // Test One: upload and download of 10 files of sizes 1 KB, 1 MB, 2 MB, 3 MB, 4 MB, 5 MB, 10 MB, 20 MB
            //int[] sizes = { 1, 1, 2, 3, 4, 5, 10, 20 };
            //Type[] types = { Type.KB, Type.MB, Type.MB, Type.MB, Type.MB, Type.MB, Type.MB, Type.MB };

            //for (var i = 0; i < 4; i++)
            //{
            //    foreach (var cloud in clouds)
            //    {
            //        Test(cloud, 10, sizes[i], types[i]);
            //    }
            //}

            //// Test Two: upload and download of different number of files with same size (1 MB)
            //int[] numFiles = { 5, 10, 20, 50, 100 };

            //for (var i = 0; i < 5; i++)
            //{
            //    foreach (var cloud in clouds)
            //    {
            //        Test(cloud, numFiles[i], 1, Type.MB);
            //    }
            //}
            
            //var task = Task.Run((Func<Task>)db.ListFiles);
            //task.Wait();
            Console.ReadLine();
        }

        // Upload 1 MB file every 10-ish seconds
        private static void PingTest(IClouds cloud)
        {
            GenerateLoad(1, 1, Type.MB);
            byte[] byteArray = File.ReadAllBytes($"0.jpg");
            Stream stream = new MemoryStream(byteArray);

            StreamWriter sw = File.CreateText("results.txt");
            
            for (var i = 0; i < 21600; i++)
            {
                DateTime t1 = DateTime.Now;
                cloud.UploadFile(stream, $"0.jpg");
                TimeSpan t = DateTime.Now - t1;
                
                sw.WriteLine("{0}", t.TotalSeconds);
                sw.Flush();
                stream.Seek(0, SeekOrigin.Begin);
                System.Threading.Thread.Sleep(10000);
            }
            sw.Close();
            Console.WriteLine("Done");
        }
        
        // Run test
        private static void Test(IClouds cloud, int n, int k, Type type)
        {
            GenerateLoad(n, k, type);

            List<Stream> streams = new List<Stream>();

            for (var i = 0; i < n; i++)
            {
                byte[] byteArray = File.ReadAllBytes($"{i}.jpg");
                streams.Add(new MemoryStream(byteArray));
            }
            
            List<TimeSpan> timesUpload = new List<TimeSpan>();
            List<TimeSpan> timesDownload = new List<TimeSpan>();

            Console.WriteLine("{0} - {1} files each of size {2} {3}", cloud.GetName(), n, k, type);
            for (var i = 0; i < 4; i++)
            {
                cloud.DeleteFiles();
                DeleteLoad(n);

                DateTime t1 = DateTime.Now;

                for (var j = 0; j < n; j++)
                {
                    cloud.UploadFile(streams[j], $"{j}.jpg");
                    streams[j].Seek(0, SeekOrigin.Begin);
                }

                TimeSpan t = DateTime.Now - t1;
                timesUpload.Add(t);
                Console.WriteLine("t{0}(upload): {1} s", i+1, t.TotalSeconds);

                t1 = DateTime.Now;
                cloud.DownloadFiles();
                t = DateTime.Now - t1;
                timesDownload.Add(t);
                Console.WriteLine("t{0}(download): {1} s\n", i + 1, t.TotalSeconds);
            }

            timesUpload.RemoveAt(0);
            timesDownload.RemoveAt(0);
            double avgUpload = timesUpload.Count > 0 ? timesUpload.Average(ts => ts.TotalSeconds) : 0.0;
            double avgDownload = timesDownload.Count > 0 ? timesDownload.Average(ts => ts.TotalSeconds) : 0.0;

            Console.WriteLine("avg t(upload): {0} s", avgUpload);
            Console.WriteLine("avg t(download): {0} s", avgDownload);
            Console.WriteLine();
        }

        // Delete files on disk
        private static void DeleteLoad(int n)
        {
            for (var i = 0; i < n; i++)
            {
                File.Delete($"{i}.jpg");
            }
        }
        
        // Generate n files on disk, each file of size k
        private static void GenerateLoad(int n, int k, Type type)
        {
            var fileSize = (int)(Math.Pow(2, (int)type) / 4) * k;
            
            var rand = new Random();

            for (var i = 0; i < n; i++)
            {
                File.Delete($"{i}.jpg");
                using (var writer = new BinaryWriter(File.Open($"{i}.jpg", FileMode.CreateNew)))
                    for (var j = 0; j < fileSize; j++)
                        writer.Write(rand.Next());
            }
        }  
    }
}