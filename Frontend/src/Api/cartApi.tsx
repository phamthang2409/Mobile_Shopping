import axiosClient from "./axiosClient";

// Định nghĩa cấu trúc Item trong đơn hàng khớp với OrderItemRequestDTO bên C#
export interface OrderItemRequest {
  productId: number;
  quantity: number;
  price: number;
}

// Định nghĩa cấu trúc dữ liệu đặt hàng khớp với OrderRequestDTO bên C#
export interface OrderRequestData {
  Name: string;    
  Phone: string;   
  Address: string; 
  Note?: string;  
  Items: OrderItemRequest[]; 
}

const cartApi = {
  getCart: (userId: number) => {
    return axiosClient.get(`/Cart/${userId}`);
  },
  
  addToCart: (userId: number, productId: number, quantity: number = 1) => {
    return axiosClient.post(`/Cart/add/${userId}`, { productId, quantity });
  },

  removeFromCart: (userId: number, productId: number) => {
    return axiosClient.delete(`/Cart/remove/${userId}/${productId}`);
  },

  checkout: (userId: number, orderData: OrderRequestData) => {
    return axiosClient.post(`/Cart/checkout/${userId}`, orderData);
  }
};

export default cartApi;