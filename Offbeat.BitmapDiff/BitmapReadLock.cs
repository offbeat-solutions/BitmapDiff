using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace Offbeat.BitmapDiff {
    internal class BitmapReadLock : IDisposable {
        private readonly Bitmap source;

        public BitmapData Data { get; }

        public BitmapReadLock(Bitmap source, PixelFormat format) {
            this.source = source;
            Data = source.LockBits(format);
        }


        public void Dispose() {
            source.UnlockBits(Data);
        }
    }
}