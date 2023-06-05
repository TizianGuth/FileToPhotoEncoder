using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

namespace EncodeFile
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
                int size;
                int nameLength;
                byte[] enconded;

                Pen p = new Pen(Color.White);
                {
                    // convert file path/binary data to base64
                    string name = Convert.ToBase64String(Encoding.ASCII.GetBytes(arg));
                    enconded = Encoding.ASCII.GetBytes(name + Convert.ToBase64String(File.ReadAllBytes(arg)));

                    // we devide by 3 because each pixel has RGB
                    // Sqrt maximises PNGs width/height limitations
                    size = (int)Math.Ceiling(Math.Sqrt((enconded.Length) / 3));
                    nameLength = name.Length;
                }

                Console.WriteLine(enconded.Length);

                DirectBitmap dbm = new DirectBitmap(size, size);

                // Loop through every pixel and setPixel(RGB) to the 3 correct base64 values
                // when no more data: fill blank spave with white
                for(int x = 0; x<size; x++)
                {
                    for (int y = 0; y < size; y++)
                    {
                        int[] color = new int[3] { 255, 255, 255 };
                        int index = GetIndex(x, y, size);
                        if(index+2<enconded.Length)
                        {
                            color = new int[3] { enconded[index], enconded[index + 1], enconded[index + 2] };
                        }
                        dbm.SetPixel(x, y, Color.FromArgb(255, color[0], color[1], color[2]));
                    }
                }

                // Last Pixel guaranteed to be not used
                //      -> save information about which part of the image contains file name + type
                //
                // 8 bits per pixel = max 255 chars as encoded path length (Problem!)
                //      -> R = 0-255, G = x * 255, B = unused; to retrieve final length = 255 * G + R
                dbm.SetPixel(dbm.Width - 1, dbm.Height - 1, Color.FromArgb(255, nameLength - ((nameLength / 255) * 255), nameLength / 255, 255));

                // remove the extension from the filepath, add PNG insted
                dbm.Save().Save(arg.Replace(arg.Split('.').Last(), String.Empty)  + "PNG" );
                dbm.Dispose();
            }

            watch.Stop();
            Console.WriteLine($"Finished in {watch.ElapsedMilliseconds/1000}s");
            Console.ReadKey();
        }

        static int GetIndex(int x, int y, int width)
        {
            return y * width * 3 + x * 3;
        }
    }
}