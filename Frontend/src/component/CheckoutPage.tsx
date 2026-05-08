import React, { useState } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import cartApi, { OrderRequestData } from '../Api/cartApi';
import '../CSS/CheckoutPage.css';

const CheckoutPage: React.FC = () => {
    const { state } = useLocation();
    const navigate = useNavigate();
    const [isSubmitting, setIsSubmitting] = useState(false);
    
    const items = state?.items || [];
    
    const [info, setInfo] = useState({
        name: '',      
        address: '',
        phone: '',
        note: '',      
        paymentMethod: 'COD'
    });

    const subTotal = items.reduce((acc: number, item: any) => acc + (item.price * item.quantity), 0);
    const total = subTotal + (subTotal * 0.1);

    const handleOrder = async () => {
        if (!info.name.trim() || !info.address.trim() || !info.phone.trim()) {
            alert("Vui lòng nhập đầy đủ Tên, Địa chỉ và Số điện thoại!");
            return;
        }

        setIsSubmitting(true);

        try {
            const userData = localStorage.getItem('user');
            if (!userData) {
                alert("Phiên đăng nhập hết hạn, vui lòng đăng nhập lại!");
                navigate('/login');
                return;
            }
            
            const user = JSON.parse(userData);
            const userIdRaw = user.Id || user.id || (user.User && (user.User.Id || user.User.id));
            const userId = Number(userIdRaw);

            if (!userId || isNaN(userId)) {
                alert("Lỗi hệ thống: Không xác định được danh tính người dùng.");
                navigate('/login');
                return;
            }

            const orderData: OrderRequestData = {
                Name: info.name.trim(),
                Phone: info.phone.trim(),
                Address: info.address.trim(),
                Note: info.note.trim() || "",
                Items: items.map((item: any) => ({
                    ProductId: Number(item.productId || item.ProductId || item.id || item.Id),
                    Quantity: Number(item.quantity),
                    Price: Number(item.price)
                }))
            };

            const response = await cartApi.checkout(userId, orderData);

            if (response) {
                alert("🎉 Chúc mừng! Đơn hàng của bạn đã đặt thành công.");
                const cartKey = `cart_${userId}`;
                const localCartData = localStorage.getItem(cartKey);

                if (localCartData) {
                    const fullCart = JSON.parse(localCartData);
                    
                    // Lọc: Chỉ giữ lại những sản phẩm KHÔNG nằm trong danh sách vừa thanh toán
                    const remainingCart = fullCart.filter((cartItem: any) => {
                        const cartItemId = cartItem.productId || cartItem.id;
                        return !items.some((orderedItem: any) => 
                            (orderedItem.productId || orderedItem.id) === cartItemId
                        );
                    });

                    if (remainingCart.length > 0) {
                        // Nếu còn hàng thì lưu lại phần còn lại
                        localStorage.setItem(cartKey, JSON.stringify(remainingCart));
                    } else {
                        // Nếu đã mua sạch thì mới xóa key
                        localStorage.removeItem(cartKey);
                    }
                }

                window.dispatchEvent(new Event("storage"));
                navigate('/'); 
            }

        } catch (error: any) {
            console.error("❌ Lỗi đặt hàng:", error);
            if (!localStorage.getItem('token')) {
                alert("Phiên làm việc đã hết hạn. Vui lòng đăng nhập lại!");
                navigate('/login');
                return;
            }
            const serverError = error.response?.data;
            alert("Đặt hàng thất bại: " + (serverError?.message || "Không thể kết nối máy chủ."));
        } finally {
            setIsSubmitting(false);
        }
    };

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
                    <div className="checkout-section">
                        <h3><i className="fas fa-map-marker-alt"></i> Thông tin giao hàng</h3>
                        <div className="input-group">
                            <label>Tên người nhận</label>
                            <input type="text" value={info.name} onChange={e => setInfo({...info, name: e.target.value})} disabled={isSubmitting}/>
                        </div>
                        <div className="input-group">
                            <label>Số điện thoại</label>
                            <input type="text" value={info.phone} onChange={e => setInfo({...info, phone: e.target.value})} disabled={isSubmitting}/>
                        </div>
                        <div className="input-group">
                            <label>Địa chỉ nhận hàng</label>
                            <textarea rows={2} value={info.address} onChange={e => setInfo({...info, address: e.target.value})} disabled={isSubmitting}/>
                        </div>
                        <div className="input-group">
                            <label>Ghi chú</label>
                            <input type="text" value={info.note} onChange={e => setInfo({...info, note: e.target.value})} disabled={isSubmitting}/>
                        </div>
                    </div>

                    <div className="checkout-section summary-section">
                        <h3><i className="fas fa-shopping-basket"></i> Tóm tắt sản phẩm</h3>
                        <div className="items-review">
                            {items.map((item: any) => (
                                <div key={item.productId || item.id} className="review-item">
                                    <span className="item-name">{item.productName || item.name} <strong>x{item.quantity}</strong></span>
                                    <span className="item-price">{(item.price * item.quantity).toLocaleString('vi-VN')}đ</span>
                                </div>
                            ))}
                        </div>
                        <div className="price-breakdown">
                            <div className="price-line"><span>Tạm tính:</span><span>{subTotal.toLocaleString('vi-VN')}đ</span></div>
                            <div className="price-line"><span>Thuế (10%):</span><span>{(subTotal * 0.1).toLocaleString('vi-VN')}đ</span></div>
                            <div className="price-line total-line"><span>Tổng cộng:</span><span className="final-amount">{total.toLocaleString('vi-VN')}đ</span></div>
                        </div>
                        <button className={`btn-confirm-order ${isSubmitting ? 'disabled' : ''}`} onClick={handleOrder} disabled={isSubmitting}>
                            {isSubmitting ? "ĐANG XỬ LÝ..." : "XÁC NHẬN ĐẶT HÀNG"}
                        </button>
                        <button className="btn-back" onClick={() => navigate('/CartPage')}>Quay lại giỏ hàng</button>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default CheckoutPage;