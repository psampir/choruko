/**
 * Parses error messages from backend API responses
 * Handles both validation error format and custom API error format
 */
export const parseErrorMessage = (errorMessage: string): string => {
  try {
    // Try to parse JSON error message from backend
    const errorData = JSON.parse(errorMessage);

    // Handle validation errors with specific field messages
    if (errorData.errors) {
      const fieldErrors = Object.values(errorData.errors).flat() as string[];
      return fieldErrors.join(" ");
    }

    // Handle general title/message errors
    if (errorData.title) {
      return errorData.title;
    }

    if (errorData.message) {
      return errorData.message;
    }

    // If JSON but no recognizable error structure
    return "An error occurred. Please try again.";
  } catch {
    // Check if the error is an HTML response (e.g., from nginx)
    if (errorMessage && errorMessage.trim().startsWith("<")) {
      // Parse common nginx error codes from HTML
      if (errorMessage.includes("413 Request Entity Too Large")) {
        return "File size too large. Maximum allowed size is 10MB.";
      }
      if (errorMessage.includes("502 Bad Gateway")) {
        return "Server is temporarily unavailable. Please try again later.";
      }
      if (errorMessage.includes("504 Gateway Timeout")) {
        return "Request timed out. Please try again.";
      }
      // Generic HTML error response
      return "An error occurred on the server. Please try again.";
    }
    
    // If not JSON or HTML, return the message as-is
    return errorMessage || "An unexpected error occurred. Please try again.";
  }
};
