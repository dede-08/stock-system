export interface LoginResponse {
    token: string;
    refreshToken?: string;
    email?: string;
    fullName?: string;
}

export interface RegisterRequest {
    name: string;
    lastname: string;
    email: string;
    password: string;
}
