using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEditor;

namespace McpUnity.Services
{
    /// <summary>
    /// Service for accessing Unity Test Runner functionality
    /// NOTE: Test functionality has been disabled in this build
    /// </summary>
    public class TestRunnerService : ITestRunnerService
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public TestRunnerService()
        {
            // Test runner functionality has been disabled in this build
        }


        [MenuItem("Tools/MCP Unity/Debug call path")]
        public static void DebugCallGetAllTests()
        {
            Debug.LogWarning("Test runner functionality has been disabled in this build");
        }

        /// <summary>
        /// Asynchronously retrieves all available tests.
        /// </summary>
        /// <param name="testModeFilter">Optional test mode filter (ignored)</param>
        /// <returns>Empty list since test functionality is disabled</returns>
        public Task<List<object>> GetAllTestsAsync(string testModeFilter = "")
        {
            return Task.FromResult(new List<object>());
        }

        /// <summary>
        /// Executes tests and returns the results as a JSON object.
        /// </summary>
        /// <param name="testMode">The test mode to run (ignored).</param>
        /// <param name="returnOnlyFailures">If true, only failed test results are included in the output (ignored).</param>
        /// <param name="returnWithLogs">If true, all logs are included in the output (ignored).</param>
        /// <param name="testFilter">A filter string to select specific tests to run (ignored).</param>
        /// <returns>Task that resolves with an error message since test functionality is disabled</returns>
        public Task<JObject> ExecuteTestsAsync(object testMode, bool returnOnlyFailures, bool returnWithLogs, string testFilter)
        {
            return Task.FromResult(new JObject
            {
                ["error"] = "Test functionality is disabled in this build"
            });
        }
    }
}
