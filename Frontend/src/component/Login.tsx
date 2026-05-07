import React, { useState, useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { useNavigate } from 'react-router-dom'; 
import { loginAction, clearError } from '../Redux/authSlice';
import { AppDispatch, RootState } from '../Redux/store';

interface LoginProps {
  onLoginSuccess: (userData: any) => void;
  switchToRegister: () => void;
}

const Login: React.FC<LoginProps> = ({ onLoginSuccess, switchToRegister }) => {
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);

  const dispatch = useDispatch<AppDispatch>();
  const navigate = useNavigate();

  // Lấy trạng thái từ Redux Store
  const { loading, error, user } = useSelector((state: RootState) => state.auth);

  useEffect(() => {
    if (isSubmitting && !loading) {
      if (user && !error) {
        console.log("Dữ liệu User nhận được:", user);
        
        localStorage.setItem('user', JSON.stringify(user));
        if (user.token) {
          localStorage.setItem('token', user.token);
        }

        alert(`Đăng nhập thành công! Chào ${user.UserName || user.userName}`);
        
        onLoginSuccess(user);
        setIsSubmitting(false);
        navigate('/');
      }

      if (error) {
        alert(`Đăng nhập thất bại: ${error}`);
        dispatch(clearError());
        setIsSubmitting(false);
      }
    }
  }, [user, error, loading, isSubmitting, onLoginSuccess, dispatch, navigate]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!username || !password) return;
    
    setIsSubmitting(true);
    
    dispatch(loginAction({ 
      UserName: username, 
      PassWord: password 
    }));
  };

  return (
    <div className="login-box">
      <div className="logo-wrapper" style={{ textAlign: 'center', marginBottom: '20px' }}>
        <img src="/logo.png" alt="logo" className="app-logo-large" style={{ maxWidth: '100px' }} />
      </div>

      <h2 style={{ color: '#fff', textAlign: 'center', marginBottom: '20px' }}>
        ĐĂNG NHẬP
      </h2>

      <form onSubmit={handleSubmit} className="login-form">
        <div className="input-group">
          <span className="icon">👤</span>
          <input 
            type="text" 
            placeholder="Tên đăng nhập" 
            value={username} 
            onChange={e => setUsername(e.target.value)} 
            disabled={loading} 
            required 
          />
        </div>
        <div className="input-group">
          <span className="icon">🔐</span>
          <input 
            type="password" 
            placeholder="Mật khẩu" 
            value={password} 
            onChange={e => setPassword(e.target.value)} 
            disabled={loading}
            required 
          />
        </div>
        
        <button 
          type="submit" 
          className="btn-login-submit" 
          disabled={loading}
          style={{ width: '100%', padding: '10px', cursor: 'pointer' }}
        >
          {loading ? 'ĐANG XỬ LÝ...' : 'ĐĂNG NHẬP'}
        </button>
      </form>

      <div className="form-footer" style={{ textAlign: 'center', marginTop: '15px' }}>
        <p style={{ color: '#fff', fontSize: '0.9rem' }}>
          Chưa có tài khoản?{' '}
          <span 
            onClick={switchToRegister} 
            style={{ textDecoration: 'underline', fontWeight: 'bold', cursor: 'pointer' }}
          >
            Đăng ký ngay
          </span>
        </p>
      </div>
    </div>
  );
};

export default Login;