import React, { useState, useEffect } from 'react';
import { Routes, Route, Navigate } from 'react-router-dom';
import './App.css';
import MainLayout from './MainLayout';
import Login from './component/Login';
import Register from './component/Register';
import CartPage from './component/CartPage'; 
import CheckoutPage from './component/CheckoutPage'; 

const App: React.FC = () => {
  // Trạng thái điều khiển việc hiển thị màn hình Login đè lên giao diện
  const [showLogin, setShowLogin] = useState<boolean>(() => {
    return localStorage.getItem('forceLogin') === 'true';
  });

  const [isRegistering, setIsRegistering] = useState<boolean>(() => {
  return localStorage.getItem('isRegistering') === 'true';
});

  // Lắng nghe thay đổi từ localStorage để đồng bộ trạng thái Login
  useEffect(() => {
    const handleStorageChange = () => {
      setShowLogin(localStorage.getItem('forceLogin') === 'true');
    };
    window.addEventListener('storage', handleStorageChange);
    return () => window.removeEventListener('storage', handleStorageChange);
  }, []);

  const handleLoginSuccess = (userData: any) => {
    localStorage.setItem('user', JSON.stringify(userData));
    localStorage.removeItem('forceLogin'); // Xóa trạng thái ép đăng nhập
    setShowLogin(false);
    window.location.reload(); // Tải lại để MainLayout cập nhật lại User mới
  };

  const closeLogin = () => {
    localStorage.removeItem('forceLogin');
    setShowLogin(false);
  };

  return (
    <div className="App">
      {/* 1. Nếu người dùng nhấn "Đăng nhập", hiện màn hình Login đè lên */}
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
          <Route path="/" element={<MainLayout />} />
          <Route path="/CartPage" element={<CartPage />} />
          <Route path="/checkout" element={<CheckoutPage />} />
          
          <Route path="*" element={<Navigate to="/" />} />
        </Routes>
      )}
    </div>
  );
};

export default App;