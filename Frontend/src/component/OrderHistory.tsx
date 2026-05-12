import React, { useEffect, useState } from 'react';
import { useSelector } from 'react-redux';
import { RootState } from '../Redux/store';
import { useNavigate } from 'react-router-dom';
import axiosClient from '../Api/axiosClient';
import '../CSS/OrderHistory.css';

interface OrderDetail {
    productId: number;
    productName: string;
    quantity: number;
    price: number;
}

interface Bill {
    id: number;
    createdDate: string;
    paymentMethod: string;
    totalAmount: number;
}

interface Order {
    id: number;
    orderDate: string;
    totalAmount: number;
    status: number; 
    name: string;
    address: string;
    phone: string;
    orderDetails: OrderDetail[];
    bill?: Bill;
}

const OrderHistory: React.FC = () => {
    const [orders, setOrders] = useState<Order[]>([]);
    const [loading, setLoading] = useState(true);
    const { user } = useSelector((state: RootState) => state.auth);
    const navigate = useNavigate();

    useEffect(() => {
        const fetchOrders = async () => {
            if (!user) {
                setLoading(false);
                return;
            }

            try {
                const userId = user.id || user.Id ;
                const response = await axiosClient.get(`/Order/history/${userId}`);
                setOrders(response.data);
            } catch (error) {
                console.error("Lỗi khi lấy lịch sử đơn hàng:", error);
            } finally {
                setLoading(false);
            }
        };

        fetchOrders();
    }, [user]);

    const getStatusText = (status: number) => {
        switch (status) {
            case 0: return { text: "Chờ xử lý", class: "status-pending" };
            case 1: return { text: "Hoàn thành", class: "status-completed" };
            case 2: return { text: "Đã hủy", class: "status-cancelled" };
            default: return { text: "Không xác định", class: "" };
        }
    };

    if (loading) return <div className="loading">Đang tải lịch sử đơn hàng...</div>;

    if (!user) return <div className="error-msg">Vui lòng đăng nhập để xem lịch sử!</div>;

    return (
        <div className="order-history-container">
            <div className="history-header">
                <h2>Lịch sử mua hàng của bạn</h2>
                <button onClick={() => navigate('/Shop')}>Tiếp tục mua sắm</button>
            </div>

            {orders.length === 0 ? (
                <div className="no-orders">Bạn chưa có đơn hàng nào.</div>
            ) : (
                <div className="orders-list">
                    {orders.map((order) => {
                        const statusObj = getStatusText(order.status);
                        return (
                            <div key={order.id} className="order-card">
                                <div className="order-card-header">
                                    <span className="order-id">Mã đơn: #{order.id}</span>
                                    <span className={`order-status ${statusObj.class}`}>
                                        {statusObj.text}
                                    </span>
                                </div>

                                <div className="order-info">
                                    <p>📅 <strong>Ngày đặt:</strong> {new Date(order.orderDate).toLocaleString('vi-VN')}</p>
                                    <p>📍 <strong>Địa chỉ:</strong> {order.address}</p>
                                    <p>👤 <strong>Người nhận:</strong> {order.name} - {order.phone}</p>
                                </div>

                                <div className="order-items">
                                    {order.orderDetails.map((item, index) => (
                                        <div key={index} className="item-row">
                                            <span>{item.productName} (x{item.quantity})</span>
                                            <span>{item.price.toLocaleString('vi-VN')}đ</span>
                                        </div>
                                    ))}
                                </div>

                                <div className="order-card-footer">
                                    <div className="payment-info">
                                        {order.bill ? (
                                            <span className="bill-tag">
                                                💳 Đã thanh toán ({order.bill.paymentMethod})
                                            </span>
                                        ) : (
                                            <span className="unpaid-tag">Chưa xuất hóa đơn</span>
                                        )}
                                    </div>
                                    <div className="total-price">
                                        Tổng cộng: <strong>{order.totalAmount.toLocaleString('vi-VN')}đ</strong>
                                    </div>
                                </div>
                            </div>
                        );
                    })}
                </div>
            )}
        </div>
    );
};

export default OrderHistory;