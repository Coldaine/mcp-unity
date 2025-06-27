using System;
using System.Threading.Tasks;
using McpUnity.Unity;
using UnityEngine;
using UnityEditor;
using UnityEditor.Compilation;
using Newtonsoft.Json.Linq;
using McpUnity.Utils;
using McpUnity.Services;

namespace McpUnity.Tools
{
    /// <summary>
    /// Tool for requesting Unity script compilation and monitoring its progress
    /// </summary>
    public class RequestScriptCompilationTool : McpToolBase
    {
        private TaskCompletionSource<JObject> _currentTcs;
        private bool _isCompiling = false;
        private readonly IConsoleLogsService _consoleLogsService;        public RequestScriptCompilationTool(IConsoleLogsService consoleLogsService)
        {
            Name = "request_script_compilation";
            Description = "Requests Unity to recompile all scripts and reports when compilation is complete or if errors occur";
            IsAsync = true;
            _consoleLogsService = consoleLogsService;
            
            // Subscribe to compilation events
            CompilationPipeline.compilationStarted -= OnCompilationStarted;
            CompilationPipeline.compilationStarted += OnCompilationStarted;
            
            CompilationPipeline.compilationFinished -= OnCompilationFinished;
            CompilationPipeline.compilationFinished += OnCompilationFinished;
        }
        
        /// <summary>
        /// Execute the script compilation tool asynchronously
        /// </summary>
        /// <param name="parameters">Tool parameters (currently none required)</param>
        /// <param name="tcs">TaskCompletionSource to set the result</param>
        public override void ExecuteAsync(JObject parameters, TaskCompletionSource<JObject> tcs)
        {
            try
            {
                // Check if compilation is already in progress
                if (_isCompiling)
                {
                    tcs.SetResult(new JObject
                    {
                        ["success"] = false,
                        ["type"] = "text",
                        ["message"] = "Script compilation is already in progress. Please wait for it to complete before requesting another compilation.",
                        ["isCompiling"] = true
                    });
                    return;
                }

                // Check if compilation is required
                if (!EditorApplication.isCompiling)
                {
                    _currentTcs = tcs;
                    
                    McpLogger.LogInfo("Requesting script compilation...");
                    
                    // Send initial response to let the agent know compilation has started
                    var initialResponse = new JObject
                    {
                        ["success"] = true,
                        ["type"] = "text",
                        ["message"] = "Script compilation requested. Unity will now recompile all scripts. Please wait for compilation to complete before proceeding with other actions.",
                        ["status"] = "compilation_started",
                        ["note"] = "This tool will complete once compilation finishes. Monitor the Unity console for any compilation errors."
                    };
                    
                    // Request compilation
                    CompilationPipeline.RequestScriptCompilation();
                    
                    // If compilation doesn't start immediately, return a response
                    if (!EditorApplication.isCompiling)
                    {
                        // No compilation was needed
                        tcs.SetResult(new JObject
                        {
                            ["success"] = true,
                            ["type"] = "text",
                            ["message"] = "No script compilation was required. All scripts are already up to date.",
                            ["status"] = "no_compilation_needed"
                        });
                        _currentTcs = null;
                    }
                }
                else
                {
                    // Compilation is already happening
                    _currentTcs = tcs;
                    tcs.SetResult(new JObject
                    {
                        ["success"] = true,
                        ["type"] = "text",
                        ["message"] = "Script compilation is already in progress. Waiting for it to complete...",
                        ["status"] = "compilation_in_progress"
                    });
                }
            }
            catch (Exception ex)
            {
                McpLogger.LogError($"Error requesting script compilation: {ex.Message}");
                tcs.SetResult(McpUnitySocketHandler.CreateErrorResponse(
                    $"Failed to request script compilation: {ex.Message}",
                    "compilation_error"
                ));
            }
        }

        /// <summary>
        /// Called when compilation starts
        /// </summary>
        /// <param name="obj">Compilation context (unused)</param>
        private void OnCompilationStarted(object obj)
        {
            _isCompiling = true;
            McpLogger.LogInfo("Script compilation started.");
        }

        /// <summary>
        /// Called when compilation finishes
        /// </summary>
        /// <param name="obj">Compilation context (unused)</param>
        private void OnCompilationFinished(object obj)
        {
            _isCompiling = false;
            
            if (_currentTcs != null)
            {
                try
                {
                    // Check for compilation errors
                    var hasErrors = HasCompilationErrors();
                    
                    if (hasErrors)
                    {
                        var errorResponse = new JObject
                        {
                            ["success"] = false,
                            ["type"] = "text",
                            ["message"] = "Script compilation completed with errors. Check the Unity Console for detailed error messages.",
                            ["status"] = "compilation_failed",
                            ["hasErrors"] = true,
                            ["note"] = "Please fix the compilation errors before proceeding. You can view detailed error information in the Unity Console window."
                        };
                        
                        _currentTcs.SetResult(errorResponse);
                        McpLogger.LogWarning("Script compilation completed with errors.");
                    }
                    else
                    {
                        var successResponse = new JObject
                        {
                            ["success"] = true,
                            ["type"] = "text",
                            ["message"] = "Script compilation completed successfully. All scripts have been recompiled without errors.",
                            ["status"] = "compilation_successful",
                            ["hasErrors"] = false
                        };
                        
                        _currentTcs.SetResult(successResponse);
                        McpLogger.LogInfo("Script compilation completed successfully.");
                    }
                }
                catch (Exception ex)
                {
                    McpLogger.LogError($"Error processing compilation completion: {ex.Message}");
                    _currentTcs.SetResult(McpUnitySocketHandler.CreateErrorResponse(
                        $"Error processing compilation result: {ex.Message}",
                        "compilation_processing_error"
                    ));
                }
                finally
                {
                    _currentTcs = null;
                }
            }
        }        /// <summary>
        /// Check if there are any compilation errors in the console
        /// </summary>
        /// <returns>True if compilation errors exist</returns>
        private bool HasCompilationErrors()
        {
            try
            {
                // Get console log entries to check for compilation errors
                var logsJson = _consoleLogsService.GetLogsAsJson("error", 0, 100, false);
                var logs = logsJson["logs"] as JArray;
                
                if (logs != null)
                {
                    foreach (var log in logs)
                    {
                        var message = log["message"]?.ToString() ?? "";
                        if (message.Contains("error CS") || 
                            message.Contains("Compilation failed") ||
                            (message.Contains("Assembly") && message.Contains("error")))
                        {
                            return true;
                        }
                    }
                }
                
                return false;
            }
            catch (Exception ex)
            {
                McpLogger.LogWarning($"Could not check for compilation errors: {ex.Message}");
                // If we can't check, assume no errors to avoid false negatives
                return false;
            }
        }

        /// <summary>
        /// Cleanup event subscriptions when the tool is disposed
        /// </summary>
        ~RequestScriptCompilationTool()
        {
            try
            {
                CompilationPipeline.compilationStarted -= OnCompilationStarted;
                CompilationPipeline.compilationFinished -= OnCompilationFinished;
            }
            catch
            {
                // Ignore exceptions during cleanup
            }
        }
    }
}
