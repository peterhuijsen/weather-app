export const decodeBase64Url = (encoded: string) => {
	try {
		return decodeBase64(encoded.replace(/-/g, '+').replace(/_/g, '/').replace(/\s/g, ''));
	}
	catch (_a) {
		throw new TypeError('The input to be decoded is not correctly encoded.');
	}
};

export const decodeBase64 = (encoded: string) => {
	return new Uint8Array(atob(encoded)
		.split('')
		.map((c) => c.charCodeAt(0)));
}