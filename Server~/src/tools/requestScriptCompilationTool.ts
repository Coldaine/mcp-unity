import * as z from 'zod';
import { Logger } from '../utils/logger.js';
import { McpUnity } from '../unity/mcpUnity.js';
import { McpServer } from '@modelcontextprotocol/sdk/server/mcp.js';
import { McpUnityError, ErrorType } from '../utils/errors.js';
import { CallToolResult } from '@modelcontextprotocol/sdk/types.js';

// Constants for the tool
const toolName = 'request_script_compilation';
const toolDescription = 'Requests Unity to recompile all scripts and reports when compilation is complete or if errors occur';
const paramsSchema = z.object({
  // No parameters required for this tool
});

/**
 * Creates and registers the Request Script Compilation tool with the MCP server
 * This tool triggers Unity script compilation and monitors its progress
 * 
 * @param server The MCP server instance to register with
 * @param mcpUnity The McpUnity instance to communicate with Unity
 * @param logger The logger instance for diagnostic information
 */
export function registerRequestScriptCompilationTool(server: McpServer, mcpUnity: McpUnity, logger: Logger) {
  logger.info(`Registering tool: ${toolName}`);
    // Register this tool with the MCP server
  server.tool(
    toolName,
    toolDescription,
    paramsSchema.shape,
    async (params: any = {}) => {
      try {
        logger.info(`Executing tool: ${toolName}`, params);
        const result = await toolHandler(mcpUnity, params);
        logger.info(`Tool execution successful: ${toolName}`);
        return result;
      } catch (error) {
        logger.error(`Tool execution failed: ${toolName}`, error);
        throw error;
      }
    }
  );
}

/**
 * Handles script compilation requests
 * 
 * @param mcpUnity The McpUnity instance to communicate with Unity
 * @param params The parameters for the tool (currently unused)
 * @returns The result of the compilation request
 */
async function toolHandler(mcpUnity: McpUnity, params: any): Promise<CallToolResult> {
  try {
    // Execute the Unity tool to request script compilation
    const response = await mcpUnity.sendRequest({
      method: toolName,
      params: params || {}
    });
    
    const success = response.success || false;
    const message = response.message || 'Script compilation request completed';
    const status = response.status || 'unknown';
    const hasErrors = response.hasErrors || false;
    const isCompiling = response.isCompiling || false;
    
    // Format the response based on the compilation status
    let content = `**Unity Script Compilation**\n\n`;
    
    if (status === 'compilation_started') {
      content += `✅ **Status**: Compilation Started\n`;
      content += `📝 **Message**: ${message}\n\n`;
      content += `⚠️ **Important**: Please wait for compilation to complete before proceeding with other Unity operations.\n`;
      content += `🔍 **Monitor**: Check the Unity Console for any compilation errors or warnings.`;
    } else if (status === 'compilation_successful') {
      content += `✅ **Status**: Compilation Successful\n`;
      content += `📝 **Message**: ${message}\n\n`;
      content += `🎉 **Result**: All scripts have been successfully recompiled without errors.\n`;
      content += `✅ **Ready**: You can now proceed with other Unity operations.`;
    } else if (status === 'compilation_failed') {
      content += `❌ **Status**: Compilation Failed\n`;
      content += `📝 **Message**: ${message}\n\n`;
      content += `🐛 **Errors**: Compilation completed with errors.\n`;
      content += `🔧 **Action Required**: Please check the Unity Console and fix compilation errors before proceeding.\n`;
      content += `📋 **Tip**: Look for error messages starting with "error CS" in the Unity Console.`;
    } else if (status === 'compilation_in_progress') {
      content += `⏳ **Status**: Compilation In Progress\n`;
      content += `📝 **Message**: ${message}\n\n`;
      content += `⚠️ **Please Wait**: Compilation is already running. Wait for it to complete.`;
    } else if (status === 'no_compilation_needed') {
      content += `✅ **Status**: No Compilation Needed\n`;
      content += `📝 **Message**: ${message}\n\n`;
      content += `📄 **Info**: All scripts are already up to date.`;
    } else if (isCompiling) {
      content += `⏳ **Status**: Compilation Already Running\n`;
      content += `📝 **Message**: ${message}\n\n`;
      content += `⚠️ **Please Wait**: Another compilation is in progress.`;
    } else {
      content += `📝 **Message**: ${message}\n`;
      content += `📊 **Status**: ${status}`;
    }
    
    // Add additional context for agents
    if (status === 'compilation_started' || status === 'compilation_in_progress') {
      content += `\n\n---\n**For AI Assistants**: This tool will monitor compilation progress. ` +
                `Wait for a completion message before executing other Unity-related commands. ` +
                `If compilation fails, help the user identify and fix the errors shown in the Unity Console.`;
    }

    return {
      content: [
        {
          type: 'text',
          text: content
        }
      ],
      isError: !success
    };
  } catch (error) {
    if (error instanceof McpUnityError) {
      throw error;
    }
      throw new McpUnityError(
      ErrorType.TOOL_EXECUTION,
      `Failed to request script compilation: ${error instanceof Error ? error.message : String(error)}`,
      { originalError: error }
    );
  }
}
