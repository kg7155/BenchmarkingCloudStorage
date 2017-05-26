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
        Task UploadFile(Stream stream, string filename, int chunkSize);
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
                cloud.StartService();

            //TestOne(clouds);
            //TestTwo(clouds);
            //TestThree(gd, m);
            TestFour(gd, m, db);
            Console.ReadLine();
        }

        //Test One: upload and download of 10 files of sizes 1 KB, 1 MB, 3 MB, 5 MB, 10 MB, 20 MB
        private static void TestOne(IClouds[] clouds)
        {
            File.Delete("one.txt");
            int[] sizes = { 1, 1, 3, 5, 10, 20 };
            Type[] types = { Type.KB, Type.MB, Type.MB, Type.MB, Type.MB, Type.MB };

            for (var i = 0; i < sizes.Length; i++)
            {
                foreach (var cloud in clouds)
                    Test("one.txt", cloud, 10, sizes[i], types[i], 0);
            }
        }

        // Test Two: upload and download of different number of files with same size (1 MB)
        private static void TestTwo(IClouds[] clouds)
        {
            File.Delete("two.txt");
            int[] numFiles = { 5, 10, 20, 50, 100 };

            for (var i = 0; i < numFiles.Length; i++)
            {
                foreach (var cloud in clouds)
                    Test("two.txt", cloud, numFiles[i], 1, Type.MB, 0);
            }
        }

        //Test Three: upload of 10 files of sizes 1 KB, 1 MB, 3 MB, 5 MB, 10 MB, 20 MB with chunk size y (50% lower than default) and z (50% higher than default)
        private static void TestThree(GoogleDrive gd, Mega m)
        {
            File.Delete("three_y_lower.txt");
            File.Delete("three_z_higher.txt");
            int[] sizes = { 1, 1, 3, 5, 10, 20 };
            Type[] types = { Type.KB, Type.MB, Type.MB, Type.MB, Type.MB, Type.MB };

            for (var i = 0; i < sizes.Length; i++)
            {
                Test("three_y_lower.txt", gd, 10, sizes[i], types[i], 5);
                Test("three_y_lower.txt", m, 10, sizes[i], types[i], 512);
            }

            for (var i = 0; i < sizes.Length; i++)
            {
                Test("three_z_higher.txt", gd, 10, sizes[i], types[i], 15);
                Test("three_z_higher.txt", m, 10, sizes[i], types[i], 1536);
            }
        }

        // 24-hour consecutive upload test of 1 MB file
        private static void TestFour(GoogleDrive gd, Mega m, Dropbox db)
        {
            File.Delete("four_GoogleDrive.txt");
            File.Delete("four_Mega.txt");
            File.Delete("four_Dropbox.txt");

            var tasks = new Task[3];

            while (true)
            {
                tasks[0] = new Task(() => Upload("four_GoogleDrive.txt", gd, 1, 1, Type.MB, 0));
                tasks[1] = new Task(() => Upload("four_Mega.txt", m, 1, 1, Type.MB, 0));
                tasks[2] = new Task(() => Upload("four_Dropbox.txt", db, 1, 1, Type.MB, 0));

                foreach (var task in tasks)
                    task.Start();

                Task.WaitAll(tasks);
            }
        }

        private static void Upload(string filename, IClouds cloud, int n, int k, Type type, int chunkSize)
        {
            StreamWriter sw = new StreamWriter(filename, true);
            
            var files = GenerateLoadRandomNames(n, k, type);
            List<Stream> streams = GetStreamsFromNames(files);

            DeleteLoadFromNames(files);
            cloud.DeleteFiles().Wait();

            DateTime t1 = DateTime.Now;

            foreach (var file in files)
                cloud.UploadFile(streams[0], $"{file}.jpg", chunkSize).Wait();

            TimeSpan t = DateTime.Now - t1;
            
            sw.WriteLine("{0}\t{1}", t.TotalSeconds, DateTime.Now);
            
            sw.Flush();
            sw.Close();
        }

        // Run test
        private static void Test(string filename, IClouds cloud, int n, int k, Type type, int chunkSize)
        {
            StreamWriter sw = new StreamWriter(filename, true);

            List<TimeSpan> timesUpload = new List<TimeSpan>();
            List<TimeSpan> timesDownload = new List<TimeSpan>();

            sw.WriteLine("{0} - {1} files each of size {2} {3} chunk size: {4}", cloud.GetName(), n, k, type, chunkSize);
            sw.Flush();
            Console.WriteLine("{0} - {1} files each of size {2} {3}, chunk size: {4}", cloud.GetName(), n, k, type, chunkSize);
            
            for (var i = 0; i < 4; i++)
            {
                GenerateLoad(n, k, type);
                List<Stream> streams = GetStreams(n);

                DeleteLoad(n);
                cloud.DeleteFiles().Wait();
                
                DateTime t1 = DateTime.Now;

                for (var j = 0; j < n; j++)
                {
                    cloud.UploadFile(streams[j], $"{j}.jpg", chunkSize).Wait();
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

        private static List<Stream> GetStreamsFromNames(List<string> files)
        {
            List<Stream> streams = new List<Stream>();

            foreach(var file in files)
            {
                byte[] byteArray = File.ReadAllBytes($"{file}.jpg");
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

        // Delete files on disk
        private static void DeleteLoadFromNames(IEnumerable<string> files)
        {
            foreach(var file in files)
            {
                File.Delete($"{file}.jpg");
            }
        }

        // Generate n files on disk, each file of size k
        private static void GenerateLoad(int n, int k, Type type)
        {
            var fileSize = (int)(Math.Pow(2, (int)type) / 4) * k;
            
            var rand = new Random();
            
            for (var i = 0; i < n; i++)
            {
                var fileName = Guid.NewGuid().ToString();
                File.Delete($"{fileName}.jpg");
                using (var writer = new BinaryWriter(File.Open($"{fileName}.jpg", FileMode.CreateNew)))
                    for (var j = 0; j < fileSize; j++)
                        writer.Write(rand.Next());
            }
        }

        // Generate n files on disk, each file of size k
        private static List<string> GenerateLoadRandomNames(int n, int k, Type type)
        {
            var fileSize = (int)(Math.Pow(2, (int)type) / 4) * k;

            var rand = new Random();
            var list = new List<string>();
            for (var i = 0; i < n; i++)
            {
                var fileName = Guid.NewGuid().ToString();
                File.Delete($"{fileName}.jpg");
                using (var writer = new BinaryWriter(File.Open($"{fileName}.jpg", FileMode.CreateNew)))
                    for (var j = 0; j < fileSize; j++)
                        writer.Write(rand.Next());

                list.Add(fileName);
            }

            return list;
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
                cloud.UploadFile(streams[j], $"{j}.jpg",0).Wait();
                Console.WriteLine("Done in loop");
            }

            cloud.ListFiles().Wait();
            cloud.DownloadFiles().Wait();
            Console.WriteLine("Done");
        }
    }
}