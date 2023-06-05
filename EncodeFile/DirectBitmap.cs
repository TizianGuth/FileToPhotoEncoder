using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

public class DirectBitmap : IDisposable
{
    public Bitmap Bitmap { get; private set; }
    public int[] Bits { get; private set; }
    public bool Disposed { get; private set; }
    public int Height { get; private set; }
    public int Width { get; private set; }

    protected GCHandle BitsHandle { get; private set; }

    public DirectBitmap(int width, int height)
    {
        Width = width;
        Height = height;
        Bits = new int[width * height];
        BitsHandle = GCHandle.Alloc(Bits, GCHandleType.Pinned);
        Bitmap = new Bitmap(width, height, width * 4, PixelFormat.Format32bppPArgb, BitsHandle.AddrOfPinnedObject());
    }

    public DirectBitmap(Bitmap bmp)
    {
        Width = bmp.Width;
        Height = bmp.Height;

        BitmapData bData = bmp.LockBits(new Rectangle(new Point(), bmp.Size),
        ImageLockMode.ReadOnly,
        PixelFormat.Format32bppRgb);

        // number of bytes in the bitmap
        var byteCount = bData.Stride * (bmp.Height);

        int[] bytes = new int[byteCount / 4];

        Marshal.Copy(bData.Scan0, bytes, 0, byteCount / 4);

        // don't forget to unlock the bitmap!!
        bmp.UnlockBits(bData);

        Bits = bytes;// new Int32[bmp.Width * bmp.Height];
        BitsHandle = GCHandle.Alloc(Bits, GCHandleType.Pinned);
        Bitmap = new Bitmap(bmp.Width, bmp.Height, bmp.Width * 4, PixelFormat.Format24bppRgb, BitsHandle.AddrOfPinnedObject());
    }

    public void SetPixel(int x, int y, Color color)
    {
        int index = x + (y * Width);
        int col = color.ToArgb();

        Bits[index] = col;
    }

    public Color GetPixel(int x, int y)
    {
        int index = x + (y * Width);
        int col = Bits[index];
        Color result = Color.FromArgb(col);

        return result;
    }

    public Bitmap Save()
    {
        // var b = Bitmap;
        return Bitmap.Clone(new Rectangle(0, 0, Width, Height), PixelFormat.Format24bppRgb);
        // b.Dispose();
    }

    public void Dispose()
    {
        if (Disposed) return;
        Disposed = true;
        Bitmap.Dispose();
        BitsHandle.Free();
    }
}

