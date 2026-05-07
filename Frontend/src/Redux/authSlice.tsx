import { createSlice, createAsyncThunk, PayloadAction } from "@reduxjs/toolkit";
import authApi from "../Api/authApi";
import { RootState } from "./store";

// 1. Cập nhật Interface đầy đủ các trường mà MyProfile và các trang khác đang gọi
// Sử dụng dấu ? cho các trường có thể không có khi vừa đăng nhập
interface User {
  id: number;
  Id?: number;           // Hỗ trợ PascalCase từ lỗi CartPage/ProductDetail
  userName: string;
  UserName?: string;     // Hỗ trợ PascalCase từ lỗi Login/MyProfile
  email?: string;
  Email?: string;        // Hỗ trợ lỗi MyProfile
  dob?: string;
  Dob?: string;          // Hỗ trợ lỗi MyProfile
  gender?: string;
  Gender?: string;       // Hỗ trợ lỗi MyProfile
  address?: string;
  Address?: string;      // Hỗ trợ lỗi MyProfile
  role?: string;
  Role?: string;
  token?: string;
  Token?: string;
}

interface AuthState {
  user: User | null;
  loading: boolean;
  error: string | null;
}

const getSavedUser = (): User | null => {
  try {
    const saved = localStorage.getItem("user");
    return saved ? JSON.parse(saved) : null;
  } catch {
    return null;
  }
};

const initialState: AuthState = {
  user: getSavedUser(),
  loading: false,
  error: null,
};

// ================= LOGIN =================

export const loginAction = createAsyncThunk(
  "auth/login",
  async (payload: any, { rejectWithValue }) => {
    try {
      const response = await authApi.login(payload);
      const data = response.data;

      // Lấy token linh hoạt
      const token = data.Token || data.token || data.accessToken;
      
      // Dữ liệu User trả về từ Backend thường là PascalCase (Id, UserName...)
      // Chúng ta giữ nguyên object từ Backend để khớp với các file .tsx hiện tại của bạn
      const user = data.user || data;

      if (token) localStorage.setItem("token", token);
      localStorage.setItem("user", JSON.stringify(user));

      window.dispatchEvent(new Event("storage"));

      return user as User;
    } catch (error: any) {
      return rejectWithValue(
        error.response?.data?.message || "Đăng nhập thất bại"
      );
    }
  }
);

// ================= REGISTER =================

export const registerAction = createAsyncThunk(
  "auth/register",
  async (payload: any, { rejectWithValue }) => {
    try {
      const { data } = await authApi.register(payload);
      return data;
    } catch (error: any) {
      return rejectWithValue(
        error.response?.data?.message || "Đăng ký thất bại"
      );
    }
  }
);

// ================= SLICE =================

const authSlice = createSlice({
  name: "auth",
  initialState,
  reducers: {
    clearError: (state) => {
      state.error = null;
    },
    rehydrateUser: (state) => {
      state.user = getSavedUser();
    },
    logout: (state) => {
      state.user = null;
      state.error = null;
      localStorage.removeItem("user");
      localStorage.removeItem("token");
      localStorage.removeItem("refreshToken");
      window.dispatchEvent(new Event("storage"));
    },
  },
  extraReducers: (builder) => {
    builder
      .addCase(loginAction.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(loginAction.fulfilled, (state, action: PayloadAction<User>) => {
        state.loading = false;
        state.user = action.payload;
        state.error = null;
      })
      .addCase(loginAction.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload as string;
        state.user = null;
      })
      .addMatcher(
        (action) => action.type.startsWith("auth/register/"),
        (state, action: any) => {
          state.loading = action.type.endsWith("/pending");
          if (action.type.endsWith("/rejected")) {
            state.error = action.payload as string;
          } else if (action.type.endsWith("/fulfilled")) {
            state.error = null;
          }
        }
      );
  },
});

export const { logout, clearError, rehydrateUser } = authSlice.actions;
export default authSlice.reducer;
export const selectCurrentUser = (state: RootState) => state.auth.user;