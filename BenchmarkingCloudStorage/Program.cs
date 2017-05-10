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
        Task DownloadFiles();
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

            // Test Zero: Ping with 1 MB file every 10 seconds
            //gd.StartService();
            //PingTest(gd);

            //Test One: upload and download of 10 files of sizes 1 KB, 1 MB, 3 MB, 5 MB, 10 MB, 20 MB
            //File.Delete("one.txt");
            //int[] sizes = { 1, 1, 3, 5, 10, 20 };
            //Type[] types = { Type.KB, Type.MB, Type.MB, Type.MB, Type.MB, Type.MB };

            //for (var i = 0; i < sizes.Length; i++)
            //{
            //    foreach (var cloud in clouds)
            //    {
            //        Test("one.txt", cloud, 10, sizes[i], types[i]);
            //    }
            //}

            //// Test Two: upload and download of different number of files with same size (1 MB)
            //File.Delete("two.txt");
            //int[] numFiles = { 5, 10, 20, 50, 100 };

            //for (var i = 0; i < numFiles.Length; i++)
            //{
            //    foreach (var cloud in clouds)
            //    {
            //        Test("two.txt", cloud, numFiles[i], 1, Type.MB);
            //    }
            //}

            File.Delete("one_20MB.txt");
            foreach (var cloud in clouds)
            {
                Test("one_20MB.txt", cloud, 10, 20, Type.MB);
            }

            File.Delete("dropbox_50files");
            Test("dropbox_50files", db, 50, 1, Type.MB);
            
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
                cloud.DeleteFiles().Wait();
                DateTime t1 = DateTime.Now;
                cloud.UploadFile(stream, $"0.jpg").Wait();
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
        private static void Test(string filename, IClouds cloud, int n, int k, Type type)
        {
            StreamWriter sw = new StreamWriter(filename, true);

            List<TimeSpan> timesUpload = new List<TimeSpan>();
            List<TimeSpan> timesDownload = new List<TimeSpan>();

            sw.WriteLine("{0} - {1} files each of size {2} {3}", cloud.GetName(), n, k, type);
            sw.Flush();
            Console.WriteLine("{0} - {1} files each of size {2} {3}", cloud.GetName(), n, k, type);
            
            for (var i = 0; i < 2; i++)
            {
                GenerateLoad(n, k, type);
                List<Stream> streams = GetStreams(n);

                DeleteLoad(n);
                cloud.DeleteFiles().Wait();
                
                DateTime t1 = DateTime.Now;

                for (var j = 0; j < n; j++)
                {
                    cloud.UploadFile(streams[j], $"{j}.jpg").Wait();
                }

                TimeSpan t = DateTime.Now - t1;
                timesUpload.Add(t);

                sw.WriteLine("t{0}(upload): {1} s", i + 1, t.TotalSeconds);
                sw.Flush();
                Console.WriteLine("t{0}(upload): {1} s", i+1, t.TotalSeconds);

                t1 = DateTime.Now;
                cloud.DownloadFiles().Wait();
                t = DateTime.Now - t1;
                timesDownload.Add(t);

                sw.WriteLine("t{0}(download): {1} s", i + 1, t.TotalSeconds);
                sw.Flush();
                Console.WriteLine("t{0}(download): {1} s\n", i + 1, t.TotalSeconds);
            }

            timesUpload.RemoveAt(0);
            timesDownload.RemoveAt(0);
            double avgUpload = timesUpload.Count > 0 ? timesUpload.Average(ts => ts.TotalSeconds) : 0.0;
            double avgDownload = timesDownload.Count > 0 ? timesDownload.Average(ts => ts.TotalSeconds) : 0.0;

            sw.WriteLine("avg t(upload): {0} s", avgUpload);
            sw.WriteLine("avg t(download): {0} s", avgDownload);
            sw.WriteLine("");
            sw.Flush();
            sw.Close();
            Console.WriteLine("avg t(upload): {0} s", avgUpload);
            Console.WriteLine("avg t(download): {0} s\n", avgDownload);
        }

        // Get files' streams
        private static List<Stream> GetStreams(int n)
        {
            List<Stream> streams = new List<Stream>();

            for (var i = 0; i < n; i++)
            {
                byte[] byteArray = File.ReadAllBytes($"{i}.jpg");
                streams.Add(new MemoryStream(byteArray));
            }

            return streams;
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

        private static void DebugTest(IClouds cloud, int n, int k, Type type)
        {
            GenerateLoad(n, k, type);

            List<Stream> streams = new List<Stream>();

            for (var i = 0; i < n; i++)
            {
                byte[] byteArray = File.ReadAllBytes($"{i}.jpg");
                streams.Add(new MemoryStream(byteArray));
            }

            DeleteLoad(n);
            cloud.DeleteFiles().Wait();
            Console.WriteLine("Files after delete:");
            cloud.ListFiles().Wait();
            Console.WriteLine("----------");

            for (var j = 0; j < n; j++)
            {
                cloud.ListFiles().Wait();
                cloud.UploadFile(streams[j], $"{j}.jpg").Wait();
                Console.WriteLine("Done in loop");
            }

            cloud.ListFiles().Wait();
            cloud.DownloadFiles().Wait();
            Console.WriteLine("Done");
        }
    }
}