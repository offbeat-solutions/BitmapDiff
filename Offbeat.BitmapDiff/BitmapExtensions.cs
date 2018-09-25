using System.Drawing;
using System.Drawing.Imaging;

namespace Offbeat.BitmapDiff {
    internal static class BitmapExtensions {
        public static Rectangle GetBoundingRectangle(this Bitmap bitmap) {
            return new Rectangle(0, 0, bitmap.Width, bitmap.Height);
        }

        public static BitmapData LockBits(this Bitmap bitmap, PixelFormat format) {
            return bitmap.LockBits(bitmap.GetBoundingRectangle(), ImageLockMode.ReadOnly, format);
        }

        public static BitmapReadLock ReadLock(this Bitmap bitmap, PixelFormat format) {
            return new BitmapReadLock(bitmap, format);
        }
    }
}