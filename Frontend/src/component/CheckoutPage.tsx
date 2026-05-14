import React, { useState } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import axiosClient from '../Api/axiosClient'; // Sử dụng axiosClient chung để có Interceptor Token
import '../CSS/CheckoutPage.css';

const CheckoutPage: React.FC = () => {
    const { state } = useLocation();
    const navigate = useNavigate();
    const [isSubmitting, setIsSubmitting] = useState(false);
    
    // items: Danh sách sản phẩm thanh toán
    // fromCart: Nhận diện nguồn từ Giỏ hàng hay Mua ngay
    const items = state?.items || [];
    const fromCart = state?.fromCart || false;
    
    const [info, setInfo] = useState({
        name: '',      
        address: '',
        phone: '',
        note: '',      
        paymentMethod: 'COD' 
    });

    // Tính toán hiển thị
    const subTotal = items.reduce((acc: number, item: any) => acc + (item.price * item.quantity), 0);
    const total = subTotal + (subTotal * 0.1);

    const handleOrder = async () => {
        if (!info.name.trim() || !info.address.trim() || !info.phone.trim()) {
            alert("Vui lòng nhập đầy đủ Tên, Địa chỉ và Số điện thoại!");
            return;
        }

        setIsSubmitting(true);

        try {
            // Lấy thông tin User từ localStorage
            const userData = localStorage.getItem('user');
            if (!userData) {
                alert("Phiên đăng nhập hết hạn, vui lòng đăng nhập lại!");
                navigate('/login');
                return;
            }
            
            const user = JSON.parse(userData);
            const userId = user.id || user.Id;

            if (!userId) {
                alert("Lỗi hệ thống: Không xác định được danh tính người dùng.");
                return;
            }

            // Chuẩn bị dữ liệu gửi đi (Payload) 
            const orderData = {
                Name: info.name.trim(),
                Phone: info.phone.trim(),
                Address: info.address.trim(),
                Note: info.note.trim() || "",
                PaymentMethod: info.paymentMethod,
                Items: items.map((item: any) => ({
                    productId: Number(item.productId || item.id),
                    quantity: Number(item.quantity)
                }))
            };

            // FIX LỖI 404: Gọi đúng Route ở Backend là /api/Order/checkout/{userId}
            const response = await axiosClient.post(`/Order/checkout/${userId}`, orderData);

            if (response.status === 200 || response.status === 201) {
                alert("🎉 Chúc mừng Bạn! Đơn hàng của bạn đã đặt thành công.");
                
                // LOGIC XỬ LÝ GIỎ HÀNG SAU KHI ĐẶT HÀNG 
                if (fromCart) {
                    await axiosClient.delete('/Cart/clear');
                    // Dọn dẹp giỏ hàng trong LocalStorage
                    const cartKey = `cart_${userId}`;
                    localStorage.removeItem(cartKey);

                    window.dispatchEvent(new Event("cartUpdated"));
                }
                
                window.dispatchEvent(new Event("storage"));
                navigate('/'); 
            }

        } catch (error: any) {
            console.error("❌ Lỗi đặt hàng:", error);
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
                            <input type="text" placeholder="Nhập tên..." value={info.name} onChange={e => setInfo({...info, name: e.target.value})} disabled={isSubmitting}/>
                        </div>
                        <div className="input-group">
                            <label>Số điện thoại</label>
                            <input type="text" placeholder="Nhập số điện thoại..." value={info.phone} onChange={e => setInfo({...info, phone: e.target.value})} disabled={isSubmitting}/>
                        </div>
                        <div className="input-group">
                            <label>Địa chỉ nhận hàng</label>
                            <textarea rows={2} placeholder="Địa chỉ cụ thể..." value={info.address} onChange={e => setInfo({...info, address: e.target.value})} disabled={isSubmitting}/>
                        </div>
                        <div className="input-group">
                            <label>Ghi chú</label>
                            <input type="text" placeholder="Ví dụ: Giao giờ hành chính..." value={info.note} onChange={e => setInfo({...info, note: e.target.value})} disabled={isSubmitting}/>
                        </div>
                        <div className="input-group">
                            <label>Phương thức thanh toán</label>
                            <select value={info.paymentMethod} onChange={e => setInfo({...info, paymentMethod: e.target.value})} disabled={isSubmitting}>
                                <option value="COD">Thanh toán khi nhận hàng (COD)</option>
                                <option value="BANK_TRANSFER">Chuyển khoản ngân hàng</option>
                            </select>
                        </div>
                    </div>

                    <div className="checkout-section summary-section">
                        <h3><i className="fas fa-shopping-basket"></i> Tóm tắt đơn hàng</h3>
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
                        <button className="btn-back" onClick={() => navigate(-1)}>Quay lại</button>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default CheckoutPage;