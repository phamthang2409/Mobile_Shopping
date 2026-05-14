import axiosClient from "./axiosClient";

export interface Product { 
    id: number;
    name: string;
    price: number;
    imageUrl?: string;
}
const productApi = {
    getAll: () => {
        return axiosClient.get('/Product');
    },
    searchProducts: (name: string) => {
    return axiosClient.get(`/Product/search?name=${name}`);
}
};


export default productApi;