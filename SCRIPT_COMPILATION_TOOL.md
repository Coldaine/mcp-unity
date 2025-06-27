# MCP Unity Script Compilation Tool

## Overview
This new MCP tool allows AI agents to request Unity script compilation and monitor the compilation progress. The tool provides comprehensive feedback about compilation status and any errors that occur.

## Files Created/Modified

### C# Unity Tool
- **File**: `Editor/Tools/RequestScriptCompilationTool.cs`
- **Description**: Implements the Unity-side tool that handles script compilation requests
- **Key Features**:
  - Calls `UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation()`
  - Monitors compilation progress using Unity's compilation events
  - Detects compilation errors by analyzing console logs
  - Provides detailed status feedback to the AI agent

### TypeScript Node.js Tool
- **File**: `Server~/src/tools/requestScriptCompilationTool.ts`
- **Description**: Implements the MCP protocol handler for the script compilation tool
- **Key Features**:
  - Defines the tool schema (no parameters required)
  - Formats compilation status messages for AI agents
  - Provides clear feedback about waiting times and completion status

### Tool Registration
- **File**: `Editor/UnityBridge/McpUnityServer.cs`
- **Modification**: Added registration for `RequestScriptCompilationTool`
- **File**: `Server~/src/index.ts`
- **Modification**: Added import and registration for the TypeScript tool

### Documentation
- **File**: `Editor/UnityBridge/McpUnityEditorWindow.cs`
- **Modification**: Added help documentation for the new tool

## Tool Behavior

### Usage
AI agents can invoke this tool with no parameters:
```typescript
await mcp.callTool('request_script_compilation', {});
```

### Status Messages
The tool provides detailed status feedback:

1. **Compilation Started**: Notifies that compilation has been requested and is starting
2. **Compilation In Progress**: Reports when compilation is already running
3. **Compilation Successful**: Confirms successful completion without errors
4. **Compilation Failed**: Reports completion with errors and directs to Unity Console
5. **No Compilation Needed**: Indicates scripts are already up to date

### Agent Guidance
The tool specifically includes messages to remind AI agents to:
- Wait for compilation to complete before proceeding with other Unity operations
- Check the Unity Console for detailed error information if compilation fails
- Monitor compilation progress rather than rushing to other tasks

### Error Detection
The tool analyzes Unity Console logs to detect compilation errors by looking for:
- Messages containing "error CS" (C# compiler errors)
- Messages containing "Compilation failed"
- Assembly-related error messages

## Example Usage Scenarios

1. **After Code Changes**: AI agents can request recompilation after making script modifications
2. **Build Preparation**: Ensure all scripts compile before running tests or builds
3. **Error Checking**: Verify that recent changes don't introduce compilation errors
4. **Development Workflow**: Part of automated development workflows where compilation verification is needed

## Agent Instructions
The tool includes specific guidance for AI agents:
- Wait for compilation completion before executing other Unity commands
- Help users identify and fix compilation errors when they occur
- Understand that this is an asynchronous operation that takes time
- Monitor the Unity Console for detailed error information when needed
