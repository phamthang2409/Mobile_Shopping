import React, { useState, useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { registerAction, clearError } from '../Redux/authSlice'; // Đảm bảo bạn đã export registerAction
import { AppDispatch, RootState } from '../Redux/store';

interface RegisterProps {
  switchToLogin: () => void;
}

const Register: React.FC<RegisterProps> = ({ switchToLogin }) => {
  const [formData, setFormData] = useState({
    UserName: '',
    PassWord: '',
    Email: '',
    Address: '',
    Gender: 'Nam',
    Dob: ''
  });

  const dispatch = useDispatch<AppDispatch>();
  
  const { loading, error } = useSelector((state: RootState) => state.auth);

  useEffect(() => {
    if (error) {
      alert(`Lỗi đăng ký: ${error}`);
      dispatch(clearError());
    }
  }, [error, dispatch]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    // unwrap() giúp chúng ta xử lý logic sau khi API thành công ngay tại đây
    dispatch(registerAction(formData))
      .unwrap()
      .then(() => {
        alert(`Chúc mừng ${formData.UserName}! Đăng ký thành công.`);
        switchToLogin(); 
      })
      .catch((err) => {
      });
  };

  return (
    <div className="login-box register-box">
      <div className="logo-wrapper">
        <img src="/logo.png" alt="logo" className="app-logo-large" />
      </div>
      
      <h2 className="form-title" style={{ color: '#fff', textAlign: 'center', marginBottom: '20px' }}>
        TẠO TÀI KHOẢN
      </h2>

      <form onSubmit={handleSubmit} className="login-form">
        <div className="input-group">
          <span className="icon">👤</span>
          <input 
            type="text" 
            placeholder="Tên đăng nhập" 
            value={formData.UserName}
            onChange={e => setFormData({...formData, UserName: e.target.value})} 
            disabled={loading}
            required 
          />
        </div>

        <div className="input-group">
          <span className="icon">🔐</span>
          <input 
            type="password" 
            placeholder="Mật khẩu" 
            value={formData.PassWord}
            onChange={e => setFormData({...formData, PassWord: e.target.value})} 
            disabled={loading}
            required 
          />
        </div>

        <div className="input-group">
          <span className="icon">📧</span>
          <input 
            type="email" 
            placeholder="Email" 
            value={formData.Email}
            onChange={e => setFormData({...formData, Email: e.target.value})} 
            disabled={loading}
            required 
          />
        </div>

        <div className="input-group">
          <span className="icon">📍</span>
          <input 
            type="text" 
            placeholder="Địa chỉ" 
            value={formData.Address}
            onChange={e => setFormData({...formData, Address: e.target.value})} 
            disabled={loading}
          />
        </div>

        <div className="input-group">
          <span className="icon">📅</span>
          <input 
            type="date" 
            className="date-input"
            value={formData.Dob}
            onChange={e => setFormData({...formData, Dob: e.target.value})} 
            disabled={loading}
            required 
          />
        </div>

        <div className="input-group">
          <span className="icon">🚻</span>
          <select 
            className="gender-select"
            value={formData.Gender}
            onChange={e => setFormData({...formData, Gender: e.target.value})}
            disabled={loading}
          >
            <option value="Nam">Nam</option>
            <option value="Nữ">Nữ</option>
          </select>
        </div>

        <button 
          type="submit" 
          className="btn-login-submit" 
          style={{ marginTop: '10px' }}
          disabled={loading}
        >
          {loading ? 'ĐANG XỬ LÝ...' : 'ĐĂNG KÝ'}
        </button>
      </form>

      <div className="form-footer" style={{ textAlign: 'center', marginTop: '15px' }}>
        <p onClick={switchToLogin} style={{ color: '#fff', cursor: 'pointer', fontSize: '0.9rem' }}>
          Đã có tài khoản? <span style={{ textDecoration: 'underline', fontWeight: 'bold' }}>Đăng nhập</span>
        </p>
      </div>
    </div>
  );
};

export default Register;