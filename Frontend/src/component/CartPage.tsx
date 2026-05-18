import React, { useEffect, useState, useCallback } from 'react';
import { useSelector } from 'react-redux';
import { RootState } from '../Redux/store';
import { useNavigate } from 'react-router-dom';
import axiosClient from '../Api/axiosClient'; 
import '../CSS/CartPage.css';

interface CartItem {
  productId: number; // ➔ ĐỒNG BỘ CHUẨN CHỮ THƯỜNG KHỚP VỚI HUNG THỦ JSON
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

  // --- 1. LẤY DỮ LIỆU TỪ SERVER VÀ CHUẨN HÓA ---
  const fetchCartFromServer = useCallback(async () => {
    if (!currentUserId) {
      setLoading(false);
      return;
    }
    try {
      const response = await axiosClient.get(`/Cart`);
      if (response.status === 200) {
        console.log("DỮ LIỆU GIỎ HÀNG THÔ TỪ SERVER:", response.data);

        const normalizedData = response.data.map((item: any) => {
          const pInfo = item.product || {}; 
          
          return {
            // ➔ FIX TRIỆT ĐỂ: Đồng bộ lưu vào key 'productId' viết thường
            productId: Number(item.productId || item.ProductId || pInfo.id || 0),
            productName: pInfo.productName || pInfo.name || "Sản phẩm",
            price: Number(pInfo.price || 0),
            quantity: Number(item.quantity || 0),
            imageUrl: pInfo.imageUrl || pInfo.image
          };
        });
        setItems(normalizedData);
      }
    } catch (error) {
      console.error("Lỗi lấy giỏ hàng:", error);
    } finally {
      setLoading(false);
    }
  }, [currentUserId]);

  useEffect(() => {
    fetchCartFromServer();
  }, [fetchCartFromServer]);

  // --- 2. THAY ĐỔI SỐ LƯỢNG SẢN PHẨM ---
  const handleQuantityChange = async (productId: number, currentQty: number, delta: number) => {
    try {
      const newQty = currentQty + delta;

      // 1. Nếu giảm về 0 -> Tiến hành xóa sản phẩm khỏi giỏ
      if (newQty <= 0) {
        if (window.confirm("Bạn có muốn xóa sản phẩm này khỏi giỏ hàng?")) {
          await axiosClient.delete(`/Cart/remove/${productId}`);
        } else return;
      } 
      // 2. Nếu tăng hoặc giảm số lượng hợp lệ (newQty > 0)
      else {
        await axiosClient.put(`/Cart/update-quantity`, {
          productId: Number(productId),
          quantity: Number(newQty) 
        });
      }

      // Tải lại dữ liệu mới để đồng bộ giao diện
      await fetchCartFromServer();
      window.dispatchEvent(new Event("cartUpdated"));

    } catch (error: any) {
      console.error("Lỗi cập nhật số lượng:", error.response?.data);
      alert("Không thể cập nhật số lượng sản phẩm.");
    }
  };

  // --- 3. CHUYỂN TIẾP SANG TRANG THANH TOÁN ---
  const handleCheckout = () => {
    if (items.length === 0) return alert("Giỏ hàng đang trống!");
    
    // Đóng gói mảng tường minh đảm bảo ôm theo biến productId sang CheckoutPage
    const checkoutItems = items.map(item => ({
      productId: Number(item.productId),
      productName: item.productName,
      price: item.price,
      quantity: item.quantity,
      imageUrl: item.imageUrl
    }));

    navigate('/checkout', { state: { items: checkoutItems, fromCart: true } });
  };

  // Tính toán tổng tiền an toàn
  const subTotal = items.reduce((acc, item) => acc + (item.price * item.quantity), 0);
  const tax = subTotal * 0.1;
  const total = subTotal + tax;

  if (loading) return <div className="cart-loading">Đang tải dữ liệu giỏ hàng...</div>;

  if (!user) {
    return (
      <div className="empty-cart-box">
        <p>Vui lòng đăng nhập để xem giỏ hàng.</p>
        <button className="btn-go-shop" onClick={() => navigate('/login')}>Đăng nhập</button>
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
        <span className="item-count-text">{items.length} loại sản phẩm</span>
      </div>

      <div className="cart-items-list">
        {items.length > 0 ? (
          items.map(item => (
            // ➔ CẬP NHẬT: Thay item.id bằng item.productId
            <div key={item.productId} className="cart-product-row">
              <div className="img-wrapper">
                 <img 
                  src={item.imageUrl || "https://placehold.jp/150x150.png?text=No+Image"} 
                  alt={item.productName} 
                  className="product-thumb" 
                />
              </div>
              
              <div className="product-info-col">
                <h4>{item.productName}</h4>
                <p className="product-price-text">{item.price.toLocaleString('vi-VN')}đ</p>
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
                 {(item.price * item.quantity).toLocaleString('vi-VN')}đ
              </div>
            </div>
          ))
        ) : (
          <div className="empty-cart-box">
            <p>Giỏ hàng hiện đang trống.</p>
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
          <div className="summary-line total-highlight">
            <span>Tổng thanh toán (đã gồm thuế):</span>
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