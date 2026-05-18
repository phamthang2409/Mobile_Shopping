import React, { useEffect, useState } from 'react';

const ManageOrders: React.FC = () => {
  const [orders, setOrders] = useState<any[]>([]);

  // 1. Hàm lấy danh sách đơn hàng
  const fetchOrders = () => {
    fetch('https://localhost:7180/api/Order')
      .then(res => res.json())
      .then(data => setOrders(data))
      .catch(err => console.error("Lỗi lấy đơn hàng:", err));
  };

  useEffect(() => {
    fetchOrders();
  }, []);

  // 2. Hàm cập nhật trạng thái đơn hàng
  const handleStatusChange = async (orderId: number, newStatus: number) => {
    try {
      const response = await fetch(`https://localhost:7180/api/Order/${orderId}/status`, {
        method: 'PUT', 
        headers: {
          'Content-Type': 'application/json',
          // 'Authorization': `Bearer ${sessionStorage.getItem('token')}` // Nếu API yêu cầu token
        },
        body: JSON.stringify(newStatus)
      });

      if (response.ok) {
        alert("Cập nhật trạng thái thành công!");
        fetchOrders(); // Tải lại danh sách để cập nhật giao diện
      } else {
        alert("Cập nhật thất bại!");
      }
    } catch (error) {
      console.error("Lỗi khi cập nhật trạng thái:", error);
    }
  };

  return (
    <div>
      <h1>Quản lý đơn hàng</h1>
      <table style={{ width: '100%', background: 'white', marginTop: '20px', borderCollapse: 'collapse' }}>
        <thead>
          <tr style={{ background: '#eee' }}>
            <th style={{ padding: '10px', border: '1px solid #ddd' }}>Mã đơn</th>
            <th style={{ padding: '10px', border: '1px solid #ddd' }}>Khách hàng</th>
            <th style={{ padding: '10px', border: '1px solid #ddd' }}>Tổng tiền</th>
            <th style={{ padding: '10px', border: '1px solid #ddd' }}>Trạng thái</th>
            <th style={{ padding: '10px', border: '1px solid #ddd' }}>Thao tác</th>
          </tr>
        </thead>
        <tbody>
          {orders.map(order => (
            <tr key={order.id}>
              <td style={{ padding: '10px', border: '1px solid #ddd' }}>#{order.id}</td>
              <td style={{ padding: '10px', border: '1px solid #ddd' }}>{order.name}</td>
              <td style={{ padding: '10px', border: '1px solid #ddd' }}>{order.totalAmount?.toLocaleString()} VNĐ</td>
              
              {/* 3. Chỉnh sửa cột Trạng thái thành Select Box */}
              <td style={{ padding: '10px', border: '1px solid #ddd' }}>
                <select 
                  value={order.status} 
                  onChange={(e) => handleStatusChange(order.id, parseInt(e.target.value))}
                  style={{  
                    padding: '5px',
                    borderRadius: '4px',
                    backgroundColor: order.status === 0 ? '#fff3cd' : '#d4edda',
                    color: order.status === 0 ? '#856404' : '#155724',
                    cursor: 'pointer'
                  }}
                >
                    <option value={-1}>❌ Đã hủy</option>
                    <option value={0}>🕒 Chờ xác nhận</option>
                    <option value={1}>✅ Đã thanh toán</option>
                    <option value={2}>🚚 Đang giao hàng</option>
                     <option value={3}>🏁 Đã hoàn thành</option>
                </select>
              </td>

              <td style={{ padding: '10px', border: '1px solid #ddd' }}>
                <button 
                  onClick={() => alert(`Xem chi tiết đơn #${order.id}`)}
                  style={{ padding: '5px 10px', cursor: 'pointer', background: '#3498db', color: 'white', border: 'none', borderRadius: '4px' }}
                >
                  Xem chi tiết
                </button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};

export default ManageOrders;