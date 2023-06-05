using System.Diagnostics;
using System.Drawing;
using System.Text;

namespace DecodeFile
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0) return;

            Console.WriteLine("Processing...");

            // Tracking time
            var watch = new Stopwatch();
            watch.Start();

            // Loop through each given path
            foreach (var arg in args)
            {
                string fileExtension;
                string fileEncoded;
                string pathDecoded;

                // read file as bitmap, copy it to own DirectBitmap class, then dispose
                DirectBitmap dbm;
                using (Bitmap m = new Bitmap(arg))
                {
                    dbm = new DirectBitmap(m);
                }

                {
                    // Load color data in byte array
                    byte[] allData = new byte[dbm.Width * dbm.Height * 3];

                    for (int y = 0; y < dbm.Height; y++)
                    {
                        for (int x = 0; x < dbm.Width; x++)
                        {
                            Color c = dbm.GetPixel(x, y);
                            allData[GetIndex(x, y, dbm.Width) + 0] = c.R;
                            allData[GetIndex(x, y, dbm.Width) + 1] = c.G;
                            allData[GetIndex(x, y, dbm.Width) + 2] = c.B;
                        }
                    }
                    dbm.Dispose();

                    Console.WriteLine(allData.Length);

                    // R + G * 255 = pathLength
                    int pathLength = allData[GetIndex(dbm.Width - 1, dbm.Height - 1, dbm.Width)] + allData[GetIndex(dbm.Width - 1, dbm.Height - 1, dbm.Width) + 1] * 255;

                    // slice then decode byte array 
                    pathDecoded = new string(Encoding.ASCII.GetString(allData[0..(pathLength)]).ToCharArray());
                    fileEncoded = new string(Encoding.ASCII.GetString(allData[pathLength..(allData.Length - 3)]).ToCharArray());

                }
                // Get the files original extension 
                fileExtension = Encoding.ASCII.GetString(Convert.FromBase64String(pathDecoded)).Split('.').Last();

                // decode the from the image read string of bytes(base64) to (the original) binary
                byte[] fileBytes = Convert.FromBase64String(fileEncoded);

                // Write decoded bytes to drive
                File.WriteAllBytes(arg.Replace(arg.Split('.').Last(), String.Empty) + fileExtension, fileBytes);
            }

            watch.Stop();
            Console.WriteLine($"Finished in {watch.ElapsedMilliseconds / 1000}s");
            Console.ReadKey();
        }

        static int GetIndex(int x, int y, int width)
        {
            return y * width * 3 + x * 3;
        }
    }
}