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
    // If not JSON, return the message as-is
    return errorMessage || "An unexpected error occurred. Please try again.";
  }
};
