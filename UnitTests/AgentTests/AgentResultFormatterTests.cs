using Agent;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;

namespace MyDocClassifierAgentUnitTests.AgentTests;

/// <summary>
/// Tests for AgentResultFormatter to verify console output behavior.
/// </summary>
[TestClass]
public class AgentResultFormatterTests
{
    /// <summary>
    /// Verifies that a message is printed when the result list is empty.
    /// </summary>
    [TestMethod]
    public void DisplayResults_Should_Print_Message_When_List_Is_Empty()
    {
        var output = new StringWriter();
        Console.SetOut(output);

        AgentResultFormatter.DisplayResults(new List<(string, double)>());

        var result = output.ToString();
        StringAssert.Contains(result, "No matching document classes found.");
    }

    /// <summary>
    /// Verifies that the best match and all scores are printed correctly.
    /// </summary>
    [TestMethod]
    public void DisplayResults_Should_Print_Best_Match_And_All_Scores()
    {
        var rankedResults = new List<(string ClassName, double Score)>
        {
            ("Science", 0.95),
            ("Sports", 0.60)
        };

        var originalOut = Console.Out;

        var output = new StringWriter();
        Console.SetOut(output);

        AgentResultFormatter.DisplayResults(rankedResults);

        var result = output.ToString();

        StringAssert.Contains(result, "Best matching class: Science");
        StringAssert.Contains(result, "Similarity score: 0.9500");
        StringAssert.Contains(result, " - Science: 0.9500");
        StringAssert.Contains(result, " - Sports: 0.6000");

        Console.SetOut(originalOut);
    }
}