using System;
using System.Collections.Generic;
using System.Linq;

namespace Agent;

/// <summary>
/// Displays classification results in the console.
/// </summary>
public static class AgentResultFormatter
{
    /// <summary>
    /// Shows ranked class results with similarity scores.
    /// </summary>
    /// <param name="rankedResults">List of class names with their scores.</param>
    public static void DisplayResults(
        IReadOnlyList<(string ClassName, double Score)> rankedResults)
    {
        if (rankedResults == null || rankedResults.Count == 0)
        {
            Console.WriteLine("No matching document classes found.");
            return;
        }

        var bestMatch = rankedResults.First();

        Console.WriteLine();
        Console.WriteLine("Agent Result");
        Console.WriteLine("----------------------------");

        Console.WriteLine($"Best matching class: {bestMatch.ClassName}");
        Console.WriteLine($"Similarity score: {bestMatch.Score:F4}");
        Console.WriteLine();

        Console.WriteLine("All class similarity scores:");

        foreach (var result in rankedResults)
        {
            Console.WriteLine(
                $" - {result.ClassName}: {result.Score:F4}");
        }
    }
}