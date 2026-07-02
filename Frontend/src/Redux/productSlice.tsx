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

export const fetchProducts = createAsyncThunk<
  Product[], 
  void,      
  { rejectValue: string } 
>(
  'products/fetchAll',
  async (_, thunkAPI) => {
    try {
      const response = await productApi.getAll();
      
      const data = (response as any).data || response;

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