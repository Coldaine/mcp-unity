import { McpServer } from '@modelcontextprotocol/sdk/server/mcp.js';
import { McpUnity } from '../unity/mcpUnity.js';
import { McpUnityError, ErrorType } from '../utils/errors.js';
import * as z from 'zod';
import { Logger } from '../utils/logger.js';
import { CallToolResult } from '@modelcontextprotocol/sdk/types.js';

// Constants for the tool
const toolName = 'delete_asset';
const toolDescription = 'Deletes an asset from the Unity AssetDatabase';

// Parameter schema for the tool
const paramsSchema = z.object({
  assetPath: z.string().optional().describe('The path of the asset in the AssetDatabase'),
  guid: z.string().optional().describe('The GUID of the asset'),
  moveToTrash: z.boolean().default(true).describe('Whether to move the asset to trash (true) or permanently delete it (false). Defaults to true for safety.')
});

/**
 * Creates and registers the DeleteAsset tool with the MCP server
 * 
 * @param server The MCP server to register the tool with
 * @param mcpUnity The McpUnity instance to communicate with Unity
 * @param logger The logger instance for diagnostic information
 */
export function registerDeleteAssetTool(server: McpServer, mcpUnity: McpUnity, logger: Logger) {
  logger.info(`Registering tool: ${toolName}`);
  
  server.tool(
    toolName,
    toolDescription,
    paramsSchema.shape,
    async (params: any) => {
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
 * Handler function for the DeleteAsset tool
 * 
 * @param mcpUnity The McpUnity instance to communicate with Unity
 * @param params The validated parameters for the tool
 * @returns A promise that resolves to the tool execution result
 * @throws McpUnityError if validation fails or the request to Unity fails
 */
async function toolHandler(mcpUnity: McpUnity, params: any): Promise<CallToolResult> {
  // Validate that either assetPath or guid is provided
  if (!params.assetPath && !params.guid) {
    throw new McpUnityError(
      ErrorType.VALIDATION,
      "Either 'assetPath' or 'guid' must be provided"
    );
  }
  
  const response = await mcpUnity.sendRequest({
    method: toolName,
    params: {
      assetPath: params.assetPath,
      guid: params.guid,
      moveToTrash: params.moveToTrash ?? true
    }
  });
  
  if (!response.success) {
    throw new McpUnityError(
      ErrorType.TOOL_EXECUTION,
      response.message || `Failed to delete asset`
    );
  }
  
  return {
    content: [{
      type: response.type,
      text: response.message || `Successfully deleted asset`
    }]
  };
}
