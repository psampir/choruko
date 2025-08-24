import React from 'react';
import './Tasks.scss';

const Tasks: React.FC = () => {
  return (
    <div className="tasks-container">
      <div className="page-header">
        <h1>Household Tasks</h1>
        <p>Manage and assign household tasks</p>
      </div>
      
      <div className="tasks-content">
        <div className="tasks-section">
          <h2>Active Tasks</h2>
          <div className="task-list">
            <div className="task-item">
              <span className="task-icon">🧹</span>
              <div className="task-info">
                <h3>Clean Kitchen</h3>
                <p>Assigned to: You</p>
              </div>
              <span className="task-status pending">Pending</span>
            </div>
            
            <div className="task-item">
              <span className="task-icon">🗑️</span>
              <div className="task-info">
                <h3>Take Out Trash</h3>
                <p>Assigned to: John</p>
              </div>
              <span className="task-status completed">Completed</span>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default Tasks;
