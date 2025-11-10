export interface LoginResponse {
    token: string;
}

export interface RegisterRequest {
    name: string;
    lastname: string;
    email: string;
    password: string;
}
