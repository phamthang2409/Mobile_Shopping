import React, { useEffect, useState } from 'react';
import { useSelector } from 'react-redux';
import { RootState } from '../Redux/store';
import { useNavigate } from 'react-router-dom';
import '../CSS/CartPage.css';

interface CartItem {
  productId: number;
  productName: string;
  price: number;
  quantity: number;
  imageUrl?: string;
}

const CartPage: React.FC = () => {
  const [items, setItems] = useState<CartItem[]>([]);
  const [loading, setLoading] = useState(true);
  const { user } = useSelector((state: RootState) => state.auth);
  const navigate = useNavigate();

  // Tạo key định danh dựa trên ID người dùng để tránh lẫn lộn dữ liệu giữa các tài khoản
  const storageKey = user ? `cart_${user.id || user.Id}` : '';

  // Lấy dữ liệu từ LocalStorage khi Component được gắn vào (Mount)
  useEffect(() => {
    if (user && storageKey) {
      const localData = localStorage.getItem(storageKey);
      if (localData) {
        try {
          setItems(JSON.parse(localData));
        } catch (e) {
          console.error("Lỗi đọc dữ liệu giỏ hàng:", e);
          setItems([]);
        }
      }
      setLoading(false);
    }
  }, [user, storageKey]);

  const handleCheckout = () => {
    if (items.length === 0) return alert("Giỏ hàng đang trống!");
    
    navigate('/checkout', { state: { items: items } });
  };

  // Thay đổi số lượng sản phẩm
  const handleQuantityChange = (productId: number, currentQty: number, delta: number) => {
    const newQty = currentQty + delta;
    if (!user || !storageKey) return;

    let updatedItems: CartItem[];

    if (newQty <= 0) {
      const confirmDelete = window.confirm("Bạn có muốn xóa sản phẩm này khỏi giỏ hàng?");
      if (!confirmDelete) return;
      updatedItems = items.filter(item => item.productId !== productId);
    } else {
      updatedItems = items.map(item => 
        item.productId === productId ? { ...item, quantity: newQty } : item
      );
    }

    setItems(updatedItems);
    localStorage.setItem(storageKey, JSON.stringify(updatedItems));
  };

  // Tính toán tiền dựa trên state hiện tại (Sử dụng Number() để tránh lỗi cộng chuỗi)
  const subTotal = items.reduce((acc, item) => acc + (Number(item.price) * Number(item.quantity)), 0);
  const tax = subTotal * 0.1;
  const total = subTotal + tax;

  if (loading) return <div className="cart-loading">Đang tải dữ liệu giỏ hàng...</div>;

  return (
    <div className="cart-page-container">
      <div className="cart-header-top">
        <button className="btn-back-to-shop" onClick={() => navigate('/')}>
            ← Tiếp tục mua sắm
          </button>
        <h2>Giỏ hàng của bạn</h2>
        <span className="item-count-text">{items.length} sản phẩm</span>
      </div>

      <div className="cart-items-list">
        {items.length > 0 ? (
          items.map(item => (
            <div key={item.productId} className="cart-product-row">
              <img 
                src={item.imageUrl || "https://placehold.jp/150x150.png?text=No+Image"} 
                alt={item.productName} 
                className="product-thumb" 
              />
              <div className="product-info-col">
                <h4>{item.productName || " Sản phẩm "}</h4>
                <h4 className="product-price-text">{Number(item.price).toLocaleString('vi-VN')} VND</h4>
              </div>
              <div className="quantity-actions">
                <button 
                  className="btn-qty" 
                  onClick={() => handleQuantityChange(item.productId, item.quantity, -1)}
                >-</button>
                <span className="qty-val">{item.quantity}</span>
                <button 
                  className="btn-qty" 
                  onClick={() => handleQuantityChange(item.productId, item.quantity, 1)}
                >+</button>
              </div>
            </div>
          ))
        ) : (
          <div className="empty-cart-box">
            <p>Giỏ hàng của bạn đang trống.</p>
            <button className="btn-go-shop" onClick={() => navigate('/')}>Quay lại cửa hàng</button>
          </div>
        )}
      </div>

      {items.length > 0 && (  
        <div className="cart-checkout-summary">
          <div className="summary-line">
            <span>Tạm tính:</span>
            <span>{subTotal.toLocaleString('vi-VN')} VND</span>
          </div>
          <div className="summary-line">
            <span>Thuế (10%):</span>
            <span>{tax.toLocaleString('vi-VN')} VND</span>
          </div>
          <div className="summary-line total-highlight">
            <span>Tổng thanh toán:</span>
            <span className="amount-val">{total.toLocaleString('vi-VN')} VND</span>
          </div>
          <button className="btn-checkout-final" onClick={handleCheckout}>
            THANH TOÁN
          </button>
        </div>
      )}
    </div>
  );
};

export default CartPage;