import React, { useState } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import cartApi, { OrderRequestData } from '../Api/cartApi';
import '../CSS/CheckoutPage.css';

const CheckoutPage: React.FC = () => {
    const { state } = useLocation();
    const navigate = useNavigate();
    
    // Trạng thái gửi đơn hàng
    const [isSubmitting, setIsSubmitting] = useState(false);
    
    const items = state?.items || [];
    
    const [info, setInfo] = useState({
        name: '',      
        address: '',
        phone: '',
        note: '',      
        paymentMethod: 'COD'
    });

    // Tính toán tổng tiền
    const subTotal = items.reduce((acc: number, item: any) => acc + (item.price * item.quantity), 0);
    const total = subTotal + (subTotal * 0.1); // Thuế 10%

    const handleOrder = async () => {
    // 1. Kiểm tra thông tin đầu vào cơ bản
    if (!info.name.trim() || !info.address.trim() || !info.phone.trim()) {
        alert("Vui lòng nhập đầy đủ Tên, Địa chỉ và Số điện thoại!");
        return;
    }

    setIsSubmitting(true);

    try {
        // 2. Lấy dữ liệu user từ localStorage
        const userData = localStorage.getItem('user');
        if (!userData) {
            alert("Phiên đăng nhập hết hạn, vui lòng đăng nhập lại!");
            navigate('/login');
            return;
        }
        
        const user = JSON.parse(userData);

        const userIdRaw = user.Id || user.id || (user.User && (user.User.Id || user.User.id));
        const userId = Number(userIdRaw);

        // Kiểm tra nếu userId vẫn là NaN
        if (!userId || isNaN(userId)) {
            console.error("UserID bị NaN. Dữ liệu User hiện tại:", user);
            alert("Lỗi hệ thống: Không xác định được danh tính người dùng. Vui lòng đăng nhập lại!");
            navigate('/login');
            return;
        }

        // 4. Chuẩn hóa OrderData khớp với PascalCase của Backend DTO
        const orderData: OrderRequestData = {
            Name: info.name.trim(),
            Phone: info.phone.trim(),
            Address: info.address.trim(),
            Note: info.note.trim() || "",
            // Đảm bảo ProductId đúng kiểu Number và khớp tên trường Backend
            Items: items.map((item: any) => ({
                ProductId: Number(item.productId || item.ProductId || item.id || item.Id),
                Quantity: Number(item.quantity),
                Price: Number(item.price)
            }))
        };

        console.log(" Đang gửi đơn hàng cho UserId:", userId);
        console.log(" Dữ liệu gửi lên API:", orderData);
        
        // 5. Gọi API Checkout
        const response = await cartApi.checkout(userId, orderData);
        console.log(" Phản hồi từ Server:", response);

        // 6. Kiểm tra phản hồi
        if (response) {
            alert("🎉 Chúc mừng! Đơn hàng của bạn đã đặt thành công.");
            
            // Xóa giỏ hàng cục bộ của user
            localStorage.removeItem(`cart_${userId}`);
            
            // Phát sự kiện để Header cập nhật số lượng badge về 0
            window.dispatchEvent(new Event("storage"));
            
            navigate('/'); 
        }

    } catch (error: any) {
        console.error("❌ Lỗi đặt hàng:", error);
        
        const serverError = error.response?.data;
        let errorMsg = "Lỗi kết nối máy chủ, hết token rồi bẹn ei!";
        
        if (serverError) {
            errorMsg = serverError.message || (typeof serverError === 'string' ? serverError : "Dữ liệu gửi lên không hợp lệ (400)");
        }
        
        alert("Đặt hàng thất bại: " + errorMsg);
    } finally {
        setIsSubmitting(false);
    }
};

    // Nếu không có sản phẩm trong state, hiển thị thông báo trống
    if (items.length === 0) {
        return (
            <div className="checkout-empty">
                <h3>Giỏ hàng đang trống, không có gì để thanh toán!</h3>
                <button onClick={() => navigate('/')}>Quay lại cửa hàng</button>
            </div>
        );
    }

    return (
        <div className="checkout-wrapper">
            <div className="checkout-card">
                <h2 className="checkout-title">Xác Nhận Đơn Hàng</h2>
                
                <div className="checkout-grid">
                    {/* Phần nhập thông tin */}
                    <div className="checkout-section">
                        <h3><i className="fas fa-map-marker-alt"></i> Thông tin giao hàng</h3>
                        
                        <div className="input-group">
                            <label>Tên người nhận</label>
                            <input 
                                type="text"
                                placeholder="Nhập tên người nhận hàng" 
                                value={info.name}
                                onChange={e => setInfo({...info, name: e.target.value})}
                                disabled={isSubmitting}
                            />
                        </div>

                        <div className="input-group">
                            <label>Số điện thoại</label>
                            <input 
                                type="text"
                                placeholder="Số điện thoại liên hệ" 
                                value={info.phone}
                                onChange={e => setInfo({...info, phone: e.target.value})}
                                disabled={isSubmitting}
                            />
                        </div>

                        <div className="input-group">
                            <label>Địa chỉ nhận hàng</label>
                            <textarea 
                                placeholder="Số nhà, tên đường, quận/huyện..." 
                                rows={2}
                                value={info.address}
                                onChange={e => setInfo({...info, address: e.target.value})}
                                disabled={isSubmitting}
                            />
                        </div>

                        <div className="input-group">
                            <label>Ghi chú</label>
                            <input 
                                type="text"
                                placeholder="Ví dụ: Giao giờ hành chính..." 
                                value={info.note}
                                onChange={e => setInfo({...info, note: e.target.value})}
                                disabled={isSubmitting}
                            />
                        </div>
                    </div>

                    {/* Phần tóm tắt đơn hàng */}
                    <div className="checkout-section summary-section">
                        <h3><i className="fas fa-shopping-basket"></i> Tóm tắt sản phẩm</h3>
                        <div className="items-review">
                            {items.map((item: any) => (
                                <div key={item.productId} className="review-item">
                                    <span className="item-name">
                                        {item.productName} <strong>x{item.quantity}</strong>
                                    </span>
                                    <span className="item-price">
                                        {(item.price * item.quantity).toLocaleString('vi-VN')}đ
                                    </span>
                                </div>
                            ))}
                        </div>
                        
                        <div className="price-breakdown">
                            <div className="price-line">
                                <span>Tạm tính:</span>
                                <span>{subTotal.toLocaleString('vi-VN')}đ</span>
                            </div>
                            <div className="price-line">
                                <span>Thuế GTGT (10%):</span>
                                <span>{(subTotal * 0.1).toLocaleString('vi-VN')}đ</span>
                            </div>
                            <div className="price-line total-line">
                                <span>Tổng cộng:</span>
                                <span className="final-amount">{total.toLocaleString('vi-VN')}đ</span>
                            </div>
                        </div>

                        <button 
                            className={`btn-confirm-order ${isSubmitting ? 'disabled' : ''}`} 
                            onClick={handleOrder}
                            disabled={isSubmitting}
                        >
                            {isSubmitting ? "ĐANG XỬ LÝ..." : "XÁC NHẬN ĐẶT HÀNG"}
                        </button>
                        <button className="btn-back" onClick={() => navigate('/CartPage')}>
                            Quay lại giỏ hàng
                        </button>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default CheckoutPage;