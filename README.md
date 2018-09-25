# Offbeat.BitmapDiff

## What is it?

Like the name suggests, it's a bitmap diff library. You can use it to generate 
pixel difference lists and cluster them together to form rectangular 
difference markers.

## Usage

Call `BitmapDiffer.Compare` to get pixel differences. Pass them on to `BitmapDiffer.Cluster` to get rectangular difference markers:

```csharp
    var pixelDifferences = BitmapDiffer.Compare(original, changed);

    var differenceMarkers = BitmapDiffer.Cluster(pixelDifferences, new DifferenceClusteringOptions() { ClusteringThreshold = 10 });
```

## Dependencies

For the library itself, .NET Framework 4.6.2. For running the tests, xUnit.

## Will you add .NET Standard support?

Probably, but since .NET Core doesn't have GDI, it's likely to be based on 
ImageSharp, which means the API won't be compatible.