import {parseCookie} from "next/dist/compiled/@edge-runtime/cookies";

export function extractCookieAuthenticationData(cookie: any) {
	if (cookie === undefined)
		return [undefined, undefined]

	const parsedCookie = parseCookie(cookie)
	return [parsedCookie.get("t"), parsedCookie.get("u")]
}