import { apiRequest } from '../Clients/apiClient';

export async function handleLogin(username, password) {

    const body = {
        userName: username,
        password: password
    }

    return await apiRequest("account/login", 'POST', body);
}

export async function handleRegistration(username, email, password) {
    const body = {
        userName: username,
        email: email,
        password: password
    }

    return await apiRequest("account/register", 'POST', body);
}

export async function checkLoggedIn() {
    return await apiRequest("account/check-logged", 'GET', null);
}

export async function handleLogout() {
    return await apiRequest("account/logout", 'POST', null);
}

export async function confirmEmail(userId, confirmationCode) {
    const body = {
        userId: userId,
        confirmationCode: confirmationCode
    }

    return await apiRequest("account/confirm-email", 'POST', body);
}

export async function requestPasswordReset(email) {
    const body = {
        email: email
    }

    return await apiRequest("account/request-reset-password", 'POST', body);
}

export async function confirmPasswordReset(userId, confirmationCode, newPassword) {
    const body = {
        userId: userId,
        code: confirmationCode,
        newPassword: newPassword
    }

    return await apiRequest("account/confirm-reset-password", 'POST', body);
}