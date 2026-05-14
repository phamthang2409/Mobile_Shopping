import axios, { AxiosInstance, InternalAxiosRequestConfig } from 'axios';

// Định nghĩa kiểu dữ liệu trả về từ API Refresh
interface RefreshResponse {
    accessToken: string; // Token mới
    refreshToken?: string; // Refresh token mới (nếu backend có xoay vòng token)
}

const axiosClient: AxiosInstance = axios.create({
    baseURL: 'https://localhost:7180/api',
    headers: {
        'Content-Type': 'application/json',
    },
});

/**
 * 1. Request Interceptor: Luôn đính kèm Token vào Header trước khi gửi đi
 */
axiosClient.interceptors.request.use(
    (config: InternalAxiosRequestConfig) => {
        const token = localStorage.getItem('token'); 
        if (token && config.headers) {
            config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
    },
    (error) => Promise.reject(error)
);

/**
 * 2. Response Interceptor: Xử lý tự động Refresh Token khi gặp lỗi 401
 */
axiosClient.interceptors.response.use(
    (response) => response,
    async (error) => {
        const originalRequest = error.config as InternalAxiosRequestConfig & { _retry?: boolean };

        // Kiểm tra nếu lỗi 401 (Unauthorized) và chưa thử retry lần nào
        if (error.response?.status === 401 && !originalRequest._retry) {
            
            // Tránh lặp vô tận nếu chính API login hoặc refresh bị 401
            if (originalRequest.url?.includes('/Auth/login') || originalRequest.url?.includes('/Auth/refresh')) {
                return Promise.reject(error);
            }

            originalRequest._retry = true;

            try {
                const currentRefreshToken = localStorage.getItem('refreshToken');
                if (!currentRefreshToken) throw new Error("No refresh token found");

                // Gọi API Refresh Token
                // Lưu ý: Sử dụng 'axios' thay vì 'axiosClient' ở đây để tránh bị interceptor chặn lại
                const res = await axios.post<RefreshResponse>('https://localhost:7180/api/Auth/refresh', {
                    refreshToken: currentRefreshToken 
                });

                const { accessToken, refreshToken: newRefreshToken } = res.data;

                if (accessToken) {
                    // Cập nhật lại kho lưu trữ
                    localStorage.setItem('token', accessToken);
                    if (newRefreshToken) {
                        localStorage.setItem('refreshToken', newRefreshToken);
                    }

                    // Gán token mới vào request hiện tại và thực hiện lại
                    if (originalRequest.headers) {
                        originalRequest.headers.Authorization = `Bearer ${accessToken}`;
                    }
                    
                    return axiosClient(originalRequest);
                }
            } catch (refreshError) {
                // Nếu refresh thất bại (token hết hạn hoàn toàn), xóa sạch và logout
                console.error("Session expired, logging out...", refreshError);
                
                localStorage.removeItem('token');
                localStorage.removeItem('refreshToken');
                localStorage.removeItem('user'); 

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