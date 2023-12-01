export type PasskeyAttestationConfiguration = {
	id: string,
	type: string,
	response: {
		clientDataJSON: string,
		attestationObject: string
	},
	authenticatorAttachment: string
}

export type PasskeyAssertionConfiguration = {
	id: string,
	type: string,
	response: {
		clientDataJSON: string,
		authenticatorData: string,
		signature: string,
		user: string
	},
	authenticatorAttachment: string
}

export type PasskeyAuthenticationConfiguration = {
	challenge: string
}

export type PasskeyRegistrationConfiguration = {
	challenge: string,
	user: {
		id: string,
		name: string,
		displayName: string
	}
}

export type Passkey = {
	id: Uint8Array,
	publicKey: Uint8Array
}