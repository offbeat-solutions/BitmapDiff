using System.Collections;
using System.Globalization;
using System.Runtime.ExceptionServices;

namespace Offbeat.BitmapDiff
{
    public class ImageCompareOptions {
        /// <summary>
        /// The percentage of pixels that are assumed to contain differences.
        /// This is used for memory allocation optimizations. Defaults to 1.
        /// </summary>
        public double AssumedDifferencePercentage { get; set; } = 1;
    }
}
