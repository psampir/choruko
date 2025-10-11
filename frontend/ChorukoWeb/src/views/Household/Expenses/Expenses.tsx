import React from 'react';
import './Expenses.scss';

const Expenses: React.FC = () => {
  return (
    <div className="expenses-container">
      <div className="page-header">
        <h1>Household Expenses</h1>
        <p>Track and manage shared expenses</p>
      </div>
      
      <div className="expenses-content">
        <div className="expenses-summary">
          <div className="summary-card">
            <h3>This Month</h3>
            <p className="amount">$1,234.56</p>
          </div>
          <div className="summary-card">
            <h3>Your Share</h3>
            <p className="amount">$617.28</p>
          </div>
        </div>
        
        <div className="expenses-section">
          <h2>Recent Expenses</h2>
          <div className="expense-list">
            <div className="expense-item">
              <span className="expense-icon">🛒</span>
              <div className="expense-info">
                <h3>Groceries</h3>
                <p>Paid by: You</p>
              </div>
              <span className="expense-amount">$85.42</span>
            </div>
            
            <div className="expense-item">
              <span className="expense-icon">⚡</span>
              <div className="expense-info">
                <h3>Electricity Bill</h3>
                <p>Paid by: John</p>
              </div>
              <span className="expense-amount">$120.00</span>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default Expenses;
