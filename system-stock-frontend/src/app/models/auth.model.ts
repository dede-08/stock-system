export interface LoginResponse {
    token: string;
    refreshToken?: string;
    email?: string;
    fullName?: string;
}

export interface RegisterRequest {
    name: string;
    lastname: string;
    age: number;
    email: string;
    telephone?: string;
    password: string;
}
