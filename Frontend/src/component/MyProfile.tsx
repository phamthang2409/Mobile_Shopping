import React from 'react';
import '../CSS/MyProfile.css';
import { useSelector } from 'react-redux';
import { RootState } from '../Redux/store';

const MyProfile: React.FC = () => {
  // Lấy thông tin user từ Redux Store
  const user = useSelector((state: RootState) => state.auth.user);
  console.log("Dữ liệu user trong Redux:", user);
  if (!user) {
    return <div className="profile-container">Vui lòng đăng nhập để xem thông tin</div>;
  }

  const formatDate = (dateString: string) => {
    if (!dateString) return 'N/A';
    const date = new Date(dateString);
    return isNaN(date.getTime()) ? 'N/A' : date.toLocaleDateString('vi-VN');
  };

  return (
    <div className="profile-container">
      <h2>My Profile</h2>
      <div className="profile-header">
        <img src="/avatar.png" alt="Avatar" className="large-avatar" />
        <div className="header-info">
          <h1>{user.UserName || 'Người dùng'}</h1>
          <p>Email: {user.Email || 'Chưa có email'}</p>
        </div>
      </div>

      <div className="info-list">
        <div className="info-item">
          <label>Date of birth:</label>
          <div className="val-box">{formatDate(user.Dob ?? "")} </div>
        </div>

        <div className="info-item">
          <label>Sex:</label>
          <div className="val-box">{user.Gender || 'Chưa xác định'} </div>
        </div>

        <div className="info-item">
          <label>Primary Address:</label>
          <div className="val-box underlined">
            {user.Address || 'Chưa cập nhật địa chỉ'}
          </div>
        </div>

        <div className="info-item">
          <label>Account Status:</label>
          <div className="val-box underlined">Đang hoạt động</div>
        </div>
      </div>
    </div>
  );
};

export default MyProfile;