import axios from 'axios';

const axiosClient = axios.create({
  baseURL: 'https://localhost:7180/api',
  headers: {
    'Content-Type': 'application/json',
  },
});

// 1. Gắn Access Token vào mỗi Request
axiosClient.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('token');
    if (token && config.headers) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// 2. Xử lý tự động Refresh khi hết hạn
axiosClient.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;
    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;
      try {
        // Lấy "chìa khóa" để Backend tìm dưới DB
        const currentRefreshToken = localStorage.getItem('refreshToken');   
        if (!currentRefreshToken) {
          throw new Error("No refresh token available in storage");
        }
        // Gửi lên Backend để Backend check dưới Database
        const res = await axios.post('https://localhost:7180/api/Auth/refresh', {
          RefreshToken: currentRefreshToken 
        });

        // Backend trả về cặp mới 
        const { accessToken, refreshToken: newRefreshToken } = res.data;

        if (accessToken) {
          // Lưu cặp mới vào lại LocalStorage
          localStorage.setItem('token', accessToken);
          if (newRefreshToken) {
            localStorage.setItem('refreshToken', newRefreshToken);
          }
          // Thực hiện lại request cũ với token mới
          originalRequest.headers.Authorization = `Bearer ${accessToken}`;
          return axiosClient(originalRequest);
        }
      } catch (refreshError) {
        // Nếu DB báo token không khớp hoặc hết hạn, dọn dẹp và Logout
        console.error("Session expired:", refreshError);
        localStorage.clear(); 
        
        if (!window.location.pathname.includes('/login')) {
          window.location.href = '/login';
        }
        return Promise.reject(refreshError);
      }
    }

    return Promise.reject(error);
  }
);

export default axiosClient;