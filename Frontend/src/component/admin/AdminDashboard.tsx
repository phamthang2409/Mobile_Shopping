import React from 'react';

const AdminDashboard: React.FC = () => {
  return (
    <div>
      <h1>Thống kê hệ thống</h1>
      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(3, 1fr)', gap: '20px', marginTop: '20px' }}>
        <div style={{ background: 'white', padding: '20px', borderRadius: '8px', boxShadow: '0 2px 4px rgba(0,0,0,0.1)' }}>
          <h3>Tổng đơn hàng</h3>
          <p style={{ fontSize: '24px', fontWeight: 'bold', color: '#3498db' }}>128</p>
        </div>
        <div style={{ background: 'white', padding: '20px', borderRadius: '8px', boxShadow: '0 2px 4px rgba(0,0,0,0.1)' }}>
          <h3>Doanh thu</h3>
          <p style={{ fontSize: '24px', fontWeight: 'bold', color: '#2ecc71' }}>450.000.000 VNĐ</p>
        </div>
        <div style={{ background: 'white', padding: '20px', borderRadius: '8px', boxShadow: '0 2px 4px rgba(0,0,0,0.1)' }}>
          <h3>Sản phẩm</h3>
          <p style={{ fontSize: '24px', fontWeight: 'bold', color: '#f1c40f' }}>52</p>
        </div>
      </div>
    </div>
  );
};

export default AdminDashboard;