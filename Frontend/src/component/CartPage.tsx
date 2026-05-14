import React, { useEffect, useState, useCallback } from 'react';
import { useSelector } from 'react-redux';
import { RootState } from '../Redux/store';
import { useNavigate } from 'react-router-dom';
import axiosClient from '../Api/axiosClient'; 
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

  // --- 1. LẤY DỮ LIỆU (TRUY CẬP VÀO OBJECT 'product' BÊN TRONG) ---
  const fetchCartFromServer = useCallback(async () => {
    if (!currentUserId) {
      setLoading(false);
      return;
    }
    try {
      const response = await axiosClient.get(`/Cart`);
      if (response.status === 200) {
        // Map dữ liệu dựa trên cấu trúc thực tế từ console: { productId, quantity, product: { name, price... } }
        const normalizedData = response.data.map((item: any) => {
          const pInfo = item.product || {}; // Lấy thông tin từ object lồng 'product'
          
          return {
            productId: item.productId || item.ProductId,
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

  // --- 2. THAY ĐỔI SỐ LƯỢNG (GỬI CHỮ THƯỜNG ĐỂ FIX LỖI 400) ---
  const handleQuantityChange = async (productId: number, currentQty: number, delta: number) => {
  try {
    const newQty = currentQty + delta;

    // 1. Nếu giảm về 0 -> Xóa
    if (newQty <= 0) {
      if (window.confirm("Bạn có muốn xóa sản phẩm này?")) {
        await axiosClient.delete(`/Cart/remove/${productId}`);
      } else return;
    } 
    // 2. Nếu tăng hoặc giảm hợp lệ (newQty > 0)
    else {
      // Dùng API Update để gửi số lượng TUYỆT ĐỐI (ví dụ: 3, 4, 5...)
      await axiosClient.put(`/Cart/update-quantity`, {
        productId: Number(productId),
        quantity: Number(newQty) 
      });
    }

    await fetchCartFromServer();
    window.dispatchEvent(new Event("cartUpdated"));

  } catch (error: any) {
    console.error("Lỗi cập nhật:", error.response?.data);
    alert("Không thể cập nhật số lượng.");
  }
};

  const handleCheckout = () => {
    if (items.length === 0) return alert("Giỏ hàng đang trống!");
    navigate('/checkout', { state: { items: items, fromCart: true } });
  };

  // Tính toán tiền an toàn (đã được map thành Number ở trên)
  const subTotal = items.reduce((acc, item) => acc + (item.price * item.quantity), 0);
  const tax = subTotal * 0.1;
  const total = subTotal + tax;

  if (loading) return <div className="cart-loading">Đang tải dữ liệu...</div>;

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