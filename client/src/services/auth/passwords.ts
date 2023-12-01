import {baseUrl} from "@/src/services/api";

export async function passwordRegisterUser(username: string | undefined, email: string | undefined, password: string | undefined) {
	if (username === undefined || email === undefined || password === undefined)
		throw "Cannot login user without a valid username, email, and password.";

	await fetch(`${baseUrl}/users/register`, {
		method: "POST",
		credentials: "include",
		mode: "cors",
		headers: {
			"Content-Type": "application/json"
		},
		body: JSON.stringify({
			username: username,
			email: email,
			password: password
		})
	});
}

export async function passwordLoginUser(email: string | undefined, password: string | undefined) {
	if (email === undefined || password === undefined)
		throw "Cannot login user without a valid email and password.";

	await fetch(`${baseUrl}/users/login/password`, {
		method: "POST",
		mode: "cors",
		credentials: "include",
		headers: {
			"Content-Type": "application/json"
		},
		body: JSON.stringify({
			email: email,
			password: password
		})
	});
}