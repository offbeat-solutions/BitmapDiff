using System;
using System.Drawing;
using Xunit;

namespace Offbeat.BitmapDiff.Tests
{
    public class BitmapDifferTests
    {
        [Fact]
        public void ChangedImageHasDifferentWidth_ThrowsException() {
            var original = new Bitmap(5, 5);
            var changed = new Bitmap(6, 5);

            Assert.Throws<ArgumentException>(() => BitmapDiffer.Compare(original, changed, new ImageCompareOptions()));
        }

        [Fact]
        public void ChangedImageHasDifferentHeight_ThrowsException() {
            var original = new Bitmap(5, 5);
            var changed = new Bitmap(5, 6);

            Assert.Throws<ArgumentException>(() => BitmapDiffer.Compare(original, changed, new ImageCompareOptions()));
        }

        [Fact]
        public void AssumedDifferencePercentageIsNegative_DoesNotThrowException() {
            var original = new Bitmap(5, 5);
            var changed = new Bitmap(5, 5);

            BitmapDiffer.Compare(original, changed, new ImageCompareOptions() {
                AssumedDifferencePercentage = -0.1
            });
        }

        [Fact]
        public void AssumedDifferencePercentageIsGreaterThan100_ThrowsException() {
            var original = new Bitmap(5, 5);
            var changed = new Bitmap(5, 5);

            Assert.Throws<ArgumentException>(() => BitmapDiffer.Compare(original, changed, new ImageCompareOptions() {
                AssumedDifferencePercentage = 100.1
            }));
        }

        [Fact]
        public void EqualImages_EmptyDifferenceList() {
            var original = new Bitmap(5, 5);
            var changed = new Bitmap(5, 5);

            Assert.Empty(BitmapDiffer.Compare(original, changed, new ImageCompareOptions()));
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(1, 0)]
        [InlineData(0, 1)]
        [InlineData(1, 1)]
        public void DifferentPixel_ReturnedInList(int x, int y) {
            var original = new Bitmap(2, 2);
            var changed = new Bitmap(2, 2);
            changed.SetPixel(x, y, Color.Aqua);

            Assert.Contains(new Point(x, y), BitmapDiffer.Compare(original, changed, new ImageCompareOptions()));
        }
    }
}
