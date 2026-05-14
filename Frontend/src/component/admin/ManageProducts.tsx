import React, { useState } from 'react';

const ManageProducts: React.FC = () => {
  const [product, setProduct] = useState({ productName: '', price: 0, description: '', stock: 0, imageUrl: '' });

  const handleAdd = async () => {
    const payload = { productDto: product }; // Bọc lại đúng cấu trúc Backend yêu cầu
    const res = await fetch('https://localhost:7180/api/Product', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    });
    if (res.ok) alert("Thêm thành công!");
  };

  return (
    <div>
      <h1>Quản lý sản phẩm</h1>
      <div style={{ background: 'white', padding: '20px', borderRadius: '8px', marginTop: '20px' }}>
        <h3>Thêm sản phẩm mới</h3>
        <input type="text" placeholder="Tên sản phẩm" onChange={e => setProduct({...product, productName: e.target.value})} style={{ display: 'block', margin: '10px 0', width: '100%', padding: '8px' }} />
        <input type="number" placeholder="Giá" onChange={e => setProduct({...product, price: Number(e.target.value)})} style={{ display: 'block', margin: '10px 0', width: '100%', padding: '8px' }} />
    {/* textarea để có thể viết mô ta có thể xuống hàng */}
        <textarea 
          placeholder="Mô tả chi tiết (nhấn Enter để xuống hàng)" 
          rows={5}
          onChange={e => setProduct({...product, description: e.target.value})}
          style={{ display: 'block', margin: '10px 0', width: '100%', padding: '8px' }}/>
        <input type='number' placeholder='Số lượng' onChange={e => setProduct({...product, stock: Number(e.target.value)})}style={{ display: 'block', margin: '10px 0', width: '100%', padding: '8px' }} />
         
        <button onClick={handleAdd} style={{ padding: '10px 20px', background: '#2ecc71', color: 'white', border: 'none', cursor: 'pointer' }}>Lưu sản phẩm</button>
      </div>
    </div>
  );
};

export default ManageProducts;