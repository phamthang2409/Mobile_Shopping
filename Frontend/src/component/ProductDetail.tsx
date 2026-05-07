import React, { useState, useEffect } from 'react';
import { useSelector } from 'react-redux';
import { useNavigate } from 'react-router-dom';
import { RootState } from '../Redux/store';
import '../CSS/ProductDetail.css';

interface ProductDetailProps {
  product: any;
  onBack: () => void;
}

const ProductDetail: React.FC<ProductDetailProps> = ({ product, onBack }) => {
  const { user } = useSelector((state: RootState) => state.auth);
  const navigate = useNavigate();
  
  // State quản lý số lượng hiển thị trên Badge giỏ hàng
  const [cartCount, setCartCount] = useState(0);

  const DEFAULT_IMG = "https://placehold.jp/24/cccccc/ffffff/400x400.png?text=No+Image";

  // Hàm tính toán số lượng từ LocalStorage để hiển thị badge
  const updateCartCount = () => {
    const userId = user?.id || user?.Id;
    if (userId) {
      const storageKey = `cart_${userId}`;
      const cart = JSON.parse(localStorage.getItem(storageKey) || "[]");
      const total = cart.reduce((acc: number, item: any) => acc + item.quantity, 0);
      setCartCount(total);
    }
  };

  useEffect(() => {
    updateCartCount();
  }, [user]);

  if (!product) return null;

  // Lấy tên sản phẩm chuẩn 
  const getProductName = () => {
    return product.productName || product.ProductName || product.name || product.Name || "Điện thoại";
  };

  // 1. Hàm Thêm vào giỏ hàng
  const handleAddToCart = () => {
    try {
      if (!user) {
        alert("Đăng nhập để thêm vào giỏ hàng nhé!");
        localStorage.setItem('forceLogin', 'true'); 
        window.location.reload();
        return;
      }

      const productId = product.id || product.Id;
      const storageKey = `cart_${user.id || user.Id}`;
      const localCart = localStorage.getItem(storageKey);
      let cartItems = localCart ? JSON.parse(localCart) : [];

      const existingItemIndex = cartItems.findIndex((item: any) => item.productId === productId);

      if (existingItemIndex > -1) {
        cartItems[existingItemIndex].quantity += 1;
      } else {
        cartItems.push({
          productId: productId,
          productName: getProductName(), 
          price: product.price || product.Price,
          imageUrl: product.imageUrl || product.ImageUrl,
          quantity: 1
        });
      }

      localStorage.setItem(storageKey, JSON.stringify(cartItems));
      updateCartCount();
      alert(`Đã thêm ${getProductName()} vào giỏ hàng!`);
    } catch (error) {
      console.error("Lỗi thêm giỏ hàng:", error);
    }
  };

  // 2. Hàm Mua Ngay
  const handleBuyNow = () => {
    if (!user) {
      alert("Đăng nhập để thực hiện thanh toán nhé!");
      localStorage.setItem('forceLogin', 'true');
      window.location.reload();
      return;
    }

    const buyNowItem = {
      productId: product.id || product.Id,
      productName: getProductName(),
      price: product.price || product.Price,
      imageUrl: product.imageUrl || product.ImageUrl,
      quantity: 1
    };

    // Chuyển sang trang checkout và chỉ truyền mảng chứa 1 sản phẩm này
    navigate('/checkout', { state: { items: [buyNowItem] } });
  };

  return (
    <div className="detail-container">
      <div className="deatail-header">
        <div className="breadcrumb" onClick={onBack} style={{ cursor: 'pointer' }}>
          <span>Shop</span> / <span className="current-path">Product</span>
        </div>
        
        <div className="cart-icon-wrapper" onClick={() => navigate('/CartPage')} style={{ cursor: 'pointer', position: 'relative' }}>
          <button className="icon-cart"> 🛒 </button>
          {cartCount > 0 && (
            <span className="cart-badge">{cartCount}</span>
          )}
        </div>
      </div>
      
      <div className="detail-content">
        <div className="detail-left">
          <div className="main-image-wrapper">
            <img 
              src={product.imageUrl || product.ImageUrl || DEFAULT_IMG} 
              alt={getProductName()} 
              onError={(e) => { (e.target as HTMLImageElement).src = DEFAULT_IMG; }}
            />
          </div>
          <div className="small-images">
            {[1, 2, 3].map((_, index) => (
              <img 
                key={index}
                src={product.imageUrl || product.ImageUrl || DEFAULT_IMG} 
                alt="thumb" 
                onError={(e) => { (e.target as HTMLImageElement).src = DEFAULT_IMG; }}
              />
            ))}
          </div>
        </div>

        <div className="detail-right">
          <h2 className="detail-title">{getProductName()}</h2>
          <p className="detail-desc">{product.description || product.Description || "Mô tả sản phẩm đang cập nhật..."}</p>
          <h2 className="detail-price">
            {(product.price || product.Price || 0).toLocaleString('vi-VN')} VND
          </h2>
          <div className="detail-stars">⭐⭐⭐⭐⭐</div>
          
          <div className="action-btns">
            <button className="btn-buy" onClick={handleBuyNow}>
              MUA NGAY
            </button>
            <button className="btn-add" onClick={handleAddToCart}>
              Thêm vào giỏ hàng
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};

export default ProductDetail;