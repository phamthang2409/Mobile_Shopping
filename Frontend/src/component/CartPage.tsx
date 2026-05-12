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

  const currentUserId = user?.id || user?.Id;
  const storageKey = currentUserId ? `cart_${currentUserId}` : '';

  // Lấy dữ liệu từ LocalStorage khi Mount
  useEffect(() => {
    const loadCart = () => {
      if (storageKey) {
        const localData = localStorage.getItem(storageKey);
        if (localData) {
          try {
            setItems(JSON.parse(localData));
          } catch (e) {
            console.error("Lỗi parse dữ liệu giỏ hàng:", e);
            setItems([]);
          }
        } else {
          setItems([]);
        }
      }
      setLoading(false);
    };

    loadCart();

    // Lắng nghe sự kiện storage để đồng bộ nếu người dùng mở nhiều tab
    window.addEventListener('storage', loadCart);
    return () => window.removeEventListener('storage', loadCart);
  }, [storageKey]);

  // Chuyển sang trang thanh toán
  const handleCheckout = () => {
    if (items.length === 0) return alert("Giỏ hàng đang trống!");
    
    navigate('/checkout', { state: { items: items } });
  };

  // Thay đổi số lượng sản phẩm (Tăng/Giảm/Xóa)
  const handleQuantityChange = (productId: number, currentQty: number, delta: number) => {
    if (!storageKey) {
      alert("Vui lòng đăng nhập để thực hiện thao tác này!");
      return;
    }

    const newQty = currentQty + delta;
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
    
    window.dispatchEvent(new Event("storage"));
  };

  // Tính toán số tiền 
  const subTotal = items.reduce((acc, item) => acc + (Number(item.price) * Number(item.quantity)), 0);
  const tax = subTotal * 0.1;
  const total = subTotal + tax;

  if (loading) return <div className="cart-loading">Đang tải dữ liệu giỏ hàng...</div>;

  if (!user) {
    return (
      <div className="empty-cart-box">
        <p>Vui lòng đăng nhập để xem giỏ hàng của bạn.</p>
        <button className="btn-go-shop" onClick={() => navigate('/login')}>Đăng nhập ngay</button>
      </div>
    );
  }

  return (
    <div className="cart-page-container">
      <div className="cart-header-top">
        <button className="btn-back-to-shop" onClick={() => navigate('/Shop')}>
          ← Tiếp tục mua sắm
        </button>
        <h2>Giỏ hàng của bạn</h2>
        <span className="item-count-text">{items.length} sản phẩm</span>
      </div>

      <div className="cart-items-list">
        {items.length > 0 ? (
          items.map(item => (
            <div key={item.productId} className="cart-product-row">
              <div className="img-wrapper">
                 <img 
                  src={item.imageUrl || "https://placehold.jp/150x150.png?text=No+Image"} 
                  alt={item.productName} 
                  className="product-thumb" 
                />
              </div>
              
              <div className="product-info-col">
                <h4>{item.productName || "Sản phẩm không tên"}</h4>
                <p className="product-price-text">{Number(item.price).toLocaleString('vi-VN')}đ</p>
              </div>

              <div className="quantity-actions">
                <button 
                  className="btn-qty" 
                  onClick={() => handleQuantityChange(item.productId, item.quantity, -1)}
                >–</button>
                <span className="qty-val">{item.quantity}</span>
                <button 
                  className="btn-qty" 
                  onClick={() => handleQuantityChange(item.productId, item.quantity, 1)}
                >+</button>
              </div>

              <div className="item-total-col">
                 {(Number(item.price) * item.quantity).toLocaleString('vi-VN')}đ
              </div>
            </div>
          ))
        ) : (
          <div className="empty-cart-box">
            <p>Giỏ hàng của bạn hiện đang trống.</p>
            <button className="btn-go-shop" onClick={() => navigate('/Shop')}>Khám phá sản phẩm</button>
          </div>
        )}
      </div>

      {items.length > 0 && ( 
        <div className="cart-checkout-summary">
          <div className="summary-line">
            <span>Tạm tính:</span>
            <span>{subTotal.toLocaleString('vi-VN')}đ</span>
          </div>
          <div className="summary-line">
            <span>Thuế (10%):</span>
            <span>{tax.toLocaleString('vi-VN')}đ</span>
          </div>
          <div className="summary-line total-highlight">
            <span>Tổng thanh toán:</span>
            <span className="amount-val">{total.toLocaleString('vi-VN')}đ</span>
          </div>
          <button className="btn-checkout-final" onClick={handleCheckout}>
            TIẾN HÀNH THANH TOÁN
          </button>
        </div>
      )}
    </div>
  );
};

export default CartPage;