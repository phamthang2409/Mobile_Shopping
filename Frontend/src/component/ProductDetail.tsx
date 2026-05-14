import React, { useState, useEffect, useCallback } from 'react';
import { useSelector } from 'react-redux';
import { useNavigate } from 'react-router-dom';
import { RootState } from '../Redux/store';
import '../CSS/ProductDetail.css';
import axiosClient from '../Api/axiosClient'; // Sử dụng đúng tên file axiosClient đã fix ở bước trước

interface ProductDetailProps {
  product: any;
  onBack: () => void;
}

const ProductDetail: React.FC<ProductDetailProps> = ({ product, onBack }) => {
  const { user } = useSelector((state: RootState) => state.auth);
  const navigate = useNavigate();
  
  const [cartCount, setCartCount] = useState(0);
  const DEFAULT_IMG = "https://placehold.jp/24/cccccc/ffffff/400x400.png?text=No+Image";

  // hàm cập nhật
  // Dùng useCallback để tránh render thừa và dễ dàng gọi lại
  const updateCartCount = useCallback(async () => {
    const userId = user?.id || user?.Id;
    
    if (!userId) {
      setCartCount(0);
      return;
    }

    try {
      const response = await axiosClient.get(`/Cart`);
      
      if (response.status === 200) {
        const cartItems = response.data;
        // Tính tổng quantity từ mảng sản phẩm trả về
        const total = cartItems.reduce((acc: number, item: any) => acc + (item.quantity || 0), 0);
        setCartCount(total);
      }
    } catch (error) {
      console.error("Lỗi lấy giỏ hàng:", error);
      // Fallback: Nếu lỗi do mạng, lấy tạm từ LocalStorage để người dùng đỡ hoang mang
      const storageKey = `cart_${userId}`;
      const localData = JSON.parse(localStorage.getItem(storageKey) || "[]");
      const localTotal = localData.reduce((acc: number, item: any) => acc + (item.quantity || 0), 0);
      setCartCount(localTotal);
    }
  }, [user]);

  // Cập nhật Badge khi component mount hoặc user thay đổi
  useEffect(() => { 
    updateCartCount();
  }, [updateCartCount]);

  if (!product) return null;

  const getProductName = () => {
    return product.productName || product.ProductName || product.name || product.Name || "Điện thoại";
  };

  // thêm vào giỏ hàng
  const handleAddToCart = async () => {
    if (!user) {
      alert("Bạn cần đăng nhập để thêm vào giỏ hàng nhé!");
      navigate('/login'); 
      return;
    }

    const productId = product.id || product.Id;

    try {
      //  Gọi API thêm vào giỏ.
      const response = await axiosClient.post(`/Cart/add`, {
        productId: productId,
        quantity: 1
      });

      if (response.status === 200 || response.status === 201) {
        await updateCartCount();
        
        alert(`Đã thêm ${getProductName()} vào giỏ hàng thành công!`);
        
        //  (Tùy chọn) Cập nhật LocalStorage để đồng bộ cache
        const userId = user.id || user.Id;
        const storageKey = `cart_${userId}`;
        const localCart = JSON.parse(localStorage.getItem(storageKey) || "[]");
        const existingIndex = localCart.findIndex((i: any) => (i.productId || i.ProductId) === productId);
        
        if (existingIndex > -1) {
          localCart[existingIndex].quantity += 1;
        } else {
          localCart.push({
            productId,
            productName: getProductName(),
            price: product.price || product.Price,
            imageUrl: product.imageUrl || product.ImageUrl,
            quantity: 1
          });
        }
        localStorage.setItem(storageKey, JSON.stringify(localCart));
      }
    } catch (error: any) {
      if (error.response?.status !== 401) {
        alert("Có lỗi xảy ra khi thêm vào giỏ hàng. Vui lòng thử lại!");
      }
    }
  };

  const handleBuyNow = () => {
    if (!user) {
      alert("Bạn vui lòng đăng nhập để thanh toán nhé!");
      navigate('/login');
      return;
    }

    const buyNowItem = {
      productId: product.id || product.Id,
      productName: getProductName(),
      price: product.price || product.Price,
      imageUrl: product.imageUrl || product.ImageUrl,
      quantity: 1
    };

    navigate('/checkout', { state: { items: [buyNowItem], fromCart: false } });
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