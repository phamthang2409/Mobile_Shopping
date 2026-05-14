import React from 'react';
import { Outlet, Link, useNavigate } from 'react-router-dom';

const AdminLayout: React.FC = () => {
  const navigate = useNavigate();
  const handleLogout = () => {
    localStorage.removeItem('user');
    navigate('/');
    window.location.reload();
  };

  return (
    <div style={{ display: 'flex', minHeight: '100vh' }}>
      {/* Sidebar */}
      <div style={{ width: '250px', background: '#2c3e50', color: 'white', padding: '20px' }}>
        <h2>Admin Panel</h2>
        <nav style={{ display: 'flex', flexDirection: 'column', gap: '15px', marginTop: '30px' }}>
          <Link to="/admin" style={{ color: 'white', textDecoration: 'none' }}>📊 Dashboard</Link>
          <Link to="/admin/products" style={{ color: 'white', textDecoration: 'none' }}>📦 Quản lý sản phẩm</Link>
          <Link to="/admin/orders" style={{ color: 'white', textDecoration: 'none' }}>🛒 Quản lý đơn hàng</Link>
          <hr />
          <button onClick={handleLogout} style={{ background: '#e74c3c', border: 'none', color: 'white', padding: '10px', cursor: 'pointer' }}>Đăng xuất</button>
        </nav>
      </div>

      {/* Main Content */}
      <div style={{ flex: 1, padding: '20px', background: '#f4f7f6' }}>
        <Outlet />
      </div>
    </div>
  );
};

export default AdminLayout;