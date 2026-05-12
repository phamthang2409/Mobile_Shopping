import { createSlice, createAsyncThunk, PayloadAction } from "@reduxjs/toolkit";
import authApi from "../Api/authApi";
import { RootState } from "./store";
interface User {
  id: number;
  userName: string;
  email?: string; 
  dob?: string;
  gender?: string;
  address?: string;

  Id?: number;           
  UserName?: string;     
  Email?: string;         
  Dob?: string;           
  Gender?: string;      
  Address?: string;     
  
  role?: string;
  token?: string;
  accessToken?: string;
  refreshToken?: string;
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

//Login

export const loginAction = createAsyncThunk(
  "auth/login",
  async (payload: any, { rejectWithValue }) => {
    try {
      const response = await authApi.login(payload);
      const data = response.data;

      //  Lấy token linh hoạt từ response
      const token = data.accessToken || data.token || data.Token;
      // Lấy thêm refreshToken từ response trả về
      const refreshToken = data.refreshToken || data.RefreshToken;
      
      //  Dữ liệu User
      const user = data.user || data;

      // LƯU VÀO LOCAL STORAGE
      if (token) {
        localStorage.setItem("token", token);
      }
      
      if (refreshToken) {
        localStorage.setItem("refreshToken", refreshToken); 
      }

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

// Register
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

// Slice

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