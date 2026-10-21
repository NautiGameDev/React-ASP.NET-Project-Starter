export async function apiRequest(endpoint, method, body, isRetry = false) {
    const options = {
        method: method,
        headers: {
            'Content-Type': 'application/json'
        },
        credentials: 'include'
    }

    //Ensures body is only defined when available to prevent exceptions
    if (body !== null && body !== undefined) {
        options.body = JSON.stringify(body);
    }

    try {
        const response = await fetch(`/api/${endpoint}`, options);

        //Attempts to retrieve new auth token based on refresh token.
        //If successful, retries api call. Else return status 401 to redirect client to session expired page
        if (response.status === 401 && !isRetry && endpoint !== 'account/login' && endpoint !== 'account/register') {
            const refreshResponse = await attemptRefresh();

            if (refreshResponse.status === 200) {
                return await apiRequest(endpoint, method, body, true);
            }
            else {
                return {
                    status: 401,
                    message: "Session expired",
                    data: null
                };
            }
        }

        //Handle reject access to non-authorized users
        if (response.status === 403) {
            return {
                status: response.status,
                message: "Access Denied",
                data: null
            }
        }

        //Error handling
        if (!response.ok) {
            let errorMessage = "Unknown error communicating with server";

            try {
                const errorData = await response.json();

                if (errorData && errorData.message) {
                    errorMessage = errorData.message;
                }                
            }
            catch (jsonError) {
                console.log("No JSON body found in error message");
                console.log(jsonError);
            }

            return {
                status: response.status,
                message: errorMessage,
                data: null
            }
        }

        //Response successful - data parsing
        const data = await response.json();

        const message = data.message ? data.message : "Success";

        return {
            status: response.status,
            message: message,
            data: data
        }


    }
    catch (error) {
        const errorMessage = error instanceof Error ? error.message : "An unexpected error has occurred";

        return {
            status: 500,
            message: errorMessage,
            data: null
        }
    }
}

async function attemptRefresh() {
    try {
        const response = await fetch('/api/account/refresh', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            credentials: 'include'
        });

        return response;
    } catch {
        return false;
    }
}