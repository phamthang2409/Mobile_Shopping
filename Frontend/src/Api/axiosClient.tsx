import axios, { AxiosInstance, InternalAxiosRequestConfig } from 'axios';

interface RefreshResponse {
    accessToken: string;
    refreshToken?: string;
}

const axiosClient: AxiosInstance = axios.create({
    baseURL: 'https://localhost:7180/api',
    headers: {
        'Content-Type': 'application/json',
    },
});

// Đính kèm Token vào Header trước mỗi request
axiosClient.interceptors.request.use(
    (config: InternalAxiosRequestConfig) => {
        // Lấy từ key 'token' đúng như thực tế lưu trữ của bạn
        const token = localStorage.getItem('token'); 
        if (token && config.headers) {
            config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
    },
    (error) => Promise.reject(error)
);

// Xử lý phản hồi và tự động Refresh Token khi gặp lỗi 401
axiosClient.interceptors.response.use(
    (response) => response,
    async (error) => {
        const originalRequest = error.config as InternalAxiosRequestConfig & { _retry?: boolean };

        if (error.response?.status === 401 && !originalRequest._retry) {
            
            if (originalRequest.url?.includes('/Auth/login') || originalRequest.url?.includes('/Auth/refresh')) {
                return Promise.reject(error);
            }

            originalRequest._retry = true;

            try {
                const currentRefreshToken = localStorage.getItem('refreshToken');
                if (!currentRefreshToken) throw new Error("No refresh token found");
                const res = await axios.post<RefreshResponse>('https://localhost:7180/api/Auth/refresh', {
                    RefreshToken: currentRefreshToken 
                });

                const { accessToken, refreshToken: newRefreshToken } = res.data;

                if (accessToken) {
                    // Cập nhật lại LocalStorage
                    localStorage.setItem('token', accessToken);
                    if (newRefreshToken) {
                        localStorage.setItem('refreshToken', newRefreshToken);
                    }

                    // Gán token mới vào request hiện tại
                    if (originalRequest.headers) {
                        originalRequest.headers.Authorization = `Bearer ${accessToken}`;
                    }
                    
                    // Thực hiện lại request bị lỗi
                    return axiosClient(originalRequest);
                }
            } catch (refreshError) {
                console.error("Session expired:", refreshError);
                
                // Xóa sạch thông tin để ép đăng nhập lại
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