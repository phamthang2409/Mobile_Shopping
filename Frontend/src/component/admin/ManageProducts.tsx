import React, { useState, useEffect } from 'react';

interface Product {
  id?: number;
  productName: string;
  price: number;
  description: string;
  stock: number;
  imageUrl: string;
}

const ManageProducts: React.FC = () => {
  const [products, setProducts] = useState<Product[]>([]);
  const [product, setProduct] = useState<Product>({ 
    productName: '', price: 0, description: '', stock: 0, imageUrl: '' 
  });
  const [showForm, setShowForm] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const   apiUrl = 'https://localhost:7180/api/Product';

  useEffect(() => {
    fetchProducts();
  }, []);

  const fetchProducts = async () => {
    try {
      const res = await fetch(apiUrl);
      if (res.ok) {
        const data = await res.json();
        setProducts(data);
      }
    } catch (error) {
      console.error("Lỗi lấy danh sách:", error);
    }
  };

  // Xử lý khi chọn file ảnh (Cách 1)
  const handleImageChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files && e.target.files[0]) {
      const file = e.target.files[0];
      if (file.size > 2 * 1024 * 1024) {
        alert("File quá lớn! Hãy dùng link ảnh hoặc chọn ảnh dưới 2MB.");
        return;
      }
      const reader = new FileReader();
      reader.onloadend = () => {
        setProduct({ ...product, imageUrl: reader.result as string });
      };
      reader.readAsDataURL(file);
    }
  };

  const handleAdd = async () => {
    if (!product.productName || product.price <= 0) {
      alert("Vui lòng nhập tên và giá!");
      return;
    }

    setIsSubmitting(true);
    try {
      // Gửi trực tiếp object 'product' (phẳng) theo đúng Swagger
      const res = await fetch(apiUrl, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(product) 
      });

      if (res.ok) {
        alert("Thêm sản phẩm thành công!");
        setProduct({ productName: '', price: 0, description: '', stock: 0, imageUrl: '' });
        setShowForm(false);
        fetchProducts();
      } else {
        alert("Lỗi server! Hãy kiểm tra lại định dạng dữ liệu.");
      }
    } catch (error) {
      console.error("Lỗi kết nối:", error);
      alert("Kết nối bị đóng (ERR_CONNECTION_CLOSED). Nếu bạn dùng ảnh file, hãy thử chuyển sang dùng Link ảnh.");
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleDelete = async (id: number) => {
    if (window.confirm("Xóa sản phẩm này?")) {
      try {
        const res = await fetch(`${apiUrl}/${id}`, { method: 'DELETE' });
        if (res.ok) {
          alert("Đã xóa!");
          fetchProducts();
        }
      } catch (error) {
        console.error("Lỗi xóa:", error);
      }
    }
  };

  return (
    <div style={{ padding: '20px', maxWidth: '1100px', margin: '0 auto', fontFamily: 'Arial' }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '20px' }}>
        <h2>📦 Quản lý sản phẩm</h2>
        <button onClick={() => setShowForm(!showForm)} style={btnPrimary}>
          {showForm ? "Đóng Form" : "+ Thêm mới"}
        </button>
      </div>

      {showForm && (
        <div style={{ background: '#f8f9fa', padding: '20px', borderRadius: '8px', border: '1px solid #ddd', marginBottom: '20px' }}>
          <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '15px' }}>
            <input type="text" placeholder="Tên sản phẩm" value={product.productName} onChange={e => setProduct({...product, productName: e.target.value})} style={inputStyle} />
            <input type="number" placeholder="Giá tiền" value={product.price || ''} onChange={e => setProduct({...product, price: Number(e.target.value)})} style={inputStyle} />
            <input type="number" placeholder="Số lượng" value={product.stock || ''} onChange={e => setProduct({...product, stock: Number(e.target.value)})} style={inputStyle} />
            
            {/* PHẦN CHỌN ẢNH: FILE HOẶC LINK */}
            <div style={{ display: 'flex', flexDirection: 'column', gap: '5px' }}>
              <input type="file" accept="image/*" onChange={handleImageChange} style={inputStyle} title="Chọn file từ máy" />
              <div style={{ textAlign: 'center', fontSize: '12px', color: '#666' }}>-- HOẶC --</div>
              <input 
                type="text" 
                placeholder="Dán link ảnh (URL) vào đây..." 
                value={product.imageUrl.startsWith('data:image') ? '' : product.imageUrl} 
                onChange={e => setProduct({...product, imageUrl: e.target.value})} 
                style={inputStyle} 
              />
            </div>
          </div>
          
          <textarea placeholder="Mô tả..." value={product.description} onChange={e => setProduct({...product, description: e.target.value})} style={{ ...inputStyle, height: '80px', marginTop: '15px' }} />
          
          {product.imageUrl && (
            <div style={{ marginTop: '10px' }}>
              <p style={{ fontSize: '12px', marginBottom: '5px' }}>Xem trước ảnh:</p>
              <img src={product.imageUrl} alt="preview" style={{ width: '100px', height: '100px', objectFit: 'cover', borderRadius: '5px', border: '1px solid #ccc' }} />
            </div>
          )}

          <button disabled={isSubmitting} onClick={handleAdd} style={btnSuccess}>
            {isSubmitting ? "ĐANG XỬ LÝ..." : "LƯU SẢN PHẨM"}
          </button>
        </div>
      )}

      {/* BẢNG DANH SÁCH */}
      <table style={{ width: '100%', borderCollapse: 'collapse', background: 'white' }}>
        <thead>
          <tr style={{ background: '#eee', borderBottom: '2px solid #ddd' }}>
            <th style={thStyle}>Ảnh</th>
            <th style={thStyle}>Tên sản phẩm</th>
            <th style={thStyle}>Giá</th>
            <th style={thStyle}>Thao tác</th>
          </tr>
        </thead>
        <tbody>
          {products.map(item => (
            <tr key={item.id} style={{ borderBottom: '1px solid #eee' }}>
              <td style={tdStyle}><img src={item.imageUrl} alt="p" style={{ width: '50px', height: '50px', objectFit: 'cover' }} /></td>
              <td style={tdStyle}><b>{item.productName}</b></td>
              <td style={tdStyle}>{item.price.toLocaleString()} đ</td>
              <td style={tdStyle}>
                <button onClick={() => item.id && handleDelete(item.id)} style={btnDelete}>Xóa</button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};

// Styles
const inputStyle = { width: '100%', padding: '10px', border: '1px solid #ccc', borderRadius: '4px', boxSizing: 'border-box' as 'border-box' };
const btnPrimary = { padding: '5px 20px', background: '#3498db', color: 'white', border: 'none', borderRadius: '4px', cursor: 'pointer' };
const btnSuccess = { width: '100%', padding: '12px', marginTop: '15px', background: '#2ecc71', color: 'white', border: 'none', borderRadius: '4px', cursor: 'pointer', fontWeight: 'bold' as 'bold' };
const btnDelete = { padding: '5px 10px', background: '#e74c3c', color: 'white', border: 'none', borderRadius: '4px', cursor: 'pointer' };
const thStyle = { padding: '12px', textAlign: 'left' as 'left' };
const tdStyle = { padding: '12px' };

export default ManageProducts;