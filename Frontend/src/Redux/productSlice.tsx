import { createSlice, createAsyncThunk, PayloadAction } from '@reduxjs/toolkit';
import productApi, { Product } from '../Api/productApi';

interface ProductState {
  items: Product[];
  loading: boolean;
  error: string | null;
}

const initialState: ProductState = {
  items: [],
  loading: false,
  error: null,
};

// Fetch API 
export const fetchProducts = createAsyncThunk<
  Product[], 
  void,      
  { rejectValue: string } 
>(
  'products/fetchAll',
  async (_, thunkAPI) => {
    try {
      const response = await productApi.getAll();
      
      // Kiểm tra nếu response là AxiosResponse thì lấy thuộc tính .data
      // Nếu productApi.getAll() đã được bóc tách dữ liệu, thì lấy trực tiếp response
      const data = (response as any).data || response;

      // Hỗ trợ bóc tách cấu trúc $values của .NET nếu có
      return Array.isArray(data) ? data : (data?.$values || []);
    } catch (error: any) {
      return thunkAPI.rejectWithValue(
        error.message || 'Không thể tải sản phẩm'
      );
    }
  }
);

const productSlice = createSlice({
  name: 'products',
  initialState,
  reducers: {},
  extraReducers: (builder) => {
    builder
      .addCase(fetchProducts.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(fetchProducts.fulfilled, (state, action: PayloadAction<Product[]>) => {
        state.loading = false;
        state.items = action.payload ?? []; 
      })
      .addCase(fetchProducts.rejected, (state, action) => {
        state.loading = false;
        state.items = []; 
        state.error = action.payload || action.error.message || 'Lỗi không xác định';
      });
  },
});

export default productSlice.reducer;