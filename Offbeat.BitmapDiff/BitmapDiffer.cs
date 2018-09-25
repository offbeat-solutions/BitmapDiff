using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Offbeat.BitmapDiff {
    public static class BitmapDiffer
    {
        public static IList<Point> Compare(
            Bitmap first, 
            Bitmap second, 
            ImageCompareOptions options) {
            if (first == second) {
                return new Point[0];
            }

            if (first.Width != second.Width || first.Height != second.Height) {
                throw new ArgumentException("Second image does not match dimensions of first image", nameof(second));
            }

            if (options.AssumedDifferencePercentage > 100) {
                throw new ArgumentException($"Assumed difference percentage must be between 1 and 100 (was: {options.AssumedDifferencePercentage})");
            }

            // Construct the difference list with a specific assumed size to minimize list allocations.
            int diffListDesiredSize =
                (int) Math.Floor(first.Width * first.Height * (options.AssumedDifferencePercentage / 100));
            var differences =
                new List<Point>(Math.Max(4, diffListDesiredSize));

            unsafe {
                using (var firstData = first.ReadLock(PixelFormat.Format24bppRgb))
                using (var secondData = second.ReadLock(PixelFormat.Format24bppRgb)) {

                    byte* linePtr1 = (byte*) firstData.Data.Scan0;
                    byte* linePtr2 = (byte*) secondData.Data.Scan0;

                    for (var y = 0; y < first.Height; y++) {
                        for (var x = 0; x < first.Width; x++) {
                            int* ptr1 = (int*)(linePtr1 + (x * 3));
                            int* ptr2 = (int*)(linePtr2 + (x * 3));
                            if (*ptr1 == *ptr2) {
                                // Optimization: Pixel value equal, skip ahead. 
                                continue;
                            }

                            // Here, we can now either directly add the difference to the list
                            // or adapt some perceptible difference threshold.
                            differences.Add(new Point(x, y));
                        }

                        linePtr1 += firstData.Data.Stride;
                        linePtr2 += secondData.Data.Stride;
                    }

                    return differences;
                }
            }
        }

        public static IList<Rectangle> Cluster(
            IList<Point> points, 
            DifferenceClusteringOptions opts) {
            if (points.Count == 0) {
                return new Rectangle[0];
            }

            var differences = new List<Rectangle>();

            var sorted = points.OrderBy(p => p.Y).ThenBy(p => p.X).ToList();
            foreach (var point in sorted) {

                differences.Add(new Rectangle(point.X, point.Y, 1, 1));

                if (differences.Count == 1) {
                    // This was the first difference, so we can skip right ahead
                    continue;
                }

                var matchRectangle = GetMatchRectangle(opts, point);
                var joinRectangle = differences[differences.Count - 1];

                // Start from the second-to-last index, because the last one is the one we just added
                int referenceMatch = differences.Count - 1;

                for (var index = differences.Count - 2; index >= 0; index--) {
                    var diff = differences[index];

                    if (matchRectangle.IntersectsWith(diff)) {
                        var newDifferenceRectangle = Union(diff, joinRectangle);

                        differences[index] = newDifferenceRectangle;

                        differences.RemoveAt(referenceMatch);

                        joinRectangle = matchRectangle = newDifferenceRectangle;

                        matchRectangle.Inflate(opts.GroupingThreshold, opts.GroupingThreshold);

                        referenceMatch = index;
                    }
                }
            }

            return differences;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Rectangle GetMatchRectangle(DifferenceClusteringOptions opts, Point point) {
            int threshold = opts.GroupingThreshold;
            var rect = new Rectangle(
                Math.Max(0, point.X - threshold),
                Math.Max(0, point.Y - threshold),
                threshold * 2 + 1,
                threshold * 2 + 1
            );
            return rect;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Rectangle Union(Rectangle original, Rectangle join) {
            int x1 = Math.Min(original.X, join.Location.X);
            int y1 = Math.Min(original.Y, join.Location.Y);

            int x2 = Math.Max(original.X + original.Width - 1, join.Location.X + join.Size.Width - 1);
            int y2 = Math.Max(original.Y + original.Height - 1, join.Location.Y + join.Size.Height - 1);

            var result = new Rectangle(x1, y1, x2 - x1 + 1, y2 - y1 + 1);
            return result;
        }
    }

    public static class BitmapExtensions {
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

    public class BitmapReadLock : IDisposable {
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