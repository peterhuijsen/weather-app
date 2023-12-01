import {baseUrl} from "@/src/services/api";
import {RegisterOtpResponse, VerifyOtpResponse} from "@/src/models/auth/otp";

export async function registerHotp(uuid: string | undefined, token: string | undefined) {
	if (token === undefined || uuid === undefined)
		throw "Cannot register HOTP without a valid user id and user access token.";

	const result = await fetch(`${baseUrl}/users/${uuid}/otp/register/hotp`, {
		method: "PUT",
		mode: "cors",
		headers: {
			"Content-Type": "application/json",
			"Authorization": `Bearer ${token}`
		},
	});

	let res = await result.json() as RegisterOtpResponse;
	console.log(res)
	return res;
}

export async function verifyHotp(hotp: string | undefined, uuid: string | undefined, token: string | undefined) {
	if (hotp === undefined || token === undefined || uuid === undefined)
		throw "Cannot register HOTP without a valid hotp string, user id and user access token.";

	const result = await fetch(`${baseUrl}/users/${uuid}/otp/verify/hotp?hotp=${hotp}`, {
		method: "PUT",
		mode: "cors",
		headers: {
			"Content-Type": "application/json",
			"Authorization": `Bearer ${token}`
		},
	})

	let res = await result.json() as VerifyOtpResponse;
	console.log(res)
	return res;
}

export async function registerTotp(uuid: string | undefined, token: string | undefined) {
	if (token === undefined || uuid === undefined)
		throw "Cannot register TOTP without a valid user id and user access token.";

	const result = await fetch(`${baseUrl}/users/${uuid}/otp/register/totp`, {
		method: "PUT",
		mode: "cors",
		headers: {
			"Content-Type": "application/json",
			"Authorization": `Bearer ${token}`
		},
	});

	let res = await result.json() as RegisterOtpResponse;
	console.log(res)
	return res;
}

export async function verifyTotp(hotp: string | undefined, uuid: string | undefined, token: string | undefined) {
	if (hotp === undefined || token === undefined || uuid === undefined)
		throw "Cannot register TOTP without a valid totp string, user id and user access token.";

	const result = await fetch(`${baseUrl}/users/${uuid}/otp/verify/totp?totp=${hotp}`, {
		method: "PUT",
		mode: "cors",
		headers: {
			"Content-Type": "application/json",
			"Authorization": `Bearer ${token}`
		},
	})

	let res = await result.json() as VerifyOtpResponse;
	console.log(res)
	return res;
}

export async function verifyMfa(otp: string | undefined, uuid: string | undefined, token: string | undefined) {
	if (otp === undefined || token === undefined || uuid === undefined)
		throw "Cannot verify MFA without a valid otp string, user id and user access token.";

	await fetch(`${baseUrl}/users/${uuid}/login/mfa?otp=${otp}`, {
		method: "POST",
		mode: "cors",
		credentials: "include",
		headers: {
			"Content-Type": "application/json",
			"Authorization": `Bearer ${token}`
		},
	})
}