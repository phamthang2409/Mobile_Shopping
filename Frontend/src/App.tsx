import React, { useState, useEffect } from 'react';
import { Routes, Route, Navigate } from 'react-router-dom';
import './App.css';

// Layouts
import MainLayout from './MainLayout';
import AdminLayout from './component/admin/AdminLayout'; 

// Components
import Login from './component/Login';
import Register from './component/Register';
import CartPage from './component/CartPage'; 
import CheckoutPage from './component/CheckoutPage'; 

import AdminDashboard from './component/admin/AdminDashboard';
import ManageProducts from './component/admin/ManageProducts';
import ManageOrders from './component/admin/ManageOrders';

const App: React.FC = () => {
  // Kiểm tra cả 2 nơi để đảm bảo nút đăng nhập cũ/mới đều chạy được
  const [showLogin, setShowLogin] = useState<boolean>(() => {
    return sessionStorage.getItem('forceLogin') === 'true' || localStorage.getItem('forceLogin') === 'true';
  });

  const [isRegistering, setIsRegistering] = useState<boolean>(() => {
    return sessionStorage.getItem('isRegistering') === 'true' || localStorage.getItem('isRegistering') === 'true';
  });

  // Lấy thông tin user từ sessionStorage để hỗ trợ đăng nhập nhiều acc trên nhiều tab
  const user = JSON.parse(sessionStorage.getItem('user') || '{}');
  const userRole = (user?.role || user?.Role || '').toLowerCase();
  const isAdmin = userRole === 'admin';

  useEffect(() => {
    const syncLoginState = () => {
      // Nếu nút đăng nhập cũ lưu vào localStorage, mình chuyển nó sang sessionStorage và hiện lên
      if (localStorage.getItem('forceLogin') === 'true') {
        sessionStorage.setItem('forceLogin', 'true');
        localStorage.removeItem('forceLogin'); // Dọn dẹp để không bị lặp
      }
      setShowLogin(sessionStorage.getItem('forceLogin') === 'true');
    };

    // Lắng nghe thay đổi từ các tab khác hoặc từ chính tab này
    window.addEventListener('storage', syncLoginState);
    // Kiểm tra ngay lập tức khi component mount
    syncLoginState();

    return () => window.removeEventListener('storage', syncLoginState);
  }, []);

  const handleLoginSuccess = (userData: any) => {
    // Lưu vào sessionStorage 
    sessionStorage.setItem('user', JSON.stringify(userData));
    sessionStorage.removeItem('forceLogin');
    setShowLogin(false);

    // Chuyển hướng thông minh dựa trên Role
    const role = (userData.role || userData.Role || '').toLowerCase();
    if (role === 'admin') {
      window.location.href = '/admin';
    } else {
      window.location.reload(); 
    }
  };

  const closeLogin = () => {
    sessionStorage.removeItem('forceLogin');
    localStorage.removeItem('forceLogin'); 
    setShowLogin(false);
  };

  return (
    <div className="App">
      {showLogin ? (
        <div className="login-screen-bg">
          <div className="login-container-wrapper">
            <button className="btn-close-login" onClick={closeLogin}>✕ Xem tiếp với khách</button>
            {isRegistering ? (
              <Register switchToLogin={() => setIsRegistering(false)} />
            ) : (
              <Login 
                onLoginSuccess={handleLoginSuccess} 
                switchToRegister={() => setIsRegistering(true)} 
              />
            )}
          </div>
        </div>
      ) : (
        <Routes>
          {/* LUỒNG CHO KHÁCH HÀNG (SHOP) */}
          <Route path="/" element={<MainLayout />} />
          <Route path="/CartPage" element={<CartPage />} />
          <Route path="/checkout" element={<CheckoutPage />} />

          {/* LUỒNG CHO ADMIN (DASHBOARD) - BẢO VỆ BỞI ROLE */}
          <Route 
            path="/admin" 
            element={isAdmin ? <AdminLayout /> : <Navigate to="/" />}
          >
            <Route index element={<AdminDashboard />} />
            <Route path="products" element={<ManageProducts />} />
            <Route path="orders" element={<ManageOrders />} />
          </Route>
          
          <Route path="*" element={<Navigate to="/" />} />
        </Routes>
      )}
    </div>
  );
};

export default App;