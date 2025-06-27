using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace McpUnity.Resources
{
    /// <summary>
    /// Resource for getting available tests from Unity Test Runner
    /// </summary>
    public class GetTestsResource : McpResourceBase
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public GetTestsResource()
        {
            Name = "get_tests";
            Description = "Gets available tests from Unity Test Runner (disabled in this build)";
            IsAsync = true;
        }

        /// <inheritdoc />
        public override void FetchAsync(JObject parameters, TaskCompletionSource<JObject> tcs)
        {
            // Test functionality is disabled
            tcs.SetResult(new JObject
            {
                ["error"] = "Test functionality is disabled in this build"
            });
        }
    }
}
