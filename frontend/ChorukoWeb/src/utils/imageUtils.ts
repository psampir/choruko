// Get the base URL for static files (remove /api from the API base URL)
const getStaticBaseUrl = () => {
  const apiBaseUrl = import.meta.env.VITE_API_BASE_URL;
  // Remove /api from the end if it exists
  return apiBaseUrl.replace(/\/api$/, '');
};

/**
 * Get the full URL for a profile picture
 * @param profilePicturePath - The relative path from the backend (e.g., "uploads/users/123.jpg")
 * @returns The full URL to the image, or null if no path provided
 * 
 * @example
 * // In your component:
 * import { getProfilePictureUrl } from '../utils/imageUtils';
 * import { useUser } from '../contexts/UserContext';
 * 
 * const MyComponent = () => {
 *   const { user } = useUser();
 *   const profilePictureUrl = getProfilePictureUrl(user?.profilePicturePath);
 *   
 *   return (
 *     <div>
 *       {profilePictureUrl ? (
 *         <img src={profilePictureUrl} alt="Profile" />
 *       ) : (
 *         <div>No profile picture</div>
 *       )}
 *     </div>
 *   );
 * };
 */
export const getProfilePictureUrl = (profilePicturePath?: string | null): string | null => {
  if (!profilePicturePath) {
    return null;
  }
  
  const baseUrl = getStaticBaseUrl();
  // Use the URL constructor to robustly join baseUrl and profilePicturePath
  const url = new URL(profilePicturePath, baseUrl.endsWith('/') ? baseUrl : baseUrl + '/');
  
  return url.toString();
};

