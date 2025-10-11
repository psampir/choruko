import React from 'react';
import './Tasks.scss';
import Button from '../../../components/Button';

const Tasks: React.FC = () => {
  return (
    <div className="tasks-container">
      <div className="page-header">
        <h1>Tasks</h1>
        <Button variant='primary'>Add</Button>
      </div>
      
      <div className="tasks-content">
        <div className="tasks-content overtime__section">
          <h2>Overtime</h2>
          <div className='task-info'>
            <div className='task-info-text'>
              <h3>Activity name</h3>
              <p>Activity description</p>
            </div>
            <div className='task-info-additional-data'>
              <div className='assignies'>
                <div className='assignies-avatars'>
                  <img src="https://randomuser.me/api/portraits/men/1.jpg" alt='assignees'/>
                </div>
                <h3 className='task-info-time-left'>
                  4h
                </h3>
              </div>
            </div>
          </div>
        </div>

        <div className="tasks-content to-do__section">

        </div>
        <div className="tasks-content overtime__section">

        </div>
      </div>
    </div>
  );
};

export default Tasks;
