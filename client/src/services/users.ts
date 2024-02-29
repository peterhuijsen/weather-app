import {baseUrl} from "@/src/services/api";
import {User, UserUnauthorizedResponse} from "@/src/models/user";

export async function getUser(uuid: string | undefined, token: string | undefined) {
    if (token === undefined || uuid === undefined)
        throw "Cannot fetch user without a valid user id and user access token.";

    const result = await fetch(`${baseUrl}/users/${uuid}`, {
        method: "GET",
        mode: "cors",
        cache: "reload",
        headers: {
            "Content-Type": "application/json",
            "Authorization": `Bearer ${token}`
        }
    });

    try {
        return await result.json() as User;
    } catch (e) {
        return await result.json() as UserUnauthorizedResponse;
    }
}

export async function updateUser(uuid: string | undefined, token: string | undefined, mfa?: boolean, username?: string) {
    if (token === undefined || uuid === undefined)
        throw "Cannot update user without a valid user id and user access token.";

    const configuration = {
        mfa: mfa,
        username: username
    }

    const result = await fetch(`${baseUrl}/users/${uuid}`, {
        method: "PUT",
        mode: "cors",
        headers: {
            "Content-Type": "application/json",
            "Authorization": `Bearer ${token}`
        },
        body: JSON.stringify(configuration)
    });


    let res = await result.json() as User;
    console.log(res)
    return res;
}

export async function removeUser(uuid: string | undefined, token: string | undefined) {

    if (token === undefined || uuid === undefined)
        throw "Cannot update user without a valid user id and user access token.";

    await fetch(`${baseUrl}/users/${uuid}`, {
        method: "DELETE",
        mode: "cors",
        headers: {
            "Content-Type": "application/json",
            "Authorization": `Bearer ${token}`
        }
    });
}


