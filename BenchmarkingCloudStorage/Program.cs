using System;
using System.IO;

namespace BenchmarkingCloudStorage
{
    public interface IClouds
    {
        void StartService();
        void UploadFile(string filePath);
        void ListFiles();
    }

    class Program
    {
        static void Main(string[] args)
        {
            // Number of files.
            int n = 3;

            // Size of each file.
            int k = 1;

            // Type of file's size. m = MB; k = KB
            char type = 'k';

            GenerateLoad(n, k, type);
            GoogleDriveUpload(n, k, type);
            MegaUpload(n, k, type);
            Console.ReadLine();
        }

        private static void MegaUpload(int n, int k, char type)
        {
            Mega m = new Mega();
            m.StartService();
            
            DateTime t1 = DateTime.Now;

            for (var i = 0; i < n; i++)
                m.UploadFile($"out{i}.bin");
            
            TimeSpan t = DateTime.Now - t1;

            Console.WriteLine("Mega - Upload time of {0} files each of size {1} {2}B: {3} s", n, k, type, t.TotalSeconds);
        }

        private static void GoogleDriveUpload(int n, int k, char type)
        {
            GoogleDrive gd = new GoogleDrive();
            gd.StartService();

            DateTime t1 = DateTime.Now;

            for (var i = 0; i < n; i++)
                gd.UploadFile($"out{i}.bin");

            TimeSpan t = DateTime.Now - t1;

            Console.WriteLine("Google Drive - Upload time of {0} files each of size {1} {2}B: {3} s", n, k, type, t.TotalSeconds);
        }
        
        // Generate n files, each file of size k
        // type: m - MB, k - KB
        private static void GenerateLoad(int n, int k, char type)
        {
            var fileSize = (int)(Math.Pow(2, 20) / 4);
            
            if (type == 'k')
                fileSize = (int)(Math.Pow(2, 10) / 4);
            
            var rand = new Random();

            for (var i = 0; i < n; i++)
            {
                File.Delete($"out{i}.bin");
                using (var writer = new BinaryWriter(File.Open($"out{i}.bin", FileMode.CreateNew)))
                    for (var j = 0; j < fileSize*k; j++)
                        writer.Write(rand.Next());
            }
        }  
    }
}