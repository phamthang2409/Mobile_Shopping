import axios from 'axios';

const axiosClient = axios.create({
  baseURL: 'https://localhost:7180/api',
  headers: {
    'Content-Type': 'application/json',
  },
});

// 1. Request Interceptor: Gắn Access Token vào Header trước khi gửi request
axiosClient.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('token');
    if (token && config.headers) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// 2. Response Interceptor: Xử lý lỗi 401 (Hết hạn Token) và tự động Refresh
axiosClient.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;

    // Kiểm tra nếu lỗi là 401 (Unauthorized) và request này chưa từng thử refresh (_retry)
    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;

      try {
        const refreshToken = localStorage.getItem('refreshToken');

        if (!refreshToken) {
          throw new Error("No refresh token available");
        }

        // Gọi API Refresh. 
        const response = await axios.post('https://localhost:7180/api/Auth/refresh', {
          RefreshToken: refreshToken // Key phải khớp với Class ở Backend (RefreshRequest)
        });

        const newToken = response.data.Token || response.data.token || response.data.accessToken;

        if (newToken) {
          localStorage.setItem('token', newToken);

          // Cập nhật Refresh Token mới nếu Backend có trả về cái mới
          const newRefreshToken = response.data.RefreshToken || response.data.refreshToken;
          if (newRefreshToken) {
            localStorage.setItem('refreshToken', newRefreshToken);
          }

          // Cập nhật lại Header của request cũ và thực hiện lại nó
          originalRequest.headers.Authorization = `Bearer ${newToken}`;
          return axiosClient(originalRequest);
        }
      } catch (refreshError) {
        // Nếu Refresh Token cũng hết hạn hoặc lỗi, xóa sạch và đá về trang Login
        console.error("Refresh token expired or invalid:", refreshError);
        localStorage.removeItem('token');
        localStorage.removeItem('refreshToken');
        localStorage.removeItem('user');
        
        window.location.href = '/login';
        return Promise.reject(refreshError);
      }
    }

    return Promise.reject(error);
  }
);

export default axiosClient;