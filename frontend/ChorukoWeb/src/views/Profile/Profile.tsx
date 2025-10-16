import React from 'react';
import { useAuth } from '../../contexts/AuthContext';
import { getProfilePictureUrl } from '../../utils/imageUtils';
import './Profile.scss';

const Profile: React.FC = () => {
  const { user, logout } = useAuth();

  const handleLogout = async () => {
    await logout();
  };

  const profilePictureUrl = getProfilePictureUrl(user?.profilePicturePath);

  return (
    <div className="profile">
      <div className="profile-header">
        <div className="profile-avatar">
          {profilePictureUrl ? (
            <img src={profilePictureUrl} alt="Profile" style={{ width: '100%', height: '100%', objectFit: 'cover', borderRadius: '50%' }} />
          ) : (
            user?.firstName?.charAt(0)?.toUpperCase() || '👤'
          )}
        </div>
        <h1>Profile</h1>
      </div>
      
      <div className="profile-content">
        <div className="profile-section">
          <h3>Personal Information</h3>
          <div className="profile-field">
            <label>First Name</label>
            <span>{user?.firstName || 'Not set'}</span>
          </div>
          <div className="profile-field">
            <label>Last Name</label>
            <span>{user?.lastName || 'Not set'}</span>
          </div>
          <div className="profile-field">
            <label>Email</label>
            <span>{user?.email || 'Not set'}</span>
          </div>
        </div>
        
        <div className="profile-actions">
          <button className="btn-secondary">Edit Profile</button>
          <button className="btn-secondary">Change Password</button>
          <button className="btn-danger" onClick={handleLogout}>
            🚪 Logout
          </button>
        </div>
      </div>
    </div>
  );
};

export default Profile;
