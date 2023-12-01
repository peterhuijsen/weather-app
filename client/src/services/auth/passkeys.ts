import {baseUrl} from "@/src/services/api";
import {decodeBase64Url} from "@/src/helpers/encoding";
import {
	PasskeyAssertionConfiguration,
	PasskeyAttestationConfiguration,
	PasskeyAuthenticationConfiguration,
	PasskeyRegistrationConfiguration
} from "@/src/models/auth/passkeys";

export async function buildPasskeyRegistrationConfiguration(uuid: string | undefined, token: string | undefined) {
	if (uuid === undefined)
		throw "Cannot register a passkey for an invalid user."

	const result = await fetch(`${baseUrl}/users/${uuid}/webauthn/attest/generate`, {
		method: "PUT",
		mode: "cors",
		headers: {
			"Content-Type": "application/json",
			"Authorization": `Bearer ${token}`
		},
	});

	return await result.json() as PasskeyRegistrationConfiguration
}

export async function createPasskeyAttestation(configuration: PasskeyRegistrationConfiguration) {
	const encoder = new TextEncoder()

	const challenge = decodeBase64Url(configuration.challenge)
	const id = encoder.encode(configuration.user.id)

	const publicKeyCredentialCreationOptions: PublicKeyCredentialCreationOptions = {
		challenge: challenge,
		rp: {
			name: "Thunor",
			id: "localhost",
		},
		user: {
			id: id,
			name: configuration.user.name,
			displayName: configuration.user.displayName
		},
		authenticatorSelection: {
			requireResidentKey: true
		},
		pubKeyCredParams: [{alg: -7, type: "public-key"}],
		timeout: 60_000
	};

	const response = await navigator.credentials.create({
		publicKey: publicKeyCredentialCreationOptions
	}) as PublicKeyCredential | null

	if (response === null)
		throw 'The given configuration could not create a valid key credential.'

	const attestation = response?.response as AuthenticatorAttestationResponse
	return {
		id: response.id,
		rawId: Buffer.from(response.rawId).toString('base64'),
		type: response.type,
		response: {
			clientDataJSON: Buffer.from(attestation.clientDataJSON).toString('base64'),
			attestationObject: Buffer.from(attestation.attestationObject).toString('base64')
		},
		authenticatorAttachment: response.authenticatorAttachment
	} as PasskeyAttestationConfiguration
}

export async function confirmPasskeyRegistration(uuid: string | undefined, data: PasskeyAttestationConfiguration | undefined, token: string | undefined) {
	if (uuid === undefined || data === undefined || token === undefined)
		throw "Cannot confirm registration without a valid uuid and key pair response from authenticator."

	const result = await fetch(`${baseUrl}/users/${uuid}/webauthn/attest`, {
		method: "PUT",
		cache: "no-cache",
		mode: "cors",
		headers: {
			"Content-Type": "application/json",
			"Authorization": `Bearer ${token}`
		},
		body: JSON.stringify(data)
	});

	return await result.text();
}

export async function buildPasskeyAuthenticationConfiguration(state: string) {
	const result = await fetch(`${baseUrl}/users/login/webauthn/assert/generate?state=${state}`, {
		method: "POST",
		mode: "cors",
		headers: {
			"Content-Type": "application/json"
		},
	});

	return await result.json() as PasskeyAuthenticationConfiguration
}

export async function createPasskeyAssertion(configuration: PasskeyAuthenticationConfiguration) {
	const challenge = decodeBase64Url(configuration.challenge)

	const publicKeyCredentialCreationOptions: PublicKeyCredentialRequestOptions = {
		challenge: challenge,
		rpId: "localhost",
		timeout: 60_000,
	}

	const response = await navigator.credentials.get({
		publicKey: publicKeyCredentialCreationOptions,
		mediation: 'optional'
	}) as PublicKeyCredential | null

	if (response === null)
		throw 'The given configuration could not create a valid key credential.'

	const assertion = response?.response as AuthenticatorAssertionResponse
	return {
		id: response.id,
		type: response.type,
		response: {
			clientDataJSON: Buffer.from(assertion.clientDataJSON).toString('base64'),
			authenticatorData: Buffer.from(assertion.authenticatorData).toString('base64'),
			signature: Buffer.from(assertion.signature).toString('base64'),
			user: Buffer.from(assertion.userHandle!).toString('base64')
		},
		authenticatorAttachment: response.authenticatorAttachment
	} as PasskeyAssertionConfiguration
}

export async function confirmPasskeyAuthentication(state: string, data: PasskeyAssertionConfiguration | undefined) {
	if (data === undefined)
		throw "Cannot confirm authentication without a valid response from authenticator."

	await fetch(`${baseUrl}/users/login/webauthn/assert?state=${state}`, {
		method: "POST",
		mode: "cors",
		credentials: "include",
		headers: {
			"Content-Type": "application/json"
		},
		body: JSON.stringify(data)
	});
}