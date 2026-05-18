import React, { useState } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import axiosClient from '../Api/axiosClient';
import '../CSS/CheckoutPage.css';

const CheckoutPage: React.FC = () => {
    const { state } = useLocation();
    const navigate = useNavigate();
    const [isSubmitting, setIsSubmitting] = useState(false);

    const items = state?.items || [];
    const fromCart = state?.fromCart || false;

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
            // Lấy userId từ localStorage
            const userData = localStorage.getItem('user');
            const user = userData ? JSON.parse(userData) : null;
            const userId = user?.id || user?.Id;

            if (!userId) {
                alert("Không tìm thấy thông tin người dùng. Vui lòng đăng nhập lại!");
                navigate('/login');
                return;
            }

            // Map items — hỗ trợ cả 'id' (từ ProductDTO mới) lẫn 'productId' (từ BuyNow)
            const formattedItems = items.map((item: any) => {
                const finalProductId = Number(item.productId || item.id || 0);
                if (finalProductId <= 0) {
                    console.error("❌ Không tìm thấy ID hợp lệ:", item);
                }
                return {
                    productId: finalProductId,
                    quantity: Number(item.quantity || 1)
                };
            });

            const orderData = {
                name: info.name.trim(),
                phone: info.phone.trim(),
                address: info.address.trim(),
                note: info.note.trim() || "",
                paymentMethod: info.paymentMethod,
                items: formattedItems
            };

            // ✅ Dùng axiosClient — tự gắn token qua interceptor
            // ✅ Truyền userId đúng route backend /Order/checkout/{userId}
            const response = await axiosClient.post(`/Order/checkout/${userId}`, orderData);

            if (response.status === 200 || response.status === 201) {
                alert("🎉 Đơn hàng đã đặt thành công!");

                if (fromCart) {
                    await axiosClient.delete('/Cart/clear');
                    window.dispatchEvent(new Event("cartUpdated"));
                }

                window.dispatchEvent(new Event("storage"));
                navigate('/');
            }

        } catch (error: any) {
            console.error("❌ Lỗi đặt hàng:", error.response?.data || error);

            const serverError = error.response?.data;
            let errorMessage = "Lỗi hệ thống, vui lòng thử lại.";
            if (serverError?.message) errorMessage = serverError.message;
            else if (serverError?.errors) errorMessage = JSON.stringify(serverError.errors);
            else if (typeof serverError === 'string') errorMessage = serverError;

            alert("Đặt hàng thất bại: " + errorMessage);
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

                    {/* Thông tin giao hàng */}
                    <div className="checkout-section">
                        <h3><i className="fas fa-map-marker-alt"></i> Thông tin giao hàng</h3>
                        <div className="input-group">
                            <label>Tên người nhận</label>
                            <input
                                type="text"
                                placeholder="Nhập tên..."
                                value={info.name}
                                onChange={e => setInfo({ ...info, name: e.target.value })}
                                disabled={isSubmitting}
                            />
                        </div>
                        <div className="input-group">
                            <label>Số điện thoại</label>
                            <input
                                type="text"
                                placeholder="Nhập số điện thoại..."
                                value={info.phone}
                                onChange={e => setInfo({ ...info, phone: e.target.value })}
                                disabled={isSubmitting}
                            />
                        </div>
                        <div className="input-group">
                            <label>Địa chỉ nhận hàng</label>
                            <textarea
                                rows={2}
                                placeholder="Địa chỉ cụ thể..."
                                value={info.address}
                                onChange={e => setInfo({ ...info, address: e.target.value })}
                                disabled={isSubmitting}
                            />
                        </div>
                        <div className="input-group">
                            <label>Ghi chú</label>
                            <input
                                type="text"
                                placeholder="Ví dụ: Giao giờ hành chính..."
                                value={info.note}
                                onChange={e => setInfo({ ...info, note: e.target.value })}
                                disabled={isSubmitting}
                            />
                        </div>
                        <div className="input-group">
                            <label>Phương thức thanh toán</label>
                            <select
                                value={info.paymentMethod}
                                onChange={e => setInfo({ ...info, paymentMethod: e.target.value })}
                                disabled={isSubmitting}
                            >
                                <option value="COD">Thanh toán khi nhận hàng (COD)</option>
                                <option value="BANK_TRANSFER">Chuyển khoản ngân hàng</option>
                            </select>
                        </div>
                    </div>

                    {/* Tóm tắt đơn hàng */}
                    <div className="checkout-section summary-section">
                        <h3><i className="fas fa-shopping-basket"></i> Tóm tắt đơn hàng</h3>
                        <div className="items-review">
                            {items.map((item: any, index: number) => (
                                <div key={item.productId || item.id || index} className="review-item">
                                    <span className="item-name">
                                        {item.productName || item.name} <strong>x{item.quantity}</strong>
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
                                <span>Thuế (10%):</span>
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
                        <button className="btn-back" onClick={() => navigate(-1)}>
                            Quay lại
                        </button>
                    </div>

                </div>
            </div>
        </div>
    );
};

export default CheckoutPage;