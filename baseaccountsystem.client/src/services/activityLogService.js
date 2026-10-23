import { apiRequest } from '../Clients/apiClient';

export async function GetActivityLogs() {
    return await apiRequest("activitylog", 'GET', null);
}