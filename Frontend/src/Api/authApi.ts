import axiosClient from "./axiosClient";

const authApi = {
    login: (data: any) => {
        const url = '/Auth/login'; 
        return axiosClient.post(url, data);
    },

    register: (data: any) => {
        const url = '/Auth/register';
        return axiosClient.post(url, data);
    }
};

export default authApi;