import React, { useEffect, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { fetchProducts } from '../Redux/productSlice'; 
import { RootState, AppDispatch } from '../Redux/store';
import '../CSS/ShopPage.css';

interface ShopPageProps {
  onSelectProduct?: (product: any) => void;
}

const ShopPage: React.FC<ShopPageProps> = ({ onSelectProduct }) => {
  const dispatch = useDispatch<AppDispatch>();
  
  // 1. Lấy dữ liệu gốc từ Redux Store
  const { items = [], loading, error } = useSelector(
    (state: RootState) => state.products || { items: [], loading: false, error: null }
  );

  // States quản lý hiển thị
  const [searchTerm, setSearchTerm] = useState(''); 
  const [showFilter, setShowFilter] = useState(false); 
  const [priceRange, setPriceRange] = useState({ min: 0, max: 0 }); 
  const [displayItems, setDisplayItems] = useState<any[]>([]); // Danh sách hiển thị sau khi lọc

  const DEFAULT_IMAGE = "https://placehold.jp/24/cccccc/ffffff/200x200.png?text=No+Image";

  useEffect(() => {
    dispatch(fetchProducts());
  }, [dispatch]);

  useEffect(() => {
    if (items && items.length > 0) {
      setDisplayItems(items);
      // Fix an toàn: Thêm bộ lọc chống phần tử lỗi không có giá trước khi tính Math.max
      const validPrices = items.map((i: any) => i.price || i.Price || 0);
      const maxP = validPrices.length > 0 ? Math.max(...validPrices) : 0;
      setPriceRange(prev => ({ ...prev, max: maxP }));
    }
  }, [items]);

  // HÀM XỬ LÝ LỌC TẠI FE 
  const handleApplyFilters = () => {
    if (!items) return;
    
    const result = items.filter((p: any) => {
      const pName = (p.productName || p.ProductName || "").toLowerCase();
      const pPrice = p.price || p.Price || 0;
      const sTerm = searchTerm.toLowerCase().trim();

      // Điều kiện 1: Tên phải chứa từ khóa tìm kiếm
      const matchesName = pName.includes(sTerm);

      // Điều kiện 2: Giá phải nằm trong khoảng Min - Max
      const matchesPrice = pPrice >= priceRange.min && pPrice <= (priceRange.max || Infinity);

      return matchesName && matchesPrice;
    });

    setDisplayItems(result);
  };

  // Reset về trạng thái ban đầu
  const handleReset = () => {
    setSearchTerm('');
    const validPrices = items.map((i: any) => i.price || i.Price || 0);
    const maxP = validPrices.length > 0 ? Math.max(...validPrices) : 0;
    setPriceRange({ min: 0, max: maxP });
    setDisplayItems(items);
  };

  if (loading) return <div className="loader-container"><div className="loader"></div><p>Đang tải sản phẩm...</p></div>;
  if (error) return <div className="error-message">❗ Lỗi: {error}</div>;

  return (
    <div className="shop-page">
      <div className="shop-header-search">
        <h2>Shop</h2>
        <div className="search-bar">
          <input 
            type="text" 
            placeholder="Tìm tên sản phẩm..." 
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            onKeyUp={handleApplyFilters}
            onKeyPress={(e) => e.key === 'Enter' && handleApplyFilters()}
          />
          <button className="search-btn" onClick={handleApplyFilters}>🔍</button>
          <button 
            className={`filter-btn ${showFilter ? 'active' : ''}`} 
            onClick={() => setShowFilter(!showFilter)}
          >
            {showFilter ? 'Đóng lọc' : 'Lọc giá'}
          </button>
        </div>
      </div>

      {/* Panel Lọc giá tiền */}
      {showFilter && (
        <div className="filter-panel-overlay">
          <div className="filter-panel">
            <h4>Khoảng giá (VND)</h4>
            <div className="filter-inputs">
              <div className="price-input-group">
                <label>Từ:</label>
                <input 
                  type="number" 
                  value={priceRange.min}
                  onChange={(e) => setPriceRange({ ...priceRange, min: Number(e.target.value) })}
                />
              </div>
              <div className="price-input-group">
                <label>Đến:</label>
                <input 
                  type="number" 
                  value={priceRange.max}
                  onChange={(e) => setPriceRange({ ...priceRange, max: Number(e.target.value) })}
                />
              </div>
            </div>
            <div className="filter-buttons">
              <button className="apply-btn" onClick={handleApplyFilters}>Áp dụng</button>
              <button className="reset-btn" onClick={handleReset}>Xóa hết</button>
            </div>
          </div>
        </div>
      )}
      
      {/* Danh sách sản phẩm hiển thị */}
      <div className="product-grid">
        {displayItems && displayItems.length > 0 ? (
          displayItems.map((product: any, index: number) => {
            // Quét tìm ID nguyên bản từ Server đổ về Redux Store
            const currentId = product.productId || product.id || product.ProductId || product.Id || index;

            return (
              <div 
                key={currentId} 
                className="product-card" 
                onClick={() => {
                  console.log("RAW JSON:", JSON.stringify(product));
                  const secureProduct = {
                    ...product,
                    id: Number(product.id || product.productId || product.ProductId || product.Id || 0)
                  };
                  
                  console.log("Dữ liệu sản phẩm truyền từ Shop sang Detail:", secureProduct);
                  onSelectProduct?.(secureProduct);
                }} 
              >
                <div className="image-container">
                  <img 
                    src={product.imageUrl || product.ImageUrl || DEFAULT_IMAGE} 
                    alt={product.productName || product.ProductName} 
                    onError={(e) => { (e.target as HTMLImageElement).src = DEFAULT_IMAGE; }}
                  />
                </div>
                
                <div className="product-info">
                  <h3>{product.productName || product.ProductName || "Sản phẩm không tên"}</h3>
                  <div className="stars">⭐⭐⭐⭐⭐</div>
                  <p className="price">
                    {(product.price || product.Price || 0).toLocaleString('vi-VN')} VND
                  </p>
                </div>
              </div>
            );
          })
        ) : (
          <div className="no-products">
            <p>Không tìm thấy sản phẩm nào phù hợp với yêu cầu.</p>
            <button onClick={handleReset}>Xem tất cả sản phẩm</button>
          </div>
        )}
      </div>
    </div>
  );
};

export default ShopPage;