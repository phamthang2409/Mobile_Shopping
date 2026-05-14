import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import ShopPage from './component/ShopPage';
import CartPage from './component/CartPage';
import MyProfile from './component/MyProfile';
import ProductDetail from './component/ProductDetail';
import './MainLayout.css';

const MainLayout: React.FC = () => {
  const [activeTab, setActiveTab] = useState<'shop' | 'cart' | 'profile' | 'detail'>('shop');
  const [selectedProduct, setSelectedProduct] = useState<any>(null);
  const [isCollapsed, setIsCollapsed] = useState(false);
  const [user, setUser] = useState<any>(null);
  const navigate = useNavigate();

  useEffect(() => {
    // Ưu tiên đọc từ sessionStorage để đồng bộ với App.tsx
    const storedUser = sessionStorage.getItem('user');
    if (storedUser) {
      setUser(JSON.parse(storedUser));
    } else {
      // Nếu sessionStorage trống, kiểm tra và dọn dẹp localStorage cũ
      localStorage.removeItem('user');
      setUser(null);
    }
  }, []);

  const handleLogout = () => {
    if (window.confirm("Bạn muốn đăng xuất à?")) {
      // Xóa sạch cả 2 nơi để không bị dính "admin" cũ
      sessionStorage.clear();
      localStorage.clear();
      setUser(null);
      window.location.href = '/'; 
    }
  };

  const handleGoToLogin = () => {
    // Dùng sessionStorage để App.tsx nhận diện được lệnh mở màn hình Login
    sessionStorage.setItem('forceLogin', 'true');
    sessionStorage.setItem('isRegistering', 'false'); 
    window.location.reload();
  };

  const handleGoToRegister = () => {
    sessionStorage.setItem('forceLogin', 'true');
    sessionStorage.setItem('isRegistering', 'true'); 
    window.location.reload();
  };

  const handleProductClick = (product: any) => {
    setSelectedProduct(product);
    setActiveTab('detail');
  };

  const handleTabChange = (tab: 'shop' | 'cart' | 'profile') => {
    if (tab !== 'shop' && !user) {
      alert("Đăng nhập đei ben ei");
      handleGoToLogin();
      return;
    }
    setActiveTab(tab);
  };

  return (
    <div className="app-container">
      {/* --- HEADER --- */}
      <header className="app-header">
        <div className="header-logo" onClick={() => setActiveTab('shop')}>
          <img src="/logo.png" alt="Logo" className="app-logo-small" />
          <span className="brand-name">Mobile Shopping</span>
        </div>
        
        <div className="header-right-actions">
          {user ? (
            <div className="user-logged-in">
              <div className="user-info" onClick={() => handleTabChange('profile')}>
                <span className="user-display-name">Chào, {user.userName || user.Name}</span>
                <img src="/avatar.png" alt="Avatar" className="mini-avatar" />
              </div>
              <button className="btn-logout-header" onClick={handleLogout}>Đăng xuất</button>
            </div>
          ) : (
            <div className="guest-actions">
              <button className="btn-login-header" onClick={handleGoToLogin}>Đăng nhập</button>
              <button className="btn-register-header" onClick={handleGoToRegister}>Đăng ký</button>
            </div>
          )}
        </div>
      </header>

      <div className="app-body">
        <aside className={`sidebar ${isCollapsed ? 'collapsed' : ''}`}>
          <div className="sidebar-toggle-wrapper" onClick={() => setIsCollapsed(!isCollapsed)}>
            <span>☰</span>
            {!isCollapsed && <span style={{ fontWeight: 'bold' }}>Menu</span>}
          </div>

          <ul className="nav-menu">
            <li className={`nav-item ${activeTab === 'shop' || activeTab === 'detail' ? 'active' : ''}`} onClick={() => handleTabChange('shop')}>
              <span>🏠</span> {!isCollapsed && <span>Shop</span>}
            </li>
            <li className={`nav-item ${activeTab === 'cart' ? 'active' : ''}`} onClick={() => handleTabChange('cart')}>
              <span>🛒</span> {!isCollapsed && <span>Cart</span>}
            </li>
            <li className={`nav-item ${activeTab === 'profile' ? 'active' : ''}`} onClick={() => handleTabChange('profile')}>
              <span>👤</span> {!isCollapsed && <span>My Profile</span>}
            </li>
          </ul>
        </aside>

        <main className="main-view">
          {activeTab === 'shop' && <ShopPage onSelectProduct={handleProductClick} />}
          {activeTab === 'detail' && selectedProduct && (
            <ProductDetail product={selectedProduct} onBack={() => setActiveTab('shop')} />
          )}
          {activeTab === 'cart' && user && <CartPage />}
          {activeTab === 'profile' && user && <MyProfile />}
        </main>
      </div>
    </div>
  );
};

export default MainLayout;