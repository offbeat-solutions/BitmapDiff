using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Offbeat.BitmapDiff.Benchmark
{
    class Program
    {
        static void Main(string[] args) {
            if (args.Length != 2) {
                Console.WriteLine("Usage: diffbenchmark.exe <first> <second>");
                return;
            }

            var original = TryLoadFile(args[0]);
            if (original == null) {
                Console.WriteLine($"Failed to load {args[0]}. Please ensure the file exists and is readable.");
                return;
            }

            var changed = TryLoadFile(args[1]);
            if (changed == null) {
                Console.WriteLine($"Failed to load {args[1]}. Please ensure the file exists and is readable.");
                return;
            }

            Console.WriteLine($"Loaded images. Original image size: {original.Width} x {original.Height}. Press enter to continue.");
            Console.ReadLine();

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var differences = BitmapDiffer.Compare(original, changed, new ImageCompareOptions());
        
            stopwatch.Stop();

            Console.WriteLine($"Diffed images in {stopwatch.Elapsed}. Difference count: {differences.Count}. Press enter to continue.");
            Console.ReadLine();

            stopwatch.Reset();
            stopwatch.Start();

            var clusters = BitmapDiffer.Cluster(differences, new DifferenceClusteringOptions() {
                GroupingThreshold = 10
            });

            stopwatch.Stop();

            Console.WriteLine($"Generated difference clusters in {stopwatch.Elapsed}. Cluster count: {clusters.Count}. Press enter to continue.");
            Console.ReadLine();

            foreach (var cluster in clusters) {
                var rect = cluster;
                Console.WriteLine($" (x: {rect.X}, y: {rect.Y}, w: {rect.Width}, h: {rect.Height})");
            }

            using (var result = new Bitmap(original.Width * 2, original.Height)) {
                using (var ctx = Graphics.FromImage(result)) {

                    ctx.DrawImage(original,new Point(0, 0));
                    ctx.DrawImage(changed, new Point(original.Width, 0));


                    for (var index = 0; index < clusters.Count; index++) {
                        var cluster = clusters[index];
                        var rect = cluster;
                        var location = $"({rect.X}, {rect.Y})";

                        rect.Inflate(1, 1);

                        ctx.DrawRectangle(new Pen(Brushes.Red, 2), rect);
                        ctx.DrawString(location, SystemFonts.DefaultFont, new SolidBrush(Color.Red), rect.Location);

                        rect.Offset(original.Width, 0);

                        ctx.DrawRectangle(new Pen(Brushes.Red, 2), rect);
                    }

                    var diffName = $"{Guid.NewGuid()}.png";

                    using (var stream = File.OpenWrite(diffName)) {
                        result.Save(stream, ImageFormat.Png);
                    }

                    Console.WriteLine($"Wrote diff result to {diffName}.");
                }
            }
        }

        private static Bitmap TryLoadFile(string fileName) {
            try {
                return (Bitmap) Image.FromFile(fileName);
            } catch {
                return null;
            }
        }

        private static PointF[] GetPoints(Rectangle input) {
            return new[] {
                new PointF(input.X, input.Y),
                new PointF(input.X + input.Width - 1, input.Y),
                new PointF(input.X + input.Width - 1, input.Y + input.Height - 1),
                new PointF(input.X, input.Y + input.Height - 1),
            };
        }


    }
}
