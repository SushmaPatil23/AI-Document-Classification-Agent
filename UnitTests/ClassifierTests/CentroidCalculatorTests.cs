using Classifier.Embeddings;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace MyDocClassifierAgentUnitTests.ClassifierTests;

/// <summary>
/// Tests for CentroidCalculator to verify centroid computation logic.
/// </summary>
[TestClass]
public class CentroidCalculatorTests
{
    /// <summary>
    /// Verifies that the correct average vector is calculated.
    /// </summary>
    [TestMethod]
    public void Compute_ReturnsCorrectAverageVector()
    {
        var vectors = new[]
        {
            new float[] { 1f, 2f },
            new float[] { 3f, 4f }
        };

        var result = CentroidCalculator.Compute(vectors);

        Assert.AreEqual(2f, result[0]);
        Assert.AreEqual(3f, result[1]);
    }

    /// <summary>
    /// Verifies that an exception is thrown when no vectors are provided.
    /// </summary>
    [TestMethod]
    public void Compute_ThrowsArgumentException_WhenNoVectorsProvided()
    {
        var emptyVectors = Array.Empty<float[]>();

        Assert.ThrowsExactly<ArgumentException>(() =>
    CentroidCalculator.Compute(emptyVectors));
    }

    /// <summary>
    /// Verifies that an exception is thrown when vectors have different sizes.
    /// </summary>
    [TestMethod]
    public void Compute_ThrowsException_WhenVectorSizesDiffer()
    {
        var vectors = new[]
        {
            new float[] {1f, 2f},
            new float[] {1f, 2f, 3f}
        };

        Assert.ThrowsExactly<ArgumentException>(() =>
            CentroidCalculator.Compute(vectors));
    }
}