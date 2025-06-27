using System;
using System.Threading.Tasks;
using McpUnity.Unity;
using UnityEngine;
using Newtonsoft.Json.Linq;
// Test Runner API references removed

namespace McpUnity.Tools
{
    /// <summary>
    /// Tool for running Unity Test Runner tests
    /// </summary>
    public class RunTestsTool : McpToolBase
    {
        public RunTestsTool()
        {
            Name = "run_tests";
            Description = "Runs tests using Unity's Test Runner (disabled in this build)";
            IsAsync = true;
        }
        
        /// <summary>
        /// Executes the RunTests tool asynchronously on the main thread.
        /// </summary>
        /// <param name="parameters">Tool parameters</param>
        /// <param name="tcs">TaskCompletionSource to set the result or exception.</param>
        public override void ExecuteAsync(JObject parameters, TaskCompletionSource<JObject> tcs)
        {
            // Test functionality is disabled
            tcs.SetResult(new JObject
            {
                ["error"] = "Test functionality is disabled in this build"
            });
        }
    }
}
