﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Offbeat.BitmapDiff {
    public static class BitmapDiffer
    {
        private static readonly ImageCompareOptions DefaultCompareOptions = new ImageCompareOptions();

        /// <summary>
        /// Compares two bitmaps, producing a list of points where they differ.
        /// </summary>
        /// <param name="first">First image to compare</param>
        /// <param name="second">Second image to compare</param>
        /// <param name="options">Options for comparing the image</param>
        /// <returns>A list of Points representing the differences in the images.</returns>
        /// <remarks>
        /// Use <paramref name="options"/> to control memory allocation behavior.
        /// <see cref="ImageCompareOptions.AssumedDifferencePercentage"/> is used to allocate the difference list.
        /// If your images are likely to be very dissimilar, setting the percentage to a higher value will avoid
        /// allocations from resizing the list.
        /// </remarks>
        public static IList<Point> Compare(
            Bitmap first, 
            Bitmap second, 
            ImageCompareOptions options = null) {
            if (first == second) {
                return new Point[0];
            }

            if (first.Width != second.Width || first.Height != second.Height) {
                throw new ArgumentException("Second image does not match dimensions of first image", nameof(second));
            }

            if (options?.AssumedDifferencePercentage > 100) {
                throw new ArgumentException($"Assumed difference percentage must be between 1 and 100 (was: {options.AssumedDifferencePercentage})");
            }

            double assumedDifference = options?.AssumedDifferencePercentage ??
                DefaultCompareOptions.AssumedDifferencePercentage;

            // Construct the difference list with a specific assumed size to minimize list allocations.
            int diffListDesiredSize =
                (int) Math.Floor(first.Width * first.Height * (assumedDifference / 100));
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

        /// <summary>
        /// Cluster a list of points into containing rectangles. This method is used to generate difference markers.
        /// </summary>
        /// <param name="points">The list of points to cluster.</param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IList<Rectangle> Cluster(
            IList<Point> points, 
            DifferenceClusteringOptions options) {
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

                var clusteringThreshold = options.ClusteringThreshold;

                Rectangle matchRectangle = GetMatchRectangle(options.ClusteringThreshold, point);
                Rectangle joinRectangle = differences[differences.Count - 1];

                int joinRectangleIndex = differences.Count - 1;

                cluster:
                for (var index = differences.Count - 1; index >= 0; index--) {
                    if (index == joinRectangleIndex) {
                        continue;
                    }

                    var diff = differences[index];

                    if (matchRectangle.IntersectsWith(diff)) {
                        differences.RemoveAt(joinRectangleIndex);

                        // If we have restarted clustering, we might match to an element that is later
                        // in the sequence than we are. That means we have to adjust the index, or we'll be
                        // off by one.
                        if (joinRectangleIndex <= index) {
                            index--;
                        }

                        joinRectangle = matchRectangle = differences[index] = Union(diff, joinRectangle);

                        matchRectangle.Inflate(clusteringThreshold, clusteringThreshold);

                        joinRectangleIndex = index;

                        // Restart the clustering process using this rectangle as the match
                        goto cluster;
                    }

                    int bottomEdge = diff.Y + diff.Height - 1;
                    if (bottomEdge < matchRectangle.X - clusteringThreshold) {
                        // Since our list of points is sorted by Y coordinate first,
                        // when we first encounter a Y coordinate that's out of range,
                        // we know we can stop looking
                    }
                }
            }

            return differences;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Rectangle GetMatchRectangle(int threshold, Point point) {
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
}