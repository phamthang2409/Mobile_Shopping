import axiosClient from "./axiosClient";

export interface OrderItemRequest {
  productId: number;
  quantity: number;
}

export interface OrderRequestData {
  Name: string;       
  Phone: string; 
  Address: string; 
  Note?: string; 
  PaymentMethod: string; 
  Items: OrderItemRequest[]; 
}

const cartApi = {
  // Đồng bộ userId là string để khớp với Identity GUID
  checkout: (userId: string, orderData: OrderRequestData) => {
    return axiosClient.post(`/Order/checkout/${userId}`, orderData);
  },

  getHistory: (userId: string) => {
    return axiosClient.get(`/Order/history/${userId}`);
  },

  getAllOrders: () => {
    return axiosClient.get(`/Order`);
  }
};

export default cartApi;