using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Offbeat.BitmapDiff.Tests
{
    public class ClusterTests
    {
        [Fact]
        public void Cluster_TwoSeparatePoints_ReturnsTwoClusters() {
            var points = new List<Point>() {
                new Point(1, 1),
                new Point(3, 3)
            };

            var result = BitmapDiffer.Cluster(points, new DifferenceClusteringOptions() {
                GroupingPadding = 0,
                GroupingThreshold = 1
            });

            Assert.Equal(2, result.Count);
            Assert.Equal(new Rectangle(1, 1, 1, 1), result[0]);
            Assert.Equal(new Rectangle(1, 1, 1, 1), result[0]);
            Assert.Equal(new Rectangle(3, 3, 1, 1), result[1]);
            Assert.Equal(new Rectangle(3, 3, 1, 1), result[1]);
        }

        [Fact]
        public void Cluster_TwoAdjacentPoints_ReturnsOneCluster() {
            var points = new List<Point>() {
                new Point(1, 1),
                new Point(2, 2)
            };

            var result = BitmapDiffer.Cluster(points, new DifferenceClusteringOptions() {
                GroupingPadding = 0,
                GroupingThreshold = 1
            });

            Assert.Equal(1, result.Count);
            Assert.Equal(new Rectangle(1, 1, 2, 2), result[0]);
            Assert.Equal(new Rectangle(1, 1, 2, 2), result[0]);
        }

        [Fact]
        public void Cluster_TwoAdjacentPointsWithinThreshold_ReturnsOneCluster() {
            var points = new List<Point>() {
                new Point(1, 1),
                new Point(3, 3)
            };

            var result = BitmapDiffer.Cluster(points, new DifferenceClusteringOptions() {
                GroupingPadding = 0,
                GroupingThreshold = 2
            });

            Assert.Equal(1, result.Count);
            Assert.Equal(new Rectangle(1, 1, 3, 3), result[0]);
            Assert.Equal(new Rectangle(1, 1, 3, 3), result[0]);
        }


        [Fact]
        public void Cluster_TwoClustersOfTwoAdjacentPointsWithinThreshold_ReturnsTwoClusters() {
            var points = new List<Point>() {
                // Cluster 1
                new Point(1, 1),
                new Point(3, 3),

                // Cluster 2
                new Point(6, 6),
                new Point(6, 7)
            };

            var result = BitmapDiffer.Cluster(points, new DifferenceClusteringOptions() {
                GroupingPadding = 0,
                GroupingThreshold = 2
            });

            Assert.Equal(2, result.Count);
            Assert.Contains(result, r => r == new Rectangle(1, 1, 3, 3));
            Assert.Contains(result, r => r == new Rectangle(1, 1, 3, 3));
            Assert.Contains(result, r => r == new Rectangle(6, 6, 1, 2));
            Assert.Contains(result, r => r == new Rectangle(6, 6, 1, 2));
        }

        [Fact]
        public void Cluster_ThreeAdjacentPointsWithinThreshold_ReturnsOneCluster() {
            var points = new List<Point>() {
                // Cluster 1
                new Point(1, 1),
                new Point(5, 2),
                new Point(3, 3),
            };

            var result = BitmapDiffer.Cluster(points, new DifferenceClusteringOptions() {
                GroupingPadding = 0,
                GroupingThreshold = 2
            });

            Assert.Equal(1, result.Count);
            Assert.Equal(new Rectangle(1, 1, 5, 3), result[0]);
            Assert.Equal(new Rectangle(1, 1, 5, 3), result[0]);
        }
    }
}
