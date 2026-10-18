using System;
using System.Collections.Generic;
using System.Text;
using Azure;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace BaseAccountSystem.Server.Tests.Helpers
{
    public static class EntryLogHandler
    {
        public static string GetEntryString(string testType, string testScenario, string expected, string actual, string body, string result)
        {
            return $"""
                    =====
                    [TEST TYPE] {testType}
                    [TEST SCENARIO] {testScenario}
                    [EXPECTED] HTTP Status: {expected}
                    [ACTUAL]   HTTP Status: {actual}
                    [RESPONSE BODY] {body}
                    [TEST RESULT] {result}
                    """;
        }
    }
}
